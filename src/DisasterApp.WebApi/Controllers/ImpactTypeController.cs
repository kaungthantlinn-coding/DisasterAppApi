using DisasterApp.Application.DTOs;
using DisasterApp.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DisasterApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ImpactTypeController : ControllerBase
    {
        private readonly IImpactTypeService _service;

        public ImpactTypeController(IImpactTypeService service)
        {
            _service = service;
        }
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<List<ImpactTypeDto>>> GetAll()
        {
            var types = await _service.GetAllAsync();
            return Ok(types);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ImpactTypeDto>> Create([FromBody] ImpactTypeCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest("Name is required.");

            var created = await _service.CreateAsync(dto);
            return Ok(created);
        }
    }
}