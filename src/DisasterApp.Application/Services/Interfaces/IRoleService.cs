using DisasterApp.Domain.Entities;

namespace DisasterApp.Application.Services.Interfaces;

public interface IRoleService
{
    Task<IEnumerable<Role>> GetAllRolesAsync();
    Task<Role?> GetRoleByNameAsync(string roleName);
    Task<Role?> GetDefaultRoleAsync();
    Task AssignRoleToUserAsync(Guid userId, string roleName, Guid? performedByUserId = null, string? performedByUserName = null, string? ipAddress = null, string? userAgent = null);
    Task AssignDefaultRoleToUserAsync(Guid userId);//
    Task<IEnumerable<Role>> GetUserRolesAsync(Guid userId);
    Task<bool> UserHasRoleAsync(Guid userId, string roleName);
    Task RemoveRoleFromUserAsync(Guid userId, string roleName, Guid? performedByUserId = null, string? performedByUserName = null, string? ipAddress = null, string? userAgent = null);
    Task RemoveRoleFromUserDirectAsync(Guid userId, string roleName, Guid? performedByUserId = null, string? performedByUserName = null, string? ipAddress = null, string? userAgent = null);
    Task<bool> CanRemoveRoleAsync(Guid userId, string roleName);
    Task<int> GetAdminCountAsync();
    Task<bool> IsLastAdminAsync(Guid userId);
    Task<int> CleanupDuplicateUserRolesAsync();
    Task<bool> FixRoleNamesAsync();
    Task ReplaceUserRolesAsync(Guid userId, IEnumerable<string> roleNames, Guid? performedByUserId = null, string? performedByUserName = null, string? ipAddress = null, string? userAgent = null);
    Task<Role?> GetSuperAdminRoleAsync();
    Task<bool> IsSuperAdminAsync(Guid userId);
    Task<int> GetSuperAdminCountAsync();
    Task AssignSuperAdminRoleAsync(Guid userId, Guid? performedByUserId = null, string? performedByUserName = null, string? ipAddress = null, string? userAgent = null);
}