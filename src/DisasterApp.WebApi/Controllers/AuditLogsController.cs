using DisasterApp.Application.DTOs;
using DisasterApp.Application.Services.Interfaces;
using DisasterApp.WebApi.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DisasterApp.WebApi.Controllers;

[ApiController]
[Route("api/audit-logs")]
[Authorize]
public class AuditLogsController : ControllerBase
{
    private readonly ILogger<AuditLogsController> _logger;
    private readonly IAuditService _auditService;

    public AuditLogsController(ILogger<AuditLogsController> logger, IAuditService auditService)
    {
        _logger = logger;
        _auditService = auditService;
    }

    /// <summary>
    /// Get audit logs with filtering and pagination
    /// </summary>
    /// <param name="page">Page number for pagination</param>
    /// <param name="limit">Number of records per page</param>
    /// <param name="search">Search term for filtering</param>
    /// <param name="severity">Filter by severity level</param>
    /// <param name="action">Filter by action type</param>
    /// <param name="userId">Filter by specific user ID</param>
    /// <param name="startDate">Start date for date range filter</param>
    /// <param name="endDate">End date for date range filter</param>
    /// <returns>Paginated audit logs</returns>
    [HttpGet]
    [AdminOnly]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? severity = null,
        [FromQuery] string? action = null,
        [FromQuery] string? userId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var filters = new AuditLogFiltersDto
            {
                Page = page,
                PageSize = Math.Min(limit, 100), // Cap at 100 records per page
                Search = search,
                Severity = severity,
                Action = action,
                UserId = userId,
                DateFrom = startDate,
                DateTo = endDate
            };

            var result = await _auditService.GetLogsAsync(filters);
            
            // Transform to match the required response format
            var response = new
            {
                logs = result.Logs.Select(log => new
                {
                    id = log.Id,
                    timestamp = log.Timestamp,
                    userId = log.User?.Id,
                    userName = log.User?.Name,
                    action = log.Action,
                    severity = log.Severity,
                    details = log.Details,
                    ipAddress = log.IpAddress,
                    userAgent = log.UserAgent,
                    metadata = log.Metadata ?? new Dictionary<string, object>()
                }).ToList(),
                total = result.TotalCount,
                page = result.Page,
                limit = result.PageSize,
                totalPages = (int)Math.Ceiling((double)result.TotalCount / result.PageSize)
            };
            
            // Log admin access to audit logs
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(currentUserId, out var adminUserId))
            {
                await _auditService.LogUserActionAsync(
                    "AUDIT_LOGS_ACCESSED",
                    "info",
                    adminUserId,
                    "Admin accessed audit logs",
                    "audit",
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    Request.Headers.UserAgent.ToString()
                );
            }
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve audit logs");
            return StatusCode(500, new { message = "Failed to retrieve audit logs" });
        }
    }

    /// <summary>
    /// Get audit log statistics
    /// </summary>
    /// <returns>Audit log statistics</returns>
    [HttpGet("stats")]
    [AdminOnly]
    public async Task<IActionResult> GetAuditLogStatistics()
    {
        try
        {
            var stats = await _auditService.GetStatisticsAsync();
            
            // Transform to match the required response format
            var response = new
            {
                totalLogs = stats.TotalLogs,
                todayLogs = stats.RecentActivity, // Using RecentActivity as proxy for today's logs
                criticalAlerts = stats.CriticalAlerts,
                activeUsers = await GetActiveUsersCountAsync()
            };
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve audit log statistics");
            return StatusCode(500, new { message = "Failed to retrieve audit log statistics" });
        }
    }

    /// <summary>
    /// Export audit logs in CSV or Excel format
    /// </summary>
    /// <param name="format">Export format (csv or excel)</param>
    /// <param name="page">Page number for pagination</param>
    /// <param name="limit">Number of records per page</param>
    /// <param name="search">Search term for filtering</param>
    /// <param name="severity">Filter by severity level</param>
    /// <param name="action">Filter by action type</param>
    /// <param name="userId">Filter by specific user ID</param>
    /// <param name="startDate">Start date for date range filter</param>
    /// <param name="endDate">End date for date range filter</param>
    /// <returns>File download</returns>
    [HttpGet("export")]
    [AdminOnly]
    public async Task<IActionResult> ExportAuditLogs(
        [FromQuery] string format = "csv",
        [FromQuery] int page = 1,
        [FromQuery] int limit = 1000,
        [FromQuery] string? search = null,
        [FromQuery] string? severity = null,
        [FromQuery] string? action = null,
        [FromQuery] string? userId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            if (string.IsNullOrEmpty(format) || (format.ToLower() != "csv" && format.ToLower() != "excel"))
            {
                return BadRequest(new { message = "Invalid format. Use 'csv' or 'excel'." });
            }

            var filters = new AuditLogFiltersDto
            {
                Page = page,
                PageSize = Math.Min(limit, 10000), // Cap at 10000 for exports
                Search = search,
                Severity = severity,
                Action = action,
                UserId = userId,
                DateFrom = startDate,
                DateTo = endDate
            };

            var exportData = await _auditService.ExportLogsAsync(format, filters);
            
            var contentType = format.ToLower() == "csv" ? "text/csv" : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            var fileName = $"audit-logs-{DateTime.UtcNow:yyyy-MM-dd-HH-mm-ss}.{format.ToLower()}";
            
            // Log export action
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(currentUserId, out var adminUserId))
            {
                await _auditService.LogUserActionAsync(
                    "AUDIT_LOGS_EXPORTED",
                    "info",
                    adminUserId,
                    $"Admin exported audit logs in {format.ToUpper()} format",
                    "audit",
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    Request.Headers.UserAgent.ToString(),
                    new Dictionary<string, object> { { "format", format }, { "filters", filters } }
                );
            }
            
            return File(exportData, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export audit logs in format {Format}", format);
            return StatusCode(500, new { message = "Failed to export audit logs" });
        }
    }

    private async Task<int> GetActiveUsersCountAsync()
    {
        try
        {
            // Get count of unique users who have logged actions in the last 24 hours
            var yesterday = DateTime.UtcNow.AddDays(-1);
            var filters = new AuditLogFiltersDto
            {
                DateFrom = yesterday,
                PageSize = 1000 // Get enough to count unique users
            };
            
            var result = await _auditService.GetLogsAsync(filters);
            return result.Logs
                .Where(log => log.User != null)
                .Select(log => log.User!.Id)
                .Distinct()
                .Count();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get active users count, returning 0");
            return 0;
        }
    }
}