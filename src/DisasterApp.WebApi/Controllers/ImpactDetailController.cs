using DisasterApp.Application.DTOs;
using DisasterApp.Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DisasterApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImpactDetailController : ControllerBase
    {
        private readonly IImpactDetailService _impactDetailService;

        public ImpactDetailController(IImpactDetailService impactDetailService)
        {
            _impactDetailService = impactDetailService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ImpactDetailDto>>> GetAll()
        {
            var impactDetails = await _impactDetailService.GetAllAsync();
            return Ok(impactDetails);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ImpactDetailDto>> GetById(int id)
        {
            var impactDetail = await _impactDetailService.GetByIdAsync(id);
            if (impactDetail == null)
                return NotFound();

            return Ok(impactDetail);
        }

        [HttpPost]
        public async Task<ActionResult<ImpactDetailDto>> Create(ImpactDetailCreateDto dto)
        {
            try
            {
                var created = await _impactDetailService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ImpactDetailDto>> Update(int id, ImpactDetailUpdateDto dto)
        {
            try
            {
                var updated = await _impactDetailService.UpdateAsync(id, dto);
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
                await _impactDetailService.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }

}
