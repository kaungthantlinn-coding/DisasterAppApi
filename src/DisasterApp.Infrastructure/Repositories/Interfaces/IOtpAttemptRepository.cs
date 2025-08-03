using DisasterApp.Domain.Entities;

namespace DisasterApp.Infrastructure.Repositories.Interfaces;

/// <summary>
/// Repository interface for OTP attempt operations
/// </summary>
public interface IOtpAttemptRepository
{
    /// <summary>
    /// Create a new OTP attempt record
    /// </summary>
    /// <param name="otpAttempt">OTP attempt entity</param>
    /// <returns>Created OTP attempt</returns>
    Task<OtpAttempt> CreateAsync(OtpAttempt otpAttempt);

    /// <summary>
    /// Get OTP attempts for a user within a time period
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="since">Start time</param>
    /// <param name="attemptType">Attempt type (optional)</param>
    /// <returns>List of OTP attempts</returns>
    Task<List<OtpAttempt>> GetUserAttemptsAsync(Guid userId, DateTime since, string? attemptType = null);

    /// <summary>
    /// Get OTP attempts for an IP address within a time period
    /// </summary>
    /// <param name="ipAddress">IP address</param>
    /// <param name="since">Start time</param>
    /// <param name="attemptType">Attempt type (optional)</param>
    /// <returns>List of OTP attempts</returns>
    Task<List<OtpAttempt>> GetIpAttemptsAsync(string ipAddress, DateTime since, string? attemptType = null);

    /// <summary>
    /// Get OTP attempts for an email within a time period
    /// </summary>
    /// <param name="email">Email address</param>
    /// <param name="since">Start time</param>
    /// <param name="attemptType">Attempt type (optional)</param>
    /// <returns>List of OTP attempts</returns>
    Task<List<OtpAttempt>> GetEmailAttemptsAsync(string email, DateTime since, string? attemptType = null);

    /// <summary>
    /// Get failed attempts for a user within a time period
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="since">Start time</param>
    /// <param name="attemptType">Attempt type (optional)</param>
    /// <returns>List of failed attempts</returns>
    Task<List<OtpAttempt>> GetFailedAttemptsAsync(Guid userId, DateTime since, string? attemptType = null);

    /// <summary>
    /// Get failed attempts for an IP address within a time period
    /// </summary>
    /// <param name="ipAddress">IP address</param>
    /// <param name="since">Start time</param>
    /// <param name="attemptType">Attempt type (optional)</param>
    /// <returns>List of failed attempts</returns>
    Task<List<OtpAttempt>> GetFailedIpAttemptsAsync(string ipAddress, DateTime since, string? attemptType = null);

    /// <summary>
    /// Count attempts for a user within a time period
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="since">Start time</param>
    /// <param name="attemptType">Attempt type (optional)</param>
    /// <param name="successOnly">Count only successful attempts</param>
    /// <returns>Count of attempts</returns>
    Task<int> CountUserAttemptsAsync(Guid userId, DateTime since, string? attemptType = null, bool? successOnly = null);

    /// <summary>
    /// Count attempts for an IP address within a time period
    /// </summary>
    /// <param name="ipAddress">IP address</param>
    /// <param name="since">Start time</param>
    /// <param name="attemptType">Attempt type (optional)</param>
    /// <param name="successOnly">Count only successful attempts</param>
    /// <returns>Count of attempts</returns>
    Task<int> CountIpAttemptsAsync(string ipAddress, DateTime since, string? attemptType = null, bool? successOnly = null);

    /// <summary>
    /// Count attempts for an email within a time period
    /// </summary>
    /// <param name="email">Email address</param>
    /// <param name="since">Start time</param>
    /// <param name="attemptType">Attempt type (optional)</param>
    /// <param name="successOnly">Count only successful attempts</param>
    /// <returns>Count of attempts</returns>
    Task<int> CountEmailAttemptsAsync(string email, DateTime since, string? attemptType = null, bool? successOnly = null);

    /// <summary>
    /// Delete old attempt records
    /// </summary>
    /// <param name="olderThan">Delete records older than this date</param>
    /// <returns>Number of records deleted</returns>
    Task<int> DeleteOldAttemptsAsync(DateTime olderThan);
}
