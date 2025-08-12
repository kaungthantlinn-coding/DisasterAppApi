namespace DisasterApp.Application.Services.Interfaces
{
    public interface IRateLimitingService
    {
        Task<bool> CanSendOtpAsync(Guid userId, string ipAddress);
        Task<bool> CanVerifyOtpAsync(Guid userId, string ipAddress);
        Task<bool> CanSendOtpAsync(string email, string ipAddress);
        Task RecordAttemptAsync(Guid? userId, string? email, string ipAddress, string attemptType, bool success);
        Task<bool> IsAccountLockedAsync(Guid userId);
        Task<bool> IsIpBlockedAsync(string ipAddress);
        Task<TimeSpan?> GetOtpSendCooldownAsync(Guid userId);
        Task<TimeSpan?> GetAccountLockoutTimeAsync(Guid userId);
        Task<int> CleanupOldAttemptsAsync(TimeSpan? olderThan = null);
    }
}
