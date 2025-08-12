using DisasterApp.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterApp.Application.Services
{
    public interface IDisasterEventService
    {
        Task<List<DisasterEventDto>> GetAllAsync();
        Task<DisasterEventDto?> GetByIdAsync(Guid id);
        Task AddAsync(CreateDisasterEventDto dto);
        Task UpdateAsync(Guid id, UpdateDisasterEventDto dto);
        Task<bool> DeleteAsync(Guid id);
    }
}
