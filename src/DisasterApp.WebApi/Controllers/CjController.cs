using DisasterApp.WebApi.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DisasterApp.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CjController : ControllerBase
{
    private readonly ILogger<CjController> _logger;

    public CjController(ILogger<CjController> logger)
    {
        _logger = logger;
    }

    [HttpGet("dashboard")]
    [CjOnly]
    public IActionResult GetCjDashboard()
    {
        return Ok(new { message = "Welcome to CJ Dashboard", timestamp = DateTime.UtcNow });
    }

    [HttpGet("pending-approvals")]
    [CjOnly]
    public IActionResult GetPendingApprovals()
    {
        return Ok(new { message = "CJ can view pending approvals", data = "Pending approvals would be here" });
    }

    [HttpPost("approve-disaster-report/{reportId}")]
    [CjOnly]
    public IActionResult ApproveDisasterReport(Guid reportId)
    {
        return Ok(new { message = $"Disaster report {reportId} approved by CJ", timestamp = DateTime.UtcNow });
    }

    [HttpPost("reject-disaster-report/{reportId}")]
    [CjOnly]
    public IActionResult RejectDisasterReport(Guid reportId, [FromBody] string reason)
    {
        return Ok(new { message = $"Disaster report {reportId} rejected by CJ", reason, timestamp = DateTime.UtcNow });
    }

    [HttpGet("verification-queue")]
    [AdminOrCj]
    public IActionResult GetVerificationQueue()
    {
        return Ok(new { message = "Admin or CJ can view verification queue", data = "Verification queue would be here" });
    }

    [HttpGet("statistics")]
    [CjOnly]
    public IActionResult GetCjStatistics()
    {
        return Ok(new 
        { 
            message = "CJ statistics", 
            data = new 
            {
                totalReportsReviewed = 150,
                pendingReports = 25,
                approvedReports = 120,
                rejectedReports = 5
            }
        });
    }
}