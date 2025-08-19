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
    public class DonationRepository : IDonationRepository
    {
        private readonly DisasterDbContext _context;

        public DonationRepository(DisasterDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Donation donation)
        {
            _context.Donations.Add(donation);
            await _context.SaveChangesAsync();
        }

        public async Task<Donation?> GetByIdAsync(int id) =>
            await _context.Donations.FindAsync(id);

        public async Task UpdateAsync(Donation donation)
        {
            _context.Donations.Update(donation);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Donation>> GetByOrganizationIdAsync(int organizationId) =>
            await _context.Donations
                .Where(d => d.OrganizationId == organizationId)
                .ToListAsync();

        public async Task<List<Donation>> GetPendingDonationsAsync() =>
            await _context.Donations
                .Where(d => d.Status == Domain.Enums.DonationStatus.Pending)
                .ToListAsync();
    }
}
