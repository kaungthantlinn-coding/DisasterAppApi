using DisasterApp.Application.DTOs;
using DisasterApp.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DisasterApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]

    public class DisasterReportController : ControllerBase
    {
        private readonly IDisasterReportService _service;
        public DisasterReportController(IDisasterReportService service)
        {
            _service = service;
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

        [Authorize]

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<DisasterReportDto>> Create([FromForm] DisasterReportCreateDto dto)
        {
            if (!User.Identity?.IsAuthenticated ?? true)
                return Unauthorized();

            // ✅ Extract userId from token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            var userId = Guid.Parse(userIdClaim);

            // ✅ Create report
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

        // DELETE api/<DisasterReportController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }
}
