using DisasterApp.Application.DTOs;
using DisasterApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterApp.Application.Services
{
    public interface IImpactDetailService
    {
        Task<IEnumerable<ImpactDetailDto>> GetAllAsync();
        Task<ImpactDetailDto?> GetByIdAsync(int id);
        Task<ImpactDetailDto> CreateAsync(ImpactDetailCreateDto impactDetail);
        Task<ImpactDetailDto> UpdateAsync(int id, ImpactDetailUpdateDto dto);
        Task DeleteAsync(int id);
    }
}
