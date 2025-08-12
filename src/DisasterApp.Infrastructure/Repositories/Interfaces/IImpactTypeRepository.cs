using DisasterApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterApp.Infrastructure.Repositories
{
    public interface IImpactTypeRepository
    {
        Task<ImpactType?> GetByIdAsync(int id);
        Task<IEnumerable<ImpactType>> GetAllAsync();
        Task<ImpactType?> GetByNameAsync(string name);
        Task<ImpactType> CreateAsync(ImpactType entity);
        Task<ImpactType> UpdateAsync(ImpactType entity);
        Task DeleteAsync(int id);
    }
}
