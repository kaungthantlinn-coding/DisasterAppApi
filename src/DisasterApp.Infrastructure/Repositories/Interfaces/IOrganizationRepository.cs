using DisasterApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterApp.Infrastructure.Repositories.Interfaces
{
    public interface IOrganizationRepository
    {
        Task<int> AddAsync(Organization org);
        Task<Organization?> GetByIdAsync(int id);
        Task<List<Organization>> GetAllAsync();
        Task<bool> UpdateAsync(Organization org);
        Task<bool> DeleteAsync(int id);
    }
}
