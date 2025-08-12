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
    public class ImpactTypeRepository : IImpactTypeRepository
    {
        private readonly DisasterDbContext _context;

        public ImpactTypeRepository(DisasterDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ImpactType>> GetAllAsync()
        {
            return await _context.ImpactTypes.
                Include(it => it.ImpactDetails).
                ToListAsync();
        }

        public async Task<ImpactType?> GetByIdAsync(int id)
        {
            return await _context.ImpactTypes.
                Include(it => it.ImpactDetails).
                FirstOrDefaultAsync(it => it.Id == id);
        }
        public async Task<ImpactType?> GetByNameAsync(string name)
        {
            return await _context.ImpactTypes
                .FirstOrDefaultAsync(it => it.Name.ToLower() == name.ToLower());
        }

        public async Task<ImpactType> CreateAsync(ImpactType impactType)
        {
            _context.ImpactTypes.Add(impactType);
            await _context.SaveChangesAsync();
            return impactType;
        }
        public async Task<ImpactType> UpdateAsync(ImpactType impactType)
        {
            _context.ImpactTypes.Update(impactType);
            await _context.SaveChangesAsync();
            return impactType;
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.ImpactTypes.FindAsync(id);
            if (entity == null)
            {
                throw new KeyNotFoundException("ImpactType not found.");
            }
            _context.ImpactTypes.Remove(entity);
            await _context.SaveChangesAsync();
        }

    }
}

