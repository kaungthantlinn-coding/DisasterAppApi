namespace DisasterApp.Application.Services.Interfaces;

/// <summary>
/// Service interface for backup code operations
/// </summary>
public interface IBackupCodeService
{
    /// <summary>
    /// Generate backup codes for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="count">Number of backup codes to generate (default: 8)</param>
    /// <returns>List of backup codes (plain text)</returns>
    Task<List<string>> GenerateBackupCodesAsync(Guid userId, int count = 8);

    /// <summary>
    /// Verify a backup code and mark it as used
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="backupCode">Backup code to verify</param>
    /// <returns>True if code is valid and was marked as used</returns>
    Task<bool> VerifyAndUseBackupCodeAsync(Guid userId, string backupCode);

    /// <summary>
    /// Get the number of unused backup codes for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Number of unused backup codes</returns>
    Task<int> GetUnusedBackupCodeCountAsync(Guid userId);

    /// <summary>
    /// Invalidate all backup codes for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Number of codes invalidated</returns>
    Task<int> InvalidateAllBackupCodesAsync(Guid userId);

    /// <summary>
    /// Generate a single backup code
    /// </summary>
    /// <returns>8-character backup code</returns>
    string GenerateBackupCode();

    /// <summary>
    /// Hash a backup code for secure storage
    /// </summary>
    /// <param name="backupCode">Plain text backup code</param>
    /// <returns>Hashed backup code</returns>
    string HashBackupCode(string backupCode);

    /// <summary>
    /// Verify a backup code against its hash
    /// </summary>
    /// <param name="backupCode">Plain text backup code</param>
    /// <param name="hash">Stored hash</param>
    /// <returns>True if code matches hash</returns>
    bool VerifyBackupCode(string backupCode, string hash);
}
