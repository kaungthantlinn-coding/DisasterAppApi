using DisasterApp.Domain.Entities;
using DisasterApp.Infrastructure.Data;
using DisasterApp.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterApp.Infrastructure.Repositories.Implementations
{
    public class OrganizationRepository : IOrganizationRepository
    {
        private readonly DisasterDbContext _context;

        public OrganizationRepository(DisasterDbContext context)
        {
            _context = context;
        }

        public async Task<int> AddAsync(Organization org)
        {
            org.CreatedAt = DateTime.UtcNow;
            _context.Organizations.Add(org);
            await _context.SaveChangesAsync();
            return org.Id;
        }

        public async Task<Organization?> GetByIdAsync(int id) =>
            await _context.Organizations
                .Include(o => o.Donations)
                .FirstOrDefaultAsync(o => o.Id == id);

        public async Task<List<Organization>> GetAllAsync() =>
            await _context.Organizations
                .Include(o => o.Donations)
                .ToListAsync();

        public async Task<bool> UpdateAsync(Organization org)
        {
            _context.Organizations.Update(org);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var org = await _context.Organizations.FindAsync(id);
            if (org == null) return false;
            _context.Organizations.Remove(org);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
