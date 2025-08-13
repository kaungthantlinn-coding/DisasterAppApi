using DisasterApp.Domain.Entities;
using DisasterApp.Infrastructure.Data;
using DisasterApp.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DisasterApp.Infrastructure.Repositories.Implementations;

public class SupportRequestRepository : ISupportRequestRepository
{
    private readonly DisasterDbContext _context;

    public SupportRequestRepository(DisasterDbContext context)
    {
        _context = context;
    }

    public async Task<SupportRequest?> GetByIdAsync(int id)
    {
        return await _context.SupportRequests
            .Include(sr => sr.Report)
            .Include(sr => sr.User)
            .Include(sr => sr.SupportTypes)
            .FirstOrDefaultAsync(sr => sr.Id == id);
    }

    public async Task<IEnumerable<SupportRequest>> GetAllAsync()
    {
        return await _context.SupportRequests
            .Include(sr => sr.Report)
            .Include(sr => sr.User)
            .Include(sr => sr.SupportTypes)
            .ToListAsync();
    }

    public async Task<IEnumerable<SupportRequest>> GetByUserIdAsync(Guid userId)
    {
        return await _context.SupportRequests
            .Include(sr => sr.Report)
            .Include(sr => sr.User)
            .Include(sr => sr.SupportTypes)
            .Where(sr => sr.UserId == userId)
            .ToListAsync();
    }

    public async Task<IEnumerable<SupportRequest>> GetByReportIdAsync(Guid reportId)
    {
        return await _context.SupportRequests
            .Include(sr => sr.Report)
            .Include(sr => sr.User)
            .Include(sr => sr.SupportTypes)
            .Where(sr => sr.ReportId == reportId)
            .ToListAsync();
    }

    public async Task<SupportRequest> CreateAsync(SupportRequest supportRequest)
    {
        _context.SupportRequests.Add(supportRequest);
        await _context.SaveChangesAsync();
        return supportRequest;
    }

    public async Task<SupportRequest> UpdateAsync(SupportRequest supportRequest)
    {
        _context.SupportRequests.Update(supportRequest);
        await _context.SaveChangesAsync();
        return supportRequest;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var supportRequest = await _context.SupportRequests.FindAsync(id);
        if (supportRequest == null)
            return false;

        _context.SupportRequests.Remove(supportRequest);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.SupportRequests.AnyAsync(sr => sr.Id == id);
    }
}