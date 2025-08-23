using DisasterApp.Domain.Entities;

namespace DisasterApp.Infrastructure.Repositories.Interfaces;

public interface IRoleRepository
{
    Task<Role?> GetByIdAsync(Guid roleId);
    Task<Role?> GetByNameAsync(string name);
    Task<List<Role>> GetAllAsync();
    Task<(List<Role> Roles, int TotalCount)> GetRolesAsync(
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
    // Note: GetActiveRolesCountAsync and GetSystemRolesCountAsync methods removed
    // as IsActive and IsSystem properties don't exist on Role entity
    Task<List<Role>> GetRolesByIdsAsync(List<Guid> roleIds);
    Task<List<Role>> GetRolesByNamesAsync(List<string> names);
}
