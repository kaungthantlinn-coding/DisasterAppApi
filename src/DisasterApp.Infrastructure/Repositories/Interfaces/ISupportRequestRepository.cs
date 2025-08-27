using DisasterApp.Domain.Entities;

namespace DisasterApp.Infrastructure.Repositories.Interfaces;

public interface ISupportRequestRepository
{
    Task<IEnumerable<SupportRequest>> GetAllAsync();
    Task<SupportRequest?> GetByIdAsync(int id);
    Task<IEnumerable<SupportType>> GetSupportTypeAsync();

    Task<IEnumerable<SupportRequest>> GetPendingSupportRequestsAsync();
    Task<IEnumerable<SupportRequest>> GetAcceptedForReportIdAsync(Guid reportId);
    Task<IEnumerable<SupportRequest>> GetAcceptedSupportRequestsAsync();
    Task<IEnumerable<SupportRequest>> GetRejectedSupportRequestsAsync();



    Task AddAsync(SupportRequest request);
    Task AddSupportTypesAsync(List<SupportType> supportTypes);
    Task UpdateAsync(SupportRequest request);
    Task DeleteAsync(SupportRequest request);
    Task SaveChangesAsync();

    Task<List<SupportType>> GetSupportTypesByNamesAsync(List<string> names);
    Task<SupportRequestMetrics> GetMetricsAsync();
}
public class SupportRequestMetrics
{
    public int TotalRequests { get; set; }
    public int PendingRequests { get; set; }
    public int VerifiedRequests { get; set; }
    public int RejectedRequests { get; set; }
}