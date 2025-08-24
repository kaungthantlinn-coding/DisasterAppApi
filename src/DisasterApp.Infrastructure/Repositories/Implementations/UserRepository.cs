using DisasterApp.Domain.Entities;
using DisasterApp.Infrastructure.Data;
using DisasterApp.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DisasterApp.Infrastructure.Repositories.Implementations;

public class UserRepository : IUserRepository
{
    private readonly DisasterDbContext _context;

    public UserRepository(DisasterDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetByIdAsync(Guid userId)
    {
        return await _context.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.UserId == userId);
    }

    public async Task<User?> GetByAuthProviderAsync(string authProvider, string authId)
    {
        return await _context.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.AuthProvider == authProvider && u.AuthId == authId);
    }

    public async Task<User> CreateAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<User> UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<bool> ExistsAsync(string email)
    {
        return await _context.Users.AnyAsync(u => u.Email == email);
    }

    public async Task<List<string>> GetUserRolesAsync(Guid userId)
    {
        return await _context.Users
            .Where(u => u.UserId == userId)
            .SelectMany(u => u.Roles)
            .Select(r => r.Name)
            .ToListAsync();
    }

    public async Task<(List<User> Users, int TotalCount)> GetUsersAsync(
        int pageNumber,
        int pageSize,
        string? searchTerm = null,
        string? role = null,
        bool? isBlacklisted = null,
        string? authProvider = null,
        DateTime? createdAfter = null,
        DateTime? createdBefore = null,
        string sortBy = "CreatedAt",
        string sortDirection = "desc")
    {
        var query = _context.Users
            .Include(u => u.Roles)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(searchTerm))
        {
            var searchTermLower = searchTerm.ToLower();
            query = query.Where(u => u.Name.ToLower().Contains(searchTermLower) ||
                                   u.Email.ToLower().Contains(searchTermLower));
        }

        if (!string.IsNullOrEmpty(role))
        {
            query = query.Where(u => u.Roles.Any(r => r.Name.ToLower() == role.ToLower()));
        }

        if (isBlacklisted.HasValue)
        {
            query = query.Where(u => u.IsBlacklisted == isBlacklisted.Value);
        }

        if (!string.IsNullOrEmpty(authProvider))
        {
            query = query.Where(u => u.AuthProvider.ToLower() == authProvider.ToLower());
        }

        if (createdAfter.HasValue)
        {
            query = query.Where(u => u.CreatedAt >= createdAfter.Value);
        }

        if (createdBefore.HasValue)
        {
            query = query.Where(u => u.CreatedAt <= createdBefore.Value);
        }

        // Apply sorting
        query = sortBy.ToLower() switch
        {
            "name" => sortDirection.ToLower() == "desc"
                ? query.OrderByDescending(u => u.Name)
                : query.OrderBy(u => u.Name),
            "email" => sortDirection.ToLower() == "desc"
                ? query.OrderByDescending(u => u.Email)
                : query.OrderBy(u => u.Email),
            "createdat" => sortDirection.ToLower() == "desc"
                ? query.OrderByDescending(u => u.CreatedAt)
                : query.OrderBy(u => u.CreatedAt),
            _ => query.OrderByDescending(u => u.CreatedAt)
        };

        var totalCount = await query.CountAsync();

        var users = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (users, totalCount);
    }

    public async Task<User?> GetUserWithDetailsAsync(Guid userId)
    {
        return await _context.Users
            .Include(u => u.Roles)
            .Include(u => u.DisasterReportUsers)
            .Include(u => u.SupportRequests)
            .Include(u => u.DonationUsers)
            .Include(u => u.Organizations)
            .FirstOrDefaultAsync(u => u.UserId == userId);
    }

    public async Task<bool> DeleteUserAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(Guid userId)
    {
        return await _context.Users.AnyAsync(u => u.UserId == userId);
    }

    public async Task<bool> ExistsAsync(string email, Guid excludeUserId)
    {
        return await _context.Users.AnyAsync(u => u.Email == email && u.UserId != excludeUserId);
    }

    public async Task<int> GetTotalUsersCountAsync()
    {
        return await _context.Users.CountAsync();
    }

    public async Task<int> GetActiveUsersCountAsync()
    {
        return await _context.Users.CountAsync(u => u.IsBlacklisted != true);
    }

    public async Task<int> GetSuspendedUsersCountAsync()
    {
        return await _context.Users.CountAsync(u => u.IsBlacklisted == true);
    }

    public async Task<int> GetAdminUsersCountAsync()
    {
        return await _context.Users
            .Where(u => u.Roles.Any(r => r.Name.ToLower() == "admin"))
            .CountAsync();
    }

    public async Task<List<User>> GetUsersByIdsAsync(List<Guid> userIds)
    {
        return await _context.Users
            .Include(u => u.Roles)
            .Where(u => userIds.Contains(u.UserId))
            .ToListAsync();
    }

    public async Task<bool> BulkUpdateUsersAsync(List<User> users)
    {
        try
        {
            _context.Users.UpdateRange(users);
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<(int DisasterReports, int SupportRequests, int Donations, int Organizations)> GetUserStatisticsAsync(Guid userId)
    {
        var user = await _context.Users
            .Include(u => u.DisasterReportUsers)
            .Include(u => u.SupportRequests)
            .Include(u => u.DonationUsers)
            .Include(u => u.Organizations)
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user == null)
        {
            return (0, 0, 0, 0);
        }

        return (
            user.DisasterReportUsers.Count,
            user.SupportRequests.Count,
            user.DonationUsers.Count,
            user.Organizations.Count
        );
    }

    // Role management methods
    public async Task<int> GetUserCountByRoleAsync(Guid roleId)
    {
        return await _context.Users
            .Where(u => u.Roles.Any(r => r.RoleId == roleId))
            .CountAsync();
    }

    public async Task<List<User>> GetUsersByRoleAsync(Guid roleId)
    {
        return await _context.Users
            .Where(u => u.Roles.Any(r => r.RoleId == roleId))
            .OrderBy(u => u.Name)
            .ToListAsync();
    }
}