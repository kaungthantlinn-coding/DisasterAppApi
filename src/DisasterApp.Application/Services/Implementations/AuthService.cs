using DisasterApp.Application.DTOs;
using DisasterApp.Application.Services.Interfaces;
using DisasterApp.Domain.Entities;
using DisasterApp.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;//
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
    private readonly ITwoFactorService _twoFactorService;
    private readonly IOtpService _otpService;
    private readonly IBackupCodeService _backupCodeService;
    private readonly IRateLimitingService _rateLimitingService;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordResetTokenRepository passwordResetTokenRepository,
        IEmailService emailService,
        IRoleService roleService,
        IPasswordValidationService passwordValidationService,
        ITwoFactorService twoFactorService,
        IOtpService otpService,
        IBackupCodeService backupCodeService,
        IRateLimitingService rateLimitingService,
        ITokenService tokenService,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordResetTokenRepository = passwordResetTokenRepository;
        _emailService = emailService;
        _roleService = roleService;
        _passwordValidationService = passwordValidationService;
        _twoFactorService = twoFactorService;
        _otpService = otpService;
        _backupCodeService = backupCodeService;
        _rateLimitingService = rateLimitingService;
        _tokenService = tokenService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user is null)
            throw new UnauthorizedAccessException("Invalid email or password");

        // Debug logging to check user data from database
        _logger.LogInformation("Retrieved user from DB: UserId={UserId}, Name={Name}, Email={Email}, PhotoUrl={PhotoUrl}, AuthProvider={AuthProvider}", 
            user.UserId, user.Name, user.Email, user.PhotoUrl, user.AuthProvider);

        // For OAuth users, they don't have a password stored
        if (user.AuthProvider != "Email")
            throw new UnauthorizedAccessException("Please use social login for this account");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.AuthId))
            throw new UnauthorizedAccessException("Invalid email or password");

        // Check if user is blacklisted
        if (user.IsBlacklisted == true)
            throw new UnauthorizedAccessException("Account has been suspended");

        var roles = await _userRepository.GetUserRolesAsync(user.UserId);
        _logger.LogInformation("Retrieved roles for user {UserId}: {Roles}", user.UserId, string.Join(",", roles));
        
        // Ensure user has at least the default role (fix for users created before role assignment was implemented)
        if (roles.Count == 0)
        {
            _logger.LogInformation("User {UserId} has no roles assigned, assigning default role", user.UserId);
            await _roleService.AssignDefaultRoleToUserAsync(user.UserId);
            roles = await _userRepository.GetUserRolesAsync(user.UserId);
            _logger.LogInformation("After assigning default role, user {UserId} now has roles: {Roles}", user.UserId, string.Join(",", roles));
        }
        
        var accessToken = GenerateAccessToken(user, roles);
        var refreshToken = await GenerateRefreshTokenAsync(user.UserId);

        var accessTokenExpirationMinutes = int.Parse(_configuration["Jwt:AccessTokenExpirationMinutes"] ?? "60");

        var userDto = new UserDto
        {
            UserId = user.UserId,
            Name = user.Name,
            Email = user.Email,
            PhotoUrl = user.PhotoUrl,
            Roles = roles
        };
        
        _logger.LogInformation("Created UserDto: UserId={UserId}, Name={Name}, Email={Email}, PhotoUrl={PhotoUrl}, Roles={Roles}", 
            userDto.UserId, userDto.Name, userDto.Email, userDto.PhotoUrl, string.Join(",", userDto.Roles));

        return new AuthResponseDto
        {
            AccessToken = accessToken ?? throw new InvalidOperationException("Failed to generate access token"),
            RefreshToken = refreshToken?.Token ?? throw new InvalidOperationException("Failed to generate refresh token"),
            ExpiresAt = DateTime.UtcNow.AddMinutes(accessTokenExpirationMinutes),
            User = userDto
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
            AccessToken = accessToken ?? throw new InvalidOperationException("Failed to generate access token"),
            RefreshToken = refreshToken?.Token ?? throw new InvalidOperationException("Failed to generate refresh token"),
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

            _logger.LogInformation("🔍 GoogleLogin - Starting authentication with ClientId: {ClientId}", clientId?.Length > 10 ? string.Concat(clientId.AsSpan(0, 10), "...") : clientId + "...");
            _logger.LogInformation("🔍 GoogleLogin - Received IdToken length: {TokenLength}", request.IdToken?.Length ?? 0);

            // Verify the Google ID token
            var payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = [clientId]
            });

            if (payload == null)
            {
                _logger.LogWarning("❌ GoogleLogin - Google token validation returned null payload");
                throw new UnauthorizedAccessException("Invalid Google token");
            }

            _logger.LogInformation("✅ GoogleLogin - Token validated successfully. Email: {Email}, Name: {Name}, Subject: {Subject}", 
                payload.Email, payload.Name, payload.Subject);

            // Check if user exists
            var existingUser = await _userRepository.GetByEmailAsync(payload.Email);

            if (existingUser != null)
            {
                _logger.LogInformation("👤 GoogleLogin - Existing user found: {UserId}, Name: {Name}, Email: {Email}", 
                    existingUser.UserId, existingUser.Name, existingUser.Email);
                
                // Check if user is blacklisted
                if (existingUser.IsBlacklisted == true)
                {
                    _logger.LogWarning("❌ GoogleLogin - User {UserId} is blacklisted", existingUser.UserId);
                    throw new UnauthorizedAccessException("Account has been suspended");
                }
                
                // User exists, log them in
                if (existingUser.AuthProvider != "Google")
                {
                    _logger.LogInformation("🔄 GoogleLogin - Updating user auth provider from {OldProvider} to Google", existingUser.AuthProvider);
                    // Update existing local user to Google auth
                    existingUser.AuthProvider = "Google";
                    existingUser.AuthId = payload.Subject;
                    existingUser.PhotoUrl = payload.Picture;
                    await _userRepository.UpdateAsync(existingUser);
                }

                var roles = await _userRepository.GetUserRolesAsync(existingUser.UserId);
                _logger.LogInformation("🔐 GoogleLogin - User roles: {Roles}", string.Join(", ", roles));
                
                // Ensure user has at least the default role (fix for users created before role assignment was implemented)
                if (roles.Count == 0)
                {
                    _logger.LogInformation("🔐 GoogleLogin - User {UserId} has no roles assigned, assigning default role", existingUser.UserId);
                    await _roleService.AssignDefaultRoleToUserAsync(existingUser.UserId);
                    roles = await _userRepository.GetUserRolesAsync(existingUser.UserId);
                    _logger.LogInformation("🔐 GoogleLogin - After assigning default role, user {UserId} now has roles: {Roles}", existingUser.UserId, string.Join(", ", roles));
                }
                
                var accessToken = GenerateAccessToken(existingUser, roles);
                var refreshToken = await GenerateRefreshTokenAsync(existingUser.UserId);
                
                _logger.LogInformation("🎫 GoogleLogin - Generated tokens for existing user. AccessToken length: {AccessTokenLength}", 
                    accessToken?.Length ?? 0);

                var accessTokenExpirationMinutes = int.Parse(_configuration["Jwt:AccessTokenExpirationMinutes"] ?? "60");

                var existingUserResponse = new AuthResponseDto
                {
                    AccessToken = accessToken ?? throw new InvalidOperationException("Failed to generate access token"),
                    RefreshToken = refreshToken?.Token ?? throw new InvalidOperationException("Failed to generate refresh token"),
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
                
                _logger.LogInformation("✅ GoogleLogin - Returning response for existing user: {UserId}, Name: {Name}, Email: {Email}, Roles: {Roles}", 
                    existingUserResponse.User.UserId, existingUserResponse.User.Name, existingUserResponse.User.Email, 
                    string.Join(", ", existingUserResponse.User.Roles));
                
                return existingUserResponse;
            }
            else
            {
                _logger.LogInformation("👤 GoogleLogin - Creating new user from Google payload: {Email}, {Name}", payload.Email, payload.Name);
                
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

                _logger.LogInformation("💾 GoogleLogin - Saving new user: {UserId}, Name: {Name}, Email: {Email}", 
                    newUser.UserId, newUser.Name, newUser.Email);
                
                var createdUser = await _userRepository.CreateAsync(newUser);

                // Assign default admin role to new user
                await _roleService.AssignDefaultRoleToUserAsync(createdUser.UserId);
                
                _logger.LogInformation("🔐 GoogleLogin - Assigned default role to new user: {UserId}", createdUser.UserId);

                var userRoles = await _roleService.GetUserRolesAsync(createdUser.UserId);
                var roles = userRoles.Select(r => r.Name).ToList();
                var accessToken = GenerateAccessToken(createdUser, roles);
                var refreshToken = await GenerateRefreshTokenAsync(createdUser.UserId);
                
                _logger.LogInformation("🎫 GoogleLogin - Generated tokens for new user. AccessToken length: {AccessTokenLength}", 
                    accessToken?.Length ?? 0);

                var accessTokenExpirationMinutes = int.Parse(_configuration["Jwt:AccessTokenExpirationMinutes"] ?? "60");

                var newUserResponse = new AuthResponseDto
                {
                    AccessToken = accessToken ?? throw new InvalidOperationException("Failed to generate access token"),
                    RefreshToken = refreshToken?.Token ?? throw new InvalidOperationException("Failed to generate refresh token"),
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
                
                _logger.LogInformation("✅ GoogleLogin - Returning response for new user: {UserId}, Name: {Name}, Email: {Email}, Roles: {Roles}", 
                    newUserResponse.User.UserId, newUserResponse.User.Name, newUserResponse.User.Email, 
                    string.Join(", ", newUserResponse.User.Roles));
                
                return newUserResponse;
            }
        }
        catch (Exception ex) when (!(ex is UnauthorizedAccessException || ex is InvalidOperationException))
        {
            _logger.LogError(ex, "❌ GoogleLogin - Error during Google login: {ErrorMessage}. StackTrace: {StackTrace}", 
                ex.Message, ex.StackTrace);
            
            // Log additional details if it's a Google validation exception
            if (ex is InvalidJwtException || ex.Message.Contains("Google"))
            {
                _logger.LogError("🔍 GoogleLogin - Google token validation failed. This might be due to:");
                _logger.LogError("   - Invalid or expired Google ID token");
                _logger.LogError("   - Incorrect Google Client ID configuration");
                _logger.LogError("   - Network issues with Google's validation service");
            }
            
            throw new UnauthorizedAccessException("Failed to authenticate with Google", ex);
        }
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request)
    {
        _logger.LogInformation("🔍 RefreshTokenAsync - Attempting to refresh token with length: {Length}", request.RefreshToken?.Length ?? 0);
        
        var refreshToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken);
        
        _logger.LogInformation("🔍 RefreshTokenAsync - Token found: {Found}, Expired: {Expired}", 
            refreshToken != null, 
            refreshToken?.ExpiredAt <= DateTime.UtcNow);
            
        if (refreshToken == null)
        {
            _logger.LogWarning("❌ RefreshTokenAsync - Refresh token not found in database");
            throw new UnauthorizedAccessException("Invalid or expired refresh token");
        }
            
        if (refreshToken.ExpiredAt <= DateTime.UtcNow)
        {
            _logger.LogWarning("❌ RefreshTokenAsync - Refresh token expired. ExpiredAt: {ExpiredAt}, Now: {Now}", 
                refreshToken.ExpiredAt, DateTime.UtcNow);
            throw new UnauthorizedAccessException("Invalid or expired refresh token");
        }

        var user = refreshToken.User;
        var roles = await _userRepository.GetUserRolesAsync(user.UserId);
        
        // Ensure user has at least the default role (fix for users created before role assignment was implemented)
        if (roles.Count == 0)
        {
            _logger.LogInformation("RefreshToken - User {UserId} has no roles assigned, assigning default role", user.UserId);
            await _roleService.AssignDefaultRoleToUserAsync(user.UserId);
            roles = await _userRepository.GetUserRolesAsync(user.UserId);
            _logger.LogInformation("RefreshToken - After assigning default role, user {UserId} now has roles: {Roles}", user.UserId, string.Join(", ", roles));
        }
        
        var newAccessToken = GenerateAccessToken(user, roles);
        var newRefreshToken = await GenerateRefreshTokenAsync(user.UserId);

        // Delete old refresh token (atomic operation handles concurrency)
        var deleted = await _refreshTokenRepository.DeleteAsync(request.RefreshToken);
        if (!deleted)
        {
            _logger.LogDebug("Old refresh token {Token} was already deleted or not found", request.RefreshToken);
        }

        var accessTokenExpirationMinutes = int.Parse(_configuration["Jwt:AccessTokenExpirationMinutes"] ?? "60");

        return new AuthResponseDto
        {
            AccessToken = newAccessToken ?? throw new InvalidOperationException("Failed to generate access token"),
            RefreshToken = newRefreshToken?.Token ?? throw new InvalidOperationException("Failed to generate refresh token"),
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
            _logger.LogInformation("Processing forgot password request for email: {Email}", request.Email);

            var user = await _userRepository.GetByEmailAsync(request.Email);

            // Always return success to prevent email enumeration attacks
            if (user == null)
            {
                _logger.LogInformation("Forgot password request for non-existent user: {Email}", request.Email);
                return new ForgotPasswordResponseDto
                {
                    Success = true,
                    Message = "If an account with that email exists, a password reset link has been sent."
                };
            }

            // Handle different auth providers
            if (user.AuthProvider != "Email")
            {
                _logger.LogInformation("Forgot password request for user with {AuthProvider} authentication: {Email}", user.AuthProvider, request.Email);

                // Send informational email about their auth provider
                var authProviderEmailSent = await _emailService.SendAuthProviderNotificationEmailAsync(user.Email, user.AuthProvider);

                if (!authProviderEmailSent)
                {
                    _logger.LogWarning("Failed to send auth provider notification email to: {Email}", user.Email);
                }
                else
                {
                    _logger.LogInformation("Auth provider notification email sent successfully to: {Email}", user.Email);
                }

                // Still return success to prevent enumeration attacks
                return new ForgotPasswordResponseDto
                {
                    Success = true,
                    Message = "If an account with that email exists, a password reset link has been sent."
                };
            }

            // Delete any existing password reset tokens for this user
            _logger.LogInformation("Deleting existing password reset tokens for user: {UserId}", user.UserId);
            await _passwordResetTokenRepository.DeleteAllUserTokensAsync(user.UserId);

            // Generate new reset token
            _logger.LogInformation("Generating new password reset token for user: {UserId}", user.UserId);
            var resetToken = await GeneratePasswordResetTokenAsync(user.UserId);

            // Send reset email
            var resetUrl = _configuration["Frontend:BaseUrl"] + "/reset-password";
            _logger.LogInformation("Attempting to send password reset email to: {Email}", user.Email);
            var emailSent = await _emailService.SendPasswordResetEmailAsync(user.Email, resetToken.Token, resetUrl);

            if (!emailSent)
            {
                _logger.LogError("Failed to send password reset email to: {Email}", user.Email);
                return new ForgotPasswordResponseDto
                {
                    Success = false,
                    Message = "Failed to send password reset email. Please try again later."
                };
            }

            _logger.LogInformation("Password reset email sent successfully to: {Email}", user.Email);
            return new ForgotPasswordResponseDto
            {
                Success = true,
                Message = "If an account with that email exists, a password reset link has been sent."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while processing forgot password request for email: {Email}", request.Email);
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
            _logger.LogInformation("Processing password reset request for token: {Token}", request.Token);

            // Validate password strength
            var passwordValidation = _passwordValidationService.ValidatePassword(request.NewPassword);
            if (!passwordValidation.IsValid)
            {
                _logger.LogWarning("Password validation failed for reset token: {Token}. Errors: {Errors}",
                    request.Token, string.Join("; ", passwordValidation.Errors));
                return new ForgotPasswordResponseDto
                {
                    Success = false,
                    Message = string.Join("; ", passwordValidation.Errors)
                };
            }

            var resetToken = await _passwordResetTokenRepository.GetByTokenAsync(request.Token);

            if (resetToken == null)
            {
                _logger.LogWarning("Password reset attempted with non-existent token: {Token}", request.Token);
                return new ForgotPasswordResponseDto
                {
                    Success = false,
                    Message = "Invalid or expired reset token."
                };
            }

            if (resetToken.ExpiredAt <= DateTime.UtcNow)
            {
                _logger.LogWarning("Password reset attempted with expired token: {Token}. Expired at: {ExpiredAt}",
                    request.Token, resetToken.ExpiredAt);
                return new ForgotPasswordResponseDto
                {
                    Success = false,
                    Message = "Invalid or expired reset token."
                };
            }

            if (resetToken.IsUsed)
            {
                _logger.LogWarning("Password reset attempted with already used token: {Token}", request.Token);
                return new ForgotPasswordResponseDto
                {
                    Success = false,
                    Message = "Invalid or expired reset token."
                };
            }

            var user = resetToken.User;
            if (user.AuthProvider != "Email")
            {
                _logger.LogWarning("Password reset attempted for non-email user: {UserId}, AuthProvider: {AuthProvider}",
                    user.UserId, user.AuthProvider);
                return new ForgotPasswordResponseDto
                {
                    Success = false,
                    Message = "Password reset is not available for social login accounts."
                };
            }

            // Update user password
            _logger.LogInformation("Updating password for user: {UserId}", user.UserId);
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.AuthId = hashedPassword;

            await _userRepository.UpdateAsync(user);

            // Mark token as used
            _logger.LogInformation("Marking reset token as used: {Token}", request.Token);
            await _passwordResetTokenRepository.MarkAsUsedAsync(request.Token);

            _logger.LogInformation("Password reset completed successfully for user: {UserId}", user.UserId);
            return new ForgotPasswordResponseDto
            {
                Success = true,
                Message = "Password has been reset successfully."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while resetting password for token: {Token}", request.Token);
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
            _logger.LogInformation("Verifying reset token: {Token}", request.Token);

            // Get the token from database for detailed validation
            var resetToken = await _passwordResetTokenRepository.GetByTokenAsync(request.Token);

            if (resetToken == null)
            {
                _logger.LogWarning("Token verification failed: Token not found in database: {Token}", request.Token);
                return new VerifyResetTokenResponseDto
                {
                    IsValid = false,
                    Message = "Invalid or expired token."
                };
            }

            _logger.LogInformation("Token found. UserId: {UserId}, ExpiredAt: {ExpiredAt}, IsUsed: {IsUsed}, CreatedAt: {CreatedAt}",
                resetToken.UserId, resetToken.ExpiredAt, resetToken.IsUsed, resetToken.CreatedAt);

            var currentTime = DateTime.UtcNow;
            _logger.LogInformation("Current UTC time: {CurrentTime}", currentTime);

            if (resetToken.ExpiredAt <= currentTime)
            {
                _logger.LogWarning("Token verification failed: Token expired. Token: {Token}, ExpiredAt: {ExpiredAt}, CurrentTime: {CurrentTime}",
                    request.Token, resetToken.ExpiredAt, currentTime);
                return new VerifyResetTokenResponseDto
                {
                    IsValid = false,
                    Message = "Invalid or expired token."
                };
            }

            if (resetToken.IsUsed)
            {
                _logger.LogWarning("Token verification failed: Token already used: {Token}", request.Token);
                return new VerifyResetTokenResponseDto
                {
                    IsValid = false,
                    Message = "Invalid or expired token."
                };
            }

            _logger.LogInformation("Token verification successful: {Token}", request.Token);
            return new VerifyResetTokenResponseDto
            {
                IsValid = true,
                Message = "Token is valid."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while verifying reset token: {Token}", request.Token);
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

    // Two-Factor Authentication methods

    public async Task<EnhancedAuthResponseDto> LoginWithTwoFactorAsync(LoginRequestDto request)
    {
        try
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null)
            {
                return new EnhancedAuthResponseDto
                {
                    RequiresOTP = false,
                    Message = "Invalid email or password"
                };
            }

            // For OAuth users, they don't have a password stored
            if (user.AuthProvider != "Email")
            {
                return new EnhancedAuthResponseDto
                {
                    RequiresOTP = false,
                    Message = "Please use OAuth login for this account"
                };
            }

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.AuthId))
            {
                return new EnhancedAuthResponseDto
                {
                    RequiresOTP = false,
                    Message = "Invalid email or password"
                };
            }

            // Check if user is blacklisted
            if (user.IsBlacklisted == true)
            {
                return new EnhancedAuthResponseDto
                {
                    RequiresOTP = false,
                    Message = "Account has been suspended"
                };
            }

            // Check if 2FA is enabled
            if (user.TwoFactorEnabled)
            {
                // Generate login token for 2FA verification
                var loginToken = _tokenService.GenerateLoginToken(user.UserId);

                return new EnhancedAuthResponseDto
                {
                    RequiresOTP = true,
                    LoginToken = loginToken,
                    Message = "Please verify your identity with the code sent to your email"
                };
            }

            // Complete login without 2FA
            var userRoles = await _roleService.GetUserRolesAsync(user.UserId);
            var roles = userRoles.Select(r => r.Name).ToList();
            
            // Ensure user has at least the default role (fix for users created before role assignment was implemented)
            if (roles.Count == 0)
            {
                _logger.LogInformation("VerifyOtp - User {UserId} has no roles assigned, assigning default role", user.UserId);
                await _roleService.AssignDefaultRoleToUserAsync(user.UserId);
                userRoles = await _roleService.GetUserRolesAsync(user.UserId);
                roles = userRoles.Select(r => r.Name).ToList();
                _logger.LogInformation("VerifyOtp - After assigning default role, user {UserId} now has roles: {Roles}", user.UserId, string.Join(", ", roles));
            }
            
            var accessToken = GenerateAccessToken(user, roles);
            var refreshToken = await GenerateRefreshTokenAsync(user.UserId);

            var accessTokenExpirationMinutes = int.Parse(_configuration["Jwt:AccessTokenExpirationMinutes"] ?? "60");

            return new EnhancedAuthResponseDto
            {
                RequiresOTP = false,
                AuthResponse = new AuthResponseDto
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
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during enhanced login for email: {Email}", request.Email);
            return new EnhancedAuthResponseDto
            {
                RequiresOTP = false,
                Message = "An error occurred during login"
            };
        }
    }

    public async Task<SendOtpResponseDto> SendOtpAsync(SendOtpRequestDto request, string ipAddress)
    {
        try
        {
            Guid userId;
            string email;

            // Determine user ID and email
            if (!string.IsNullOrEmpty(request.LoginToken))
            {
                var userIdFromToken = await _tokenService.ValidateLoginTokenAsync(request.LoginToken);
                if (userIdFromToken == null)
                {
                    return new SendOtpResponseDto
                    {
                        Success = false,
                        Message = "Invalid or expired login token"
                    };
                }

                userId = userIdFromToken.Value;
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return new SendOtpResponseDto
                    {
                        Success = false,
                        Message = "User not found"
                    };
                }
                email = user.Email;
            }
            else if (!string.IsNullOrEmpty(request.Email))
            {
                var userByEmail = await _userRepository.GetByEmailAsync(request.Email);
                if (userByEmail == null)
                {
                    return new SendOtpResponseDto
                    {
                        Success = false,
                        Message = "User not found"
                    };
                }
                userId = userByEmail.UserId;
                email = userByEmail.Email;
            }
            else
            {
                return new SendOtpResponseDto
                {
                    Success = false,
                    Message = "Either login token or email is required"
                };
            }

            // Check rate limiting
            if (!await _rateLimitingService.CanSendOtpAsync(userId, ipAddress))
            {
                var cooldown = await _rateLimitingService.GetOtpSendCooldownAsync(userId);
                var message = cooldown.HasValue
                    ? $"Too many OTP requests. Please wait {cooldown.Value.Minutes} minutes before requesting again."
                    : "Too many OTP requests. Please try again later.";

                return new SendOtpResponseDto
                {
                    Success = false,
                    Message = message
                };
            }

            // Send OTP
            var response = await _otpService.SendOtpAsync(userId, request.Type);

            // Record attempt
            await _rateLimitingService.RecordAttemptAsync(userId, email, ipAddress, OtpAttemptTypes.SendOtp, response.Success);

            if (response.Success && !string.IsNullOrEmpty(request.LoginToken))
            {
                response.LoginToken = request.LoginToken;
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending OTP");
            return new SendOtpResponseDto
            {
                Success = false,
                Message = "An error occurred while sending OTP"
            };
        }
    }

    public async Task<AuthResponseDto> VerifyOtpAsync(VerifyOtpRequestDto request, string ipAddress)
    {
        try
        {
            Guid userId;

            // Determine user ID
            if (!string.IsNullOrEmpty(request.LoginToken))
            {
                var userIdFromToken = await _tokenService.ValidateLoginTokenAsync(request.LoginToken);
                if (userIdFromToken is null)
                {
                    throw new UnauthorizedAccessException("Invalid or expired login token");
                }
                userId = userIdFromToken.Value;
            }
            else if (!string.IsNullOrEmpty(request.Email))
            {
                var userByEmail = await _userRepository.GetByEmailAsync(request.Email);
                if (userByEmail is null)
                {
                    throw new UnauthorizedAccessException("User not found");
                }
                userId = userByEmail.UserId;
            }
            else
            {
                throw new UnauthorizedAccessException("Either login token or email is required");
            }

            // Check rate limiting
            if (!await _rateLimitingService.CanVerifyOtpAsync(userId, ipAddress))
            {
                await _rateLimitingService.RecordAttemptAsync(userId, null, ipAddress, OtpAttemptTypes.VerifyOtp, false);
                throw new UnauthorizedAccessException("Too many verification attempts. Please try again later.");
            }

            // Verify OTP
            var isValidOtp = await _otpService.VerifyOtpAsync(userId, request.Code, request.Type);

            // Record attempt
            await _rateLimitingService.RecordAttemptAsync(userId, null, ipAddress, OtpAttemptTypes.VerifyOtp, isValidOtp);

            if (!isValidOtp)
            {
                throw new UnauthorizedAccessException("Invalid or expired verification code");
            }

            // Update 2FA last used
            await _twoFactorService.UpdateLastUsedAsync(userId);

            // Mark OTP as used now that authentication is successful
            await _otpService.MarkOtpAsUsedAsync(userId, request.Code, request.Type);

            // Complete login
            var user = await _userRepository.GetByIdAsync(userId);
            if (user is null)
            {
                throw new UnauthorizedAccessException("User not found");
            }

            var userRoles = await _roleService.GetUserRolesAsync(user.UserId);
            var roles = userRoles.Select(r => r.Name).ToList();
            var accessToken = GenerateAccessToken(user, roles);
            var refreshToken = await GenerateRefreshTokenAsync(user.UserId);

            var accessTokenExpirationMinutes = int.Parse(_configuration["Jwt:AccessTokenExpirationMinutes"] ?? "60");

            return new AuthResponseDto
            {
                AccessToken = accessToken ?? throw new InvalidOperationException("Failed to generate access token"),
                RefreshToken = refreshToken?.Token ?? throw new InvalidOperationException("Failed to generate refresh token"),
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying OTP");
            throw;
        }
    }

    public async Task<AuthResponseDto> VerifyBackupCodeAsync(VerifyBackupCodeRequestDto request, string ipAddress)
    {
        try
        {
            Guid userId;

            // Determine user ID
            if (!string.IsNullOrEmpty(request.LoginToken))
            {
                var userIdFromToken = await _tokenService.ValidateLoginTokenAsync(request.LoginToken);
                if (userIdFromToken is null)
                {
                    throw new UnauthorizedAccessException("Invalid or expired login token");
                }
                userId = userIdFromToken.Value;
            }
            else if (!string.IsNullOrEmpty(request.Email))
            {
                var userByEmail = await _userRepository.GetByEmailAsync(request.Email);
            if (userByEmail is null)
            {
                throw new UnauthorizedAccessException("User not found");
            }
                userId = userByEmail.UserId;
            }
            else
            {
                throw new UnauthorizedAccessException("Either login token or email is required");
            }

            // Check rate limiting
            if (!await _rateLimitingService.CanVerifyOtpAsync(userId, ipAddress))
            {
                await _rateLimitingService.RecordAttemptAsync(userId, null, ipAddress, OtpAttemptTypes.VerifyOtp, false);
                throw new UnauthorizedAccessException("Too many verification attempts. Please try again later.");
            }

            // Verify backup code
            var isValidBackupCode = await _backupCodeService.VerifyAndUseBackupCodeAsync(userId, request.BackupCode);

            // Record attempt
            await _rateLimitingService.RecordAttemptAsync(userId, null, ipAddress, OtpAttemptTypes.VerifyOtp, isValidBackupCode);

            if (!isValidBackupCode)
            {
                throw new UnauthorizedAccessException("Invalid backup code");
            }

            // Update 2FA last used
            await _twoFactorService.UpdateLastUsedAsync(userId);

            // Get remaining backup codes count
            var user = await _userRepository.GetByIdAsync(userId);
            if (user != null && user.BackupCodesRemaining > 0)
            {
                // Send notification about backup code usage
                await _emailService.SendBackupCodeUsedEmailAsync(user.Email, user.BackupCodesRemaining);
            }

            // Complete login
            if (user is null)
            {
                throw new UnauthorizedAccessException("User not found");
            }

            var userRoles = await _roleService.GetUserRolesAsync(user.UserId);
            var roles = userRoles.Select(r => r.Name).ToList();
            var accessToken = GenerateAccessToken(user, roles);
            var refreshToken = await GenerateRefreshTokenAsync(user.UserId);

            var accessTokenExpirationMinutes = int.Parse(_configuration["Jwt:AccessTokenExpirationMinutes"] ?? "60");

            return new AuthResponseDto
            {
                AccessToken = accessToken ?? throw new InvalidOperationException("Failed to generate access token"),
                RefreshToken = refreshToken?.Token ?? throw new InvalidOperationException("Failed to generate refresh token"),
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying backup code");
            throw;
        }
    }


}