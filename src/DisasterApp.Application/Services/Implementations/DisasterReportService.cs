using DisasterApp.Application.DTOs;
using DisasterApp.Application.Services.Interfaces;
using DisasterApp.Domain.Entities;
using DisasterApp.Domain.Enums;
using DisasterApp.Infrastructure.Data;
using DisasterApp.Infrastructure.Repositories;
using DisasterApp.Infrastructure.Repositories.Implementations;
using DisasterApp.Infrastructure.Repositories.Interfaces;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using OfficeOpenXml;
using System.Collections.Concurrent;
using System.Text.Json;
using Paragraph = iText.Layout.Element.Paragraph;



namespace DisasterApp.Application.Services
{
    public class DisasterReportService(
        IDisasterReportRepository repository,
        IPhotoService photoService,
        IHttpClientFactory httpClientFactory,
        IDisasterTypeRepository disasterTypeRepository,
        IImpactTypeRepository impactTypeRepository,
        IUserRepository userRepository,
        INotificationService notificationService,
        IDisasterEventRepository eventRepository) : IDisasterReportService
    {
        private readonly IDisasterReportRepository _repository = repository;
        private readonly IDisasterTypeRepository _disasterTypeRepository = disasterTypeRepository;
        private readonly IDisasterEventRepository _eventRepository = eventRepository;
        private readonly IImpactTypeRepository _impactTypeReository = impactTypeRepository;
        private readonly HttpClient _httpClient = httpClientFactory.CreateClient("Nominatim");
        private readonly IPhotoService _photoService = photoService;
        private readonly IUserRepository _userRepository = userRepository;
        private readonly INotificationService _notificationService = notificationService;
        private static readonly ConcurrentDictionary<(decimal, decimal), string> GeocodeCache = new();
        private static DateTime _lastGeocode = DateTime.MinValue;
        private static readonly System.Threading.Lock ThrottleLock = new();
        private static async Task ThrottleAsync()
        {
            TimeSpan delay;
            lock (ThrottleLock)
            {
                var since = DateTime.UtcNow - _lastGeocode;
                delay = since < TimeSpan.FromSeconds(1) ? TimeSpan.FromSeconds(1) - since : TimeSpan.Zero;
                _lastGeocode = DateTime.UtcNow;
            }
            if (delay > TimeSpan.Zero)
                await Task.Delay(delay);
        }
        public async Task<DisasterReportDto> CreateAsync(DisasterReportCreateDto dto, Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            try
            {

                int disasterTypeId = dto.DisasterTypeId;
                if (disasterTypeId == 0 && !string.IsNullOrWhiteSpace(dto.NewDisasterTypeName))
                {
                    var existingType = await _disasterTypeRepository.GetByNameAsync(dto.NewDisasterTypeName);

                    if (existingType != null)
                    {
                        disasterTypeId = existingType.Id;
                    }
                    else
                    {
                        var newType = new DisasterType
                        {

                            Name = dto.NewDisasterTypeName,
                            Category = dto.DisasterCategory ?? throw new ArgumentException("DisasterCategory cannot be null"),

                        };
                        await _disasterTypeRepository.AddAsync(newType);
                        //await _context.SaveChangesAsync();
                        disasterTypeId = newType.Id;

                    }
                }
                else
                {
                    var exists = await _disasterTypeRepository.ExistsAsync(disasterTypeId);
                    if (!exists) throw new Exception($"DisasterType with Id {disasterTypeId} not found");

                }


                if (string.IsNullOrWhiteSpace(dto.DisasterEventName))
                    throw new Exception("DisasterEventName is required");
                var existingEvent = await _eventRepository.GetByNameAsync(dto.DisasterEventName);
                DisasterEvent disasterEvent;
                if (existingEvent != null)
                {
                    disasterEvent = existingEvent;
                }
                else
                {

                    disasterEvent = new DisasterEvent
                    {
                        Id = Guid.NewGuid(),
                        Name = dto.DisasterEventName,
                        DisasterTypeId = disasterTypeId
                    };
                    await _eventRepository.AddAsync(disasterEvent);
                }


                var address = string.IsNullOrWhiteSpace(dto.Address)
              ? await ReverseGeocodeAsync(dto.Latitude, dto.Longitude)
              : dto.Address;
                var report = new DisasterReport
                {
                    Id = Guid.NewGuid(),
                    Title = dto.Title,
                    Description = dto.Description,
                    Timestamp = dto.Timestamp,
                    Severity = dto.Severity,
                    Status = ReportStatus.Pending,
                    UserId = userId,

                    DisasterEventId = disasterEvent.Id,
                    DisasterEvent = disasterEvent,
                    CreatedAt = DateTime.UtcNow,
                    ImpactDetails = new List<ImpactDetail>(),
                    Photos = new List<Photo>()

                };

                // Process impact details
                foreach (var impactDto in dto.ImpactDetails)
                {
                    var impactDetail = new ImpactDetail
                    {
                        //Id= Guid.NewGuid(), // Ensure a new ID is generated
                        Description = impactDto.Description,
                        Severity = impactDto.Severity,
                        ImpactTypes = new List<ImpactType>()
                    };

                    foreach (var impactTypeId in impactDto.ImpactTypeIds)
                    {
                        var impactType = await _impactTypeReository.GetByIdAsync(impactTypeId) ?? throw new Exception($"ImpactType with ID {impactTypeId} not found");
                        impactDetail.ImpactTypes.Add(impactType);
                    }

                    report.ImpactDetails.Add(impactDetail);
                }

                var location = new Location
                {
                    LocationId = Guid.NewGuid(),
                    ReportId = report.Id,
                    Latitude = dto.Latitude,
                    Longitude = dto.Longitude,
                    Address = address,
                    FormattedAddress = address,
                    Report = report
                };

                await _repository.CreateAsync(report, location);
                await _notificationService.SendReportSubmittedNotificationAsync(report.Id, userId);
                if (dto.Photos != null && dto.Photos.Any())
                {
                    foreach (var file in dto.Photos)
                    {
                        var photo = await _photoService.UploadPhotoAsync(new CreatePhotoDto
                        {
                            File = file,
                            ReportId = report.Id
                        });
                        report.Photos.Add(photo);
                    }
                }
                await _repository.UpdateAsync(report);


                return new DisasterReportDto
                {
                    Id = report.Id,
                    Title = report.Title,
                    Description = report.Description,
                    Timestamp = report.Timestamp,
                    Severity = report.Severity,
                    Status = report.Status,
                    DisasterEventId = report.DisasterEventId,
                    DisasterEventName = report.DisasterEvent?.Name,
                    DisasterTypeId = disasterTypeId,
                    UserId = userId,
                    UserName = user.Name,

                    ImpactDetails = report.ImpactDetails.Select(i => new ImpactDetailDto
                    {
                        Id = i.Id,
                        Severity = i.Severity,
                        IsResolved = i.IsResolved,
                        ImpactTypes = i.ImpactTypes.Select(t => new ImpactTypeDto
                        {
                            Id = t.Id,
                            Name = t.Name
                        }).ToList(),
                        Description = i.Description
                    }).ToList(),
                    PhotoUrls = report.Photos.Select(p => p.Url).ToList(),
                };


            }
            catch (Exception ex)
            {

                Console.WriteLine($"[ERROR] CreateAsync failed: {ex}");
                throw;
            }
        }
        public async Task<IEnumerable<DisasterReportDto>> GetAllAsync()
        {
            var reports = await _repository.GetAllAsync();

            var activeReports = reports.Where(r => r.IsDeleted != true);

            var result = activeReports.Select(r => new DisasterReportDto
            {
                Id = r.Id,
                Title = r.Title,
                Description = r.Description,
                Timestamp = r.Timestamp,
                Severity = r.Severity,
                Status = r.Status,
                DisasterEventId = r.DisasterEventId,
                DisasterEventName = r.DisasterEvent?.Name ?? string.Empty,
                DisasterTypeName = r.DisasterEvent?.DisasterType?.Name ?? string.Empty,
                UserId = r.UserId,
                UserName = r.User.Name,
                Latitude = r.Location?.Latitude ?? 0,
                Longitude = r.Location?.Longitude ?? 0,
                Address = r.Location?.Address ?? string.Empty,

                ImpactDetails = r.ImpactDetails.Select(i => new ImpactDetailDto
                {
                    Description = i.Description,
                    Severity = i.Severity,
                    IsResolved = i.IsResolved,
                    ResolvedAt = i.ResolvedAt,
                    ImpactTypes = i.ImpactTypes.Select(t => new ImpactTypeDto
                    {
                        Id = t.Id,
                        Name = t.Name
                    }).ToList()

                }).ToList(),
                PhotoUrls = r.Photos.Select(p => p.Url).ToList()
            }).ToList();
            return result;
        }

        public async Task<IEnumerable<DisasterReportDto>> GetAcceptedReportsAsync()
        {
            var reports = await _repository.GetAllAsync();

            // Only include reports that are not deleted and have Status = Verified
            var acceptedReports = reports
                .Where(r => r.IsDeleted != true && r.Status == ReportStatus.Verified)
                .Select(r => new DisasterReportDto
                {
                    Id = r.Id,
                    Title = r.Title,
                    Description = r.Description,
                    Timestamp = r.Timestamp,
                    Severity = r.Severity,
                    Status = r.Status,
                    DisasterEventId = r.DisasterEventId,
                    DisasterEventName = r.DisasterEvent?.Name ?? string.Empty,
                    DisasterTypeName = r.DisasterEvent?.DisasterType?.Name ?? string.Empty,
                    UserId = r.UserId,
                    UserName = r.User.Name ?? string.Empty,
                    Longitude = r.Location?.Longitude ?? 0,
                    Latitude = r.Location?.Latitude ?? 0,
                    Address = r.Location?.Address ?? string.Empty,
                    ImpactDetails = r.ImpactDetails.Select(i => new ImpactDetailDto
                    {
                        Id = i.Id,
                        Description = i.Description,
                        Severity = i.Severity,
                        IsResolved = i.IsResolved,
                        ResolvedAt = i.ResolvedAt,
                        ImpactTypes = i.ImpactTypes.Select(t => new ImpactTypeDto
                        {
                            Id = t.Id,
                            Name = t.Name
                        }).ToList()
                    }).ToList(),
                    PhotoUrls = r.Photos.Select(p => p.Url).ToList()
                }).ToList();

            return acceptedReports;
        }

        public async Task<IEnumerable<DisasterReportDto>> GetRejectedReportsAsync()
        {
            var reports = await _repository.GetAllAsync();

            // Only include reports that are not deleted and have Status = Rejected
            var rejectedReports = reports
                .Where(r => r.IsDeleted != true && r.Status == ReportStatus.Rejected)
                .Select(r => new DisasterReportDto
                {
                    Id = r.Id,
                    Title = r.Title,
                    Description = r.Description,
                    Timestamp = r.Timestamp,
                    Severity = r.Severity,
                    Status = r.Status,
                    DisasterEventId = r.DisasterEventId,
                    DisasterEventName = r.DisasterEvent?.Name ?? string.Empty,
                    DisasterTypeName = r.DisasterEvent?.DisasterType?.Name ?? string.Empty,
                    UserId = r.UserId,
                    UserName = r.User.Name ?? string.Empty,
                    Longitude = r.Location?.Longitude ?? 0,
                    Latitude = r.Location?.Latitude ?? 0,
                    Address = r.Location?.Address ?? string.Empty,
                    ImpactDetails = r.ImpactDetails.Select(i => new ImpactDetailDto
                    {
                        Id = i.Id,
                        Description = i.Description,
                        Severity = i.Severity,
                        IsResolved = i.IsResolved,
                        ResolvedAt = i.ResolvedAt,
                        ImpactTypes = i.ImpactTypes.Select(t => new ImpactTypeDto
                        {
                            Id = t.Id,
                            Name = t.Name
                        }).ToList()
                    }).ToList(),
                    PhotoUrls = r.Photos.Select(p => p.Url).ToList()
                }).ToList();

            return rejectedReports;
        }


        public async Task<DisasterReportDto?> GetByIdAsync(Guid id)
        {
            var report = await _repository.GetByIdAsync(id);
            if (report == null || report.IsDeleted == true) return null;

            var dto = new DisasterReportDto
            {
                Id = report.Id,
                Title = report.Title,
                Description = report.Description,
                Timestamp = report.Timestamp,
                Severity = report.Severity,
                Status = report.Status,
                DisasterTypeId = report.DisasterEvent.DisasterTypeId,
                DisasterCategory=report.DisasterEvent.DisasterType.Category,
                DisasterEventId = report.DisasterEventId,
                DisasterEventName = report.DisasterEvent?.Name ?? string.Empty,
                DisasterTypeName = report.DisasterEvent?.DisasterType?.Name ?? string.Empty,
                UserId = report.UserId,
                UserName = report.User.Name ?? string.Empty,
                UserEmail = report.User.Email,
                Longitude = report.Location?.Longitude ?? 0,
                Latitude = report.Location?.Latitude ?? 0,
                Address = report.Location?.Address ?? string.Empty,
                ImpactDetails = report.ImpactDetails.Select(i => new ImpactDetailDto
                {
                    Id = i.Id,
                    Description = i.Description,
                    Severity = i.Severity,
                    IsResolved = i.IsResolved,
                    ResolvedAt = i.ResolvedAt,
                    ImpactTypeIds = i.ImpactTypes.Select(t => t.Id).ToList(),
                    ImpactTypes = i.ImpactTypes.Select(t => new ImpactTypeDto
                    {
                        Id = t.Id,
                        Name = t.Name
                    }).ToList()
                }).ToList(),
                PhotoUrls = report.Photos.Select(p => p.Url).ToList()

            };
            return dto;
        }

        public async Task<IEnumerable<DisasterReportDto>> GetReportsByUserIdAsync(Guid userId)
        {
            var reports = await _repository.GetReportsByUserIdAsync(userId);
            var activeReports = reports.Where(r => r.IsDeleted != true);
            return activeReports.Select(r => new DisasterReportDto
            {
                Id = r.Id,
                Title = r.Title,
                Description = r.Description,
                Timestamp = r.Timestamp,
                Severity = r.Severity,
                Status = r.Status,
                DisasterEventId = r.DisasterEventId,
                DisasterEventName = r.DisasterEvent?.Name ?? string.Empty,
                DisasterTypeName = r.DisasterEvent?.DisasterType?.Name ?? string.Empty,
                UserId = r.UserId,
                UserName = r.User.Name ?? string.Empty,
                Latitude = r.Location?.Latitude ?? 0,
                Longitude = r.Location?.Longitude ?? 0,
                Address = r.Location?.Address ?? string.Empty,
                ImpactDetails = r.ImpactDetails.Select(i => new ImpactDetailDto
                {
                    Id = i.Id,
                    Description = i.Description,
                    Severity = i.Severity,
                    IsResolved = i.IsResolved,
                    ResolvedAt = i.ResolvedAt,
                    ImpactTypes = i.ImpactTypes.Select(t => new ImpactTypeDto
                    {
                        Id = t.Id,
                        Name = t.Name
                    }).ToList()
                }).ToList(),
                PhotoUrls = r.Photos.Select(p => p.Url).ToList()
            }).ToList();
        }
        public async Task<IEnumerable<DisasterReportDto>> GetPendingReportsForAdminAsync(Guid adminUserId)
        {
            var adminUser = await _userRepository.GetByIdAsync(adminUserId);
            if (adminUser == null || !adminUser.Roles.Any(r => r.Name == "admin"))
                throw new UnauthorizedAccessException("Only admins can view pending reports.");

            var reports = await _repository.GetPendingReportsAsync();
            return reports.Select(r => new DisasterReportDto
            {
                Id = r.Id,
                Title = r.Title,
                Description = r.Description,
                Severity = r.Severity,
                Status = r.Status,
                DisasterEventName = r.DisasterEvent.Name,
                Timestamp = r.Timestamp,
                Latitude = r.Location?.Latitude ?? 0,
                Longitude = r.Location?.Longitude ?? 0,
                Address = r.Location?.Address ?? string.Empty,
                DisasterTypeName = r.DisasterEvent.DisasterType.Name,
                UserId = adminUserId,
                UserName = r.User.Name ?? string.Empty,
                PhotoUrls = r.Photos.Select(p => p.Url).ToList(),
                ImpactDetails = r.ImpactDetails.Select(i => new ImpactDetailDto
                {
                    Id = i.Id,
                    Description = i.Description,
                    Severity = i.Severity,
                    IsResolved = i.IsResolved,
                    ResolvedAt = i.ResolvedAt,
                    ImpactTypes = i.ImpactTypes.Select(t => new ImpactTypeDto
                    {
                        Id = t.Id,
                        Name = t.Name
                    }).ToList()
                }).ToList(),

            }).ToList();
        }

        public async Task<bool> ApproveDisasterReportAsync(Guid id, Guid adminUserId)
        {
            var adminUser = await _userRepository.GetByIdAsync(adminUserId);
            if (adminUser == null || !adminUser.Roles.Any(r => r.Name == "admin"))
                throw new UnauthorizedAccessException("Only admins can update report status.");

            var success= await _repository.UpdateStatusAsync(id, ReportStatus.Verified, adminUserId);
          if(success)
            {
                var report = await _repository.GetByIdAsync(id);
                if(report != null)
                {
                    await _notificationService.SendEmailAcceptedNotificationAsync(report);    
                       
                }
            }
            
            return success;
        }

        public async Task<bool> RejectDisasterReportAsync(Guid reportId, Guid adminUserId)
        {
            var adminUser = await _userRepository.GetByIdAsync(adminUserId);
            if (adminUser == null || !adminUser.Roles.Any(r => r.Name == "admin"))
                throw new UnauthorizedAccessException("Only admins can reject reports");
            return await _repository.UpdateStatusAsync(reportId, ReportStatus.Rejected, adminUserId);
        }

        public async Task<bool> ApproveOrRejectReportAsync(Guid id, ReportStatus status, Guid adminUserId)
        {
            var adminUser = await _userRepository.GetByIdAsync(adminUserId);
            if (adminUser == null || !adminUser.Roles.Any(r => r.Name == "admin"))
                throw new UnauthorizedAccessException("Only admins can update report status.");

            if (status != ReportStatus.Verified && status != ReportStatus.Rejected)
                throw new ArgumentException("Status must be Accepted or Rejected");

            return await _repository.UpdateStatusAsync(id, status, adminUserId);
        }

        public async Task<DisasterReportDto?> UpdateAsync(Guid id, DisasterReportUpdateDto dto, Guid userId)
        {
            var report = await _repository.GetByIdAsync(id);
            if (report == null || report.IsDeleted == true)
                return null;

            if (report.UserId != userId)
                throw new UnauthorizedAccessException("You are not authorized to update this report.");

            report.Title = dto.Title ?? report.Title;
            report.Description = dto.Description ?? report.Description;
            report.Severity = dto.Severity ?? report.Severity;
            report.Timestamp = dto.Timestamp ?? report.Timestamp;
            report.Status = dto.Status ?? report.Status;
            report.UpdatedAt = DateTime.UtcNow;

            if (report.Location != null)
            {
                report.Location.Latitude = dto.Latitude;
                report.Location.Longitude = dto.Longitude;

                var address = string.IsNullOrWhiteSpace(dto.Address)
                    ? await ReverseGeocodeAsync(dto.Latitude, dto.Longitude)
                    : dto.Address;

                report.Location.Address = address;
            }
            if (dto.ImpactDetails != null)
            {
                report.ImpactDetails.Clear();
                foreach (var impactDto in dto.ImpactDetails)
                {
                    var impactDetail = new ImpactDetail
                    {
                        //Id = Guid.NewGuid(), // Ensure a new ID is generated
                        ImpactTypes = new List<ImpactType>(),
                        Description = impactDto.Description,
                        Severity = impactDto.Severity,
                        IsResolved = impactDto.IsResolved
                    };
                    foreach (var impactTypeId in impactDto.ImpactTypeIds)
                    {
                        var impactType = await _impactTypeReository.GetByIdAsync(impactTypeId);
                        if (impactType == null)
                        {
                            throw new Exception($"ImpactType with ID {impactTypeId} not found");
                        }
                        impactDetail.ImpactTypes.Add(impactType);
                    }
                    report.ImpactDetails.Add(impactDetail);
                }
            }
            if (dto.NewPhotos != null && dto.NewPhotos.Any())
            {
                foreach (var file in dto.NewPhotos)
                {
                    var photo = await _photoService.UploadPhotoAsync(new CreatePhotoDto
                    {
                        File = file,
                        ReportId = report.Id
                    });
                    report.Photos.Add(photo);
                }
            }

            // 5️⃣ Handle Photo Deletions
            if (dto.RemovePhotoIds != null && dto.RemovePhotoIds.Any())
            {
                foreach (var photoId in dto.RemovePhotoIds)
                {
                    var photo = report.Photos.FirstOrDefault(p => p.Id == photoId);
                    if (photo != null)
                    {
                        await _photoService.DeletePhotoAsync(photoId);
                        report.Photos.Remove(photo);
                    }
                }
            }

            await _repository.UpdateAsync(report);
            return new DisasterReportDto
            {
                Id = report.Id,
                Title = report.Title,
                Description = report.Description,
                Timestamp = report.Timestamp,
                Severity = report.Severity,
                Status = report.Status,
                DisasterEventId = report.DisasterEventId,
                DisasterEventName = report.DisasterEvent?.Name ?? string.Empty,
                DisasterTypeName = report.DisasterEvent?.DisasterType?.Name ?? string.Empty,
                UserId = report.UserId,
                UserName = report.User?.Name ?? string.Empty,
                Longitude = report.Location?.Longitude ?? 0,
                Latitude = report.Location?.Latitude ?? 0,
                Address = report.Location?.Address ?? string.Empty,
                ImpactDetails = report.ImpactDetails.Select(i => new ImpactDetailDto
                {
                    Id = i.Id,
                    Description = i.Description,
                    Severity = i.Severity,
                    IsResolved = i.IsResolved,
                    ResolvedAt = i.ResolvedAt,
                    ImpactTypes = i.ImpactTypes.Select(t => new ImpactTypeDto
                    {
                        Id = t.Id,
                        Name = t.Name
                    }).ToList()
                }).ToList(),
                PhotoUrls = report.Photos.Select(p => p.Url).ToList()
            };
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var report = await _repository.GetByIdAsync(id);
            if (report == null || report.IsDeleted == true)
                return false;

            // Mark as deleted
            report.IsDeleted = true;
            report.UpdatedAt = DateTime.UtcNow; // optional if you track delete timestamp

            await _repository.UpdateAsync(report);
            return true;
        }

        private async Task<string> ReverseGeocodeAsync(decimal lat, decimal lng)
        {
            if (GeocodeCache.TryGetValue((lat, lng), out var cachedAddress))
                return cachedAddress;

            await ThrottleAsync();

            var response = await _httpClient.GetAsync($"reverse?lat={lat}&lon={lng}&format=json");
            if (!response.IsSuccessStatusCode)
                return "Unknown Address";

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var address = doc.RootElement.GetProperty("display_name").GetString() ?? "Unknown Address";

            GeocodeCache[(lat, lng)] = address;
            return address;
        }    

       
    }
}