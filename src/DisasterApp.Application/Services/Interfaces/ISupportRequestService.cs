using DisasterApp.Application.DTOs;
using DisasterApp.Domain.Entities;
using DisasterApp.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DisasterApp.Application.DTOs.SupportRequestDto;

namespace DisasterApp.Application.Services
{
    public interface ISupportRequestService
    {
        Task<IEnumerable<SupportRequestsDto>> GetAllAsync();
        Task<IEnumerable<string>> GetSupportTypeNamesAsync();
        Task<SupportRequestsDto?> GetByIdAsync(int id);
        Task<IEnumerable<SupportRequestsDto>> GetPendingRequestsAsync();
        Task<IEnumerable<SupportRequestResponseDto>> GetAcceptedRequestsAsync();
        Task<IEnumerable<SupportRequestResponseDto>> GetRejectedRequestsAsync();
        Task<SupportRequestMetricsDto> GetMetricsAsync();

        Task<IEnumerable<SupportRequestResponseDto>> GetAcceptedReportIdAsync(Guid ReportId);
        Task<SupportRequestResponseDto?> ApproveSupportRequestAsync(int id, Guid adminUserId);
        Task<SupportRequestResponseDto?> RejectSupportRequestAsync(int id, Guid adminUserId);
        Task<bool> ApproveOrRejectSupportRequestAsync(int id, ReportStatus status, Guid adminUserId);
        Task CreateAsync(Guid userId, SupportRequestCreateDto dto);
        Task<SupportRequestResponseDto?> UpdateAsync(int id, Guid currentUserId, SupportRequestUpdateDto dto);

        Task<bool> DeleteAsync(int requestId, Guid currentUserId, bool isAdmin);
        Task<IEnumerable<SupportRequestsDto>> SearchByKeywordAsync(string? keyword, byte? urgency, string? status);
    }
}
