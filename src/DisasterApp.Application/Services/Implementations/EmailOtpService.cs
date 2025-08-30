using DisasterApp.Application.DTOs;
using DisasterApp.Application.Services.Interfaces;
using DisasterApp.Infrastructure.Repositories.Interfaces;
using DisasterApp.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace DisasterApp.Application.Services.Implementations;
public class EmailOtpService : IEmailOtpService
{
    private readonly IUserRepository _userRepository;
    private readonly IOtpCodeRepository _otpCodeRepository;
    private readonly IEmailService _emailService;
    private readonly IAuthService _authService;
    private readonly IRateLimitingService _rateLimitingService;
    private readonly IRoleService _roleService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailOtpService> _logger;

    public EmailOtpService(
        IUserRepository userRepository,
        IOtpCodeRepository otpCodeRepository,
        IEmailService emailService,
        IAuthService authService,
        IRateLimitingService rateLimitingService,
        IRoleService roleService,
        IRefreshTokenRepository refreshTokenRepository,
        IConfiguration configuration,
        ILogger<EmailOtpService> logger)
    {
        _userRepository = userRepository;
        _otpCodeRepository = otpCodeRepository;
        _emailService = emailService;
        _authService = authService;
        _rateLimitingService = rateLimitingService;
        _roleService = roleService;
        _refreshTokenRepository = refreshTokenRepository;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<SendEmailOtpResponseDto> SendOtpAsync(SendEmailOtpRequestDto request, string ipAddress)
    {
        try
        {
            // Check rate limiting for email
            var canSend = await _rateLimitingService.CanSendOtpAsync(request.email, ipAddress);

            if (!canSend)
            {
                _logger.LogWarning("Rate limit exceeded for sending OTP to {Email} from IP {IP}", request.email, ipAddress);
                throw new InvalidOperationException("Too many requests. Please wait before requesting another code.");
            }

            // Check rate limiting for IP
            var user = await _userRepository.GetByEmailAsync(request.email);
            if (user == null)
            {
                // Create a new user
                user = new User
                {
                    UserId = Guid.NewGuid(),
                    AuthProvider = "email",
                    AuthId = request.email,
                    Name = ExtractNameFromEmail(request.email),
                    Email = request.email,
                    CreatedAt = DateTime.UtcNow
                };
                await _userRepository.CreateAsync(user);
            }

            // Generate 6-digit OTP
            var otpCode = GenerateOtpCode();
            var expiresAt = DateTime.UtcNow.AddMinutes(5);

            // Clean up old codes for this user
            await _otpCodeRepository.DeleteByUserAsync(user.UserId, request.purpose);

            // Store OTP in database
            var otpEntity = new OtpCode
            {
                Id = Guid.NewGuid(),
                UserId = user.UserId,
                Code = otpCode,
                Type = request.purpose,
                ExpiresAt = expiresAt,
                AttemptCount = 0,
                CreatedAt = DateTime.UtcNow
            };

            await _otpCodeRepository.CreateAsync(otpEntity);

            // Send email
            var emailSent = await SendOtpEmail(request.email, otpCode);

            if (!emailSent)
            {
                _logger.LogError("Failed to send OTP email to {Email}", request.email);
                // Rollback
                await _otpCodeRepository.DeleteAsync(otpEntity.Id);
                throw new InvalidOperationException("Failed to send verification code. Please try again.");
            }

            // Record attempt
            await _rateLimitingService.RecordAttemptAsync(user.UserId, request.email, ipAddress, "send_otp", true);

            _logger.LogInformation("OTP sent successfully to {Email} for purpose {Purpose}. Code: {Code}, UserId: {UserId}",
                request.email, request.purpose, otpCode, user.UserId);

            return new SendEmailOtpResponseDto
            {
                message = "Verification code sent to your email",
                expiresAt = expiresAt,
                retryAfter = 60
            };
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending OTP to {Email}", request.email);
            throw new InvalidOperationException("An error occurred while sending the verification code");
        }
    }

    public async Task<VerifyEmailOtpResponseDto> VerifyOtpAsync(VerifyEmailOtpRequestDto request, string ipAddress)
    {
        try
        {
            // Find user by email
            var user = await _userRepository.GetByEmailAsync(request.email);
            if (user == null)
            {
                _logger.LogWarning("User not found for {Email}", request.email);
                throw new UnauthorizedAccessException("Invalid or expired verification code");
            }

            // Find OTP code
            var otpCode = await _otpCodeRepository.GetByUserAndCodeAsync(user.UserId, request.otp, request.purpose);

            if (otpCode == null)
            {
                _logger.LogWarning("OTP not found for {Email} with code {Code} and purpose {Purpose}", request.email, request.otp, request.purpose);
                await _rateLimitingService.RecordAttemptAsync(user.UserId, request.email, ipAddress, "verify_otp", false);
                throw new UnauthorizedAccessException("Invalid or expired verification code");
            }

            // Check validity
            if (!otpCode.IsValid)
            {
                _logger.LogWarning("Invalid OTP code for {Email} - expired or already used", request.email);
                await _rateLimitingService.RecordAttemptAsync(user.UserId, request.email, ipAddress, "verify_otp", false);
                throw new UnauthorizedAccessException("Verification code has expired or was already used");
            }

            // Check attempt limit
            if (otpCode.HasReachedMaxAttempts)
            {
                _logger.LogWarning("Too many OTP attempts for {Email}", request.email);
                await _otpCodeRepository.DeleteAsync(otpCode.Id);
                await _rateLimitingService.RecordAttemptAsync(user.UserId, request.email, ipAddress, "verify_otp", false);
                throw new UnauthorizedAccessException("Too many failed attempts. Please request a new code.");
            }

            // Increment attempt count
            otpCode.AttemptCount++;
            await _otpCodeRepository.UpdateAsync(otpCode);

            // Check if this is a new user (just created for email OTP)
            var isNewUser = user.AuthProvider == "email" && user.CreatedAt.HasValue &&
                          user.CreatedAt.Value > DateTime.UtcNow.AddMinutes(-10);
            // Record successful attempt
            await _rateLimitingService.RecordAttemptAsync(user.UserId, request.email, ipAddress, "verify_otp", true);
            // Generate tokens
            var userRoles = await _roleService.GetUserRolesAsync(user.UserId);
            var roles = userRoles.Select(r => r.Name).ToList();
            var accessToken = GenerateAccessToken(user, roles);
            var refreshToken = await GenerateRefreshTokenAsync(user.UserId);

            var accessTokenExpirationMinutes = int.Parse(_configuration["Jwt:AccessTokenExpirationMinutes"] ?? "60");

            // Mark OTP as used after successful authentication
            otpCode.UsedAt = DateTime.UtcNow;
            await _otpCodeRepository.UpdateAsync(otpCode);

            _logger.LogInformation("Email OTP verification successful for {Email}", request.email);

            return new VerifyEmailOtpResponseDto
            {
                accessToken = accessToken,
                refreshToken = refreshToken.Token,
                expiresAt = DateTime.UtcNow.AddMinutes(accessTokenExpirationMinutes),
                isNewUser = isNewUser,
                user = new EmailOtpUserDto
                {
                    userId = user.UserId.ToString(),
                    name = user.Name,
                    email = user.Email,
                    photoUrl = user.PhotoUrl,
                    roles = roles
                }
            };
        }
        catch (UnauthorizedAccessException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying OTP for {Email}", request.email);
            throw new InvalidOperationException("An error occurred during verification");
        }
    }

    private string GenerateOtpCode()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[4];
        rng.GetBytes(bytes);
        var code = Math.Abs(BitConverter.ToInt32(bytes, 0)) % 1000000;
        return code.ToString("D6");
    }

