using DisasterApp.Domain.Entities;
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
    public class DisasterEventRepository : IDisasterEventRepository
    {
        private readonly DisasterDbContext _context;

        public DisasterEventRepository(DisasterDbContext context)
        {
            _context = context;
        }

        public async Task<List<DisasterEvent>> GetAllAsync()
        {
            return await _context.DisasterEvents.ToListAsync();
        }

        public async Task<DisasterEvent?> GetByIdAsync(Guid id)
        {
            return await _context.DisasterEvents.FindAsync(id);
        }

        public async Task AddAsync(DisasterEvent entity)
        {
            _context.DisasterEvents.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(DisasterEvent entity)
        {
            _context.DisasterEvents.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(DisasterEvent entity)
        {
            _context.DisasterEvents.Remove(entity);
            await _context.SaveChangesAsync();
        }
        public async Task<DisasterEvent?> GetByNameAsync(string name)
        {
            return await _context.DisasterEvents.FirstOrDefaultAsync(e => e.Name.ToLower() == name.ToLower());
        }

    }
}
