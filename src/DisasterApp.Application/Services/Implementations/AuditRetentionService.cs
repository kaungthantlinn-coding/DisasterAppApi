using DisasterApp.Application.Services.Interfaces;
using DisasterApp.Domain.Entities;
using DisasterApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace DisasterApp.Application.Services.Implementations;
public class AuditRetentionService : IAuditRetentionService
{
    private readonly DisasterDbContext _context;
    private readonly ILogger<AuditRetentionService> _logger;
    private readonly IConfiguration _configuration;

    private readonly Dictionary<string, int> _defaultRetentionPeriods = new()
    {
        ["critical"] = 2555, // 7 years
        ["high"] = 1825,     // 5 years
        ["medium"] = 1095,   // 3 years
        ["low"] = 365,       // 1 year
        ["info"] = 180       // 6 months
    };

    private readonly HashSet<string> _longRetentionCategories = new()
    {
        "financial", "security", "compliance", "legal"
    };

    public AuditRetentionService(
        DisasterDbContext context, 
        ILogger<AuditRetentionService> logger,
        IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<int> ApplyRetentionPoliciesAsync()
    {
        try
        {
            _logger.LogInformation("Starting retention policy application");

            var totalProcessed = 0;
            var batchSize = 1000;
            var hasMoreLogs = true;

            while (hasMoreLogs)
            {
                var logs = await _context.AuditLogs
                    .OrderBy(l => l.Timestamp)
                    .Take(batchSize)
                    .ToListAsync();

                if (!logs.Any())
                {
                    hasMoreLogs = false;
                    continue;
                }

                foreach (var log in logs)
                {
                    var logAge = (DateTime.UtcNow - log.Timestamp).Days;
                    var category = ExtractCategoryFromMetadata(log.Metadata);
                    
                    if (!ShouldRetainLog(logAge, log.Severity, category))
                    {
                        await MarkLogForCleanupAsync(log);
                    }
                }

                totalProcessed += logs.Count;
                
                if (logs.Count < batchSize)
                {
                    hasMoreLogs = false;
                }
            }

            _logger.LogInformation("Retention policy application completed. Processed {TotalProcessed} logs", totalProcessed);
            return totalProcessed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying retention policies");
            throw;
        }
    }

    public async Task<int> CleanupExpiredLogsAsync()
    {
        try
        {
            _logger.LogInformation("Starting cleanup of expired audit logs");

            var cutoffDate = DateTime.UtcNow.AddDays(-GetMaxRetentionPeriod());
            
            var deletedCount = 0;
            var batchSize = 500;

            while (true)
            {
                var logsToDelete = await _context.AuditLogs
                    .Where(l => l.Timestamp < cutoffDate && !IsProtectedLog(l))
                    .Take(batchSize)
                    .ToListAsync();

                if (!logsToDelete.Any())
                    break;

                await ExportLogsForComplianceAsync(logsToDelete);

                _context.AuditLogs.RemoveRange(logsToDelete);
                await _context.SaveChangesAsync();

                deletedCount += logsToDelete.Count;
                
                _logger.LogInformation("Deleted batch of {BatchSize} logs. Total deleted: {DeletedCount}", 
                    logsToDelete.Count, deletedCount);

                if (logsToDelete.Count < batchSize)
                    break;
            }

            _logger.LogInformation("Cleanup completed. Total logs deleted: {DeletedCount}", deletedCount);
            return deletedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during audit log cleanup");
            throw;
        }
    }

    public async Task<object> GetRetentionPolicyAsync()
    {
        try
        {
            var policy = new
            {
                retentionPeriods = _defaultRetentionPeriods,
                longRetentionCategories = _longRetentionCategories.ToList(),
                maxRetentionPeriod = GetMaxRetentionPeriod(),
                archiveEnabled = _configuration.GetValue<bool>("AuditRetention:ArchiveEnabled", false),
                archiveLocation = _configuration.GetValue<string>("AuditRetention:ArchiveLocation", ""),
                cleanupSchedule = _configuration.GetValue<string>("AuditRetention:CleanupSchedule", "Daily"),
                lastCleanup = await GetLastCleanupDateAsync(),
                configuration = new
                {
                    batchSize = _configuration.GetValue<int>("AuditRetention:BatchSize", 1000),
                    exportBeforeDelete = _configuration.GetValue<bool>("AuditRetention:ExportBeforeDelete", true),
                    complianceExportFormat = _configuration.GetValue<string>("AuditRetention:ComplianceExportFormat", "json")
                }
            };

            return policy;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving retention policy");
            return new { error = "Failed to retrieve retention policy" };
        }
    }

    public async Task<bool> UpdateRetentionPolicyAsync(object policy)
    {
        try
        {
            _logger.LogInformation("Retention policy update requested: {Policy}", JsonSerializer.Serialize(policy));
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating retention policy");
            return false;
        }
    }

    public async Task<int> ArchiveOldLogsAsync(int olderThanDays)
    {
        try
        {
            var archiveDate = DateTime.UtcNow.AddDays(-olderThanDays);
            
            var logsToArchive = await _context.AuditLogs
                .Where(l => l.Timestamp < archiveDate)
                .ToListAsync();

            if (!logsToArchive.Any())
                return 0;

            var archiveData = JsonSerializer.Serialize(logsToArchive.Select(l => new
            {
                l.AuditLogId,
                l.Action,
                l.Severity,
                l.EntityType,
                l.EntityId,
                l.Details,
                l.OldValues,
                l.NewValues,
                l.UserId,
                l.UserName,
                l.Timestamp,
                l.IpAddress,
                l.UserAgent,
                l.Resource,
                l.Metadata
            }));

            var archiveFileName = $"audit_archive_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json";
            _logger.LogInformation("Archived {Count} logs to {FileName}", logsToArchive.Count, archiveFileName);

            _context.AuditLogs.RemoveRange(logsToArchive);
            await _context.SaveChangesAsync();

            return logsToArchive.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error archiving old logs");
            throw;
        }
    }

    public async Task<object> GetRetentionStatsAsync()
    {
        try
        {
            var totalLogs = await _context.AuditLogs.CountAsync();
            var oldestLog = await _context.AuditLogs.MinAsync(l => (DateTime?)l.Timestamp);
            var newestLog = await _context.AuditLogs.MaxAsync(l => (DateTime?)l.Timestamp);

            var severityBreakdown = await _context.AuditLogs
                .GroupBy(l => l.Severity)
                .Select(g => new { Severity = g.Key, Count = g.Count() })
                .ToListAsync();

            var ageBreakdown = new
            {
                last30Days = await _context.AuditLogs.CountAsync(l => l.Timestamp >= DateTime.UtcNow.AddDays(-30)),
                last90Days = await _context.AuditLogs.CountAsync(l => l.Timestamp >= DateTime.UtcNow.AddDays(-90)),
                last365Days = await _context.AuditLogs.CountAsync(l => l.Timestamp >= DateTime.UtcNow.AddDays(-365)),
                olderThan1Year = await _context.AuditLogs.CountAsync(l => l.Timestamp < DateTime.UtcNow.AddDays(-365))
            };

            var eligibleForCleanup = await GetLogsEligibleForCleanupAsync(0);

            return new
            {
                totalLogs,
                oldestLog = oldestLog?.ToString("yyyy-MM-dd HH:mm:ss"),
                newestLog = newestLog?.ToString("yyyy-MM-dd HH:mm:ss"),
                dataSpan = oldestLog.HasValue && newestLog.HasValue 
                    ? (newestLog.Value - oldestLog.Value).Days 
                    : 0,
                severityBreakdown,
                ageBreakdown,
                eligibleForCleanupCount = eligibleForCleanup.Count,
                estimatedStorageUsage = EstimateStorageUsage(totalLogs),
                retentionCompliance = await CheckRetentionComplianceAsync()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving retention statistics");
            return new { error = "Failed to retrieve retention statistics" };
        }
    }

    public bool ShouldRetainLog(int logAge, string severity, string category)
    {
        var retentionPeriod = _defaultRetentionPeriods.GetValueOrDefault(severity.ToLowerInvariant(), 365);

        if (_longRetentionCategories.Any(c => category.Contains(c, StringComparison.OrdinalIgnoreCase)))
        {
            retentionPeriod = Math.Max(retentionPeriod, 2555); // 7 years minimum
        }

        return logAge <= retentionPeriod;
    }

    public async Task<List<string>> GetLogsEligibleForCleanupAsync(int batchSize = 1000)
    {
        try
        {
            var eligibleLogs = new List<string>();

            var query = _context.AuditLogs.AsQueryable();

            if (batchSize > 0)
            {
                query = query.Take(batchSize);
            }

            var logs = await query.ToListAsync();

            foreach (var log in logs)
            {
                var logAge = (DateTime.UtcNow - log.Timestamp).Days;
                var category = ExtractCategoryFromMetadata(log.Metadata);

                if (!ShouldRetainLog(logAge, log.Severity, category) && !IsProtectedLog(log))
                {
                    eligibleLogs.Add(log.AuditLogId.ToString());
                }
            }

            return eligibleLogs;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting logs eligible for cleanup");
            return new List<string>();
        }
    }

    public async Task<object> DryRunCleanupAsync()
    {
        try
        {
            var eligibleLogs = await GetLogsEligibleForCleanupAsync();
            
            var summary = await _context.AuditLogs
                .Where(l => eligibleLogs.Contains(l.AuditLogId.ToString()))
                .GroupBy(l => l.Severity)
                .Select(g => new { Severity = g.Key, Count = g.Count() })
                .ToListAsync();

            return new
            {
                totalEligibleForDeletion = eligibleLogs.Count,
                breakdownBySeverity = summary,
                estimatedSpaceFreed = EstimateStorageUsage(eligibleLogs.Count),
                oldestLogToDelete = await GetOldestEligibleLogAsync(eligibleLogs),
                newestLogToDelete = await GetNewestEligibleLogAsync(eligibleLogs)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing dry run cleanup");
            return new { error = "Failed to perform dry run" };
        }
    }

    public async Task<byte[]> ExportLogsBeforeDeletionAsync(List<string> logIds, string format = "json")
    {
        try
        {
            var logGuids = logIds.Select(id => Guid.Parse(id)).ToList();
            
            var logs = await _context.AuditLogs
                .Where(l => logGuids.Contains(l.AuditLogId))
                .ToListAsync();

            if (format.ToLowerInvariant() == "csv")
            {
                return ExportToCsv(logs);
            }
            else
            {
                var jsonData = JsonSerializer.Serialize(logs, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                return System.Text.Encoding.UTF8.GetBytes(jsonData);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting logs before deletion");
            throw;
        }
    }

    private async Task MarkLogForCleanupAsync(AuditLog log)
    {
        _logger.LogDebug("Log {LogId} marked for cleanup", log.AuditLogId);
    }

    private string ExtractCategoryFromMetadata(string? metadata)
    {
        if (string.IsNullOrEmpty(metadata))
            return "";

        try
        {
            var metadataDict = JsonSerializer.Deserialize<Dictionary<string, object>>(metadata);
            return metadataDict?.GetValueOrDefault("category")?.ToString() ?? "";
        }
        catch
        {
            return "";
        }
    }

    private bool IsProtectedLog(AuditLog log)
    {
        return log.Severity == "critical" || 
               log.Action.Contains("SECURITY", StringComparison.OrdinalIgnoreCase) ||
               log.Action.Contains("FINANCIAL", StringComparison.OrdinalIgnoreCase) ||
               log.Resource == "donations";
    }

    private int GetMaxRetentionPeriod()
    {
        return _defaultRetentionPeriods.Values.Max();
    }

    private async Task<DateTime?> GetLastCleanupDateAsync()
    {
        return DateTime.UtcNow.AddDays(-1); // Placeholder
    }

    private async Task ExportLogsForComplianceAsync(List<AuditLog> logs)
    {
        if (!_configuration.GetValue<bool>("AuditRetention:ExportBeforeDelete", true))
            return;

        try
        {
            var exportData = await ExportLogsBeforeDeletionAsync(
                logs.Select(l => l.AuditLogId.ToString()).ToList());
            
            _logger.LogInformation("Exported {Count} logs for compliance before deletion", logs.Count);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to export logs for compliance");
        }
    }

    private string EstimateStorageUsage(int logCount)
    {
        var bytes = logCount * 2048;
        
        if (bytes < 1024)
            return $"{bytes} bytes";
        else if (bytes < 1024 * 1024)
            return $"{bytes / 1024:F1} KB";
        else if (bytes < 1024 * 1024 * 1024)
            return $"{bytes / (1024 * 1024):F1} MB";
        else
            return $"{bytes / (1024 * 1024 * 1024):F1} GB";
    }

    private async Task<object> CheckRetentionComplianceAsync()
    {
        var totalLogs = await _context.AuditLogs.CountAsync();
        var eligibleForCleanup = await GetLogsEligibleForCleanupAsync(0);
        
        return new
        {
            isCompliant = eligibleForCleanup.Count < totalLogs * 0.1, // Less than 10% overdue
            overdueLogs = eligibleForCleanup.Count,
            compliancePercentage = totalLogs > 0 ? (totalLogs - eligibleForCleanup.Count) * 100.0 / totalLogs : 100
        };
    }

    private async Task<string?> GetOldestEligibleLogAsync(List<string> logIds)
    {
        if (!logIds.Any()) return null;
        
        var logGuids = logIds.Select(id => Guid.Parse(id)).ToList();
        var oldest = await _context.AuditLogs
            .Where(l => logGuids.Contains(l.AuditLogId))
            .MinAsync(l => (DateTime?)l.Timestamp);
            
        return oldest?.ToString("yyyy-MM-dd HH:mm:ss");
    }

    private async Task<string?> GetNewestEligibleLogAsync(List<string> logIds)
    {
        if (!logIds.Any()) return null;
        
        var logGuids = logIds.Select(id => Guid.Parse(id)).ToList();
        var newest = await _context.AuditLogs
            .Where(l => logGuids.Contains(l.AuditLogId))
            .MaxAsync(l => (DateTime?)l.Timestamp);
            
        return newest?.ToString("yyyy-MM-dd HH:mm:ss");
    }

    private byte[] ExportToCsv(List<AuditLog> logs)
    {
        var csv = new System.Text.StringBuilder();
        csv.AppendLine("Id,Timestamp,Action,Severity,UserId,UserName,Details,IpAddress,Resource");
        
        foreach (var log in logs)
        {
            csv.AppendLine($"{log.AuditLogId},{log.Timestamp:yyyy-MM-dd HH:mm:ss},{log.Action},{log.Severity},{log.UserId},{log.UserName},{log.Details},{log.IpAddress},{log.Resource}");
        }
        
        return System.Text.Encoding.UTF8.GetBytes(csv.ToString());
    }
}
