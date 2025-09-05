using DisasterApp.Application.DTOs;
using DisasterApp.Application.Services;
using Microsoft.AspNetCore.Mvc;


namespace DisasterApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DisasterEventController : ControllerBase
    {
        private readonly IDisasterEventService _service;
        public DisasterEventController(IDisasterEventService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<List<DisasterEventDto>>> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<DisasterEventDto>> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] CreateDisasterEventDto dto)
        {
            await _service.AddAsync(dto);
            return Ok(new { message = "DisasterEvent created successfully." });
        }
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(Guid id, [FromBody] UpdateDisasterEventDto dto)
        {
            try
            {
                await _service.UpdateAsync(id, dto);
                return Ok(new { message = "DisasterEvent updated successfully." });
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            var success = await _service.DeleteAsync(id);
            if (!success) return NotFound();

            return Ok(new { message = "DisasterEvent deleted successfully." });
        }
    }
}
