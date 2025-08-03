using DisasterApp.Application.DTOs;
using DisasterApp.Application.Services.Interfaces;
using DisasterApp.Domain.Entities;
using DisasterApp.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace DisasterApp.Application.Services.Implementations;

/// <summary>
/// Service implementation for two-factor authentication operations
/// </summary>
public class TwoFactorService : ITwoFactorService
{
    private readonly IUserRepository _userRepository;
    private readonly IOtpService _otpService;
    private readonly IBackupCodeService _backupCodeService;
    private readonly IEmailService _emailService;
    private readonly ILogger<TwoFactorService> _logger;

    public TwoFactorService(
        IUserRepository userRepository,
        IOtpService otpService,
        IBackupCodeService backupCodeService,
        IEmailService emailService,
        ILogger<TwoFactorService> logger)
    {
        _userRepository = userRepository;
        _otpService = otpService;
        _backupCodeService = backupCodeService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<TwoFactorStatusDto> GetTwoFactorStatusAsync(Guid userId)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }

            return new TwoFactorStatusDto
            {
                TwoFactorEnabled = user.TwoFactorEnabled,
                BackupCodesRemaining = user.BackupCodesRemaining,
                LastUsed = user.TwoFactorLastUsed,
                EnabledAt = user.TwoFactorEnabled ? user.CreatedAt : null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting 2FA status for user {UserId}", userId);
            throw;
        }
    }

    public async Task<SetupTwoFactorResponseDto> SetupTwoFactorAsync(Guid userId, string currentPassword)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return new SetupTwoFactorResponseDto
                {
                    Success = false,
                    Message = "User not found"
                };
            }

            // Verify current password (only for email auth users)
            if (user.AuthProvider == "Email")
            {
                if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.AuthId))
                {
                    return new SetupTwoFactorResponseDto
                    {
                        Success = false,
                        Message = "Invalid current password"
                    };
                }
            }

            // Check if 2FA is already enabled
            if (user.TwoFactorEnabled)
            {
                return new SetupTwoFactorResponseDto
                {
                    Success = false,
                    Message = "Two-factor authentication is already enabled"
                };
            }

            // Send setup OTP
            var otpResponse = await _otpService.SendOtpAsync(userId, user.Email, OtpCodeTypes.Setup);
            if (!otpResponse.Success)
            {
                return new SetupTwoFactorResponseDto
                {
                    Success = false,
                    Message = "Failed to send setup verification code"
                };
            }

            return new SetupTwoFactorResponseDto
            {
                Success = true,
                Message = "Setup verification code sent to your email",
                Instructions = "Please check your email for a 6-digit verification code and enter it to complete 2FA setup. The code will expire in 5 minutes."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting up 2FA for user {UserId}", userId);
            return new SetupTwoFactorResponseDto
            {
                Success = false,
                Message = "An error occurred during 2FA setup"
            };
        }
    }

    public async Task<VerifySetupResponseDto> VerifySetupAsync(Guid userId, string otpCode)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return new VerifySetupResponseDto
                {
                    Success = false,
                    Message = "User not found"
                };
            }

            // Verify the setup OTP
            var isValidOtp = await _otpService.VerifyOtpAsync(userId, otpCode, OtpCodeTypes.Setup);
            if (!isValidOtp)
            {
                return new VerifySetupResponseDto
                {
                    Success = false,
                    Message = "Invalid or expired verification code"
                };
            }

            // Enable 2FA for the user
            user.TwoFactorEnabled = true;
            await _userRepository.UpdateAsync(user);

            // Generate backup codes
            var backupCodes = await _backupCodeService.GenerateBackupCodesAsync(userId);

            // Send confirmation email
            await _emailService.SendTwoFactorEnabledEmailAsync(user.Email);

            _logger.LogInformation("2FA enabled successfully for user {UserId}", userId);

            return new VerifySetupResponseDto
            {
                Success = true,
                Message = "Two-factor authentication has been enabled successfully",
                BackupCodes = backupCodes
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying 2FA setup for user {UserId}", userId);
            return new VerifySetupResponseDto
            {
                Success = false,
                Message = "An error occurred during 2FA setup verification"
            };
        }
    }

    public async Task<bool> DisableTwoFactorAsync(Guid userId, string currentPassword, string? otpCode = null)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Attempt to disable 2FA for non-existent user {UserId}", userId);
                return false;
            }

            // Verify current password (only for email auth users)
            if (user.AuthProvider == "Email")
            {
                if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.AuthId))
                {
                    _logger.LogWarning("Invalid password attempt when disabling 2FA for user {UserId}", userId);
                    return false;
                }
            }

            // If 2FA is enabled and OTP code is provided, verify it
            if (user.TwoFactorEnabled && !string.IsNullOrEmpty(otpCode))
            {
                var isValidOtp = await _otpService.VerifyOtpAsync(userId, otpCode, OtpCodeTypes.Disable);
                if (!isValidOtp)
                {
                    _logger.LogWarning("Invalid OTP when disabling 2FA for user {UserId}", userId);
                    return false;
                }
            }

            // Disable 2FA
            user.TwoFactorEnabled = false;
            user.BackupCodesRemaining = 0;
            user.TwoFactorLastUsed = null;
            await _userRepository.UpdateAsync(user);

            // Remove all backup codes
            await _backupCodeService.InvalidateAllBackupCodesAsync(userId);

            // Remove all OTP codes
            await _otpService.InvalidateCodesAsync(userId);

            // Send notification email
            await _emailService.SendTwoFactorDisabledEmailAsync(user.Email);

            _logger.LogInformation("2FA disabled successfully for user {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disabling 2FA for user {UserId}", userId);
            return false;
        }
    }

    public async Task<GenerateBackupCodesResponseDto> GenerateBackupCodesAsync(Guid userId, string currentPassword, string? otpCode = null)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return new GenerateBackupCodesResponseDto
                {
                    Success = false,
                    Message = "User not found"
                };
            }

            // Check if 2FA is enabled
            if (!user.TwoFactorEnabled)
            {
                return new GenerateBackupCodesResponseDto
                {
                    Success = false,
                    Message = "Two-factor authentication is not enabled"
                };
            }

            // Verify current password (only for email auth users)
            if (user.AuthProvider == "Email")
            {
                if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.AuthId))
                {
                    return new GenerateBackupCodesResponseDto
                    {
                        Success = false,
                        Message = "Invalid current password"
                    };
                }
            }

            // If OTP code is provided, verify it
            if (!string.IsNullOrEmpty(otpCode))
            {
                var isValidOtp = await _otpService.VerifyOtpAsync(userId, otpCode, OtpCodeTypes.BackupGenerate);
                if (!isValidOtp)
                {
                    return new GenerateBackupCodesResponseDto
                    {
                        Success = false,
                        Message = "Invalid or expired verification code"
                    };
                }
            }

            // Generate new backup codes
            var backupCodes = await _backupCodeService.GenerateBackupCodesAsync(userId);

            _logger.LogInformation("New backup codes generated for user {UserId}", userId);

            return new GenerateBackupCodesResponseDto
            {
                Success = true,
                Message = "New backup codes generated successfully",
                BackupCodes = backupCodes,
                CodesGenerated = backupCodes.Count
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating backup codes for user {UserId}", userId);
            return new GenerateBackupCodesResponseDto
            {
                Success = false,
                Message = "An error occurred while generating backup codes"
            };
        }
    }

    public async Task<bool> IsTwoFactorEnabledAsync(Guid userId)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            return user?.TwoFactorEnabled ?? false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking 2FA status for user {UserId}", userId);
            return false;
        }
    }

    public async Task UpdateLastUsedAsync(Guid userId)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user != null)
            {
                user.TwoFactorLastUsed = DateTime.UtcNow;
                await _userRepository.UpdateAsync(user);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating 2FA last used for user {UserId}", userId);
        }
    }
}
