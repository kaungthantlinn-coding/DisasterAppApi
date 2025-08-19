using DisasterApp.Application.DTOs;
using DisasterApp.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DisasterApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DonationController : ControllerBase
    {
        private readonly IDonationService _donationService;

        public DonationController(IDonationService donationService)
        {
            _donationService = donationService;
        }

        [HttpPost]
        [Authorize] // User must be logged in
        public async Task<IActionResult> CreateDonation([FromBody] CreateDonationDto dto)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var donationId = await _donationService.CreateDonationAsync(userId, dto);
            return Ok(new { DonationId = donationId });
        }

        [HttpGet("organization/{organizationId}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetByOrganization(int organizationId)
        {
            var donations = await _donationService.GetDonationsByOrganizationIdAsync(organizationId);
            return Ok(donations);
        }

        [HttpGet("pending")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetPendingDonations()
        {
            var donations = await _donationService.GetPendingDonationsAsync();
            return Ok(donations);
        }

        [HttpPost("{donationId}/verify")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> VerifyDonation(int donationId, [FromForm] VerifyDonationDto dto)
        {
            var adminUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _donationService.VerifyDonationAsync(donationId, adminUserId, dto);
            if (!result) return NotFound();
            return Ok(new { Success = true });
        }


        //[HttpPost("{donationId}/verify")]
        //[Authorize(Policy = "AdminOnly")]
        //public async Task<IActionResult> VerifyDonation(int donationId, [FromBody] VerifyDonationDto dto)
        //{
        //    var adminUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        //    var result = await _donationService.VerifyDonationAsync(donationId, adminUserId, dto);
        //    if (!result) return NotFound();
        //    return Ok(new { Success = true });
        //}
    }
}
