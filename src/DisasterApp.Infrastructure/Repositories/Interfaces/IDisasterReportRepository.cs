using DisasterApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterApp.Infrastructure.Repositories
{
    public interface IDisasterReportRepository
    {
        Task<List<DisasterReport>> GetAllReportsAsync();
        Task<Guid> AddReportWithLocationAsync(DisasterReport report, Location loction);
        Task<DisasterReport?> GetByIdAsync(Guid id);
        Task<IEnumerable<DisasterReport>> SearchAsync(string keyword);
        Task UpdateAsync(DisasterReport report);
        Task DeleteAsync(DisasterReport report);

    }
}
