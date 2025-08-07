namespace DisasterApp.Application.Services.Interfaces;

/// <summary>
/// Service interface for rate limiting operations
/// </summary>
public interface IRateLimitingService
{
    /// <summary>
    /// Check if a user can send an OTP (rate limiting)
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="ipAddress">IP address</param>
    /// <returns>True if allowed, false if rate limited</returns>
    Task<bool> CanSendOtpAsync(Guid userId, string ipAddress);

    /// <summary>
    /// Check if a user can verify an OTP (rate limiting)
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="ipAddress">IP address</param>
    /// <returns>True if allowed, false if rate limited</returns>
    Task<bool> CanVerifyOtpAsync(Guid userId, string ipAddress);

    /// <summary>
    /// Check if an email can be used for OTP operations (rate limiting)
    /// </summary>
    /// <param name="email">Email address</param>
    /// <param name="ipAddress">IP address</param>
    /// <returns>True if allowed, false if rate limited</returns>
    Task<bool> CanSendOtpAsync(string email, string ipAddress);

    /// <summary>
    /// Record an OTP attempt
    /// </summary>
    /// <param name="userId">User ID (can be null for failed login attempts)</param>
    /// <param name="email">Email address (can be null if user ID is known)</param>
    /// <param name="ipAddress">IP address</param>
    /// <param name="attemptType">Type of attempt</param>
    /// <param name="success">Whether the attempt was successful</param>
    /// <returns>Task</returns>
    Task RecordAttemptAsync(Guid? userId, string? email, string ipAddress, string attemptType, bool success);

    /// <summary>
    /// Check if a user account is locked due to too many failed attempts
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>True if account is locked</returns>
    Task<bool> IsAccountLockedAsync(Guid userId);

    /// <summary>
    /// Check if an IP address is blocked due to too many failed attempts
    /// </summary>
    /// <param name="ipAddress">IP address</param>
    /// <returns>True if IP is blocked</returns>
    Task<bool> IsIpBlockedAsync(string ipAddress);

    /// <summary>
    /// Get the remaining time until a user can send another OTP
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Time remaining, or null if no restriction</returns>
    Task<TimeSpan?> GetOtpSendCooldownAsync(Guid userId);

    /// <summary>
    /// Get the remaining time until a user account is unlocked
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Time remaining, or null if not locked</returns>
    Task<TimeSpan?> GetAccountLockoutTimeAsync(Guid userId);

    /// <summary>
    /// Clean up old attempt records
    /// </summary>
    /// <param name="olderThan">Remove records older than this timespan (default: 24 hours)</param>
    /// <returns>Number of records cleaned up</returns>
    Task<int> CleanupOldAttemptsAsync(TimeSpan? olderThan = null);
}
