using DisasterApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterApp.Infrastructure.Repositories
{
    public interface ISupportRequestRepository
    {
        Task<IEnumerable<SupportRequest>> GetAllAsync();
        Task<SupportRequest?> GetByIdAsync(int id);
        Task AddAsync(SupportRequest request);
        Task AddSupportTypeAsync(SupportType supportType);
        Task UpdateAsync(SupportRequest request);
        Task DeleteAsync(SupportRequest request);
        Task SaveChangesAsync();

        Task<SupportType?> GetSupportTypeByNameAsync(string name);
    }
}
