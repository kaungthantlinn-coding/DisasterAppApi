using DisasterApp.Application.DTOs; //
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
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
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

    // Audit Log Endpoints
    [HttpGet("audit-logs")]
    [AdminOnly]
    public async Task<IActionResult> GetAuditLogs([FromQuery] AuditLogFiltersDto filters)
    {
        try
        {
            var result = await _auditService.GetLogsAsync(filters);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve audit logs");
            return StatusCode(500, new { message = "Failed to retrieve audit logs", error = ex.Message });
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
            return StatusCode(500, new { message = "Failed to retrieve audit log statistics", error = ex.Message });
        }
    }

    [HttpGet("audit-logs/export")]
    [AdminOnly]
    public async Task<IActionResult> ExportAuditLogs([FromQuery] string format = "csv", [FromQuery] AuditLogFiltersDto filters = null)
    {
        try
        {
            filters ??= new AuditLogFiltersDto();
            var data = await _auditService.ExportLogsAsync(format, filters);
            
            var contentType = format.ToLower() == "excel" ? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" : "text/csv";
            var fileName = $"audit-logs-{DateTime.UtcNow:yyyyMMdd-HHmmss}.{(format.ToLower() == "excel" ? "xlsx" : "csv")}";
            
            return File(data, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export audit logs");
            return StatusCode(500, new { message = "Failed to export audit logs", error = ex.Message });
        }
    }
}