using DisasterApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterApp.Infrastructure.Repositories
{
    public interface IDisasterTypeRepository
    {       
            Task<List<DisasterType>> GetAllAsync();
            Task<DisasterType?> GetByIdAsync(int id);
            Task AddAsync(DisasterType disasterType);
            Task UpdateAsync(DisasterType disasterType);       
            Task<bool> DeleteAsync(int id);
        

    }
}
