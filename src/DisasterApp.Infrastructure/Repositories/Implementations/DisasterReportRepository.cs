using DisasterApp.Domain.Entities;
using DisasterApp.Infrastructure.Data;
using DisasterApp.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterApp.Infrastructure.Repositories
{
    public class DisasterReportRepository : IDisasterReportRepository
    {
        private readonly DisasterDbContext _context;
        public DisasterReportRepository(DisasterDbContext context)
        {
            _context = context;
        }
        
        public async Task<List<DisasterReport>> GetAllReportsAsync()
        {
            return await _context.DisasterReports
                .Include(r => r.DisasterEvent)
                .Include(r => r.User)
                .Include(r => r.Location)
                .Where(r => r.IsDeleted == false)
                .ToListAsync();
        }

        public async Task<DisasterReport?> GetByIdAsync(Guid id)
        {
            return await _context.DisasterReports
                 .Include(r => r.Location)
                 .Include(r => r.User)
                 .Include(r => r.DisasterEvent)
                 .FirstOrDefaultAsync(r => r.Id == id);
        }
        public async Task<Guid> AddReportWithLocationAsync(DisasterReport report, Location loction)
        {
            await _context.DisasterReports.AddAsync(report);
            await _context.Locations.AddAsync(loction);
            await _context.SaveChangesAsync();
            return report.Id;
        }
        public async Task<IEnumerable<DisasterReport>> SearchAsync(string keyword)
        {
            keyword=keyword?.Trim().Replace (" "," ") ?? "";
            
            if(string.IsNullOrEmpty(keyword))
            {
                return await _context.DisasterReports
                    .Include(r=>r.DisasterEvent)
                    .Include(r=>r.Location)
                    .Where(r=>!r.IsDeleted.HasValue || r.IsDeleted==false)
                    .OrderByDescending(r => r.Timestamp)
                    .ToListAsync();
            }
              return await _context.DisasterReports
        .Include(r => r.DisasterEvent)
        .Include(r => r.Location)
        .Where(r => (!r.IsDeleted.HasValue || r.IsDeleted == false) &&
            (
                EF.Functions.Like(
                    EF.Functions.Collate(
                        r.Title.Replace(" ", "").ToLower(), 
                        "SQL_Latin1_General_CP1_CI_AI" // Case & Accent Insensitive
                    ),
                    $"%{keyword}%"
                ) ||
                EF.Functions.Like(
                    EF.Functions.Collate(
                        r.Description.Replace(" ", "").ToLower(), 
                        "SQL_Latin1_General_CP1_CI_AI"
                    ),
                    $"%{keyword}%"
                ) ||
                EF.Functions.Like(
                    EF.Functions.Collate(
                        r.DisasterEvent.Name.Replace(" ", "").ToLower(), 
                        "SQL_Latin1_General_CP1_CI_AI"
                    ),
                    $"%{keyword}%"
                ) ||
                (r.Location != null && r.Location.Address != null &&
                    EF.Functions.Like(
                        EF.Functions.Collate(
                            r.Location.Address.Replace(" ", "").ToLower(),
                            "SQL_Latin1_General_CP1_CI_AI"
                        ),
                        $"%{keyword}%"
                    )
                )
            )
        )
        .OrderByDescending(r => r.Timestamp)
        .ToListAsync();
}
        public async Task UpdateAsync(DisasterReport report)
        {
            _context.DisasterReports.Update(report);
            await _context.SaveChangesAsync();
        }
        public Task DeleteAsync(DisasterReport report)
        {
           report.IsDeleted = true;
            _context.DisasterReports.Update(report);
            return _context.SaveChangesAsync();
        }

        
    }
}
