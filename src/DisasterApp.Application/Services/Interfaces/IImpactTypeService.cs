using DisasterApp.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterApp.Application.Services
{
    public interface IImpactTypeService
    {
        Task<List<ImpactTypeDto>> GetAllAsync();
        Task<ImpactTypeDto> CreateAsync(ImpactTypeCreateDto dto);

    }
}
