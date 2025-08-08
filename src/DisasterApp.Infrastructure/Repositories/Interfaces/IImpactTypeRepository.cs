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
        Task<List<ImpactType>> GetAllAsync();
        Task<ImpactType?> GetByIdAsync(int id);
        Task<ImpactType> AddAsync(ImpactType impactType);
    }
}
