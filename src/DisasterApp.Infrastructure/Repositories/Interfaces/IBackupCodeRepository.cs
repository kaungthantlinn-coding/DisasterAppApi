using DisasterApp.Domain.Entities;

namespace DisasterApp.Infrastructure.Repositories.Interfaces;

/// <summary>
/// Repository interface for backup code operations
/// </summary>
public interface IBackupCodeRepository
{
    /// <summary>
    /// Create a new backup code
    /// </summary>
    /// <param name="backupCode">Backup code entity</param>
    /// <returns>Created backup code</returns>
    Task<BackupCode> CreateAsync(BackupCode backupCode);

    /// <summary>
    /// Create multiple backup codes
    /// </summary>
    /// <param name="backupCodes">List of backup code entities</param>
    /// <returns>Created backup codes</returns>
    Task<List<BackupCode>> CreateManyAsync(List<BackupCode> backupCodes);

    /// <summary>
    /// Get a backup code by ID
    /// </summary>
    /// <param name="id">Backup code ID</param>
    /// <returns>Backup code or null if not found</returns>
    Task<BackupCode?> GetByIdAsync(Guid id);

    /// <summary>
    /// Get unused backup codes for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>List of unused backup codes</returns>
    Task<List<BackupCode>> GetUnusedCodesAsync(Guid userId);

    /// <summary>
    /// Get a backup code by user ID and code hash
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="codeHash">Backup code hash</param>
    /// <returns>Backup code or null if not found</returns>
    Task<BackupCode?> GetByUserAndHashAsync(Guid userId, string codeHash);

    /// <summary>
    /// Update a backup code
    /// </summary>
    /// <param name="backupCode">Backup code to update</param>
    /// <returns>Updated backup code</returns>
    Task<BackupCode> UpdateAsync(BackupCode backupCode);

    /// <summary>
    /// Delete a backup code
    /// </summary>
    /// <param name="id">Backup code ID</param>
    /// <returns>True if deleted</returns>
    Task<bool> DeleteAsync(Guid id);

    /// <summary>
    /// Delete all backup codes for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Number of codes deleted</returns>
    Task<int> DeleteByUserAsync(Guid userId);

    /// <summary>
    /// Get the count of unused backup codes for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Count of unused codes</returns>
    Task<int> GetUnusedCountAsync(Guid userId);

    /// <summary>
    /// Mark a backup code as used
    /// </summary>
    /// <param name="id">Backup code ID</param>
    /// <returns>True if marked as used</returns>
    Task<bool> MarkAsUsedAsync(Guid id);
}
