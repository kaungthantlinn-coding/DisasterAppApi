namespace DisasterApp.Application.Services.Interfaces;

public interface IAuditRetentionService
{
    Task<int> ApplyRetentionPoliciesAsync();
    Task<int> CleanupExpiredLogsAsync();
    Task<object> GetRetentionPolicyAsync();
    Task<bool> UpdateRetentionPolicyAsync(object policy);
    Task<int> ArchiveOldLogsAsync(int olderThanDays);
    Task<object> GetRetentionStatsAsync();
    bool ShouldRetainLog(int logAge, string severity, string category);
    Task<List<string>> GetLogsEligibleForCleanupAsync(int batchSize = 1000);
    Task<object> DryRunCleanupAsync();
    Task<byte[]> ExportLogsBeforeDeletionAsync(List<string> logIds, string format = "json");
}
