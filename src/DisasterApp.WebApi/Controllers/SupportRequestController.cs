using DisasterApp.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static DisasterApp.Application.DTOs.SupportRequestDto;

namespace DisasterApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupportRequestController : ControllerBase
    {
        private readonly ISupportRequestService _service;
        public SupportRequestController(ISupportRequestService service)
        {
            _service = service;
        }
        [Authorize]
        
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _service.GetAllAsync();
            return Ok(data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await _service.GetByIdAsync(id);
            if (data == null) return NotFound();
            return Ok(data);
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SupportRequestCreateDto dto)
        {
            if (!User.Identity?.IsAuthenticated ?? true)
                return Unauthorized();

            // ✅ Extract userId from token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            var userId = Guid.Parse(userIdClaim);
            await _service.CreateAsync(userId,dto);
            return Ok(new { message = "Support request created." });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] SupportRequestUpdateDto dto)
        {
            await _service.UpdateAsync(id, dto);
            return Ok(new { message = "Support request updated." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted) return NotFound();
            return Ok(new { message = "Support request deleted." });
        }
    }
}
