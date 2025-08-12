using DisasterApp.Application.DTOs;
using DisasterApp.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace DisasterApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    
    public class DisasterTypeController : ControllerBase
    {
        private readonly IDisasterTypeService _service;

        public DisasterTypeController(IDisasterTypeService service)
        {
            _service = service;
        }
        
        [HttpGet]
        public async Task<ActionResult<List<DisasterTypeDto>>> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DisasterTypeDto>> GetById(int id)
        {
            var disasterType = await _service.GetByIdAsync(id);
            if (disasterType == null)
                return NotFound();

            return Ok(disasterType);
        }


        [HttpPost]
        public async Task<ActionResult> Create([FromBody] CreateDisasterTypeDto dto)
        {
            await _service.AddAsync(dto);
            return Ok(new { message = "DisasterType created successfully." });
        }


        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] UpdateDisasterTypeDto dto)
        {
            try
            {
                await _service.UpdateAsync(id, dto);
                return Ok(new { message = "DisasterType updated successfully." });
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var success = await _service.DeleteAsync(id);
            if (!success)
                return NotFound();

            return Ok(new { message = "DisasterType deleted successfully." });
        }

        
    }
}
