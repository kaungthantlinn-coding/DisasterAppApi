using DisasterApp.Domain.Entities;
using DisasterApp.Infrastructure.Data;
using DisasterApp.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DisasterApp.Infrastructure.Repositories.Implementations;

public class RoleRepository : IRoleRepository
{
    private readonly DisasterDbContext _context;

    public RoleRepository(DisasterDbContext context)
    {
        _context = context;
    }

    public async Task<Role?> GetByIdAsync(Guid roleId)
    {
        return await _context.Roles
            .Include(r => r.Users)
            .FirstOrDefaultAsync(r => r.RoleId == roleId);
    }

    public async Task<Role?> GetByNameAsync(string name)
    {
        return await _context.Roles
            .Include(r => r.Users)
            .FirstOrDefaultAsync(r => r.Name.ToLower() == name.ToLower());
    }

    public async Task<List<Role>> GetAllAsync()
    {
        return await _context.Roles
            .Include(r => r.Users)
            .OrderBy(r => r.Name)
            .ToListAsync();
    }

    public async Task<(List<Role> Roles, int TotalCount)> GetRolesAsync(
        int pageNumber,
        int pageSize,
        string? searchTerm = null,
        string sortBy = "Name",
        string sortDirection = "asc")
    {
        var query = _context.Roles.Include(r => r.Users).AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(r => r.Name.Contains(searchTerm));
        }

        // Note: isSystem and isActive filters removed as these properties don't exist on Role entity
        // The Role entity only has RoleId, Name, and Users collection

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply sorting
        query = sortBy.ToLower() switch
        {
            "name" => sortDirection.ToLower() == "desc" ? query.OrderByDescending(r => r.Name) : query.OrderBy(r => r.Name),
            // Note: createdat and lastmodified sorting removed as these properties don't exist on Role entity
            _ => query.OrderBy(r => r.Name)
        };

        // Apply pagination
        var roles = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (roles, totalCount);
    }

    public async Task<Role> CreateAsync(Role role)
    {
        _context.Roles.Add(role);
        await _context.SaveChangesAsync();
        return role;
    }

    public async Task<Role> UpdateAsync(Role role)
    {
        // Note: LastModified property removed as it doesn't exist on Role entity
        _context.Roles.Update(role);
        await _context.SaveChangesAsync();
        return role;
    }

    public async Task<bool> DeleteAsync(Guid roleId)
    {
        var role = await _context.Roles.FindAsync(roleId);
        if (role == null)
            return false;

        _context.Roles.Remove(role);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(string name)
    {
        return await _context.Roles.AnyAsync(r => r.Name.ToLower() == name.ToLower());
    }

    public async Task<bool> ExistsAsync(Guid roleId)
    {
        return await _context.Roles.AnyAsync(r => r.RoleId == roleId);
    }

    public async Task<bool> ExistsAsync(string name, Guid excludeRoleId)
    {
        return await _context.Roles.AnyAsync(r => r.Name.ToLower() == name.ToLower() && r.RoleId != excludeRoleId);
    }

    public async Task<bool> IsRoleAssignedToUsersAsync(Guid roleId)
    {
        return await _context.Users.AnyAsync(u => u.Roles.Any(r => r.RoleId == roleId));
    }

    public async Task<int> GetTotalRolesCountAsync()
    {
        return await _context.Roles.CountAsync();
    }

    // Note: GetActiveRolesCountAsync and GetSystemRolesCountAsync methods removed
    // as IsActive and IsSystem properties don't exist on Role entity



    public async Task<List<Role>> GetRolesByIdsAsync(List<Guid> roleIds)
    {
        return await _context.Roles
            .Where(r => roleIds.Contains(r.RoleId))
            .ToListAsync();
    }

    public async Task<List<Role>> GetRolesByNamesAsync(List<string> names)
    {
        var lowerNames = names.Select(n => n.ToLower()).ToList();
        return await _context.Roles
            .Where(r => lowerNames.Contains(r.Name.ToLower()))
            .ToListAsync();
    }
}
