using DisasterApp.Application.DTOs;
using DisasterApp.Domain.Entities;

namespace DisasterApp.Application.Services.Interfaces;
public interface IAuditService
{
    Task LogRoleAssignmentAsync(Guid userId, string roleName, Guid? performedByUserId, string? performedByUserName, string? ipAddress, string? userAgent);
    Task LogRoleRemovalAsync(Guid userId, string roleName, Guid? performedByUserId, string? performedByUserName, string? ipAddress, string? userAgent);
    Task LogRoleUpdateAsync(Guid userId, List<string> oldRoles, List<string> newRoles, Guid? performedByUserId, string? performedByUserName, string? ipAddress, string? userAgent, string? reason = null);
    Task<(List<AuditLog> logs, int totalCount)> GetUserAuditLogsAsync(Guid userId, int pageNumber = 1, int pageSize = 50);
    Task<(List<AuditLog> logs, int totalCount)> GetRoleAuditLogsAsync(int pageNumber = 1, int pageSize = 50);
    
    Task<AuditLog> CreateLogAsync(CreateAuditLogDto data);
    Task<PaginatedAuditLogsDto> GetLogsAsync(AuditLogFiltersDto filters);
    Task<AuditLogStatsDto> GetStatisticsAsync();
    Task<byte[]> ExportLogsAsync(string format, AuditLogFiltersDto filters);
    
    Task LogUserActionAsync(string action, string severity, Guid? userId, string details, string resource, string? ipAddress = null, string? userAgent = null, Dictionary<string, object>? metadata = null);
    Task LogSystemEventAsync(string action, string severity, string details, string resource, Dictionary<string, object>? metadata = null);
    Task LogSecurityEventAsync(string action, string details, Guid? userId = null, string? ipAddress = null, string? userAgent = null, Dictionary<string, object>? metadata = null);
    Task LogErrorAsync(string action, string details, Exception? exception = null, Guid? userId = null, string? resource = null);
}
