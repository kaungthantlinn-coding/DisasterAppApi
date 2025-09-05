using DisasterApp.Application.Services.Interfaces;
using DisasterApp.Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DisasterApp.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthDiagnosticsController : ControllerBase
{
    private readonly IRoleService _roleService;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<AuthDiagnosticsController> _logger;

    public AuthDiagnosticsController(
        IRoleService roleService,
        IUserRepository userRepository,
        ILogger<AuthDiagnosticsController> logger)
    {
        _roleService = roleService;
        _userRepository = userRepository;
        _logger = logger;
    }

    [HttpGet("check-auth")]
    public IActionResult CheckAuth()
    {
        try
        {
            var isAuthenticated = User.Identity?.IsAuthenticated ?? false;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            
            var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            
            return Ok(new
            {
                IsAuthenticated = isAuthenticated,
                UserId = userId,
                UserEmail = userEmail,
                UserName = userName,
                Roles = roles,
                AllClaims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking authentication");
            return StatusCode(500, new { message = "Error checking authentication", error = ex.Message });
        }
    }

    [HttpGet("roles")]
    public async Task<IActionResult> GetAllRoles()
    {
        try
        {
            var roles = await _roleService.GetAllRolesAsync();
            return Ok(roles.Select(r => new { r.RoleId, r.Name, r.Description }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving roles");
            return StatusCode(500, new { message = "Error retrieving roles", error = ex.Message });
        }
    }

    [HttpGet("user-roles/{userId}")]
    public async Task<IActionResult> GetUserRoles(Guid userId)
    {
        try
        {
            var userRoles = await _roleService.GetUserRolesAsync(userId);
            var roleNames = await _userRepository.GetUserRolesAsync(userId);
            
            return Ok(new
            {
                UserRoleObjects = userRoles.Select(r => new { r.RoleId, r.Name, r.Description }),
                UserRoleNames = roleNames
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user roles for {UserId}", userId);
            return StatusCode(500, new { message = "Error retrieving user roles", error = ex.Message });
        }
    }

    [HttpGet("test-auth")]
    [Authorize]
    public async Task<IActionResult> TestBasicAuth()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                return BadRequest(new { message = "Invalid user ID in token" });
            }

            var userRoles = await _roleService.GetUserRolesAsync(userGuid);
            return Ok(new
            {
                message = "Basic auth successful",
                userId = userGuid,
                roles = userRoles.Select(r => r.Name).ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in test basic auth");
            return StatusCode(500, new { message = "Error in test basic auth", error = ex.Message });
        }
    }

    [HttpGet("test-admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public IActionResult TestAdminAuth()
    {
        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        return Ok(new
        {
            message = "Admin auth successful",
            userRoles = roles
        });
    }

    [HttpPost("create-test-token")]
    public async Task<IActionResult> CreateTestToken([FromBody] CreateTestTokenRequest request)
    {
        try
        {
            var userId = Guid.Parse(request.UserId);
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            var userRoles = await _roleService.GetUserRolesAsync(userId);
            var roleNames = userRoles.Select(r => r.Name).ToList();

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("ThisIsAVeryLongSecretKeyForJWTTokenGenerationThatShouldBeAtLeast32Characters");

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new(ClaimTypes.Name, user.Name),
                new(ClaimTypes.Email, user.Email)
            };

            claims.AddRange(roleNames.Select(role => new Claim(ClaimTypes.Role, role)));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(60),
                Issuer = "DisasterApp",
                Audience = "DisasterAppUsers",
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(new
            {
                accessToken = tokenString,
                user = new { user.UserId, user.Name, user.Email },
                roles = roleNames
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating test token");
            return StatusCode(500, new { message = "Error creating test token", error = ex.Message });
        }
    }
}

public class CreateTestTokenRequest
{
    public string UserId { get; set; } = string.Empty;
}