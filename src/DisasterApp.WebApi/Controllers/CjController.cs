using DisasterApp.WebApi.Authorization;
using DisasterApp.Infrastructure.Data;
using DisasterApp.Domain.Entities;
using DisasterApp.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DisasterApp.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CjController : ControllerBase
{
    private readonly ILogger<CjController> _logger;
    private readonly DisasterDbContext _context;

    public CjController(ILogger<CjController> logger, DisasterDbContext context)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _context = context ?? throw new ArgumentNullException(nameof(context));
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
    public async Task<IActionResult> GetVerificationQueue()
    {
        var pendingReports = await _context.DisasterReports
            .Where(r => r.Status == ReportStatus.Pending)
            .Include(r => r.User)
            .Select(r => new 
            {
                id = r.Id,
                title = r.Title,
                description = r.Description,
                severity = r.Severity,
                timestamp = r.Timestamp,
                user = new 
                {
                    userId = r.User!.UserId,
                    name = r.User.Name,
                    email = r.User.Email
                }
            })
            .ToListAsync();

        return Ok(pendingReports);
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

    [HttpPost("approve-report/{reportId}")]
    [CjOnly]
    public async Task<IActionResult> ApproveReport(Guid reportId)
    {
        if (reportId == Guid.Empty)
        {
            return BadRequest(new { message = "Invalid report ID" });
        }

        var report = await _context.DisasterReports.FindAsync(reportId);
        if (report == null)
        {
            return NotFound(new { message = "Report not found" });
        }

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            return Unauthorized();
        }

        report.Status = ReportStatus.Verified;
        report.VerifiedBy = Guid.Parse(userId);
        report.VerifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Report approved successfully", reportId });
    }

    [HttpPost("reject-report/{reportId}")]
    [CjOnly]
    public async Task<IActionResult> RejectReport(Guid reportId, [FromBody] string? reason = null)
    {
        if (reportId == Guid.Empty)
        {
            return BadRequest(new { message = "Invalid report ID" });
        }

        var report = await _context.DisasterReports.FindAsync(reportId);
        if (report == null)
        {
            return NotFound(new { message = "Report not found" });
        }

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            return Unauthorized();
        }

        report.Status = ReportStatus.Rejected;
        report.VerifiedBy = Guid.Parse(userId);
        report.VerifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Report rejected successfully", reportId, reason });
    }

    [HttpGet("report-details/{reportId}")]
    [CjOnly]
    public async Task<IActionResult> GetReportDetails(Guid reportId)
    {
        if (reportId == Guid.Empty)
        {
            return BadRequest(new { message = "Invalid report ID" });
        }

        var report = await _context.DisasterReports
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == reportId);

        if (report == null)
        {
            return NotFound(new { message = "Report not found" });
        }

        return Ok(new 
        { 
            reportId = report.Id,
            title = report.Title,
            description = report.Description,
            status = report.Status,
            severity = report.Severity,
            timestamp = report.Timestamp,
            user = new 
            {
                userId = report.User?.UserId,
                name = report.User?.Name,
                email = report.User?.Email
            }
        });
    }
}