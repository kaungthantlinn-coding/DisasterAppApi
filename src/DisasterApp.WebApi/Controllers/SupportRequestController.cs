using DisasterApp.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static DisasterApp.Application.DTOs.SupportRequestDto;

namespace DisasterApp.WebApi.Controllers;


    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
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
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await _service.GetByIdAsync(id);
            if (data == null) return NotFound();
            return Ok(data);
        }
        [Authorize]
        [HttpGet("metrics")]
        public async Task<IActionResult> GetMetrics()
        {
            var metrics = await _service.GetMetricsAsync();
            return Ok(metrics);
        }
        [Authorize]
        [HttpGet("pending")]
        public async Task<IActionResult> GetPending()
        {
            var result = await _service.GetPendingRequestsAsync();
            return Ok(result);
        }
        [Authorize]

        [HttpGet("accepted")]
        public async Task<IActionResult> GetAccepted()
        {
            var result = await _service.GetAcceptedRequestsAsync();
            return Ok(result);
        }

        [HttpGet("rejected")]
        public async Task<IActionResult> GetRejected()
        {
            var result = await _service.GetRejectedRequestsAsync();
            return Ok(result);
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
            await _service.CreateAsync(userId, dto);
            return Ok(new { message = "Support request created." });
        }
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] SupportRequestUpdateDto dto)
        {
            await _service.UpdateAsync(id, dto);
            return Ok(new { message = "Support request updated." });
        }
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted) return NotFound();
            return Ok(new { message = "Support request deleted." });
        }
        [Authorize]
        [HttpGet("support-types")]
        public async Task<ActionResult<IEnumerable<string>>> GetSupportTypes()
        {
            var types = await _service.GetSupportTypeNamesAsync();
            return Ok(types);
        }
        [Authorize]
        [HttpPut("{id}/accept")]
        public async Task<IActionResult> Accept(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            var adminId = Guid.Parse(userIdClaim);
            var updatedDto = await _service.ApproveSupportRequestAsync(id, adminId);
            if (updatedDto == null) return NotFound();
            return Ok(updatedDto);
        }
        [HttpPut("{id}/reject")]
        public async Task<IActionResult> Reject(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            var adminId = Guid.Parse(userIdClaim);

            var updatedDto = await _service.RejectSupportRequestAsync(id, adminId);
            if (updatedDto == null) return NotFound();

            return Ok(updatedDto);

        }
        [Authorize]
        [HttpGet("accepted/{reportId}")]
        public async Task<IActionResult> GetAcceptedRequests(Guid reportId)
        {
            var result = await _service.GetAcceptedReportIdAsync(reportId);

            if (result == null || !result.Any())
                return NotFound(new { Message = "No accepted support requests found for this report." });

            return Ok(result);
        }
    }