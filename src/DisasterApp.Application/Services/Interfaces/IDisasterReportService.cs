using DisasterApp.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DisasterApp.Application.DTOs.DisasterReportDto;

namespace DisasterApp.Application.Services
{
    public interface IDisasterReportService
    {
        Task<List<DisasterReportResponseDto>> GetAllAsync();
        Task<IEnumerable<DisasterReportSearchDto>> SearchAsync(string keyword);
        Task<Guid> CreateReportAsync(ReportCreateDto dto);
        Task<bool> UpdateAsync(Guid id, DisasterReportUpdateDto dto);
        Task<bool> DeleteReportAsync(Guid id);
    }
}
