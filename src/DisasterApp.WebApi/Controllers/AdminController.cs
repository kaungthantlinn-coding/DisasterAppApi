using DisasterApp.WebApi.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DisasterApp.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AdminController : ControllerBase
{
    private readonly ILogger<AdminController> _logger;

    public AdminController(ILogger<AdminController> logger)
    {
        _logger = logger;
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
    public IActionResult GetAuditLogs()
    {
        return Ok(new { message = "Admin viewing audit logs", data = "Audit logs would be here" });
    }
}