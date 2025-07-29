using DisasterApp.Application.DTOs;
using DisasterApp.Application.Services.Interfaces;
using DisasterApp.Domain.Entities;
using DisasterApp.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BCrypt.Net;
using Google.Apis.Auth;

namespace DisasterApp.Application.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordResetTokenRepository _passwordResetTokenRepository;
    private readonly IEmailService _emailService;
    private readonly IRoleService _roleService;
    private readonly IPasswordValidationService _passwordValidationService;
    private readonly IConfiguration _configuration;

    public AuthService(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordResetTokenRepository passwordResetTokenRepository,
        IEmailService emailService,
        IRoleService roleService,
        IPasswordValidationService passwordValidationService,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordResetTokenRepository = passwordResetTokenRepository;
        _emailService = emailService;
        _roleService = roleService;
        _passwordValidationService = passwordValidationService;
        _configuration = configuration;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null)
            throw new UnauthorizedAccessException("Invalid email or password");

        // For OAuth users, they don't have a password stored
        if (user.AuthProvider != "Email")
            throw new UnauthorizedAccessException("Please use social login for this account");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.AuthId))
            throw new UnauthorizedAccessException("Invalid email or password");

        var roles = await _userRepository.GetUserRolesAsync(user.UserId);
        var accessToken = GenerateAccessToken(user, roles);
        var refreshToken = await GenerateRefreshTokenAsync(user.UserId);

        var accessTokenExpirationMinutes = int.Parse(_configuration["Jwt:AccessTokenExpirationMinutes"] ?? "60");

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(accessTokenExpirationMinutes),
            User = new UserDto
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                PhotoUrl = user.PhotoUrl,
                Roles = roles
            }
        };
    }

    public async Task<AuthResponseDto> SignupAsync(SignupRequestDto request)
    {
        if (await _userRepository.ExistsAsync(request.Email))
            throw new InvalidOperationException("User with this email already exists");

        if (!request.AgreeToTerms)
            throw new InvalidOperationException("You must agree to the terms of service");

        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User
        {
            UserId = Guid.NewGuid(),
            Name = request.FullName,
            Email = request.Email,
            AuthProvider = "Email",
            AuthId = hashedPassword,
            CreatedAt = DateTime.UtcNow,
            IsBlacklisted = false
        };

        var createdUser = await _userRepository.CreateAsync(user);

        // Assign default admin role to new user
        await _roleService.AssignDefaultRoleToUserAsync(createdUser.UserId);

        var userRoles = await _roleService.GetUserRolesAsync(createdUser.UserId);
        var roles = userRoles.Select(r => r.Name).ToList();
        var accessToken = GenerateAccessToken(createdUser, roles);
        var refreshToken = await GenerateRefreshTokenAsync(createdUser.UserId);

        var accessTokenExpirationMinutes = int.Parse(_configuration["Jwt:AccessTokenExpirationMinutes"] ?? "60");

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(accessTokenExpirationMinutes),
            User = new UserDto
            {
                UserId = createdUser.UserId,
                Name = createdUser.Name,
                Email = createdUser.Email,
                PhotoUrl = createdUser.PhotoUrl,
                Roles = roles
            }
        };
    }

    public async Task<AuthResponseDto> GoogleLoginAsync(GoogleLoginRequestDto request)
    {
        try
        {
            var clientId = _configuration["GoogleAuth:ClientId"] ?? throw new InvalidOperationException("Google Client ID not configured");

            // Verify the Google ID token
            var payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { clientId }
            });

            if (payload == null)
                throw new UnauthorizedAccessException("Invalid Google token");

            // Check if user exists
            var existingUser = await _userRepository.GetByEmailAsync(payload.Email);

            if (existingUser != null)
            {
                // User exists, log them in
                if (existingUser.AuthProvider != "Google")
                {
                    // Update existing local user to Google auth
                    existingUser.AuthProvider = "Google";
                    existingUser.AuthId = payload.Subject;
                    existingUser.PhotoUrl = payload.Picture;
                    await _userRepository.UpdateAsync(existingUser);
                }

                var roles = await _userRepository.GetUserRolesAsync(existingUser.UserId);
                var accessToken = GenerateAccessToken(existingUser, roles);
                var refreshToken = await GenerateRefreshTokenAsync(existingUser.UserId);

                var accessTokenExpirationMinutes = int.Parse(_configuration["Jwt:AccessTokenExpirationMinutes"] ?? "60");

                return new AuthResponseDto
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken.Token,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(accessTokenExpirationMinutes),
                    User = new UserDto
                    {
                        UserId = existingUser.UserId,
                        Name = existingUser.Name,
                        Email = existingUser.Email,
                        PhotoUrl = existingUser.PhotoUrl,
                        Roles = roles
                    }
                };
            }
            else
            {
                // Create new user
                var newUser = new User
                {
                    UserId = Guid.NewGuid(),
                    Name = payload.Name,
                    Email = payload.Email,
                    AuthProvider = "Google",
                    AuthId = payload.Subject,
                    PhotoUrl = payload.Picture,
                    CreatedAt = DateTime.UtcNow,
                    IsBlacklisted = false
                };

                var createdUser = await _userRepository.CreateAsync(newUser);

                // Assign default admin role to new user
                await _roleService.AssignDefaultRoleToUserAsync(createdUser.UserId);

                var userRoles = await _roleService.GetUserRolesAsync(createdUser.UserId);
                var roles = userRoles.Select(r => r.Name).ToList();
                var accessToken = GenerateAccessToken(createdUser, roles);
                var refreshToken = await GenerateRefreshTokenAsync(createdUser.UserId);

                var accessTokenExpirationMinutes = int.Parse(_configuration["Jwt:AccessTokenExpirationMinutes"] ?? "60");

                return new AuthResponseDto
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken.Token,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(accessTokenExpirationMinutes),
                    User = new UserDto
                    {
                        UserId = createdUser.UserId,
                        Name = createdUser.Name,
                        Email = createdUser.Email,
                        PhotoUrl = createdUser.PhotoUrl,
                        Roles = roles
                    }
                };
            }
        }
        catch (Exception ex) when (!(ex is UnauthorizedAccessException || ex is InvalidOperationException))
        {
            throw new UnauthorizedAccessException("Failed to authenticate with Google", ex);
        }
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request)
    {
        var refreshToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken);
        if (refreshToken == null || refreshToken.ExpiredAt <= DateTime.UtcNow)
            throw new UnauthorizedAccessException("Invalid or expired refresh token");

        var user = refreshToken.User;
        var roles = await _userRepository.GetUserRolesAsync(user.UserId);
        var newAccessToken = GenerateAccessToken(user, roles);
        var newRefreshToken = await GenerateRefreshTokenAsync(user.UserId);

        // Delete old refresh token
        await _refreshTokenRepository.DeleteAsync(request.RefreshToken);

        var accessTokenExpirationMinutes = int.Parse(_configuration["Jwt:AccessTokenExpirationMinutes"] ?? "60");

        return new AuthResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken.Token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(accessTokenExpirationMinutes),
            User = new UserDto
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                PhotoUrl = user.PhotoUrl,
                Roles = roles
            }
        };
    }

    public async Task<bool> LogoutAsync(string refreshToken)
    {
        return await _refreshTokenRepository.DeleteAsync(refreshToken);
    }

    public Task<bool> ValidateTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT key not configured"));

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public string GenerateJwtToken(User user)
    {
        // Get user roles
        var roles = _userRepository.GetUserRolesAsync(user.UserId).GetAwaiter().GetResult();
        return GenerateAccessToken(user, roles);
    }

    private string GenerateAccessToken(User user, List<string> roles)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT key not configured"));

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Email, user.Email)
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var accessTokenExpirationMinutes = int.Parse(_configuration["Jwt:AccessTokenExpirationMinutes"] ?? "60");

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(accessTokenExpirationMinutes),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private async Task<RefreshToken> GenerateRefreshTokenAsync(Guid userId)
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);

        var refreshTokenExpirationDays = int.Parse(_configuration["Jwt:RefreshTokenExpirationDays"] ?? "30");

        var refreshToken = new RefreshToken
        {
            RefreshTokenId = Guid.NewGuid(),
            Token = Convert.ToBase64String(randomBytes),
            UserId = userId,
            ExpiredAt = DateTime.UtcNow.AddDays(refreshTokenExpirationDays),
            CreatedAt = DateTime.UtcNow
        };

        return await _refreshTokenRepository.CreateAsync(refreshToken);
    }


    public async Task<ForgotPasswordResponseDto> ForgotPasswordAsync(ForgotPasswordRequestDto request)
    {
        try
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);

            // Always return success to prevent email enumeration attacks
            if (user == null || user.AuthProvider != "Email")
            {
                return new ForgotPasswordResponseDto
                {
                    Success = true,
                    Message = "If an account with that email exists, a password reset link has been sent."
                };
            }

            // Delete any existing password reset tokens for this user
            await _passwordResetTokenRepository.DeleteAllUserTokensAsync(user.UserId);

            // Generate new reset token
            var resetToken = await GeneratePasswordResetTokenAsync(user.UserId);

            // Send reset email
            var resetUrl = _configuration["Frontend:BaseUrl"] + "/reset-password";
            var emailSent = await _emailService.SendPasswordResetEmailAsync(user.Email, resetToken.Token, resetUrl);

            return new ForgotPasswordResponseDto
            {
                Success = true,
                Message = "If an account with that email exists, a password reset link has been sent."
            };
        }
        catch (Exception)
        {
            return new ForgotPasswordResponseDto
            {
                Success = false,
                Message = "An error occurred while processing your request. Please try again later."
            };
        }
    }

    public async Task<ForgotPasswordResponseDto> ResetPasswordAsync(ResetPasswordRequestDto request)
    {
        try
        {
            // Validate password strength
            var passwordValidation = _passwordValidationService.ValidatePassword(request.NewPassword);
            if (!passwordValidation.IsValid)
            {
                return new ForgotPasswordResponseDto
                {
                    Success = false,
                    Message = string.Join("; ", passwordValidation.Errors)
                };
            }

            var resetToken = await _passwordResetTokenRepository.GetByTokenAsync(request.Token);

            if (resetToken == null || resetToken.ExpiredAt <= DateTime.UtcNow || resetToken.IsUsed)
            {
                return new ForgotPasswordResponseDto
                {
                    Success = false,
                    Message = "Invalid or expired reset token."
                };
            }

            var user = resetToken.User;
            if (user.AuthProvider != "Email")
            {
                return new ForgotPasswordResponseDto
                {
                    Success = false,
                    Message = "Password reset is not available for social login accounts."
                };
            }

            // Update user password
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.AuthId = hashedPassword;

            await _userRepository.UpdateAsync(user);

            // Mark token as used
            await _passwordResetTokenRepository.MarkAsUsedAsync(request.Token);

            return new ForgotPasswordResponseDto
            {
                Success = true,
                Message = "Password has been reset successfully."
            };
        }
        catch (Exception)
        {
            return new ForgotPasswordResponseDto
            {
                Success = false,
                Message = "An error occurred while resetting your password. Please try again later."
            };
        }
    }

    public async Task<VerifyResetTokenResponseDto> VerifyResetTokenAsync(VerifyResetTokenRequestDto request)
    {
        try
        {
            var isValid = await _passwordResetTokenRepository.IsValidAsync(request.Token);

            return new VerifyResetTokenResponseDto
            {
                IsValid = isValid,
                Message = isValid ? "Token is valid." : "Invalid or expired token."
            };
        }
        catch (Exception)
        {
            return new VerifyResetTokenResponseDto
            {
                IsValid = false,
                Message = "An error occurred while verifying the token."
            };
        }
    }

    private async Task<PasswordResetToken> GeneratePasswordResetTokenAsync(Guid userId)
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);

        var resetTokenExpirationHours = int.Parse(_configuration["PasswordReset:ExpirationHours"] ?? "1");

        var passwordResetToken = new PasswordResetToken
        {
            PasswordResetTokenId = Guid.NewGuid(),
            Token = Convert.ToBase64String(randomBytes),
            UserId = userId,
            ExpiredAt = DateTime.UtcNow.AddHours(resetTokenExpirationHours),
            CreatedAt = DateTime.UtcNow,
            IsUsed = false
        };

        return await _passwordResetTokenRepository.CreateAsync(passwordResetToken);

    }
}