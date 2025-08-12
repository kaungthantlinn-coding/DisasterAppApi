using DisasterApp.Application.DTOs;
using DisasterApp.Application.Services.Interfaces;
using DisasterApp.WebApi.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DisasterApp.WebApi.Controllers;


[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserManagementController : ControllerBase
{
    private readonly IUserManagementService _userManagementService;
    private readonly IRoleService _roleService;
    private readonly ILogger<UserManagementController> _logger;

    public UserManagementController(
        IUserManagementService userManagementService,
        IRoleService roleService,
        ILogger<UserManagementController> logger)
    {
        _userManagementService = userManagementService;
        _roleService = roleService;
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


    /// Blacklist/suspend a user

    [HttpPost("{userId}/blacklist")]
    [AdminOnly]
    public async Task<IActionResult> BlacklistUser(Guid userId)
    {
        try
        {
            var result = await _userManagementService.BlacklistUserAsync(userId);
            if (!result)
            {
                return NotFound(new { message = "User not found" });
            }

            return Ok(new { message = "User blacklisted successfully" });
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
    public async Task<IActionResult> UnblacklistUser(Guid userId)
    {
        try
        {
            var result = await _userManagementService.UnblacklistUserAsync(userId);
            if (!result)
            {
                return NotFound(new { message = "User not found" });
            }

            return Ok(new { message = "User unblacklisted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unblacklisting user {UserId}", userId);
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

            var affectedCount = await _userManagementService.BulkOperationAsync(bulkOperation);
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
}
