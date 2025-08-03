using DisasterApp.Domain.Entities;

namespace DisasterApp.Infrastructure.Repositories.Interfaces;

/// <summary>
/// Repository interface for OTP code operations
/// </summary>
public interface IOtpCodeRepository
{
    /// <summary>
    /// Create a new OTP code
    /// </summary>
    /// <param name="otpCode">OTP code entity</param>
    /// <returns>Created OTP code</returns>
    Task<OtpCode> CreateAsync(OtpCode otpCode);

    /// <summary>
    /// Get an OTP code by ID
    /// </summary>
    /// <param name="id">OTP code ID</param>
    /// <returns>OTP code or null if not found</returns>
    Task<OtpCode?> GetByIdAsync(Guid id);

    /// <summary>
    /// Get an OTP code by user ID and code
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="code">OTP code</param>
    /// <param name="type">OTP type</param>
    /// <returns>OTP code or null if not found</returns>
    Task<OtpCode?> GetByUserAndCodeAsync(Guid userId, string code, string type);

    /// <summary>
    /// Get active OTP codes for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="type">OTP type (optional)</param>
    /// <returns>List of active OTP codes</returns>
    Task<List<OtpCode>> GetActiveCodesAsync(Guid userId, string? type = null);

    /// <summary>
    /// Update an OTP code
    /// </summary>
    /// <param name="otpCode">OTP code to update</param>
    /// <returns>Updated OTP code</returns>
    Task<OtpCode> UpdateAsync(OtpCode otpCode);

    /// <summary>
    /// Delete an OTP code
    /// </summary>
    /// <param name="id">OTP code ID</param>
    /// <returns>True if deleted</returns>
    Task<bool> DeleteAsync(Guid id);

    /// <summary>
    /// Delete expired OTP codes
    /// </summary>
    /// <returns>Number of codes deleted</returns>
    Task<int> DeleteExpiredAsync();

    /// <summary>
    /// Delete all OTP codes for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="type">OTP type (optional, null for all types)</param>
    /// <returns>Number of codes deleted</returns>
    Task<int> DeleteByUserAsync(Guid userId, string? type = null);

    /// <summary>
    /// Get the count of active OTP codes for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="type">OTP type (optional)</param>
    /// <returns>Count of active codes</returns>
    Task<int> GetActiveCountAsync(Guid userId, string? type = null);
}
