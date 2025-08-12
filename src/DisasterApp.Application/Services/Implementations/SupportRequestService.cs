using DisasterApp.Application.DTOs;
using DisasterApp.Domain.Entities;
using DisasterApp.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DisasterApp.Application.DTOs.SupportRequestDto;

namespace DisasterApp.Application.Services
{
    public class SupportRequestService: ISupportRequestService
    {
        private readonly ISupportRequestRepository _supportRepo;
        public SupportRequestService(ISupportRequestRepository supportRepo)
        {
            _supportRepo = supportRepo;
        }

        public async Task<IEnumerable<SupportRequestResponseDto>> GetAllAsync()
        {
            var data = await _supportRepo.GetAllAsync();
            var supportRequests = data.Select(sr => new SupportRequestResponseDto
            {
                Id = sr.Id,
                ReportId = sr.ReportId,
                Description = sr.Description,
                Urgency = sr.Urgency,
                Status = sr.Status,
                UserId = sr.UserId,
                SupportTypeName = sr.SupportType.Name,
                CreatedAt = sr.CreatedAt
            });
            return supportRequests;
        }

        public async Task<SupportRequestResponseDto?> GetByIdAsync(int id)
        {
            var data = await _supportRepo.GetByIdAsync(id);
            if (data == null) return null;

            return new SupportRequestResponseDto
            {
                Id = data.Id,
                ReportId = data.ReportId,
                Description = data.Description,
                Urgency = data.Urgency,
                Status = data.Status,
                UserId = data.UserId,
                SupportTypeName = data.SupportType.Name,
                CreatedAt = data.CreatedAt

            };
        }
        public async Task CreateAsync(Guid userId,SupportRequestCreateDto dto)
        {
            var supportType = await _supportRepo.GetSupportTypeByNameAsync(dto.SupportTypeName);
            if (supportType == null)
            {
                supportType=new SupportType { Name = dto.SupportTypeName};
                await _supportRepo.AddSupportTypeAsync(supportType);
                await _supportRepo.SaveChangesAsync();
            }

            var request = new SupportRequest
            {
                ReportId = dto.ReportId,
                Description = dto.Description,
                Urgency = dto.Urgency,
                UserId =userId,
                SupportTypeId = supportType.Id,
                CreatedAt = DateTime.UtcNow,
                Status = Domain.Enums.SupportRequestStatus.Pending,

            };
            await _supportRepo.AddAsync(request);
            await _supportRepo.SaveChangesAsync();
        }
        public async Task UpdateAsync(int id, SupportRequestUpdateDto dto)
        {
            var request = await _supportRepo.GetByIdAsync(id);
            if (request == null) return;
            var supportType = await _supportRepo.GetSupportTypeByNameAsync(dto.SupportTypeName);

            request.Description = dto.Description;
            request.Urgency = dto.Urgency;
            request.SupportTypeId = supportType.Id;
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

    }
}
