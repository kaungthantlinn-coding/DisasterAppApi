using DisasterApp.Domain.Entities;
using DisasterApp.Infrastructure.Data;
using DisasterApp.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DisasterApp.Infrastructure.Repositories.Implementations;

public class PasswordResetTokenRepository : IPasswordResetTokenRepository
{
    private readonly DisasterDbContext _context;
    private readonly ILogger<PasswordResetTokenRepository> _logger;

    public PasswordResetTokenRepository(DisasterDbContext context, ILogger<PasswordResetTokenRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PasswordResetToken?> GetByTokenAsync(string token)
    {
        return await _context.PasswordResetTokens
            .Include(prt => prt.User)
            .FirstOrDefaultAsync(prt => prt.Token == token);
    }

    public async Task<PasswordResetToken> CreateAsync(PasswordResetToken passwordResetToken)
    {
        _context.PasswordResetTokens.Add(passwordResetToken);
        await _context.SaveChangesAsync();
        return passwordResetToken;
    }

    public async Task<bool> DeleteAsync(string token)
    {
        var passwordResetToken = await _context.PasswordResetTokens
            .FirstOrDefaultAsync(prt => prt.Token == token);

        if (passwordResetToken == null)
            return false;

        _context.PasswordResetTokens.Remove(passwordResetToken);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAllUserTokensAsync(Guid userId)
    {
        var tokens = await _context.PasswordResetTokens
            .Where(prt => prt.UserId == userId)
            .ToListAsync();

        if (!tokens.Any())
            return false;

        _context.PasswordResetTokens.RemoveRange(tokens);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IsValidAsync(string token)
    {
        _logger.LogInformation("Checking token validity in repository: {Token}", token);

        var passwordResetToken = await _context.PasswordResetTokens
            .FirstOrDefaultAsync(prt => prt.Token == token);

        if (passwordResetToken == null)
        {
            _logger.LogWarning("Token not found in database: {Token}", token);
            return false;
        }

        var currentTime = DateTime.UtcNow;
        _logger.LogInformation("Token found. UserId: {UserId}, ExpiredAt: {ExpiredAt}, IsUsed: {IsUsed}, CurrentTime: {CurrentTime}",
            passwordResetToken.UserId, passwordResetToken.ExpiredAt, passwordResetToken.IsUsed, currentTime);

        var isValid = passwordResetToken.ExpiredAt > currentTime && !passwordResetToken.IsUsed;

        if (!isValid)
        {
            if (passwordResetToken.ExpiredAt <= currentTime)
                _logger.LogWarning("Token expired: {Token}, ExpiredAt: {ExpiredAt}, CurrentTime: {CurrentTime}",
                    token, passwordResetToken.ExpiredAt, currentTime);

            if (passwordResetToken.IsUsed)
                _logger.LogWarning("Token already used: {Token}", token);
        }
        else
        {
            _logger.LogInformation("Token is valid: {Token}", token);
        }

        return isValid;
    }

    public async Task<bool> MarkAsUsedAsync(string token)
    {
        var passwordResetToken = await _context.PasswordResetTokens
            .FirstOrDefaultAsync(prt => prt.Token == token);

        if (passwordResetToken == null)
            return false;

        passwordResetToken.IsUsed = true;
        await _context.SaveChangesAsync();
        return true;
    }
}
