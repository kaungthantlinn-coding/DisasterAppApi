using DisasterApp.Application.Services.Interfaces;
using DisasterApp.Domain.Entities;
using DisasterApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace DisasterApp.Application.Services.Implementations;

public class AuditService : IAuditService
{
    private readonly DisasterDbContext _context;
    private readonly ILogger<AuditService> _logger;

    public AuditService(DisasterDbContext context, ILogger<AuditService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task LogRoleAssignmentAsync(Guid userId, string roleName, Guid? performedByUserId, string? performedByUserName, string? ipAddress, string? userAgent)
    {
        try
        {
            var auditLog = new AuditLog
            {
                AuditLogId = Guid.NewGuid(),
                Action = "ROLE_ASSIGNED",
                EntityType = "UserRole",
                EntityId = userId.ToString(),
                OldValues = null,
                NewValues = JsonSerializer.Serialize(new { RoleName = roleName }),
                UserId = performedByUserId,
                UserName = performedByUserName,
                Timestamp = DateTime.UtcNow,
                IpAddress = ipAddress,
                UserAgent = userAgent
            };

            await _context.AuditLogs.AddAsync(auditLog);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Logged role assignment: User {UserId} assigned role {RoleName} by {PerformedBy}", 
                userId, roleName, performedByUserName ?? "System");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log role assignment for user {UserId}", userId);
        }
    }

    public async Task LogRoleRemovalAsync(Guid userId, string roleName, Guid? performedByUserId, string? performedByUserName, string? ipAddress, string? userAgent)
    {
        try
        {
            var auditLog = new AuditLog
            {
                AuditLogId = Guid.NewGuid(),
                Action = "ROLE_REMOVED",
                EntityType = "UserRole",
                EntityId = userId.ToString(),
                OldValues = JsonSerializer.Serialize(new { RoleName = roleName }),
                NewValues = null,
                UserId = performedByUserId,
                UserName = performedByUserName,
                Timestamp = DateTime.UtcNow,
                IpAddress = ipAddress,
                UserAgent = userAgent
            };

            await _context.AuditLogs.AddAsync(auditLog);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Logged role removal: User {UserId} removed role {RoleName} by {PerformedBy}", 
                userId, roleName, performedByUserName ?? "System");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log role removal for user {UserId}", userId);
        }
    }

    public async Task LogRoleUpdateAsync(Guid userId, List<string> oldRoles, List<string> newRoles, Guid? performedByUserId, string? performedByUserName, string? ipAddress, string? userAgent)
    {
        try
        {
            var auditLog = new AuditLog
            {
                AuditLogId = Guid.NewGuid(),
                Action = "ROLES_UPDATED",
                EntityType = "UserRole",
                EntityId = userId.ToString(),
                OldValues = JsonSerializer.Serialize(new { Roles = oldRoles }),
                NewValues = JsonSerializer.Serialize(new { Roles = newRoles }),
                UserId = performedByUserId,
                UserName = performedByUserName,
                Timestamp = DateTime.UtcNow,
                IpAddress = ipAddress,
                UserAgent = userAgent
            };

            await _context.AuditLogs.AddAsync(auditLog);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Logged role update: User {UserId} roles changed from [{OldRoles}] to [{NewRoles}] by {PerformedBy}", 
                userId, string.Join(", ", oldRoles), string.Join(", ", newRoles), performedByUserName ?? "System");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log role update for user {UserId}", userId);
        }
    }

    public async Task<(List<AuditLog> logs, int totalCount)> GetUserAuditLogsAsync(Guid userId, int pageNumber = 1, int pageSize = 50)
    {
        try
        {
            var query = _context.AuditLogs
                .Where(a => a.EntityId == userId.ToString() && a.EntityType == "UserRole")
                .OrderByDescending(a => a.Timestamp);

            var totalCount = await query.CountAsync();
            var logs = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (logs, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get audit logs for user {UserId}", userId);
            throw;
        }
    }

    public async Task<(List<AuditLog> logs, int totalCount)> GetRoleAuditLogsAsync(int pageNumber = 1, int pageSize = 50)
    {
        try
        {
            var query = _context.AuditLogs
                .Where(a => a.EntityType == "UserRole")
                .OrderByDescending(a => a.Timestamp);

            var totalCount = await query.CountAsync();
            var logs = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (logs, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get role audit logs");
            throw;
        }
    }
}
