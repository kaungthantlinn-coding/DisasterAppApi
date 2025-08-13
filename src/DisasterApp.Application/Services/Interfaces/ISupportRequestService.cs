using DisasterApp.Application.DTOs;

namespace DisasterApp.Application.Services.Interfaces;

public interface ISupportRequestService
{
    Task<SupportRequestDto> CreateAsync(SupportRequestCreateDto dto, Guid userId);
    Task<IEnumerable<SupportRequestDto>> GetAllAsync();
    Task<SupportRequestDto?> GetByIdAsync(int id);
    Task<IEnumerable<SupportRequestDto>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<SupportRequestDto>> GetByReportIdAsync(Guid reportId);
    Task<SupportRequestDto?> UpdateAsync(int id, SupportRequestUpdateDto dto, Guid userId);
    Task<bool> DeleteAsync(int id, Guid userId);
    Task<IEnumerable<SupportTypeDto>> GetSupportTypesAsync();
}