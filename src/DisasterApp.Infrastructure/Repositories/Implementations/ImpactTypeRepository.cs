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

        public async Task<List<ImpactType>> GetAllAsync()
        {
            return await _context.ImpactTypes.ToListAsync();
        }

        public async Task<ImpactType?> GetByIdAsync(int id)
        {
            return await _context.ImpactTypes.FindAsync(id);
        }

        public async Task<ImpactType> AddAsync(ImpactType impactType)
        {
            _context.ImpactTypes.Add(impactType);
            await _context.SaveChangesAsync();
            return impactType;
        }
    }

}

