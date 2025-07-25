using Microsoft.AspNetCore.Mvc;

namespace DisasterApp.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConfigController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public ConfigController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("google-client-id")]
        public IActionResult GetGoogleClientId()
        {
            var clientId = _configuration["GoogleAuth:ClientId"];
            
            if (string.IsNullOrEmpty(clientId))
            {
                return BadRequest("Google Client ID not configured");
            }

            return Ok(new { clientId });
        }
    }
}