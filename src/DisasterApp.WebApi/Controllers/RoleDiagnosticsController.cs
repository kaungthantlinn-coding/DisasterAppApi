using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DisasterApp.Application.Services.Interfaces;
using DisasterApp.WebApi.Authorization;
using DisasterApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DisasterApp.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RoleDiagnosticsController : ControllerBase
{
    private readonly DisasterDbContext _context;
    private readonly IRoleService _roleService;
    private readonly ILogger<RoleDiagnosticsController> _logger;

    public RoleDiagnosticsController(
        DisasterDbContext context,
        IRoleService roleService,
        ILogger<RoleDiagnosticsController> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _roleService = roleService ?? throw new ArgumentNullException(nameof(roleService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet("roles-status")]
    [AdminOnly]
    public async Task<IActionResult> GetRolesStatus()
    {
        try
        {
            var roles = await _context.Roles.ToListAsync();
            var userRolesCount = await _context.Users
                .Include(u => u.Roles)
                .Select(u => u.Roles.Count)
                .SumAsync();

            return Ok(new
            {
                roles = roles.Select(r => new { r.RoleId, r.Name }),
                totalRoles = roles.Count,
                userRoleAssignments = userRolesCount,
                message = "Role diagnostics completed successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting roles status");
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    [HttpGet("user-roles/{userId}")]
    [AdminOnly]
    public async Task<IActionResult> GetUserRolesDiagnostics(Guid userId)
    {
        try
        {
            var user = await _context.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                return NotFound(new { error = "User not found", userId });
            }

            var roleDetails = user.Roles.Select(r => new
            {
                roleId = r.RoleId,
                roleName = r.Name,
                isValidRole = !string.IsNullOrEmpty(r.Name)
            }).ToList();

            return Ok(new
            {
                userId = user.UserId,
                userName = user.Name,
                userEmail = user.Email,
                totalRoles = user.Roles.Count,
                roles = roleDetails,
                hasValidRoles = roleDetails.All(r => r.isValidRole)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user roles diagnostics for user {UserId}", userId);
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    [HttpPost("fix-role-names")]
    [AdminOnly]
    public async Task<IActionResult> FixRoleNames()
    {
        try
        {
            var roles = await _context.Roles.ToListAsync();
            var fixedCount = 0;

            foreach (var role in roles)
            {
                if (string.IsNullOrEmpty(role.Name))
                {
                    role.Name = role.RoleId.ToString().ToLower().Contains("admin") ? "admin" :
                               role.RoleId.ToString().ToLower().Contains("user") ? "user" :
                               role.RoleId.ToString().ToLower().Contains("cj") ? "cj" :
                               "user";
                    fixedCount++;
                }
            }

            if (fixedCount > 0)
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Fixed {Count} role names", fixedCount);
            }

            return Ok(new
            {
                message = $"Fixed {fixedCount} role names",
                totalRoles = roles.Count,
                fixedRoles = fixedCount
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fixing role names");
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    [HttpPost("cleanup-duplicate-roles")]
    [AdminOnly]
    public async Task<IActionResult> CleanupDuplicateRoles()
    {
        try
        {
            await _roleService.CleanupDuplicateUserRolesAsync();
            return Ok(new { message = "Duplicate roles cleanup completed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up duplicate roles");
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    [HttpPost("cleanup-duplicates-public")]
    [AllowAnonymous]
    public async Task<IActionResult> CleanupDuplicateRolesPublic()
    {
        try
        {
            var duplicatesRemoved = await _roleService.CleanupDuplicateUserRolesAsync();
            return Ok(new { 
                message = "Duplicate roles cleanup completed successfully", 
                duplicatesRemoved = duplicatesRemoved 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up duplicate roles");
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    [HttpGet("roles-status-public")]
    [AllowAnonymous]
    public async Task<IActionResult> GetRolesStatusPublic()
    {
        try
        {
            var users = await _context.Users
            .Include(u => u.Roles)
            .ToListAsync();

        var result = users.Select(user => new
        {
            UserId = user.UserId,
            UserName = user.Name,
            Email = user.Email,
            Roles = user.Roles.Select(r => new
            {
                RoleId = r.RoleId,
                RoleName = r.Name
            }).ToList(),
            DuplicateRoles = user.Roles
                .GroupBy(r => r.RoleId)
                .Where(g => g.Count() > 1)
                .Select(g => new
                {
                    RoleId = g.Key,
                    RoleName = g.First().Name,
                    Count = g.Count()
                }).ToList()
        }).ToList();

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting roles status");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}