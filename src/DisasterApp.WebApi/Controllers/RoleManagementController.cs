using DisasterApp.Application.DTOs;
using DisasterApp.Application.Services.Interfaces;
using DisasterApp.WebApi.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DisasterApp.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RoleManagementController : ControllerBase
{
    private readonly IRoleManagementService _roleManagementService;
    private readonly ILogger<RoleManagementController> _logger;

    public RoleManagementController(
        IRoleManagementService roleManagementService,
        ILogger<RoleManagementController> logger)//
    {
        _roleManagementService = roleManagementService;
        _logger = logger;
    }

    [HttpGet]
    [SuperAdminOrAdmin]
    public async Task<ActionResult<RoleManagementResponse>> GetRoles(
        [FromQuery] string? search = null,
        [FromQuery] string? filter = null)
    {
        try
        {
            var response = await _roleManagementService.GetRolesAsync(search, filter);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving roles with search: {Search}, filter: {Filter}", search, filter);
            return StatusCode(500, new { message = "Internal server error", type = "SystemError" });
        }
    }


    [HttpGet("{id}")]
    [SuperAdminOrAdmin]
    public async Task<ActionResult<RoleDto>> GetRole(Guid id)
    {
        try
        {
            var role = await _roleManagementService.GetRoleByIdAsync(id);
            if (role == null)
            {
                return NotFound(new { message = $"Role with ID {id} not found", type = "NotFound" });
            }

            return Ok(role);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving role with ID: {RoleId}", id);
            return StatusCode(500, new { message = "Internal server error", type = "SystemError" });
        }
    }


    [HttpPost]
    [SuperAdminOrAdmin]
    public async Task<ActionResult<RoleDto>> CreateRole(CreateRoleDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new { message = "Validation failed", errors, type = "ValidationError" });
            }

            var currentUser = User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
            var createdRole = await _roleManagementService.CreateRoleAsync(dto, currentUser);

            return CreatedAtAction(nameof(GetRole), new { id = createdRole.Id }, createdRole);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Role creation failed: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message, type = "ValidationError" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating role: {RoleName}", dto.Name);
            return StatusCode(500, new { message = "Internal server error", type = "SystemError" });
        }
    }


    [HttpPut("{id}")]
    [SuperAdminOrAdmin]
    public async Task<ActionResult<RoleDto>> UpdateRole(Guid id, UpdateRoleDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new { message = "Validation failed", errors, type = "ValidationError" });
            }

            var currentUser = User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
            var updatedRole = await _roleManagementService.UpdateRoleAsync(id, dto, currentUser);

            return Ok(updatedRole);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Role update failed: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message, type = "ValidationError" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Role update failed due to business rule: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message, type = "BusinessRuleViolation" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating role with ID: {RoleId}", id);
            return StatusCode(500, new { message = "Internal server error", type = "SystemError" });
        }
    }

    [HttpDelete("{id}")]
    [SuperAdminOrAdmin]
    public async Task<ActionResult> DeleteRole(Guid id)
    {
        try
        {
            // Get current user information for audit log
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
            Guid? adminUserId = currentUserId != null ? Guid.Parse(currentUserId) : null;
            
            var deleted = await _roleManagementService.DeleteRoleAsync(id, userName, adminUserId);
            if (!deleted)
            {
                return NotFound(new { message = $"Role with ID {id} not found", type = "NotFound" });
            }

            return Ok(new { message = "Role deleted successfully", roleId = id, timestamp = DateTime.UtcNow });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Role deletion failed: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message, type = "ValidationError" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Role deletion failed due to business rule: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message, type = "BusinessRuleViolation" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting role with ID: {RoleId}", id);
            return StatusCode(500, new { message = "Internal server error", type = "SystemError" });
        }
    }

  
    [HttpGet("{id}/users")]
    [SuperAdminOrAdmin]
    public async Task<ActionResult<List<RoleUserDto>>> GetRoleUsers(Guid id)
    {
        try
        {
            var users = await _roleManagementService.GetRoleUsersAsync(id);
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users for role ID: {RoleId}", id);
            return StatusCode(500, new { message = "Internal server error", type = "SystemError" });
        }
    }
}
