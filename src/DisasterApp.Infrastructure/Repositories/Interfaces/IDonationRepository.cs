using DisasterApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterApp.Infrastructure.Repositories.Interfaces
{
    public interface IDonationRepository
    {
        Task AddAsync(Donation donation);
        Task<Donation?> GetByIdAsync(int id);
        Task UpdateAsync(Donation donation);
        Task<List<Donation>> GetByOrganizationIdAsync(int organizationId);
        Task<List<Donation>> GetPendingDonationsAsync();
    }
}
