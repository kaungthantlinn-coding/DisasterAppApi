using DisasterApp.Application.DTOs;
using DisasterApp.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DisasterApp.Controllers
{
    [Route("api/reports")]
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

        /// <summary>
        /// Get disaster reports statistics
        /// </summary>
        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            try
            {
                var stats = await _service.GetStatisticsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to retrieve statistics", error = ex.Message });
            }
        }

        /// <summary>
        /// Get verified disaster reports
        /// </summary>
        [HttpGet("verified")]
        public async Task<IActionResult> GetVerifiedReports()
        {
            try
            {
                var reports = await _service.GetAcceptedReportsAsync();
                return Ok(reports);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to retrieve verified reports", error = ex.Message });
            }
        }

        /// <summary>
        /// Get verified disaster reports (alias for backwards compatibility)
        /// </summary>
        [HttpGet("accepted")]
        public async Task<IActionResult> GetAcceptedReports()
        {
            try
            {
                var reports = await _service.GetAcceptedReportsAsync();
                return Ok(reports);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to retrieve accepted reports", error = ex.Message });
            }
        }
    }
}
