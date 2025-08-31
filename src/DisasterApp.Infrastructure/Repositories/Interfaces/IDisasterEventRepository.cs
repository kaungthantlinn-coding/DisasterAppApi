using DisasterApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterApp.Infrastructure.Repositories
{
    public interface IDisasterEventRepository
    {
        Task<List<DisasterEvent>> GetAllAsync();
        Task<DisasterEvent?> GetByIdAsync(Guid id);
        Task AddAsync(DisasterEvent entity);
        Task UpdateAsync(DisasterEvent entity);
        Task DeleteAsync(DisasterEvent entity);
        Task<DisasterEvent> GetByNameAsync(string name);

    }
}
