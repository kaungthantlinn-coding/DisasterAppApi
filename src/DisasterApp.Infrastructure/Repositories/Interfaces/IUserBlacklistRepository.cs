using DisasterApp.Domain.Entities;

namespace DisasterApp.Infrastructure.Repositories.Interfaces;

public interface IUserBlacklistRepository
{
    Task<UserBlacklist> CreateAsync(UserBlacklist userBlacklist);
    Task<UserBlacklist?> GetActiveBlacklistAsync(Guid userId);  
    Task<IEnumerable<UserBlacklist>> GetBlacklistHistoryAsync(Guid userId);
    Task<UserBlacklist?> GetByIdAsync(Guid id);
    Task<UserBlacklist> UpdateAsync(UserBlacklist userBlacklist);
    Task<bool> HasActiveBlacklistAsync(Guid userId);
    Task<IEnumerable<UserBlacklist>> GetRecentBlacklistsAsync(int count = 10);
    Task<int> GetBlacklistCountAsync(DateTime? fromDate = null, DateTime? toDate = null);
}