using DisasterApp.Domain.Entities;
using DisasterApp.Infrastructure.Data;
using DisasterApp.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DisasterApp.Infrastructure.Repositories.Implementations;

public class PasswordResetTokenRepository : IPasswordResetTokenRepository
{
    private readonly DisasterDbContext _context;

    public PasswordResetTokenRepository(DisasterDbContext context)
    {
        _context = context;
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
        var passwordResetToken = await _context.PasswordResetTokens
            .FirstOrDefaultAsync(prt => prt.Token == token);

        return passwordResetToken != null && 
               passwordResetToken.ExpiredAt > DateTime.UtcNow && 
               !passwordResetToken.IsUsed;
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
