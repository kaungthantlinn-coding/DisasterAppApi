using DisasterApp.Application.DTOs;
using DisasterApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterApp.Application.Services
{
    public interface IImpactTypeService
    {
        Task<IEnumerable<ImpactTypeDto>> GetAllAsync();
        Task<ImpactTypeDto?> GetByIdAsync(int id);
        Task<ImpactTypeDto> CreateAsync(ImpactTypeCreateDto impactType);
        Task<ImpactTypeDto> UpdateAsync(int id, ImpactTypeUpdateDto dto);
        Task DeleteAsync(int id);

    }
}
