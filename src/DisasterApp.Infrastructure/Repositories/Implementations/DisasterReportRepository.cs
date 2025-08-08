using DisasterApp.Domain.Entities;
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
                .Include(r => r.User)
                .Include(r => r.Location)
                .Include(r => r.Photos)
                .Include(r => r.ImpactDetails)
                .Where(x => x.IsDeleted != true)
                .ToListAsync();
        }
        public async Task<DisasterReport?> GetByIdAsync(Guid id)
        {
            return await _context.DisasterReports
                .Include(r => r.DisasterEvent)
                .Include(r => r.User)
                .Include(r => r.Location)
                .Include(r => r.Photos)
                .Include(r => r.ImpactDetails)
                .FirstOrDefaultAsync(r => r.Id == id && r.IsDeleted != true);
        }
        public async Task<DisasterReport> CreateAsync(DisasterReport report, Location location)
        {
           await _context.DisasterReports.AddAsync(report);
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
        //For Location
        //public async Task<Guid> AddReportWithLocationAsync(DisasterReport report, Location loction)
        //{
        //    await _context.DisasterReports.AddAsync(report);
        //    await _context.Locations.AddAsync(loction);
        //    await _context.SaveChangesAsync();
        //    return report.Id;
        //}

        public async Task<IEnumerable<DisasterReport>> SearchAsync(string keyword)
        {
            keyword = keyword?.Trim().Replace(" ", " ") ?? "";

            if (string.IsNullOrEmpty(keyword))
            {
                return await _context.DisasterReports
                    .Include(r => r.DisasterEvent)
                    .Include(r => r.Location)
                    .Where(r => !r.IsDeleted.HasValue || r.IsDeleted == false)
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

    }
}
