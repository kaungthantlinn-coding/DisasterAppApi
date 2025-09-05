using DisasterApp.Application.DTOs;
using DisasterApp.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


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
        [HttpGet]
        public async Task<ActionResult<List<ImpactTypeDto>>> GetAll()
        {
            var types = await _service.GetAllAsync();
            return Ok(types);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ImpactTypeDto>> GetById(int id)
        {
            var impactType = await _service.GetByIdAsync(id);
            if (impactType == null)
                return NotFound();

            return Ok(impactType);
        }


        [HttpPost]
        public async Task<ActionResult<ImpactTypeDto>> Create([FromBody] ImpactTypeCreateDto dto)
        {
            try
            {
                var created = await _service.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ImpactTypeDto>> Update(int id, ImpactTypeUpdateDto dto)
        {
            try
            {
                var updated = await _service.UpdateAsync(id, dto);
                return Ok(updated);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _service.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}