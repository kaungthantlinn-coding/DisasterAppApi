using CloudinaryDotNet.Core;
using DisasterApp.Application.DTOs;
using DisasterApp.Domain.Entities;
using DisasterApp.Domain.Enums;
using DisasterApp.Infrastructure.Repositories;
using DisasterApp.Infrastructure.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DisasterApp.Application.DTOs.SupportRequestDto;

namespace DisasterApp.Application.Services
{
    public class SupportRequestService : ISupportRequestService
    {
        private readonly ISupportRequestRepository _supportRepo;
        private readonly IUserRepository _userRepo;
        public SupportRequestService(ISupportRequestRepository supportRepo, IUserRepository userRepo)
        {
            _supportRepo = supportRepo;
            _userRepo = userRepo;
        }

        public async Task<IEnumerable<SupportRequestsDto>> GetAllAsync()
        {
            var data = await _supportRepo.GetAllAsync();
            var supportRequests = data.Select(sr => new SupportRequestsDto
            {
                Id = sr.Id,

                ReportId = sr.ReportId,
                Description = sr.Description,
                FullName = sr.User.Name,
                Email = sr.User.Email,
                Location = sr.Report.Location.Address,
                UrgencyLevel = ((UrgencyLevel)sr.Urgency).ToString(),

                AdminRemarks = "No remarks",

                DateReported = sr.CreatedAt,
                Status = sr.Status.ToString(),
                SupportTypeNames = sr.SupportTypes
                             .Select(st => st.Name)
                             .ToList() ?? new List<string>(),
            });
            return supportRequests;
        }

