using DisasterApp.Application.DTOs;
using DisasterApp.Domain.Entities;
using DisasterApp.Domain.Enums;
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

        Task<IEnumerable<DisasterReportDto>> GetAcceptedReportsAsync();
        Task<IEnumerable<DisasterReportDto>> GetRejectedReportsAsync();
        Task<DisasterReportDto?> GetByIdAsync(Guid id);
        Task<IEnumerable<DisasterReportDto>> GetReportsByUserIdAsync(Guid userId);
        Task<IEnumerable<DisasterReportDto>> GetPendingReportsForAdminAsync(Guid adminUserId);

        Task<bool> ApproveDisasterReportAsync(Guid id, Guid adminUserId);
        Task<bool> RejectDisasterReportAsync(Guid reportId, Guid adminUserId);
        Task<bool> ApproveOrRejectReportAsync(Guid id, ReportStatus status, Guid adminUserId);

        Task<DisasterReportDto?> UpdateAsync(Guid id, DisasterReportUpdateDto dto, Guid userId);
        Task<bool> DeleteAsync(Guid id);


    }

}