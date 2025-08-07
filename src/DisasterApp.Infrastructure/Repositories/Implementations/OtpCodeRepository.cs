using DisasterApp.Domain.Entities;
using DisasterApp.Infrastructure.Data;
using DisasterApp.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DisasterApp.Infrastructure.Repositories.Implementations;
public class OtpCodeRepository : IOtpCodeRepository
{
    private readonly DisasterDbContext _context;

    public OtpCodeRepository(DisasterDbContext context)
    {
        _context = context;
    }

    public async Task<OtpCode> CreateAsync(OtpCode otpCode)
    {
        _context.OtpCodes.Add(otpCode);
        await _context.SaveChangesAsync();
        return otpCode;
    }

    public async Task<OtpCode?> GetByIdAsync(Guid id)
    {
        return await _context.OtpCodes
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<OtpCode?> GetByUserAndCodeAsync(Guid userId, string code, string type)
    {
        return await _context.OtpCodes
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.UserId == userId && 
                                    o.Code == code && 
                                    o.Type == type && 
                                    o.UsedAt == null && 
                                    o.ExpiresAt > DateTime.UtcNow);
    }

    public async Task<List<OtpCode>> GetActiveCodesAsync(Guid userId, string? type = null)
    {
        var query = _context.OtpCodes
            .Where(o => o.UserId == userId && 
                       o.UsedAt == null && 
                       o.ExpiresAt > DateTime.UtcNow);

        if (!string.IsNullOrEmpty(type))
        {
            query = query.Where(o => o.Type == type);
        }

        return await query.ToListAsync();
    }

    public async Task<OtpCode> UpdateAsync(OtpCode otpCode)
    {
        _context.OtpCodes.Update(otpCode);
        await _context.SaveChangesAsync();
        return otpCode;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var otpCode = await _context.OtpCodes.FindAsync(id);
        if (otpCode == null)
            return false;

        _context.OtpCodes.Remove(otpCode);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> DeleteExpiredAsync()
    {
        var expiredCodes = await _context.OtpCodes
            .Where(o => o.ExpiresAt <= DateTime.UtcNow || o.UsedAt != null)
            .ToListAsync();

        _context.OtpCodes.RemoveRange(expiredCodes);
        await _context.SaveChangesAsync();
        return expiredCodes.Count;
    }

    public async Task<int> DeleteByUserAsync(Guid userId, string? type = null)
    {
        var query = _context.OtpCodes.Where(o => o.UserId == userId);

        if (!string.IsNullOrEmpty(type))
        {
            query = query.Where(o => o.Type == type);
        }

        var codes = await query.ToListAsync();
        _context.OtpCodes.RemoveRange(codes);
        await _context.SaveChangesAsync();
        return codes.Count;
    }

    public async Task<int> GetActiveCountAsync(Guid userId, string? type = null)
    {
        var query = _context.OtpCodes
            .Where(o => o.UserId == userId && 
                       o.UsedAt == null && 
                       o.ExpiresAt > DateTime.UtcNow);

        if (!string.IsNullOrEmpty(type))
        {
            query = query.Where(o => o.Type == type);
        }

        return await query.CountAsync();
    }
}
