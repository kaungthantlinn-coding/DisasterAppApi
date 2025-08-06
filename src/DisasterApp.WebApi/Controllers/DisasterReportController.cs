using DisasterApp.Application.DTOs;
using DisasterApp.Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static DisasterApp.Application.DTOs.DisasterReportDto;

namespace DisasterApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DisasterReportController : ControllerBase
    {
        private readonly IDisasterReportService _service;
        public DisasterReportController(IDisasterReportService service)
        {
            _service = service;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var reports = await _service.GetAllAsync();
            return Ok(reports);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string keyword)
        {
            var results = await _service.SearchAsync(keyword);
            return Ok(results);
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ReportCreateDto dto)
        {
            var id = await _service.CreateReportAsync(dto);
            return Ok(new { ReportId = id });
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] DisasterReportUpdateDto dto)
        {
            var updated = await _service.UpdateAsync(id, dto);
            if (!updated) return NotFound();
            return Ok(new { message = "Updated successfully." });
        }
        [HttpDelete("id")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _service.DeleteReportAsync(id);
            if (!deleted) return NotFound();
            return Ok(new { message = "Deleted successfully." });

        }
    }
}