    private string ExtractNameFromEmail(string email)
    {
        var localPart = email.Split('@')[0];
        var name = localPart.Replace(".", " ").Replace("_", " ");
        return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(name.ToLower());
    }

    private async Task<bool> SendOtpEmail(string email, string otpCode)
    {
        var subject = "Your verification code";
        var body = $@"
            <html>
            <body>
                <h2>Your verification code is: <strong>{otpCode}</strong></h2>
                <p>This code expires in 5 minutes.</p>
                <p>If you didn't request this code, please ignore this email.</p>
            </body>
            </html>";

        return await _emailService.SendEmailAsync(email, subject, body);
    }

    private string GenerateAccessToken(User user, List<string> roles)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var key = Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Key not configured"));
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("auth_provider", user.AuthProvider ?? "email")
            }.Concat(roles.Select(role => new Claim(ClaimTypes.Role, role)))),
            Expires = DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["AccessTokenExpirationMinutes"] ?? "60")),
            Issuer = jwtSettings["Issuer"],
            Audience = jwtSettings["Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private async Task<RefreshToken> GenerateRefreshTokenAsync(Guid userId)
    {
        var refreshToken = new RefreshToken
        {
            RefreshTokenId = Guid.NewGuid(),
            UserId = userId,
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            ExpiredAt = DateTime.UtcNow.AddDays(int.Parse(_configuration["Jwt:RefreshTokenExpirationDays"] ?? "7")),
            CreatedAt = DateTime.UtcNow
        };

        await _refreshTokenRepository.CreateAsync(refreshToken);
        return refreshToken;
    }
}