using DisasterApp.Domain.Entities;
using DisasterApp.Infrastructure.Data;
using DisasterApp.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DisasterApp.Infrastructure.Repositories.Implementations;

public class UserBlacklistRepository : IUserBlacklistRepository
{
    private readonly DisasterDbContext _context;
    private readonly ILogger<UserBlacklistRepository> _logger;

    public UserBlacklistRepository(DisasterDbContext context, ILogger<UserBlacklistRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<UserBlacklist> CreateAsync(UserBlacklist userBlacklist)
    {
        try
        {
            _context.UserBlacklists.Add(userBlacklist);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Created blacklist record {Id} for user {UserId}", userBlacklist.Id, userBlacklist.UserId);
            return userBlacklist;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating blacklist record for user {UserId}", userBlacklist.UserId);
            throw;
        }
    }

    public async Task<UserBlacklist?> GetActiveBlacklistAsync(Guid userId)
    {
        try
        {
            return await _context.UserBlacklists
                .Include(ub => ub.BlacklistedByUser)
                .Include(ub => ub.UnblacklistedByUser)
                .FirstOrDefaultAsync(ub => ub.UserId == userId && ub.IsActive);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active blacklist for user {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<UserBlacklist>> GetBlacklistHistoryAsync(Guid userId)
    {
        try
        {
            return await _context.UserBlacklists
                .Include(ub => ub.BlacklistedByUser)
                .Include(ub => ub.UnblacklistedByUser)
                .Where(ub => ub.UserId == userId)
                .OrderByDescending(ub => ub.BlacklistedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving blacklist history for user {UserId}", userId);
            throw;
        }
    }

    public async Task<UserBlacklist?> GetByIdAsync(Guid id)
    {
        try
        {
            return await _context.UserBlacklists
                .Include(ub => ub.User)
                .Include(ub => ub.BlacklistedByUser)
                .Include(ub => ub.UnblacklistedByUser)
                .FirstOrDefaultAsync(ub => ub.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving blacklist record {Id}", id);
            throw;
        }
    }

    public async Task<UserBlacklist> UpdateAsync(UserBlacklist userBlacklist)
    {
        try
        {
            userBlacklist.UpdatedAt = DateTime.UtcNow;
            _context.UserBlacklists.Update(userBlacklist);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Updated blacklist record {Id}", userBlacklist.Id);
            return userBlacklist;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating blacklist record {Id}", userBlacklist.Id);
            throw;
        }
    }

    public async Task<bool> HasActiveBlacklistAsync(Guid userId)
    {
        try
        {
            return await _context.UserBlacklists
                .AnyAsync(ub => ub.UserId == userId && ub.IsActive);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking active blacklist for user {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<UserBlacklist>> GetRecentBlacklistsAsync(int count = 10)
    {
        try
        {
            return await _context.UserBlacklists
                .Include(ub => ub.User)
                .Include(ub => ub.BlacklistedByUser)
                .OrderByDescending(ub => ub.BlacklistedAt)
                .Take(count)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving recent blacklists");
            throw;
        }
    }

    public async Task<int> GetBlacklistCountAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        try
        {
            var query = _context.UserBlacklists.AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(ub => ub.BlacklistedAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(ub => ub.BlacklistedAt <= toDate.Value);

            return await query.CountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting blacklist count");
            throw;
        }
    }
}