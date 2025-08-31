using DisasterApp.Domain.Entities;
using DisasterApp.Infrastructure.Data;
using DisasterApp.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace DisasterApp.Infrastructure.Repositories.Implementations;

public class RefreshTokenRepository : IRefreshTokenRepository//
{
    private readonly DisasterDbContext _context;
    private readonly ILogger<RefreshTokenRepository> _logger;

    public RefreshTokenRepository(DisasterDbContext context, ILogger<RefreshTokenRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        _logger.LogDebug("🔍 GetByTokenAsync - Searching for token with length: {Length}", token?.Length ?? 0);
        _logger.LogDebug("🔍 GetByTokenAsync - Token preview: {TokenPreview}", 
            token?.Length > 10 ? token[..10] + "..." : token);
        
        var result = await _context.RefreshTokens
            .Include(rt => rt.User)
            .ThenInclude(u => u.Roles)
            .FirstOrDefaultAsync(rt => rt.Token == token);
            
        if (result == null)
        {
            _logger.LogDebug("🔍 GetByTokenAsync - Token not found in database");
            
            // Log recent tokens for comparison
            var recentTokens = await _context.RefreshTokens
                .Include(rt => rt.User)
                .OrderByDescending(rt => rt.CreatedAt)
                .Take(3)
                .ToListAsync();
                
            _logger.LogDebug("🔍 GetByTokenAsync - Token not found. Recent tokens in DB: {RecentTokens}", 
                 string.Join(", ", recentTokens.Select(rt => $"ID: {rt.RefreshTokenId}, Token: {rt.Token[..10]}..., User: {rt.User?.Name}, Roles: [{string.Join(", ", rt.User?.Roles?.Select(r => r.Name) ?? [])}]")));
        }
        else
        {
            _logger.LogDebug("🔍 GetByTokenAsync - Token found: ExpiredAt: {ExpiredAt}", result.ExpiredAt);
        }
            
        return result;
    }

    public async Task<RefreshToken> CreateAsync(RefreshToken refreshToken)
    {
        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();
        return refreshToken;
    }

    public async Task<bool> DeleteAsync(string token)
    {
        var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == token);

        if (refreshToken == null)
            return false;

        _context.RefreshTokens.Remove(refreshToken);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAllUserTokensAsync(Guid userId)
    {
        var tokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId)
            .ToListAsync();

        if (!tokens.Any())
            return false;

        _context.RefreshTokens.RemoveRange(tokens);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IsValidAsync(string token)
    {
        var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == token);

        return refreshToken != null && refreshToken.ExpiredAt > DateTime.UtcNow;
    }
}