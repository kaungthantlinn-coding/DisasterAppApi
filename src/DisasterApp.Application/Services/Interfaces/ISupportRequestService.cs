using DisasterApp.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DisasterApp.Application.DTOs.SupportRequestDto;

namespace DisasterApp.Application.Services
{
    public interface ISupportRequestService
    {
        Task<IEnumerable<SupportRequestResponseDto>> GetAllAsync();
        Task<SupportRequestResponseDto?> GetByIdAsync(int id);
        Task CreateAsync(Guid userId,SupportRequestCreateDto dto);
        Task UpdateAsync(int id, SupportRequestUpdateDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
