using DisasterApp.Application.Services.Interfaces;
using DisasterApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
namespace DisasterApp.WebApi;

public class DuplicateRoleCleanupService
{
    private readonly IRoleService _roleService;
    private readonly ILogger<DuplicateRoleCleanupService> _logger;

    public DuplicateRoleCleanupService(IRoleService roleService, ILogger<DuplicateRoleCleanupService> logger)
    {
        _roleService = roleService;
        _logger = logger;
    }

    public async Task CleanupDuplicateRolesAsync()
    {
        try
        {
            _logger.LogInformation("Starting duplicate role cleanup...");
            var duplicatesRemoved = await _roleService.CleanupDuplicateUserRolesAsync();
            _logger.LogInformation("Duplicate role cleanup completed. Removed {Count} duplicates", duplicatesRemoved);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during duplicate role cleanup");
            throw;
        }
    }

    public async Task FixSpecificUserRolesAsync(Guid userId, string[] desiredRoles)
    {
        try
        {
            _logger.LogInformation("Fixing roles for user {UserId}", userId);
            await _roleService.ReplaceUserRolesAsync(userId, desiredRoles);
            _logger.LogInformation("Successfully updated roles for user {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fixing roles for user {UserId}", userId);
            throw;
        }
    }
}