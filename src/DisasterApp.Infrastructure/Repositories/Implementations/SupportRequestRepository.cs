using DisasterApp.Domain.Entities;
using DisasterApp.Infrastructure.Data;
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
                .Include(sr=>sr.SupportType)
                .ToListAsync();
        }

        public async Task<SupportRequest?> GetByIdAsync(int id)
        {
           return await _context.SupportRequests
                .Include(sr=>sr.SupportType)
                .FirstOrDefaultAsync(sr => sr.Id == id);
        }

        public async Task<SupportType> GetOrCreateSupportTypeByNameAsync(string name)
        {
            var supportType = _context.SupportTypes
                .FirstOrDefault(st=>st.Name.ToLower() == name.ToLower());
            if(supportType==null)
            {
                supportType = new SupportType { Name = name };
                await _context.SupportTypes.AddAsync(supportType);
                await _context.SaveChangesAsync();
            }
            return supportType;
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

        
    }
}
