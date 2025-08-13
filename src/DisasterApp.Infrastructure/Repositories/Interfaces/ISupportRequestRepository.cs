using DisasterApp.Domain.Entities;

namespace DisasterApp.Infrastructure.Repositories.Interfaces;

public interface ISupportRequestRepository
{
    Task<SupportRequest?> GetByIdAsync(int id);
    Task<IEnumerable<SupportRequest>> GetAllAsync();
    Task<IEnumerable<SupportRequest>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<SupportRequest>> GetByReportIdAsync(Guid reportId);
    Task<SupportRequest> CreateAsync(SupportRequest supportRequest);
    Task<SupportRequest> UpdateAsync(SupportRequest supportRequest);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}