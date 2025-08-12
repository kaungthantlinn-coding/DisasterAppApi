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
    public class ImpactDetailRepository:IImpactDetailRepository
    {
        private readonly DisasterDbContext _context;
        public ImpactDetailRepository(DisasterDbContext context)
        {
            _context = context;
        }
        public async Task<ImpactDetail?> GetByIdAsync(int id)
        {
            return await _context.ImpactDetails.
                Include(i=>i.ImpactTypes).
                FirstOrDefaultAsync(i=>i.Id ==id);
        }
        public async Task<IEnumerable<ImpactDetail>> GetAllAsync()
        {
            return await _context.ImpactDetails
           .Include(i => i.ImpactTypes)
           .ToListAsync();
        }
        public async Task<ImpactDetail> CreateAsync(ImpactDetail entity)
        {
            _context.ImpactDetails.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
        public async Task<ImpactDetail> UpdateAsync(ImpactDetail entity)
        {
            _context.ImpactDetails.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
        public async Task DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _context.ImpactDetails.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}
