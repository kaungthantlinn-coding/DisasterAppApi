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
        // Legacy method - convert Guid to string and try to match by name for backward compatibility
        return await _context.Roles
            .Include(r => r.Users)
            .FirstOrDefaultAsync(r => r.Name == roleId.ToString());
    }

    // New simplified role management methods
    public async Task<Role?> GetRoleByIdAsync(Guid id)
    {
        return await _context.Roles
            .Include(r => r.Users)
            .FirstOrDefaultAsync(r => r.RoleId == id);
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

 
        var totalCount = await query.CountAsync();

        // Apply sorting
        query = sortBy.ToLower() switch
        {
            "name" => sortDirection.ToLower() == "desc" ? query.OrderByDescending(r => r.Name) : query.OrderBy(r => r.Name),
            // createdat and lastmodified sorting removed as these properties don't exist on Role entity
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
        //LastModified property removed as it doesn't exist on Role entity
        _context.Roles.Update(role);
        await _context.SaveChangesAsync();
        return role;
    }

    public async Task<bool> DeleteAsync(Guid roleId)
    {
        // Legacy method - find by name for backward compatibility
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleId.ToString());
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
        return await _context.Roles.AnyAsync(r => r.Name == roleId.ToString());
    }

    public async Task<bool> ExistsAsync(string name, Guid excludeRoleId)
    {
        return await _context.Roles.AnyAsync(r => r.Name.ToLower() == name.ToLower() && r.Name != excludeRoleId.ToString());
    }

    public async Task<bool> IsRoleAssignedToUsersAsync(Guid roleId)
    {
        return await _context.Users.AnyAsync(u => u.Roles.Any(r => r.Name == roleId.ToString()));
    }

    public async Task<int> GetTotalRolesCountAsync()
    {
        return await _context.Roles.CountAsync();
    }

    // Note: GetActiveRolesCountAsync and GetSystemRolesCountAsync methods removed
    // as IsActive and IsSystem properties don't exist on Role entity



    public async Task<List<Role>> GetRolesByIdsAsync(List<Guid> roleIds)
    {
        var roleNames = roleIds.Select(id => id.ToString()).ToList();
        return await _context.Roles
            .Where(r => roleNames.Contains(r.Name))
            .ToListAsync();
    }

    public async Task<List<Role>> GetRolesByNamesAsync(List<string> names)
    {
        var lowerNames = names.Select(n => n.ToLower()).ToList();
        return await _context.Roles
            .Where(r => lowerNames.Contains(r.Name.ToLower()))
            .ToListAsync();
    }

    public async Task<IEnumerable<Role>> GetAllRolesAsync()
    {
        return await _context.Roles
            .Include(r => r.Users)
            .OrderBy(r => r.Name)
            .ToListAsync();
    }

    public async Task<Role> CreateRoleAsync(Role role)
    {
        role.CreatedAt = DateTime.UtcNow;
        role.UpdatedAt = DateTime.UtcNow;
        _context.Roles.Add(role);
        await _context.SaveChangesAsync();
        return role;
    }

    public async Task<Role> UpdateRoleAsync(Role role)
    {
        role.UpdatedAt = DateTime.UtcNow;
        _context.Roles.Update(role);
        await _context.SaveChangesAsync();
        return role;
    }

    public async Task<bool> DeleteRoleAsync(Guid id)
    {
        var role = await _context.Roles.FindAsync(id);
        if (role == null)
            return false;

        _context.Roles.Remove(role);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RoleExistsAsync(string name, Guid? excludeId = null)
    {
        var query = _context.Roles.Where(r => r.Name.ToLower() == name.ToLower());
        
        if (excludeId.HasValue)
        {
            query = query.Where(r => r.RoleId != excludeId.Value);
        }
        
        return await query.AnyAsync();
    }
}
