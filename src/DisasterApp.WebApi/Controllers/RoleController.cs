using DisasterApp.Application.Services.Interfaces;
using DisasterApp.WebApi.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace DisasterApp.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RoleController : ControllerBase
{
    private readonly IRoleService _roleService;
    private readonly ILogger<RoleController> _logger;

    public RoleController(IRoleService roleService, ILogger<RoleController> logger)
    {
        _roleService = roleService;
        _logger = logger;
    }

    [HttpGet]
    [AdminOnly]
    public async Task<IActionResult> GetAllRoles()
    {
        try
        {
            var roles = await _roleService.GetAllRolesAsync();
            return Ok(roles.Select(r => new { r.RoleId, r.Name }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving roles");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("my-roles")]
    public async Task<IActionResult> GetMyRoles()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return BadRequest("Invalid user ID");
            }

            var roles = await _roleService.GetUserRolesAsync(userId);
            return Ok(roles.Select(r => new { r.RoleId, r.Name }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user roles");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("assign")]
    [AdminOnly]
    public async Task<IActionResult> AssignRole([FromBody] AssignRoleRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new { message = "Validation failed", errors });
            }

            await _roleService.AssignRoleToUserAsync(request.UserId, request.RoleName);
            return Ok(new
            {
                message = $"Role '{request.RoleName}' assigned to user successfully",
                userId = request.UserId,
                roleName = request.RoleName,
                timestamp = DateTime.UtcNow
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Role assignment failed: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message, type = "ValidationError" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning role {RoleName} to user {UserId}", request.RoleName, request.UserId);
            return StatusCode(500, new { message = "Internal server error", type = "SystemError" });
        }
    }

    [HttpPost("assign-default")]
    [AdminOnly]
    public async Task<IActionResult> AssignDefaultRole([FromBody] AssignDefaultRoleRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _roleService.AssignDefaultRoleToUserAsync(request.UserId);
            return Ok(new { message = "Default role (user) assigned to user successfully" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning default role to user {UserId}", request.UserId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("remove")]
    [AdminOnly]
    public async Task<IActionResult> RemoveRole([FromBody] RemoveRoleRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new { message = "Validation failed", errors });
            }

            // Check if role can be removed before attempting
            var canRemove = await _roleService.CanRemoveRoleAsync(request.UserId, request.RoleName);
            if (!canRemove)
            {
                return BadRequest(new
                {
                    message = $"Cannot remove role '{request.RoleName}' from user",
                    type = "BusinessRuleViolation",
                    details = "This operation would violate system constraints"
                });
            }

            await _roleService.RemoveRoleFromUserAsync(request.UserId, request.RoleName);
            return Ok(new
            {
                message = $"Role '{request.RoleName}' removed from user successfully",
                userId = request.UserId,
                roleName = request.RoleName,
                timestamp = DateTime.UtcNow
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Role removal failed: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message, type = "ValidationError" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Role removal failed due to business rule: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message, type = "BusinessRuleViolation" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing role {RoleName} from user {UserId}", request.RoleName, request.UserId);
            return StatusCode(500, new { message = "Internal server error", type = "SystemError" });
        }
    }

    [HttpGet("user/{userId}/roles")]
    [AdminOrCj]
    public async Task<IActionResult> GetUserRoles(Guid userId)
    {
        try
        {
            var roles = await _roleService.GetUserRolesAsync(userId);
            return Ok(roles.Select(r => new { r.RoleId, r.Name }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving roles for user {UserId}", userId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("user/{userId}/has-role/{roleName}")]
    [AdminOrCj]
    public async Task<IActionResult> CheckUserRole(Guid userId, string roleName)
    {
        try
        {
            var hasRole = await _roleService.UserHasRoleAsync(userId, roleName);
            return Ok(new { userId, roleName, hasRole });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking role {RoleName} for user {UserId}", roleName, userId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("cleanup-duplicates")]
    [AdminOnly]
    public async Task<IActionResult> CleanupDuplicateRoles()
    {
        try
        {
            await _roleService.CleanupDuplicateUserRolesAsync();
            return Ok(new { message = "Duplicate role cleanup completed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during duplicate role cleanup");
            return StatusCode(500, new { message = "Internal server error", type = "SystemError" });
        }
    }

    [HttpPut("{userId}/roles")]
    [AdminOnly]
    public async Task<IActionResult> ReplaceUserRoles(Guid userId, [FromBody] ReplaceUserRolesRequest request)
    {
        try
        {
            if (userId != request.UserId)
            {
                return BadRequest("User ID in URL does not match request body");
            }

            var performedByUserIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var performedByUserId = string.IsNullOrEmpty(performedByUserIdString) ? (Guid?)null : Guid.TryParse(performedByUserIdString, out var guid) ? guid : (Guid?)null;
            var performedByUserName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var userAgent = Request.Headers["User-Agent"].ToString();

            await _roleService.ReplaceUserRolesAsync(
                userId,
                request.RoleNames,
                performedByUserId,
                performedByUserName,
                ipAddress,
                userAgent
            );

            return Ok(new { message = "User roles replaced successfully" });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for replacing user roles");
            return BadRequest(new { message = ex.Message, type = "ValidationError" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation for replacing user roles");
            return BadRequest(new { message = ex.Message, type = "BusinessLogicError" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error replacing user roles for user {UserId}", userId);
            return StatusCode(500, new { message = "Internal server error", type = "SystemError" });
        }
    }
}

public class AssignRoleRequest
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Role name must be between 2 and 50 characters")]
    public string RoleName { get; set; } = null!;
}

public class AssignDefaultRoleRequest
{
    [Required]
    public Guid UserId { get; set; }
}

public class RemoveRoleRequest
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Role name must be between 2 and 50 characters")]
    public string RoleName { get; set; } = null!;
}

public class ReplaceUserRolesRequest
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public ICollection<string> RoleNames { get; set; } = new List<string>();
}