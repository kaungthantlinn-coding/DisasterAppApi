using DisasterApp.Application.DTOs;
using DisasterApp.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DisasterApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrganizationController : ControllerBase
    {
        private readonly IOrganizationService _service;

        public OrganizationController(IOrganizationService service)
        {
            _service = service;
        }

        private Guid GetUserId() =>
            Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Create([FromBody] CreateOrganizationDto dto)
        {
            var userId = GetUserId();
            var id = await _service.CreateOrganizationAsync(userId, dto);
            return Ok(new { OrganizationId = id });
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateOrganizationDto dto)
        {
            var userId = GetUserId();
            var success = await _service.UpdateOrganizationAsync(id, userId, dto);
            if (!success) return Forbid();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetUserId();
            var success = await _service.DeleteOrganizationAsync(id, userId);
            if (!success) return Forbid();
            return NoContent();
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var org = await _service.GetOrganizationByIdAsync(id);
            if (org == null) return NotFound();
            return Ok(org);
        }

        [HttpGet]
        [AllowAnonymous]

        public async Task<IActionResult> GetAll()
        {
            var orgs = await _service.GetOrganizationsAsync();
            return Ok(orgs);
        }
    }
}
