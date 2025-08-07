using DisasterApp.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterApp.Application.Services
{
    public interface IDisasterTypeService
    {
        Task<List<DisasterTypeDto>> GetAllAsync();
        Task<DisasterTypeDto?> GetByIdAsync(int id);
        Task AddAsync(CreateDisasterTypeDto disasterTypeDto);
        Task UpdateAsync(int id ,UpdateDisasterTypeDto disasterTypeDto);
        Task<bool> DeleteAsync(int id);
    }
}