        public async Task<SupportRequestsDto?> GetByIdAsync(int id)
        {
            var data = await _supportRepo.GetByIdAsync(id);
            if (data == null) return null;

            return new SupportRequestsDto
            {


                ReportId = data.ReportId,
                Description = data.Description,
                FullName = data.User.Name,
                Email = data.User.Email,
                Location = data.Report.Location.Address,
                UrgencyLevel = ((UrgencyLevel)data.Urgency).ToString(),

                AdminRemarks = "No remarks",

                DateReported = data.CreatedAt,
                SupportTypeNames = data.SupportTypes
                             .Select(st => st.Name)
                             .ToList() ?? new List<string>(),

            };
        }
        public async Task CreateAsync(Guid userId, SupportRequestCreateDto dto)
        {
            // Step 1: Get existing types
            var existingTypes = await _supportRepo.GetSupportTypesByNamesAsync(dto.SupportTypeNames);
            var existingTypeNames = existingTypes.Select(st => st.Name).ToList();

            // Step 2: Find new types that don't exist yet
            var newTypeNames = dto.SupportTypeNames
                .Except(existingTypeNames, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var newTypes = newTypeNames
                .Select(name => new SupportType { Name = name })
                .ToList();

            if (newTypes.Any())
                await _supportRepo.AddSupportTypesAsync(newTypes);

            // Step 3: Combine existing + new
            var allTypes = existingTypes.Concat(newTypes).ToList();

            // Step 4: Create SupportRequest
            var supportRequest = new SupportRequest
            {
                ReportId = dto.ReportId,
                Description = dto.Description,
                Urgency = dto.Urgency,
                Status = SupportRequestStatus.Pending,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                SupportTypes = allTypes
            };

            await _supportRepo.AddAsync(supportRequest);
            await _supportRepo.SaveChangesAsync();
        }
        public async Task UpdateAsync(int id, SupportRequestUpdateDto dto)
        {
            var request = await _supportRepo.GetByIdAsync(id);
            if (request == null) return;
            //var supportType = await _supportRepo.GetSupportTypeByNameAsync(dto.SupportTypeName);

            request.Description = dto.Description;
            request.Urgency = dto.Urgency;
            //request.SupportTypeId = supportType.Id;
            request.UpdatedAt = DateTime.UtcNow;

            _supportRepo.UpdateAsync(request);
            await _supportRepo.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var request = await _supportRepo.GetByIdAsync(id);
            if (request == null) return false;
            _supportRepo.DeleteAsync(request);
            await _supportRepo.SaveChangesAsync();
            return true;

        }

        public async Task<IEnumerable<string>> GetSupportTypeNamesAsync()
        {
            var supportTypes = await _supportRepo.GetSupportTypeAsync();
            return supportTypes.Select(st => st.Name).ToList();
        }

        public async Task<SupportRequestResponseDto?> ApproveSupportRequestAsync(int id, Guid adminUserId)
        {
            var adminUser = await _userRepo.GetByIdAsync(adminUserId);
            if (adminUser == null ||  adminUser.Roles == null || !adminUser.Roles.Any(r => r.Name == "admin"))
            {
                throw new UnauthorizedAccessException("Only Admins can approve support requests.");
            }


            var request = await _supportRepo.GetByIdAsync(id);
            if (request == null) return null;


            request.Status = SupportRequestStatus.Verified;


            request.UpdatedAt = DateTime.UtcNow;

            await _supportRepo.UpdateAsync(request);
            var dto = new SupportRequestResponseDto
            {
                Id = request.Id,

                Description = request.Description,
                Urgency = request.Urgency,
                Status = SupportRequestStatus.Rejected,
                SupportTypeNames = request.SupportTypes?.Select(st => st.Name).ToList() ?? new List<string>(),
                CreatedAt = request.CreatedAt ?? DateTime.UtcNow,
                UpdatedAt = request.UpdatedAt
            };

            return dto;

        }

        public async Task<SupportRequestResponseDto?> RejectSupportRequestAsync(int id, Guid adminUserId)
        {
            var adminUser = await _userRepo.GetByIdAsync(adminUserId);
            if (adminUser == null || adminUser.Roles == null || !adminUser.Roles.Any(r => r.Name == "admin"))
            {
                throw new UnauthorizedAccessException("Only Admins can approve support requests.");
            }


            var request = await _supportRepo.GetByIdAsync(id);
            if (request == null) return null;


            request.Status = SupportRequestStatus.Rejected;


            request.UpdatedAt = DateTime.UtcNow;

            await _supportRepo.UpdateAsync(request);
            var dto = new SupportRequestResponseDto
            {
                Id = request.Id,

                Description = request.Description,
                Urgency = request.Urgency,
                Status = SupportRequestStatus.Rejected,
                SupportTypeNames = request.SupportTypes?.Select(st => st.Name).ToList() ?? new List<string>(),
                CreatedAt = request.CreatedAt ?? DateTime.UtcNow,
                UpdatedAt = request.UpdatedAt
            };

            return dto;

        }

        public Task<bool> ApproveOrRejectSupportRequestAsync(Guid id, ReportStatus status, Guid adminUserId)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<SupportRequestsDto>> GetPendingRequestsAsync()
        {
            var requests = await _supportRepo.GetPendingSupportRequestsAsync();
            return requests.Select(r => new SupportRequestsDto
            {
                ReportId = r.ReportId,
                FullName = r.User.Name,
                Email = r.User.Email,


                UrgencyLevel = ((UrgencyLevel)r.Urgency).ToString(),
                Description = r.Description,
                AdminRemarks = "No remarks",

                DateReported = r.CreatedAt,
                Status = SupportRequestStatus.Pending.ToString(),
                SupportTypeNames = r.SupportTypes
                             .Select(st => st.Name)
                             .ToList() ?? new List<string>(),
            }).ToList();
        }

        public async Task<IEnumerable<SupportRequestResponseDto>> GetAcceptedRequestsAsync()
        {
            var requests = await _supportRepo.GetAcceptedSupportRequestsAsync();
            return requests.Select(r => new SupportRequestResponseDto
            {
                Id = r.Id,
                ReportId = r.ReportId,
                Description = r.Description,
                Urgency = r.Urgency,
                Status = r.Status,
                UserId = r.UserId,
                SupportTypeNames = r.SupportTypes.Select(st => st.Name).ToList(),
                CreatedAt = r.CreatedAt
            }).ToList();
        }

        public async Task<IEnumerable<SupportRequestResponseDto>> GetRejectedRequestsAsync()
        {
            var requests = await _supportRepo.GetRejectedSupportRequestsAsync();
            return requests.Select(r => new SupportRequestResponseDto
            {
                Id = r.Id,
                ReportId = r.ReportId,
                Description = r.Description,
                Urgency = r.Urgency,
                Status = r.Status,
                UserId = r.UserId,
                SupportTypeNames = r.SupportTypes.Select(st => st.Name).ToList(),
                CreatedAt = r.CreatedAt
            }).ToList();
        }

        public Task<bool> ApproveOrRejectSupportRequestAsync(int id, ReportStatus status, Guid adminUserId)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<SupportRequestResponseDto>> GetAcceptedReportIdAsync(Guid ReportId)
        {
            var requests = await _supportRepo.GetAcceptedForReportIdAsync(ReportId);
            return requests.Select(r => new SupportRequestResponseDto
            {
                Id = r.Id,
                ReportId = r.ReportId,
                UserName = r.User.Name,
                email = r.User.Email,
                Description = r.Description,
                Urgency = r.Urgency,
                Status = r.Status,
                UserId = r.UserId,
                SupportTypeNames = r.SupportTypes.Select(st => st.Name).ToList(),
                CreatedAt = r.CreatedAt
            }).ToList();
        }

        public async Task<SupportRequestMetricsDto> GetMetricsAsync()
        {
            var metrics = await _supportRepo.GetMetricsAsync();
            return new SupportRequestMetricsDto
            {
                TotalRequests = metrics.TotalRequests,
                PendingRequests = metrics.PendingRequests,
                VerifiedRequests = metrics.VerifiedRequests,
                RejectedRequests = metrics.RejectedRequests
            };
        }
    }
}