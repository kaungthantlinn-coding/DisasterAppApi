using DisasterApp.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace DisasterApp.WebApi.Authorization;
public class RoleAuthorizationHandler : AuthorizationHandler<RoleRequirement>
{
    private readonly IRoleService _roleService;
    private readonly ILogger<RoleAuthorizationHandler> _logger;

    public RoleAuthorizationHandler(IRoleService roleService, ILogger<RoleAuthorizationHandler> logger)
    {
        _roleService = roleService ?? throw new ArgumentNullException(nameof(roleService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        RoleRequirement requirement)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            _logger.LogWarning("User ID claim not found or invalid");
            context.Fail();
            return;
        }

        try
        {
            var userRoles = await _roleService.GetUserRolesAsync(userId);
            var userRoleNames = userRoles.Select(r => r.Name.ToLower()).ToList();

            var hasRequiredRole = requirement.AllowedRoles
                .Any(role => userRoleNames.Contains(role.ToLower()));

            if (hasRequiredRole)
            {
                context.Succeed(requirement);
                _logger.LogDebug("User {UserId} authorized with roles: {Roles}", userId, string.Join(", ", userRoleNames));
            }
            else
            {
                _logger.LogWarning("User {UserId} does not have required roles. User roles: {UserRoles}, Required: {RequiredRoles}", 
                    userId, string.Join(", ", userRoleNames), string.Join(", ", requirement.AllowedRoles));
                context.Fail();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking user roles for user {UserId}", userId);
            context.Fail();
        }
    }
}

public class RoleRequirement : IAuthorizationRequirement
{
    public IEnumerable<string> AllowedRoles { get; }

    public RoleRequirement(params string[] allowedRoles)
    {
        AllowedRoles = allowedRoles ?? throw new ArgumentNullException(nameof(allowedRoles));
    }
}