using DisasterApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterApp.Infrastructure.Repositories
{
    public interface IImpactDetailRepository
    {
        Task<ImpactDetail?> GetByIdAsync(int id);
        Task<IEnumerable<ImpactDetail>> GetAllAsync();
        Task<ImpactDetail> CreateAsync(ImpactDetail entity);
        Task<ImpactDetail> UpdateAsync(ImpactDetail entity);
        Task DeleteAsync(int id);
    }
}
