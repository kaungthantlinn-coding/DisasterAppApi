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
        Task<DisasterReport?> GetByIdAsync(Guid id);
        Task<IEnumerable<DisasterReport>> GetAllAsync();
        //Task<Guid> AddReportWithLocationAsync(DisasterReport report, Location loction);
        Task<DisasterReport> CreateAsync(DisasterReport report,Location location);
       
        Task<DisasterReport> UpdateAsync(DisasterReport report);
        Task DeleteAsync(Guid id);
    }
}
