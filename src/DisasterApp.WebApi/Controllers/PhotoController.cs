using DisasterApp.Application.DTOs;
using DisasterApp.Application.Services;
using DisasterApp.Application.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DisasterApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PhotoController : ControllerBase
    {
        private readonly IPhotoService _photoService;

        public PhotoController(IPhotoService photoService)
        {
            _photoService = photoService;
        }


        [HttpGet]
        public async Task<IActionResult> GetAllPhotos()
        {
            var photos = await _photoService.GetAllPhotosAsync();
            return Ok(photos);
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadPhoto([FromForm] CreatePhotoDto dto)
        {
            try
            {
                var result = await _photoService.UploadPhotoAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpPut("update")]
        public async Task<IActionResult> UpdatePhoto([FromForm] UpdatePhotoDto dto)
        {
            try
            {
                var updated = await _photoService.UpdatePhotoAsync(dto);
                return Ok(updated);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePhoto(int id)
        {
            try
            {
                await _photoService.DeletePhotoAsync(id);
                return Ok(new { message = "Photo deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

    }
}