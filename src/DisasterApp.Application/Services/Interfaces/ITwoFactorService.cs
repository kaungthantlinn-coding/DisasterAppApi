using DisasterApp.Application.DTOs;

namespace DisasterApp.Application.Services.Interfaces;

/// <summary>
/// Service interface for two-factor authentication operations
/// </summary>
public interface ITwoFactorService
{
    /// <summary>
    /// Get the current 2FA status for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>2FA status information</returns>
    Task<TwoFactorStatusDto> GetTwoFactorStatusAsync(Guid userId);

    /// <summary>
    /// Initialize 2FA setup for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="currentPassword">User's current password for verification</param>
    /// <returns>Setup response with instructions</returns>
    Task<SetupTwoFactorResponseDto> SetupTwoFactorAsync(Guid userId, string currentPassword);

    /// <summary>
    /// Complete 2FA setup by verifying the OTP code
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="otpCode">OTP code for verification</param>
    /// <returns>Setup completion response with backup codes</returns>
    Task<VerifySetupResponseDto> VerifySetupAsync(Guid userId, string otpCode);

    /// <summary>
    /// Disable 2FA for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="currentPassword">User's current password for verification</param>
    /// <param name="otpCode">Optional OTP code for additional verification</param>
    /// <returns>Whether 2FA was disabled successfully</returns>
    Task<bool> DisableTwoFactorAsync(Guid userId, string currentPassword, string? otpCode = null);

    /// <summary>
    /// Generate new backup codes for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="currentPassword">User's current password for verification</param>
    /// <param name="otpCode">Optional OTP code for additional verification</param>
    /// <returns>New backup codes</returns>
    Task<GenerateBackupCodesResponseDto> GenerateBackupCodesAsync(Guid userId, string currentPassword, string? otpCode = null);

    /// <summary>
    /// Check if a user has 2FA enabled
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>True if 2FA is enabled</returns>
    Task<bool> IsTwoFactorEnabledAsync(Guid userId);

    /// <summary>
    /// Update the last used timestamp for 2FA
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Task</returns>
    Task UpdateLastUsedAsync(Guid userId);
}
