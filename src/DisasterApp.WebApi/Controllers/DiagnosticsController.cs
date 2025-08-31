using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DisasterApp.Infrastructure.Data;
using DisasterApp.WebApi.Authorization;
//
namespace DisasterApp.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DiagnosticsController : ControllerBase
{
    private readonly DisasterDbContext _context;
    private readonly ILogger<DiagnosticsController> _logger;

    public DiagnosticsController(DisasterDbContext context, ILogger<DiagnosticsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet("audit-logs/summary")]
    [AdminOnly]
    public async Task<IActionResult> GetAuditLogsSummary()
    {
        try
        {
            // Get total count
            var totalLogs = await _context.AuditLogs.CountAsync();

            // Get distinct actions
            var distinctActions = await _context.AuditLogs
                .Select(a => a.Action)
                .Distinct()
                .OrderBy(a => a)
                .ToListAsync();

            // Get distinct entity types
            var distinctEntityTypes = await _context.AuditLogs
                .Select(a => a.EntityType)
                .Distinct()
                .OrderBy(e => e)
                .ToListAsync();

            // Get action counts
            var actionCounts = await _context.AuditLogs
                .GroupBy(a => a.Action)
                .Select(g => new { Action = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            // Get entity type counts
            var entityTypeCounts = await _context.AuditLogs
                .GroupBy(a => a.EntityType)
                .Select(g => new { EntityType = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            // Get recent sample logs
            var sampleLogs = await _context.AuditLogs
                .OrderByDescending(a => a.Timestamp)
                .Take(10)
                .Select(a => new
                {
                    a.AuditLogId,
                    a.Timestamp,
                    a.Action,
                    a.EntityType,
                    a.Severity,
                    a.Resource,
                    a.Details
                })
                .ToListAsync();

            // Check for USER_SUSPENDED logs specifically
            var userSuspendedLogs = await _context.AuditLogs
                .Where(a => a.Action.Contains("USER_SUSPENDED") || a.Action.Contains("USER_DEACTIVATED"))
                .Select(a => new
                {
                    a.AuditLogId,
                    a.Timestamp,
                    a.Action,
                    a.EntityType,
                    a.Severity,
                    a.Resource
                })
                .ToListAsync();

            var summary = new
            {
                TotalLogs = totalLogs,
                DistinctActions = distinctActions,
                DistinctEntityTypes = distinctEntityTypes,
                ActionCounts = actionCounts,
                EntityTypeCounts = entityTypeCounts,
                SampleRecentLogs = sampleLogs,
                UserSuspendedLogs = userSuspendedLogs,
                DatabaseConnectionString = _context.Database.GetConnectionString()
            };

            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get audit logs summary");
            return StatusCode(500, new { message = "Failed to get audit logs summary", error = ex.Message });
        }
    }

    [HttpGet("audit-logs/test-filters")]
    [AdminOnly]
    public async Task<IActionResult> TestFilters([FromQuery] string? action = null, [FromQuery] string? targetType = null)
    {
        try
        {
            var query = _context.AuditLogs.AsQueryable();
            var totalCount = await query.CountAsync();
            _logger.LogInformation("Starting filter test with {TotalLogs} total logs", totalCount);

            if (!string.IsNullOrEmpty(action))
            {
                var actions = action.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(a => a.Trim().ToUpperInvariant()).ToList();
                _logger.LogInformation("Filtering by actions: {Actions}", string.Join(", ", actions));
                query = query.Where(a => actions.Contains(a.Action.ToUpper()));
                var countAfterAction = await query.CountAsync();
                _logger.LogInformation("Records after action filter: {Count}", countAfterAction);
            }

            if (!string.IsNullOrEmpty(targetType))
            {
                var targetTypes = targetType.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim()).ToList();
                _logger.LogInformation("Filtering by target types: {TargetTypes}", string.Join(", ", targetTypes));
                query = query.Where(a => targetTypes.Any(tt => a.EntityType.ToLower().Contains(tt.ToLower())));
                var countAfterTargetType = await query.CountAsync();
                _logger.LogInformation("Records after target type filter: {Count}", countAfterTargetType);
            }

            var results = await query
                .Select(a => new
                {
                    a.AuditLogId,
                    a.Timestamp,
                    a.Action,
                    a.EntityType,
                    a.Severity,
                    a.Resource,
                    a.Details
                })
                .OrderByDescending(a => a.Timestamp)
                .Take(50)
                .ToListAsync();

            return Ok(new
            {
                TotalLogsInDatabase = totalCount,
                FilteredCount = results.Count,
                Filters = new { Action = action, TargetType = targetType },
                Results = results
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to test filters");
            return StatusCode(500, new { message = "Failed to test filters", error = ex.Message });
        }
    }

    [HttpGet("audit-logs/debug-export-filters")]
    [AdminOnly]
    public async Task<IActionResult> DebugExportFilters()
    {
        try
        {
            // Test the exact same filters from the log message
            var filters = new
            {
                Page = 1,
                PageSize = 10000,
                Search = (string?)null,
                Severity = (string?)null,
                Action = "", // Empty to see all actions first
                TargetType = "Security,System",
                DateFrom = (DateTime?)null,
                DateTo = (DateTime?)null,
                UserId = (string?)null,
                Resource = (string?)null
            };

            var query = _context.AuditLogs.AsQueryable();
            var totalCount = await query.CountAsync();
            
            _logger.LogInformation("Debug: Total logs in database: {Count}", totalCount);

            // Test Action filter
            var actionQuery = _context.AuditLogs.AsQueryable();
            if (!string.IsNullOrEmpty(filters.Action))
            {
                var actions = filters.Action.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(a => a.Trim().ToUpperInvariant()).ToList();
                actionQuery = actionQuery.Where(a => actions.Contains(a.Action.ToUpper()));
            }
            var actionCount = await actionQuery.CountAsync();
            _logger.LogInformation("Debug: Logs matching action filter: {Count}", actionCount);

            // Test TargetType filter
            var targetTypeQuery = _context.AuditLogs.AsQueryable();
            if (!string.IsNullOrEmpty(filters.TargetType))
            {
                var targetTypes = filters.TargetType.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim()).ToList();
                targetTypeQuery = targetTypeQuery.Where(a => targetTypes.Any(tt => a.EntityType.ToLower().Contains(tt.ToLower())));
            }
            var targetTypeCount = await targetTypeQuery.CountAsync();
            _logger.LogInformation("Debug: Logs matching target type filter: {Count}", targetTypeCount);

            // Test combined filters
            var combinedQuery = _context.AuditLogs.AsQueryable();
            if (!string.IsNullOrEmpty(filters.Action))
            {
                var actions = filters.Action.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(a => a.Trim().ToUpperInvariant()).ToList();
                combinedQuery = combinedQuery.Where(a => actions.Contains(a.Action.ToUpper()));
            }
            if (!string.IsNullOrEmpty(filters.TargetType))
            {
                var targetTypes = filters.TargetType.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim()).ToList();
                combinedQuery = combinedQuery.Where(a => targetTypes.Any(tt => a.EntityType.ToLower().Contains(tt.ToLower())));
            }
            var combinedCount = await combinedQuery.CountAsync();
            _logger.LogInformation("Debug: Logs matching combined filters: {Count}", combinedCount);

            // Get all distinct actions that contain authentication-related terms
            var authActions = await _context.AuditLogs
                .Where(a => a.Action.ToUpper().Contains("LOGIN") || 
                           a.Action.ToUpper().Contains("AUTH") || 
                           a.Action.ToUpper().Contains("SIGN") ||
                           a.Action.ToUpper().Contains("TOKEN") ||
                           a.Action.ToUpper().Contains("SESSION"))
                .Select(a => a.Action)
                .Distinct()
                .ToListAsync();

            // Get top 20 most common actions to see what's actually in the database
            var topActions = await _context.AuditLogs
                .GroupBy(a => a.Action)
                .Select(g => new { Action = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(20)
                .ToListAsync();

            // Get all distinct entity types
            var allEntityTypes = await _context.AuditLogs
                .Select(a => a.EntityType)
                .Distinct()
                .ToListAsync();

            return Ok(new
            {
                FiltersUsed = filters,
                TotalLogsInDatabase = totalCount,
                LogsMatchingActionFilter = actionCount,
                LogsMatchingTargetTypeFilter = targetTypeCount,
                LogsMatchingCombinedFilters = combinedCount,
                AuthenticationActions = authActions,
                TopActions = topActions,
                AllDistinctEntityTypes = allEntityTypes
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to debug export filters");
            return StatusCode(500, new { message = "Failed to debug export filters", error = ex.Message });
        }
    }

    [HttpPost("audit-logs/generate-test-data")]
    [AdminOnly]
    public async Task<IActionResult> GenerateTestAuditLogs()
    {
        try
        {
            var testLogs = new List<DisasterApp.Domain.Entities.AuditLog>
            {
                new DisasterApp.Domain.Entities.AuditLog
                {
                    AuditLogId = Guid.NewGuid(),
                    Action = "LOGIN_SUCCESS",
                    Severity = "info",
                    EntityType = "Security",
                    EntityId = Guid.NewGuid().ToString(),
                    Details = "User logged in successfully",
                    UserId = null,
                    UserName = "TestUser",
                    Timestamp = DateTime.UtcNow,
                    IpAddress = "192.168.1.1",
                    UserAgent = "Mozilla/5.0 Test Browser",
                    Resource = "auth",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new DisasterApp.Domain.Entities.AuditLog
                {
                    AuditLogId = Guid.NewGuid(),
                    Action = "LOGIN_FAILED",
                    Severity = "warning",
                    EntityType = "Security",
                    EntityId = Guid.NewGuid().ToString(),
                    Details = "Failed login attempt",
                    UserId = null,
                    UserName = "TestUser",
                    Timestamp = DateTime.UtcNow.AddMinutes(-5),
                    IpAddress = "192.168.1.2",
                    UserAgent = "Mozilla/5.0 Test Browser",
                    Resource = "auth",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new DisasterApp.Domain.Entities.AuditLog
                {
                    AuditLogId = Guid.NewGuid(),
                    Action = "USER_LOGIN_SUCCESS",
                    Severity = "info",
                    EntityType = "System",
                    EntityId = Guid.NewGuid().ToString(),
                    Details = "User authentication successful",
                    UserId = null,
                    UserName = "AnotherTestUser",
                    Timestamp = DateTime.UtcNow.AddMinutes(-10),
                    IpAddress = "192.168.1.3",
                    UserAgent = "Mozilla/5.0 Test Browser",
                    Resource = "security",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new DisasterApp.Domain.Entities.AuditLog
                {
                    AuditLogId = Guid.NewGuid(),
                    Action = "SYSTEM_EVENT",
                    Severity = "info",
                    EntityType = "General",
                    EntityId = "system",
                    Details = "System audit log entry",
                    UserId = null,
                    UserName = "System",
                    Timestamp = DateTime.UtcNow.AddMinutes(-15),
                    IpAddress = "127.0.0.1",
                    UserAgent = "System",
                    Resource = "system",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            await _context.AuditLogs.AddRangeAsync(testLogs);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Generated {Count} test audit logs", testLogs.Count);

            return Ok(new
            {
                message = $"Generated {testLogs.Count} test audit logs",
                generatedLogs = testLogs.Select(l => new
                {
                    l.Action,
                    l.EntityType,
                    l.Severity,
                    l.Details,
                    l.Timestamp
                })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate test audit logs");
            return StatusCode(500, new { message = "Failed to generate test audit logs", error = ex.Message });
        }
    }
}