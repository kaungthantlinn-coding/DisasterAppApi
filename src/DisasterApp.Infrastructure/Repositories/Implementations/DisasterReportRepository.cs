using DisasterApp.Domain.Entities;
using DisasterApp.Domain.Enums;
using DisasterApp.Infrastructure.Data;
using DisasterApp.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace DisasterApp.Infrastructure.Repositories
{
    public class DisasterReportRepository : IDisasterReportRepository
    {
        private readonly DisasterDbContext _context;
        public DisasterReportRepository(DisasterDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<DisasterReport>> GetAllAsync()
        {
            return await _context.DisasterReports
                .Include(r => r.DisasterEvent)
                 .ThenInclude(r => r.DisasterType)
                .Include(r => r.User)
                .Include(r => r.Location)
                .Include(r => r.Photos)
                .Include(r => r.ImpactDetails)
                .ThenInclude(id => id.ImpactTypes)
                .Where(x => x.IsDeleted != true)
                .ToListAsync();
        }
        public async Task<DisasterReport?> GetByIdAsync(Guid id)
        {
            return await _context.DisasterReports
                .Include(r => r.DisasterEvent)
                 .ThenInclude(r => r.DisasterType)
                .Include(r => r.User)
                .Include(r => r.Location)
                .Include(r => r.Photos)
                .Include(r => r.ImpactDetails)
                .ThenInclude(id => id.ImpactTypes)
                .FirstOrDefaultAsync(r => r.Id == id && r.IsDeleted != true);
        }

        public async Task<IEnumerable<DisasterReport>> GetReportsByUserIdAsync(Guid userId)
        {
            return await _context.DisasterReports
                .Include(r => r.DisasterEvent)
                 .ThenInclude(r => r.DisasterType)
                .Include(r => r.User)
                .Include(r => r.Location)
                .Include(r => r.Photos)
                .Include(r => r.ImpactDetails)
                .ThenInclude(id => id.ImpactTypes)
                .Where(r => r.UserId == userId && r.IsDeleted != true)
                .ToListAsync();
        }
        public async Task<DisasterReport> CreateAsync(DisasterReport report, Location location)
        {
            report.Location = location;

            _context.DisasterReports.Add(report);
            await _context.Locations.AddAsync(location);
            await _context.SaveChangesAsync();
            return report;

        }
        public async Task<DisasterReport> UpdateAsync(DisasterReport report)
        {
            _context.DisasterReports.Update(report);
            await _context.SaveChangesAsync();
            return report;
        }

        public async Task DeleteAsync(Guid id)
        {
            var report = _context.DisasterReports.Find(id);
            if (report != null)
            {
                _context.DisasterReports.Remove(report);
                await _context.SaveChangesAsync();
            }
        }
        public async Task SoftDeleteAsync(Guid id)
        {
            var report = await _context.DisasterReports.FindAsync(id);
            if (report != null)
            {
                report.IsDeleted = true;
                _context.DisasterReports.Update(report);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<DisasterReport>> GetPendingReportsAsync()
        {
            return await _context.DisasterReports
            .Include(r => r.User)
            .ThenInclude(r => r.Roles)
            .Include(r => r.DisasterEvent)
             .ThenInclude(r => r.DisasterType)
            .Include(r => r.Location)
            .Include(r => r.ImpactDetails)
            .ThenInclude(r => r.ImpactTypes)
            .Where(r => r.Status == ReportStatus.Pending && r.IsDeleted != true)
            .ToListAsync();
        }

        public async Task<IEnumerable<DisasterReport>> GetAcceptedReportsAsync()
        {
            return await _context.DisasterReports
                .Include(r => r.User)
                .ThenInclude(u => u.Roles)
                .Include(r => r.DisasterEvent)
                 .ThenInclude(r => r.DisasterType)
                .Include(r => r.Location)
                .Include(r => r.Photos)
                .Include(r => r.ImpactDetails)
                .ThenInclude(id => id.ImpactTypes)
                .Where(r => r.Status == ReportStatus.Verified && r.IsDeleted != true)
                .ToListAsync();
        }

        public async Task<IEnumerable<DisasterReport>> GetRejectedReportsAsync()
        {
            return await _context.DisasterReports
                .Include(r => r.User)
                .ThenInclude(u => u.Roles)
                .Include(r => r.DisasterEvent)
                .Include(r => r.Location)
                .Include(r => r.Photos)
                .Include(r => r.ImpactDetails)
                .ThenInclude(id => id.ImpactTypes)
                .Where(r => r.Status == ReportStatus.Rejected && r.IsDeleted != true)
                .ToListAsync();
        }

        public async Task<bool> UpdateStatusAsync(Guid id, ReportStatus status, Guid verifiedBy)
        {
            var report = await _context.DisasterReports.FirstOrDefaultAsync(r => r.Id == id);
            if (report == null)
                return false;

            report.Status = status;
            report.VerifiedBy = verifiedBy;
            report.VerifiedAt = DateTime.UtcNow;
            report.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

    }
}
