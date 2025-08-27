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

namespace DisasterApp.Infrastructure.Repositories
{
    public class SupportRequestRepository : ISupportRequestRepository
    {
        private readonly DisasterDbContext _context;
        public SupportRequestRepository(DisasterDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<SupportRequest>> GetAllAsync()
        {
            return await _context.SupportRequests
                .Include(sr => sr.SupportTypes)
                .Include(sr => sr.Report)
                .ThenInclude(r => r.Location)
                .Include(sr => sr.User)

                .ToListAsync();
        }

        public async Task<SupportRequest?> GetByIdAsync(int id)
        {
            return await _context.SupportRequests
                 .Include(sr => sr.SupportTypes)
                 .FirstOrDefaultAsync(sr => sr.Id == id);
        }


        public async Task AddAsync(SupportRequest request)
        {
            await _context.SupportRequests.AddAsync(request);
        }
        public async Task UpdateAsync(SupportRequest request)
        {
            _context.SupportRequests.Update(request);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(SupportRequest request)
        {
            _context.SupportRequests.Remove(request);

        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }



        public async Task AddSupportTypesAsync(List<SupportType> supportTypes)
        {
            await _context.SupportTypes.AddRangeAsync(supportTypes);
        }

        public async Task<List<SupportType>> GetSupportTypesByNamesAsync(List<string> names)
        {
            return await _context.SupportTypes
            .Where(st => names.Contains(st.Name))
            .ToListAsync();
        }

        public async Task<IEnumerable<SupportType>> GetSupportTypeAsync()
        {
            return await _context.SupportTypes.ToListAsync();
        }

        public async Task<IEnumerable<SupportRequest>> GetPendingSupportRequestsAsync()
        {
            return await _context.SupportRequests
                .Include(sr => sr.SupportTypes)
                .Include(sr => sr.User)
                .Where(sr => sr.Status == SupportRequestStatus.Pending)
                .Include(sr => sr.Report)

                .ToListAsync();
        }

        public async Task<IEnumerable<SupportRequest>> GetAcceptedSupportRequestsAsync()
        {

            return await _context.SupportRequests
                .Include(sr => sr.SupportTypes)
                .Where(sr => sr.Status == SupportRequestStatus.Verified)
                .ToListAsync();
        }

        public async Task<IEnumerable<SupportRequest>> GetRejectedSupportRequestsAsync()
        {

            return await _context.SupportRequests
                .Include(sr => sr.SupportTypes)
                .Where(sr => sr.Status == SupportRequestStatus.Rejected)
                .ToListAsync();
        }

        public async Task<IEnumerable<SupportRequest>> GetAcceptedForReportIdAsync(Guid reportId)
        {
            return await _context.SupportRequests
                .Include(sr => sr.SupportTypes)
                .Include(sr => sr.Report)
                .Include(sr => sr.User)
                .Where(sr => sr.Status == SupportRequestStatus.Verified && sr.ReportId == reportId)
                .OrderByDescending(sr => sr.CreatedAt)
                .ToListAsync();
        }
        public async Task<SupportRequestMetrics> GetMetricsAsync()
        {
            return new SupportRequestMetrics
            {
                TotalRequests = await _context.SupportRequests.CountAsync(),
                PendingRequests = await _context.SupportRequests.CountAsync(r => r.Status == SupportRequestStatus.Pending),
                VerifiedRequests = await _context.SupportRequests.CountAsync(r => r.Status == SupportRequestStatus.Verified),
                RejectedRequests = await _context.SupportRequests.CountAsync(r => r.Status == SupportRequestStatus.Rejected),
            };
        }


    }
}