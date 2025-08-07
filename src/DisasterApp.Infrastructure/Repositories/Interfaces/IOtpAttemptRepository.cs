using DisasterApp.Domain.Entities;

namespace DisasterApp.Infrastructure.Repositories.Interfaces;

public interface IOtpAttemptRepository
{
    Task<OtpAttempt> CreateAsync(OtpAttempt otpAttempt);
    Task<List<OtpAttempt>> GetUserAttemptsAsync(Guid userId, DateTime since, string? attemptType = null);
    Task<List<OtpAttempt>> GetIpAttemptsAsync(string ipAddress, DateTime since, string? attemptType = null);
    Task<List<OtpAttempt>> GetEmailAttemptsAsync(string email, DateTime since, string? attemptType = null);
    Task<List<OtpAttempt>> GetFailedAttemptsAsync(Guid userId, DateTime since, string? attemptType = null);
    Task<List<OtpAttempt>> GetFailedIpAttemptsAsync(string ipAddress, DateTime since, string? attemptType = null);
    Task<int> CountUserAttemptsAsync(Guid userId, DateTime since, string? attemptType = null, bool? successOnly = null);
    Task<int> CountIpAttemptsAsync(string ipAddress, DateTime since, string? attemptType = null, bool? successOnly = null);
    Task<int> CountEmailAttemptsAsync(string email, DateTime since, string? attemptType = null, bool? successOnly = null);
    Task<int> DeleteOldAttemptsAsync(DateTime olderThan);
}
