using DisasterApp.Application.DTOs;
using DisasterApp.Application.Services.Interfaces;
using DisasterApp.Domain.Entities;
using DisasterApp.Domain.Enums;
using DisasterApp.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace DisasterApp.Application.Services.Implementations;

public class SupportRequestService : ISupportRequestService
{
    private readonly ISupportRequestRepository _supportRequestRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<SupportRequestService> _logger;

    public SupportRequestService(
        ISupportRequestRepository supportRequestRepository,
        IUserRepository userRepository,
        ILogger<SupportRequestService> logger)
    {
        _supportRequestRepository = supportRequestRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<SupportRequestDto> CreateAsync(SupportRequestCreateDto dto, Guid userId)
    {
        try
        {
            var supportRequest = new SupportRequest
            {
                ReportId = dto.ReportId,
                Description = dto.Description,
                Urgency = dto.Urgency,
                Status = SupportRequestStatus.Pending,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdRequest = await _supportRequestRepository.CreateAsync(supportRequest);
            
            // Load the created request with navigation properties
            var result = await _supportRequestRepository.GetByIdAsync(createdRequest.Id);
            return MapToDto(result!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating support request for user {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<SupportRequestDto>> GetAllAsync()
    {
        try
        {
            var supportRequests = await _supportRequestRepository.GetAllAsync();
            return supportRequests.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all support requests");
            throw;
        }
    }

    public async Task<SupportRequestDto?> GetByIdAsync(int id)
    {
        try
        {
            var supportRequest = await _supportRequestRepository.GetByIdAsync(id);
            return supportRequest != null ? MapToDto(supportRequest) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving support request {Id}", id);
            throw;
        }
    }

    public async Task<IEnumerable<SupportRequestDto>> GetByUserIdAsync(Guid userId)
    {
        try
        {
            var supportRequests = await _supportRequestRepository.GetByUserIdAsync(userId);
            return supportRequests.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving support requests for user {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<SupportRequestDto>> GetByReportIdAsync(Guid reportId)
    {
        try
        {
            var supportRequests = await _supportRequestRepository.GetByReportIdAsync(reportId);
            return supportRequests.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving support requests for report {ReportId}", reportId);
            throw;
        }
    }

    public async Task<SupportRequestDto?> UpdateAsync(int id, SupportRequestUpdateDto dto, Guid userId)
    {
        try
        {
            var existingRequest = await _supportRequestRepository.GetByIdAsync(id);
            if (existingRequest == null)
                return null;

            // Update only provided fields
            if (!string.IsNullOrEmpty(dto.Description))
                existingRequest.Description = dto.Description;
            
            if (dto.Urgency.HasValue)
                existingRequest.Urgency = dto.Urgency.Value;
            
            if (dto.Status.HasValue)
                existingRequest.Status = dto.Status.Value;

            existingRequest.UpdatedAt = DateTime.UtcNow;

            var updatedRequest = await _supportRequestRepository.UpdateAsync(existingRequest);
            return MapToDto(updatedRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating support request {Id}", id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int id, Guid userId)
    {
        try
        {
            var existingRequest = await _supportRequestRepository.GetByIdAsync(id);
            if (existingRequest == null)
                return false;

            return await _supportRequestRepository.DeleteAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting support request {Id}", id);
            throw;
        }
    }

    public async Task<IEnumerable<SupportTypeDto>> GetSupportTypesAsync()
    {
        try
        {
            // This would typically come from a SupportTypeRepository
            // For now, returning empty list - implement when SupportTypeRepository is available
            return new List<SupportTypeDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving support types");
            throw;
        }
    }

    private static SupportRequestDto MapToDto(SupportRequest supportRequest)
    {
        return new SupportRequestDto
        {
            Id = supportRequest.Id,
            ReportId = supportRequest.ReportId,
            Description = supportRequest.Description,
            Urgency = supportRequest.Urgency,
            Status = supportRequest.Status,
            UserId = supportRequest.UserId,
            CreatedAt = supportRequest.CreatedAt,
            UpdatedAt = supportRequest.UpdatedAt,
            UserName = supportRequest.User?.Name,
            UserEmail = supportRequest.User?.Email,
            ReportTitle = supportRequest.Report?.Title,
            SupportTypes = supportRequest.SupportTypes?.Select(st => new SupportTypeDto
            {
                Id = st.Id,
                Name = st.Name
            }).ToList() ?? new List<SupportTypeDto>()
        };
    }
}