using DisasterApp.Domain.Entities;
using DisasterApp.Infrastructure.Data;
using DisasterApp.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DisasterApp.Infrastructure.Repositories.Implementations;
public class OtpAttemptRepository : IOtpAttemptRepository
{
    private readonly DisasterDbContext _context;

    public OtpAttemptRepository(DisasterDbContext context)
    {
        _context = context;
    }

    public async Task<OtpAttempt> CreateAsync(OtpAttempt otpAttempt)
    {
        _context.OtpAttempts.Add(otpAttempt);
        await _context.SaveChangesAsync();
        return otpAttempt;
    }

    public async Task<List<OtpAttempt>> GetUserAttemptsAsync(Guid userId, DateTime since, string? attemptType = null)
    {
        var query = _context.OtpAttempts
            .Where(a => a.UserId == userId && a.AttemptedAt >= since);

        if (!string.IsNullOrEmpty(attemptType))
        {
            query = query.Where(a => a.AttemptType == attemptType);
        }

        return await query.OrderByDescending(a => a.AttemptedAt).ToListAsync();
    }

    public async Task<List<OtpAttempt>> GetIpAttemptsAsync(string ipAddress, DateTime since, string? attemptType = null)
    {
        var query = _context.OtpAttempts
            .Where(a => a.IpAddress == ipAddress && a.AttemptedAt >= since);

        if (!string.IsNullOrEmpty(attemptType))
        {
            query = query.Where(a => a.AttemptType == attemptType);
        }

        return await query.OrderByDescending(a => a.AttemptedAt).ToListAsync();
    }

    public async Task<List<OtpAttempt>> GetEmailAttemptsAsync(string email, DateTime since, string? attemptType = null)
    {
        var query = _context.OtpAttempts
            .Where(a => a.Email == email && a.AttemptedAt >= since);

        if (!string.IsNullOrEmpty(attemptType))
        {
            query = query.Where(a => a.AttemptType == attemptType);
        }

        return await query.OrderByDescending(a => a.AttemptedAt).ToListAsync();
    }

    public async Task<List<OtpAttempt>> GetFailedAttemptsAsync(Guid userId, DateTime since, string? attemptType = null)
    {
        var query = _context.OtpAttempts
            .Where(a => a.UserId == userId && a.AttemptedAt >= since && !a.Success);

        if (!string.IsNullOrEmpty(attemptType))
        {
            query = query.Where(a => a.AttemptType == attemptType);
        }

        return await query.OrderByDescending(a => a.AttemptedAt).ToListAsync();
    }

    public async Task<List<OtpAttempt>> GetFailedIpAttemptsAsync(string ipAddress, DateTime since, string? attemptType = null)
    {
        var query = _context.OtpAttempts
            .Where(a => a.IpAddress == ipAddress && a.AttemptedAt >= since && !a.Success);

        if (!string.IsNullOrEmpty(attemptType))
        {
            query = query.Where(a => a.AttemptType == attemptType);
        }

        return await query.OrderByDescending(a => a.AttemptedAt).ToListAsync();
    }

    public async Task<int> CountUserAttemptsAsync(Guid userId, DateTime since, string? attemptType = null, bool? successOnly = null)
    {
        var query = _context.OtpAttempts
            .Where(a => a.UserId == userId && a.AttemptedAt >= since);

        if (!string.IsNullOrEmpty(attemptType))
        {
            query = query.Where(a => a.AttemptType == attemptType);
        }

        if (successOnly.HasValue)
        {
            query = query.Where(a => a.Success == successOnly.Value);
        }

        return await query.CountAsync();
    }

    public async Task<int> CountIpAttemptsAsync(string ipAddress, DateTime since, string? attemptType = null, bool? successOnly = null)
    {
        var query = _context.OtpAttempts
            .Where(a => a.IpAddress == ipAddress && a.AttemptedAt >= since);

        if (!string.IsNullOrEmpty(attemptType))
        {
            query = query.Where(a => a.AttemptType == attemptType);
        }

        if (successOnly.HasValue)
        {
            query = query.Where(a => a.Success == successOnly.Value);
        }

        return await query.CountAsync();
    }

    public async Task<int> CountEmailAttemptsAsync(string email, DateTime since, string? attemptType = null, bool? successOnly = null)
    {
        var query = _context.OtpAttempts
            .Where(a => a.Email == email && a.AttemptedAt >= since);

        if (!string.IsNullOrEmpty(attemptType))
        {
            query = query.Where(a => a.AttemptType == attemptType);
        }

        if (successOnly.HasValue)
        {
            query = query.Where(a => a.Success == successOnly.Value);
        }

        return await query.CountAsync();
    }

    public async Task<int> DeleteOldAttemptsAsync(DateTime olderThan)
    {
        var oldAttempts = await _context.OtpAttempts
            .Where(a => a.AttemptedAt < olderThan)
            .ToListAsync();

        _context.OtpAttempts.RemoveRange(oldAttempts);
        await _context.SaveChangesAsync();
        return oldAttempts.Count;
    }
}
