using DisasterApp.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using DisasterApp.Application.Services;
using DisasterApp.Application.Services.Interfaces;

namespace DisasterApp.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]

    public class DisasterReportController : ControllerBase
    {
        private readonly IDisasterReportService _service;
        private readonly IExportService _exportService;
        public DisasterReportController(IDisasterReportService service, IExportService exportService)
        {
            _service = service;
            _exportService = exportService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());


        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var report = await _service.GetByIdAsync(id);
            if (report == null) return NotFound();
            return Ok(report);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetReportsByUserId(Guid userId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            var reports = await _service.GetReportsByUserIdAsync(userId);
            return Ok(reports);
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingReports()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();
            var adminId = Guid.Parse(userIdClaim);
            var reports = await _service.GetPendingReportsForAdminAsync(adminId);
            return Ok(reports);

        }

        [HttpGet("accepted")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAcceptedReports()
        {
            var reports = await _service.GetAcceptedReportsAsync();
            return Ok(reports);
        }

        [HttpGet("rejected")]
        public async Task<IActionResult> GetRejectedReports()
        {
            var reports = await _service.GetRejectedReportsAsync();
            return Ok(reports);
        }

        [HttpPut("{id}/accept")]
        public async Task<IActionResult> Accept(Guid id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            var adminId = Guid.Parse(userIdClaim);

            var result = await _service.ApproveDisasterReportAsync(id, adminId);
            if (!result) return NotFound();
            return Ok(new { Message = "Report accepted successfully" });
        }

        [HttpPut("{id}/reject")]
        public async Task<IActionResult> Reject(Guid id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            var adminId = Guid.Parse(userIdClaim);

            var result = await _service.RejectDisasterReportAsync(id, adminId);
            if (!result) return NotFound();
            return Ok(new { Message = "Report rejected successfully" });
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            var adminId = Guid.Parse(userIdClaim);
            var result = await _service.ApproveOrRejectReportAsync(id, dto.Status, adminId);
            if (!result) return NotFound();
            return Ok(new { Message = $"Report {dto.Status}" });
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<DisasterReportDto>> Create([FromForm] DisasterReportCreateDto dto)
        {
            if (!User.Identity?.IsAuthenticated ?? true)
                return Unauthorized();

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            var userId = Guid.Parse(userIdClaim);

            var report = await _service.CreateAsync(dto, userId);
            return CreatedAtAction(nameof(GetById), new { id = report.Id }, report);
        }

        [HttpPut("{id:guid}")]
        [Consumes("multipart/form-data")]

        public async Task<IActionResult> Update(Guid id, [FromForm] DisasterReportUpdateDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            var userId = Guid.Parse(userIdClaim);
            var updatedReport = await _service.UpdateAsync(id, dto, userId);

            if (updatedReport == null)
                return NotFound();

            return Ok(updatedReport);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted)
                return NotFound();

            return NoContent();
        }

        [AllowAnonymous]
        [HttpGet("pdf")]
        public async Task<IActionResult> ExportPdf()
        {
            var reports = await _service.GetAllAsync();
            var file = await _exportService.ExportDisasterReportsToPdfAsync(reports);
            return File(file, "application/pdf", "DisasterReports.pdf");
        }
        
        [AllowAnonymous]
        [HttpGet("excel")]
        public async Task<IActionResult> ExportExcel()
        {
            var reports = await _service.GetAllAsync();
            var file = await _exportService.ExportDisasterReportsToExcelAsync(reports);
            return File(file,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "DisasterReports.xlsx");
        }

    }
}