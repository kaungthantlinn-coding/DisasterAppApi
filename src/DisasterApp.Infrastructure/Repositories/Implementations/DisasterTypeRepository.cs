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
    public class DisasterTypeRepository:IDisasterTypeRepository
    {
        private readonly DisasterDbContext _context;
        public DisasterTypeRepository(DisasterDbContext context)
        {
            _context = context;
        }
        public async Task<List<DisasterType>> GetAllAsync()
        {
            return await _context.DisasterTypes.ToListAsync();
        }


        public async Task<DisasterType?> GetByIdAsync(int id)
        {
            return await _context.DisasterTypes.FindAsync(id);
        }
        public async Task AddAsync(DisasterType disasterType)
        {
            await _context.DisasterTypes.AddAsync(disasterType);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateAsync(DisasterType disasterType)
        {
            _context.DisasterTypes.Update(disasterType);
            await _context.SaveChangesAsync(); // You can remove this if using SaveChangesAsync() separately
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var type = await _context.DisasterTypes.FindAsync(id);
            if (type == null) return false;

            _context.DisasterTypes.Remove(type);
            await _context.SaveChangesAsync();
            return true;
        }

    }
}
