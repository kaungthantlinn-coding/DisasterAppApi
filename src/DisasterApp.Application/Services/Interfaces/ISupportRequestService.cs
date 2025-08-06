using DisasterApp.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterApp.Application.Services
{
    public interface ISupportRequestService
    {
        Task<IEnumerable<SupportRequestDto>> GetAllAsync();
        Task<SupportRequestDto?> GetByIdAsync(int id);
        Task CreateAsync(SupportRequestCreateDto dto);
        Task UpdateAsync(int id, SupportRequestUpdateDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
