using DisasterApp.Domain.Entities;
using DisasterApp.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterApp.Infrastructure.Repositories
{
    public interface IDisasterReportRepository
    {
        Task<DisasterReport?> GetByIdAsync(Guid id);
        Task<IEnumerable<DisasterReport>> GetAllAsync();
        //Task<Guid> AddReportWithLocationAsync(DisasterReport report, Location loction);

        Task<IEnumerable<DisasterReport>> GetReportsByUserIdAsync(Guid userId);
        Task<IEnumerable<DisasterReport>> GetPendingReportsAsync();
        Task<IEnumerable<DisasterReport>> GetAcceptedReportsAsync();
        Task<IEnumerable<DisasterReport>> GetRejectedReportsAsync();
        Task<bool> UpdateStatusAsync(Guid id, ReportStatus status, Guid verifiedBy);
        Task<DisasterReport> CreateAsync(DisasterReport report, Location location);

        Task<DisasterReport> UpdateAsync(DisasterReport report);
        Task SoftDeleteAsync(Guid id);
        Task DeleteAsync(Guid id);

        Task<List<DisasterReport>> GetAllForExportReportsAsync();
    }

}

