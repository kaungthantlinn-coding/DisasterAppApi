using DisasterApp.Application.DTOs;
using DisasterApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterApp.Application.Services
{
    public interface IDisasterReportService
    {
        Task<DisasterReportDto> CreateAsync(DisasterReportCreateDto dto, Guid userId);
        Task<IEnumerable<DisasterReportDto>> GetAllAsync();
        Task<DisasterReportDto?> GetByIdAsync(Guid id);
        Task<DisasterReportDto?> UpdateAsync(Guid id, DisasterReportUpdateDto dto, Guid userId);
        Task<bool> DeleteAsync(Guid id);
        Task<object> GetStatisticsAsync();
        Task<IEnumerable<DisasterReportDto>> GetAcceptedReportsAsync();
    }
}
