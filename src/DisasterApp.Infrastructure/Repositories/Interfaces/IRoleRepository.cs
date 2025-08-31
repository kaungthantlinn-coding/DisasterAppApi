using DisasterApp.Domain.Entities;

namespace DisasterApp.Infrastructure.Repositories.Interfaces;

public interface IRoleRepository
{
    // Legacy methods for backward compatibility
    Task<Role?> GetByIdAsync(Guid roleId);
    Task<Role?> GetByNameAsync(string name);
    Task<List<Role>> GetAllAsync();
    Task<(List<Role> Roles, int TotalCount)> GetRolesAsync(//
        int pageNumber,
        int pageSize,
        string? searchTerm = null,
        string sortBy = "Name",
        string sortDirection = "asc");
    Task<Role> CreateAsync(Role role);
    Task<Role> UpdateAsync(Role role);
    Task<bool> DeleteAsync(Guid roleId);
    Task<bool> ExistsAsync(string name);
    Task<bool> ExistsAsync(Guid roleId);
    Task<bool> ExistsAsync(string name, Guid excludeRoleId);
    Task<bool> IsRoleAssignedToUsersAsync(Guid roleId);
    Task<int> GetTotalRolesCountAsync();
    Task<List<Role>> GetRolesByIdsAsync(List<Guid> roleIds);
    Task<List<Role>> GetRolesByNamesAsync(List<string> names);

    // New simplified role management methods
    Task<Role?> GetRoleByIdAsync(Guid id);
    Task<IEnumerable<Role>> GetAllRolesAsync();
    Task<Role> CreateRoleAsync(Role role);
    Task<Role> UpdateRoleAsync(Role role);
    Task<bool> DeleteRoleAsync(Guid id);
    Task<bool> RoleExistsAsync(string name, Guid? excludeId = null);
}
