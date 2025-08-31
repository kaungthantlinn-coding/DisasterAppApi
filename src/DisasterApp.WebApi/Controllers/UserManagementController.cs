using DisasterApp.Application.DTOs;
using DisasterApp.Application.Services.Interfaces;
using DisasterApp.WebApi.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DisasterApp.WebApi.Controllers;
//

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserManagementController : ControllerBase
{
    private readonly IUserManagementService _userManagementService;
    private readonly IRoleService _roleService;
    private readonly IBlacklistService _blacklistService;
    private readonly ILogger<UserManagementController> _logger;

    public UserManagementController(
        IUserManagementService userManagementService,
        IRoleService roleService,
        IBlacklistService blacklistService,
        ILogger<UserManagementController> logger)
    {
        _userManagementService = userManagementService;
        _roleService = roleService;
        _blacklistService = blacklistService;
        _logger = logger;
    }


    /// Get paginated list of users with filtering

    [HttpGet]
    [AdminOnly]
    public async Task<ActionResult<PagedUserListDto>> GetUsers([FromQuery] UserFilterDto filter)
    {
        try
        {
            var result = await _userManagementService.GetUsersAsync(filter);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }


    /// Get user details by ID

    [HttpGet("{userId}")]
    [AdminOnly]
    public async Task<ActionResult<UserDetailsDto>> GetUser(Guid userId)
    {
        try
        {
            var user = await _userManagementService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user {UserId}", userId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }


    /// Create a new user

    [HttpPost]
    [AdminOnly]
    public async Task<ActionResult<UserDetailsDto>> CreateUser([FromBody] CreateUserDto createUserDto)
    {
        try
        {
            if (createUserDto == null)
            {
                return BadRequest(new { message = "CreateUserDto cannot be null" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManagementService.CreateUserAsync(createUserDto);
            return CreatedAtAction(nameof(GetUser), new { userId = user.UserId }, user);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("User creation failed: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }


    /// Update an existing user


    [HttpPut("{userId}")]
    [AdminOnly]
    public async Task<ActionResult<UserDetailsDto>> UpdateUser(Guid userId, [FromBody] UpdateUserDto updateUserDto)
    {
        try
        {
            if (updateUserDto == null)
            {
                return BadRequest(new { message = "UpdateUserDto cannot be null" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManagementService.UpdateUserAsync(userId, updateUserDto);
            return Ok(user);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("User update failed: {Message}", ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("User update failed: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", userId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }


    /// Delete a user

    [HttpDelete("{userId}")]
    [AdminOnly]
    public async Task<IActionResult> DeleteUser(Guid userId)
    {
        try
        {
            var result = await _userManagementService.DeleteUserAsync(userId);
            if (!result)
            {
                return NotFound(new { message = "User not found" });
            }

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("User deletion failed: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", userId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }


    /// Blacklist/suspend a user with reason
    [HttpPost("{userId}/blacklist")]
    [AdminOnly]
    public async Task<IActionResult> BlacklistUser(Guid userId, [FromBody] BlacklistUserDto blacklistDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var adminUserId = GetCurrentUserId();
            if (adminUserId == Guid.Empty)
            {
                return Unauthorized(new { message = "Unable to identify admin user" });
            }

            var result = await _blacklistService.BlacklistUserAsync(userId, blacklistDto, adminUserId);
            
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error blacklisting user {UserId}", userId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// Remove blacklist/unsuspend a user
    [HttpPost("{userId}/unblacklist")]
    [AdminOnly]
    public async Task<IActionResult> UnblacklistUser(Guid userId, [FromBody] UnblacklistUserDto? unblacklistDto = null)
    {
        try
        {
            if (unblacklistDto != null && !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var adminUserId = GetCurrentUserId();
            if (adminUserId == Guid.Empty)
            {
                return Unauthorized(new { message = "Unable to identify admin user" });
            }

            var result = await _blacklistService.UnblacklistUserAsync(userId, unblacklistDto, adminUserId);
            
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unblacklisting user {UserId}", userId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// Get blacklist history for a user
    [HttpGet("{userId}/blacklist-history")]
    [AdminOnly]
    public async Task<IActionResult> GetBlacklistHistory(Guid userId)
    {
        try
        {
            var history = await _blacklistService.GetBlacklistHistoryAsync(userId);
            return Ok(new { success = true, data = history });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving blacklist history for user {UserId}", userId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }


    /// Change user password


    [HttpPost("{userId}/change-password")]
    [AdminOnly]
    public async Task<IActionResult> ChangeUserPassword(Guid userId, [FromBody] ChangeUserPasswordDto changePasswordDto)
    {
        try
        {
            if (changePasswordDto == null)
            {
                return BadRequest(new { message = "ChangeUserPasswordDto cannot be null" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _userManagementService.ChangeUserPasswordAsync(userId, changePasswordDto);
            if (!result)
            {
                return NotFound(new { message = "User not found" });
            }

            return Ok(new { message = "Password changed successfully" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Password change failed: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user {UserId}", userId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }


    /// Perform bulk operations on multiple users

    [HttpPost("bulk-operation")]
    [AdminOnly]
    public async Task<IActionResult> BulkOperation([FromBody] BulkUserOperationDto bulkOperation)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get current admin user ID from JWT token
            var adminUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid? adminUserId = null;
            if (Guid.TryParse(adminUserIdClaim, out var parsedAdminId))
            {
                adminUserId = parsedAdminId;
            }

            var affectedCount = await _userManagementService.BulkOperationAsync(bulkOperation, adminUserId);
            return Ok(new { message = $"Operation completed successfully", affectedCount });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Bulk operation failed: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing bulk operation");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }


    /// Get user management dashboard statistics

    [HttpGet("dashboard/stats")]
    [AdminOnly]
    public async Task<ActionResult<UserManagementStatsDto>> GetDashboardStats()
    {
        try
        {
            var stats = await _userManagementService.GetDashboardStatsAsync();
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dashboard stats");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// Get comprehensive user statistics for analytics
    [HttpGet("statistics")]
    [AdminOnly]
    public async Task<ActionResult<UserStatisticsResponseDto>> GetUserStatistics()
    {
        try
        {
            var statistics = await _userManagementService.GetUserStatisticsAsync();
            return Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user statistics");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// Get user activity trends over time
    [HttpGet("trends")]
    [AdminOnly]
    public async Task<ActionResult<UserActivityTrendsDto>> GetUserActivityTrends(
        [FromQuery] string period = "monthly",
        [FromQuery] int months = 12)
    {
        try
        {
            var trends = await _userManagementService.GetUserActivityTrendsAsync(period, months);
            return Ok(trends);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user activity trends");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// Get role distribution statistics
    [HttpGet("roles/distribution")]
    [AdminOnly]
    public async Task<ActionResult<RoleDistributionDto>> GetRoleDistribution()
    {
        try
        {
            var distribution = await _userManagementService.GetRoleDistributionAsync();
            return Ok(distribution);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving role distribution");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }


    /// Validate if user can be deleted

    [HttpGet("{userId}/validate-deletion")]
    [AdminOnly]
    public async Task<ActionResult<UserDeletionValidationDto>> ValidateUserDeletion(Guid userId)
    {
        try
        {
            var validation = await _userManagementService.ValidateUserDeletionAsync(userId);
            return Ok(validation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating user deletion for {UserId}", userId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    // update user roles only
    
    [HttpPut("{userId}/roles")]
    [AdminOnly]
    public async Task<ActionResult<UserDetailsDto>> UpdateUserRoles(Guid userId, [FromBody] UpdateUserRolesDto updateRolesDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManagementService.UpdateUserRolesAsync(userId, updateRolesDto);
            return Ok(user);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("User role update failed: {Message}", ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("User role update failed: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user roles for user {UserId}. Exception: {ExceptionType}, Message: {Message}, StackTrace: {StackTrace}", 
                userId, ex.GetType().Name, ex.Message, ex.StackTrace);
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

  
    // validate role update before applying changes
    
    [HttpPost("{userId}/roles/validate")]
    [AdminOnly]
    public async Task<ActionResult<RoleUpdateValidationDto>> ValidateRoleUpdate(Guid userId, [FromBody] UpdateUserRolesDto updateRolesDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var validation = await _userManagementService.ValidateRoleUpdateAsync(userId, updateRolesDto.Roles);
            return Ok(validation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating role update for user {UserId}", userId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    // get available roles for filtering
    
    [HttpGet("roles")]
    [AdminOnly]
    public async Task<ActionResult<List<string>>> GetAvailableRoles()
    {
        try
        {
            var roles = await _roleService.GetAllRolesAsync();
            var roleNames = roles.Select(r => r.Name).ToList();
            return Ok(roleNames);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving available roles");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// Get all CJ users (for chat)
    
    [HttpGet("cj-users")]
    [Authorize]
    public async Task<ActionResult<List<UserListItemDto>>> GetAllCjUsers()
    {
        try
        {
            var filter = new UserFilterDto { Role = "cj", PageNumber = 1, PageSize = 1000 };
            var result = await _userManagementService.GetUsersAsync(filter);
            return Ok(result.Users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving CJ users");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// Export users data in various formats
    
    [HttpPost("export")]
    [AdminOnly]
    public async Task<IActionResult> ExportUsers([FromBody] UserExportRequestDto exportRequest)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Normalize and validate filters
            if (exportRequest.Filters != null)
            {
                exportRequest.Filters.Role = exportRequest.Filters.Role?.Trim().ToLowerInvariant();
                exportRequest.Filters.Status = exportRequest.Filters.Status?.Trim().ToLowerInvariant();
            }

            // Validate format
            var validFormats = new[] { "csv", "json", "excel", "pdf" };
            if (!validFormats.Contains(exportRequest.Format.ToLower()))
            {
                return BadRequest(new { message = "Invalid format. Supported formats: csv, json, excel, pdf" });
            }

            // Validate fields if provided
            var validFields = new[] { "name", "email", "role", "status", "createdAt", "phoneNumber", "authProvider", "disasterReports", "supportRequests", "donations", "organizations" };
            if (exportRequest.Fields?.Any() == true)
            {
                var invalidFields = exportRequest.Fields.Where(f => !validFields.Contains(f, StringComparer.OrdinalIgnoreCase)).ToList();
                if (invalidFields.Any())
                {
                    return BadRequest(new { message = $"Invalid fields: {string.Join(", ", invalidFields)}. Valid fields: {string.Join(", ", validFields)}" });
                }
            }

            var exportData = await _userManagementService.ExportUsersAsync(exportRequest);
            
            var contentType = exportRequest.Format.ToLower() switch
            {
                "json" => "application/json",
                "csv" => "text/csv",
                "excel" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "pdf" => "application/pdf",
                _ => "text/csv"
            };
            
            var fileName = $"users-export-{DateTime.UtcNow:yyyy-MM-dd-HH-mm-ss}.{exportRequest.Format.ToLower()}";
            
            // Log export action with filters
            var currentUserId = GetCurrentUserId();
            if (currentUserId != Guid.Empty)
            {
                var filterInfo = "";
                if (exportRequest.Filters != null)
                {
                    var filters = new List<string>();
                    if (!string.IsNullOrEmpty(exportRequest.Filters.Role))
                        filters.Add($"role:{exportRequest.Filters.Role}");
                    if (!string.IsNullOrEmpty(exportRequest.Filters.Status))
                        filters.Add($"status:{exportRequest.Filters.Status}");
                    if (filters.Any())
                        filterInfo = $" with filters: {string.Join(", ", filters)}";
                }
                
                _logger.LogInformation("Admin {AdminId} exported users data in {Format} format{FilterInfo}", 
                    currentUserId, exportRequest.Format.ToUpper(), filterInfo);
            }
            
            return File(exportData, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export users in format {Format}", exportRequest.Format);
            return StatusCode(500, new { message = "Failed to export users data" });
        }
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}
