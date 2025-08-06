using DisasterApp.Application.DTOs;
using DisasterApp.Application.Services.Interfaces;
using DisasterApp.WebApi.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DisasterApp.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AdminController : ControllerBase
{
    private readonly ILogger<AdminController> _logger;
    private readonly IAuditService _auditService;

    public AdminController(ILogger<AdminController> logger, IAuditService auditService)
    {
        _logger = logger;
        _auditService = auditService;
    }

    [HttpGet("dashboard")]
    [AdminOnly]
    public IActionResult GetAdminDashboard()
    {
        return Ok(new { message = "Welcome to Admin Dashboard", timestamp = DateTime.UtcNow });
    }

    [HttpGet("users")]
    [AdminOnly]
    public IActionResult GetAllUsers()
    {
        return Ok(new { message = "Admin can view all users", data = "User list would be here" });
    }

    [HttpGet("reports")]
    [AdminOrCj]
    public IActionResult GetReports()
    {
        return Ok(new { message = "Admin or CJ can view reports", data = "Reports would be here" });
    }

    [HttpPost("system-settings")]
    [AdminOnly]
    public IActionResult UpdateSystemSettings([FromBody] object settings)
    {
        return Ok(new { message = "System settings updated by admin", timestamp = DateTime.UtcNow });
    }

    [HttpGet("audit-logs")]
    [AdminOnly]
    public async Task<IActionResult> GetAuditLogs([FromQuery] AuditLogFiltersDto filters)
    {
        try
        {
            var result = await _auditService.GetLogsAsync(filters);
            
            // Log admin access to audit logs
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userId, out var adminUserId))
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
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve audit logs");
            return StatusCode(500, new { message = "Failed to retrieve audit logs" });
        }
    }

    [HttpGet("audit-logs/stats")]
    [AdminOnly]
    public async Task<IActionResult> GetAuditLogStatistics()
    {
        try
        {
            var stats = await _auditService.GetStatisticsAsync();
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve audit log statistics");
            return StatusCode(500, new { message = "Failed to retrieve audit log statistics" });
        }
    }

    [HttpGet("audit-logs/export")]
    [AdminOnly]
    public async Task<IActionResult> ExportAuditLogs([FromQuery] string format, [FromQuery] AuditLogFiltersDto filters)
    {
        try
        {
            if (string.IsNullOrEmpty(format) || (format.ToLower() != "csv" && format.ToLower() != "excel"))
            {
                return BadRequest(new { message = "Invalid format. Use 'csv' or 'excel'." });
            }

            var exportData = await _auditService.ExportLogsAsync(format, filters);
            
            var contentType = format.ToLower() == "csv" ? "text/csv" : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            var fileName = $"audit-logs-{DateTime.UtcNow:yyyy-MM-dd-HH-mm-ss}.{format.ToLower()}";
            
            // Log export action
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userId, out var adminUserId))
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
}