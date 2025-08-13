using DisasterApp.Application.DTOs;
using DisasterApp.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DisasterApp.WebApi.Controllers;

[ApiController]
[Route("api/support-requests")]
[Authorize]
public class SupportRequestController : ControllerBase
{
    private readonly ISupportRequestService _supportRequestService;
    private readonly ILogger<SupportRequestController> _logger;

    public SupportRequestController(
        ISupportRequestService supportRequestService,
        ILogger<SupportRequestController> logger)
    {
        _supportRequestService = supportRequestService;
        _logger = logger;
    }

    /// <summary>
    /// Get all support requests
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SupportRequestDto>>> GetAll()
    {
        try
        {
            var supportRequests = await _supportRequestService.GetAllAsync();
            return Ok(supportRequests);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving support requests");
            return StatusCode(500, "An error occurred while retrieving support requests");
        }
    }

    /// <summary>
    /// Get support request by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<SupportRequestDto>> GetById(int id)
    {
        try
        {
            var supportRequest = await _supportRequestService.GetByIdAsync(id);
            if (supportRequest == null)
                return NotFound($"Support request with ID {id} not found");

            return Ok(supportRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving support request {Id}", id);
            return StatusCode(500, "An error occurred while retrieving the support request");
        }
    }

    /// <summary>
    /// Get support requests by user ID
    /// </summary>
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<SupportRequestDto>>> GetByUserId(Guid userId)
    {
        try
        {
            var supportRequests = await _supportRequestService.GetByUserIdAsync(userId);
            return Ok(supportRequests);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving support requests for user {UserId}", userId);
            return StatusCode(500, "An error occurred while retrieving support requests");
        }
    }

    /// <summary>
    /// Get support requests by report ID
    /// </summary>
    [HttpGet("report/{reportId}")]
    public async Task<ActionResult<IEnumerable<SupportRequestDto>>> GetByReportId(Guid reportId)
    {
        try
        {
            var supportRequests = await _supportRequestService.GetByReportIdAsync(reportId);
            return Ok(supportRequests);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving support requests for report {ReportId}", reportId);
            return StatusCode(500, "An error occurred while retrieving support requests");
        }
    }

    /// <summary>
    /// Create a new support request
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<SupportRequestDto>> Create([FromBody] SupportRequestCreateDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
                return Unauthorized("User ID not found in token");

            var supportRequest = await _supportRequestService.CreateAsync(dto, userId);
            return CreatedAtAction(nameof(GetById), new { id = supportRequest.Id }, supportRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating support request");
            return StatusCode(500, "An error occurred while creating the support request");
        }
    }

    /// <summary>
    /// Update an existing support request
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<SupportRequestDto>> Update(int id, [FromBody] SupportRequestUpdateDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
                return Unauthorized("User ID not found in token");

            var updatedRequest = await _supportRequestService.UpdateAsync(id, dto, userId);
            if (updatedRequest == null)
                return NotFound($"Support request with ID {id} not found");

            return Ok(updatedRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating support request {Id}", id);
            return StatusCode(500, "An error occurred while updating the support request");
        }
    }

    /// <summary>
    /// Delete a support request
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
                return Unauthorized("User ID not found in token");

            var deleted = await _supportRequestService.DeleteAsync(id, userId);
            if (!deleted)
                return NotFound($"Support request with ID {id} not found");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting support request {Id}", id);
            return StatusCode(500, "An error occurred while deleting the support request");
        }
    }

    /// <summary>
    /// Get all available support types
    /// </summary>
    [HttpGet("support-types")]
    public async Task<ActionResult<IEnumerable<SupportTypeDto>>> GetSupportTypes()
    {
        try
        {
            var supportTypes = await _supportRequestService.GetSupportTypesAsync();
            return Ok(supportTypes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving support types");
            return StatusCode(500, "An error occurred while retrieving support types");
        }
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}