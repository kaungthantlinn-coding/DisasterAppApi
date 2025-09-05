using DisasterApp.Application.DTOs;
using DisasterApp.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DisasterApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DonationController : ControllerBase
    {
        private readonly IDonationService _donationService;

        public DonationController(IDonationService donationService)
        {
            _donationService = donationService;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateDonation([FromForm] CreateDonationDto dto)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var donationId = await _donationService.CreateDonationAsync(userId, dto);
            return Ok(new { DonationId = donationId });
        }

        [HttpGet("organization/{organizationId}")]
        [Authorize]

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
        public async Task<IActionResult> VerifyDonation(int donationId)
        {
            var adminUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _donationService.VerifyDonationAsync(donationId, adminUserId);
            if (!result) return NotFound();
            return Ok(new { Success = true });
        }

        [HttpGet("verified")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetVerifiedDonations()
        {
            var donations = await _donationService.GetVerifiedDonationsAsync();
            return Ok(donations);
        }

        [HttpGet("dashboard")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetDonationSummary()
        {
            var summary = await _donationService.GetDonationSummaryAsync();
            return Ok(summary);
        }



    }
}
