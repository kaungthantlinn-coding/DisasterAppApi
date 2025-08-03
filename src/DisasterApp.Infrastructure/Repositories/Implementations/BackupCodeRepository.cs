using DisasterApp.Domain.Entities;
using DisasterApp.Infrastructure.Data;
using DisasterApp.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DisasterApp.Infrastructure.Repositories.Implementations;

/// <summary>
/// Repository implementation for backup code operations
/// </summary>
public class BackupCodeRepository : IBackupCodeRepository
{
    private readonly DisasterDbContext _context;

    public BackupCodeRepository(DisasterDbContext context)
    {
        _context = context;
    }

    public async Task<BackupCode> CreateAsync(BackupCode backupCode)
    {
        _context.BackupCodes.Add(backupCode);
        await _context.SaveChangesAsync();
        return backupCode;
    }

    public async Task<List<BackupCode>> CreateManyAsync(List<BackupCode> backupCodes)
    {
        _context.BackupCodes.AddRange(backupCodes);
        await _context.SaveChangesAsync();
        return backupCodes;
    }

    public async Task<BackupCode?> GetByIdAsync(Guid id)
    {
        return await _context.BackupCodes
            .Include(b => b.User)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<List<BackupCode>> GetUnusedCodesAsync(Guid userId)
    {
        return await _context.BackupCodes
            .Where(b => b.UserId == userId && b.UsedAt == null)
            .OrderBy(b => b.CreatedAt)
            .ToListAsync();
    }

    public async Task<BackupCode?> GetByUserAndHashAsync(Guid userId, string codeHash)
    {
        return await _context.BackupCodes
            .Include(b => b.User)
            .FirstOrDefaultAsync(b => b.UserId == userId && 
                                    b.CodeHash == codeHash && 
                                    b.UsedAt == null);
    }

    public async Task<BackupCode> UpdateAsync(BackupCode backupCode)
    {
        _context.BackupCodes.Update(backupCode);
        await _context.SaveChangesAsync();
        return backupCode;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var backupCode = await _context.BackupCodes.FindAsync(id);
        if (backupCode == null)
            return false;

        _context.BackupCodes.Remove(backupCode);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> DeleteByUserAsync(Guid userId)
    {
        var codes = await _context.BackupCodes
            .Where(b => b.UserId == userId)
            .ToListAsync();

        _context.BackupCodes.RemoveRange(codes);
        await _context.SaveChangesAsync();
        return codes.Count;
    }

    public async Task<int> GetUnusedCountAsync(Guid userId)
    {
        return await _context.BackupCodes
            .CountAsync(b => b.UserId == userId && b.UsedAt == null);
    }

    public async Task<bool> MarkAsUsedAsync(Guid id)
    {
        var backupCode = await _context.BackupCodes.FindAsync(id);
        if (backupCode == null || backupCode.UsedAt != null)
            return false;

        backupCode.MarkAsUsed();
        await _context.SaveChangesAsync();
        return true;
    }
}
