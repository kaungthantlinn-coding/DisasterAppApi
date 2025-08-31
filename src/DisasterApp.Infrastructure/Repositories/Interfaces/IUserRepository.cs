using DisasterApp.Domain.Entities;

namespace DisasterApp.Infrastructure.Repositories.Interfaces;

public interface IUserRepository
{
    // Existing methods
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(Guid userId);
    Task<User?> GetByAuthProviderAsync(string authProvider, string authId);
    Task<User> CreateAsync(User user);
    Task<User> UpdateAsync(User user);
    Task<bool> ExistsAsync(string email);
    Task<List<string>> GetUserRolesAsync(Guid userId);

    // User management methods
    Task<(List<User> Users, int TotalCount)> GetUsersAsync(
        int pageNumber,
        int pageSize,
        string? searchTerm = null,//
        string? role = null,
        bool? isBlacklisted = null,
        string? authProvider = null,
        DateTime? createdAfter = null,
        DateTime? createdBefore = null,
        string sortBy = "CreatedAt",
        string sortDirection = "desc");
    Task<User?> GetUserWithDetailsAsync(Guid userId);
    Task<bool> DeleteUserAsync(Guid userId);
    Task<bool> ExistsAsync(Guid userId);
    Task<bool> ExistsAsync(string email, Guid excludeUserId);
    Task<int> GetTotalUsersCountAsync();
    Task<int> GetActiveUsersCountAsync();
    Task<int> GetSuspendedUsersCountAsync();
    Task<int> GetAdminUsersCountAsync();
    Task<List<User>> GetUsersByIdsAsync(List<Guid> userIds);
    Task<bool> BulkUpdateUsersAsync(List<User> users);
    Task<(int DisasterReports, int SupportRequests, int Donations, int Organizations)> GetUserStatisticsAsync(Guid userId);
    
    // Role management methods
    Task<int> GetUserCountByRoleAsync(Guid roleId);
    Task<List<User>> GetUsersByRoleAsync(Guid roleId);
}