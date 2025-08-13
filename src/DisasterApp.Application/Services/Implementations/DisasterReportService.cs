using DisasterApp.Application.DTOs;
using DisasterApp.Domain.Entities;
using DisasterApp.Domain.Enums;
using DisasterApp.Infrastructure.Data;
using DisasterApp.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Http;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DisasterApp.Application.Services.Implementations
{
    public class DisasterReportService(
        IDisasterReportRepository repository,
        IPhotoService photoService,
        IHttpClientFactory httpClientFactory,
        IDisasterTypeRepository disasterTypeRepository,
        IImpactTypeRepository impactTypeRepository) : IDisasterReportService
    {
        private readonly IDisasterReportRepository _repository = repository;
        private readonly IDisasterTypeRepository _disasterTypeRepository = disasterTypeRepository;
        private readonly IImpactTypeRepository _impactTypeReository = impactTypeRepository;
        private readonly HttpClient _httpClient = httpClientFactory.CreateClient("Nominatim");
        private readonly IPhotoService _photoService = photoService;
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

                //if (!await _context.DisasterTypes.AnyAsync(t => t.Id == disasterTypeId))
                //    throw new Exception($"DisasterType with Id {disasterTypeId} not found.");

                if (string.IsNullOrWhiteSpace(dto.DisasterEventName))
                    throw new Exception("DisasterEventName is required");

                var disasterEvent = new DisasterEvent
                {
                    Id = Guid.NewGuid(),
                    Name = dto.DisasterEventName,
                    DisasterTypeId = disasterTypeId
                };

                
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
                    ImpactDetails = [],
                Photos = [],


                };


                var location = new Location
                {
                    LocationId = Guid.NewGuid(),
                    ReportId = report.Id,
                    Latitude = dto.Latitude,
                    Longitude = dto.Longitude,
                    Address = address,
                    FormattedAddress = address,

                    CoordinatePrecision = dto.CoordinatePrecision,
                    Report = report
                };
                               

                foreach (var impactDto in dto.ImpactDetails)
                {
                    var impactDetail = new ImpactDetail
                    {
                        Description = impactDto.Description,
                        Severity = impactDto.Severity,
                        ImpactTypes = []
                    };

                    foreach (var impactTypeId in impactDto.ImpactTypeIds)
                    {
                        var impactType = await _impactTypeReository.GetByIdAsync(impactTypeId) ?? throw new Exception($"ImpactType with ID {impactTypeId} not found");
                        impactDetail.ImpactTypes.Add(impactType);
                    }

                    report.ImpactDetails.Add(impactDetail);
                }
                await _repository.CreateAsync(report, location);
              
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
                //Location = r.Location != null ? new LocationDto
                //{
                //    Latitude = r.Location.Latitude,
                //    Longitude = r.Location.Longitude,
                //    Address = r.Location.Address,
                //    FormattedAddress = r.Location.FormattedAddress,
                //    CoordinatePrecision = r.Location.CoordinatePrecision
                //} : null,

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
                DisasterEventId = report.DisasterEventId,
                DisasterEventName = report.DisasterEvent?.Name ?? string.Empty,
                DisasterTypeName = report.DisasterEvent?.DisasterType?.Name ?? string.Empty,
                UserId = report.UserId,
                // Latitude = report.Location?.Latitude,
                // Longitude = report.Location?.Longitude,
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
            return dto;
        }

        //     public async Task<DisasterReportDto?> UpdateAsync(Guid id, DisasterReportUpdateDto dto, Guid userId)
        //     {
        //        using var transaction = await _context.Database.BeginTransactionAsync();

        //        try
        //        {
        //            var report = await _context.DisasterReports
        //                .Include(r => r.ImpactDetails)
        //                .Include(r => r.Photos)
        //                .Include(r=>r.DisasterEvent)
        //                .ThenInclude(e=>e.DisasterType)
        //                .FirstOrDefaultAsync(r => r.Id == id && r.IsDeleted !=true);

        //            if (report == null)
        //                throw new Exception($"Report with Id {id} not found.");

        //            // Validate user
        //            if (report.UserId != userId)
        //                throw new Exception("You are not authorized to update this report.");

        //            // Update main fields
        //            report.Title = dto.Title ?? report.Title;
        //            report.Description = dto.Description ?? report.Description;
        //            report.Severity = dto.Severity ?? report.Severity;
        //            report.Timestamp = dto.Timestamp ?? report.Timestamp;
        //            report.Severity=dto.Severity ?? report.Severity;
        //            report.Status = dto.Status ?? report.Status;
        //            report.UpdatedAt = DateTime.UtcNow;

        //            if (report.Location != null)
        //            {
        //                report.Location.Latitude = dto.Latitude;
        //                report.Location.Longitude = dto.Longitude;

        //                var address = string.IsNullOrWhiteSpace(dto.Address)
        //                    ? await ReverseGeocodeAsync(dto.Latitude, dto.Longitude)
        //                    : dto.Address;

        //                report.Location.Address = address;
        //            }
        //            // Update DisasterEventName if provided
        //            if (!string.IsNullOrWhiteSpace(dto.DisasterEventName))
        //            {
        //                int disasterTypeId = dto.DisasterTypeId ?? report.DisasterEvent!.DisasterTypeId;
        //                if (disasterTypeId == 0 && !string.IsNullOrWhiteSpace(dto.NewDisasterTypeName))
        //                    {
        //                    var newType = new DisasterType
        //                    {
        //                        Name = dto.NewDisasterTypeName,
        //                        Category = dto.DisasterCategory!.Value
        //                    };
        //                    _context.DisasterTypes.Add(newType);
        //                    await _context.SaveChangesAsync();
        //                    disasterTypeId = newType.Id;
        //                }

        //                report.DisasterEvent!.Name = dto.DisasterEventName;
        //                report.DisasterEvent.DisasterTypeId = disasterTypeId;
        //            }

        //                // Update ImpactDetails
        //                if (dto.ImpactDetails != null)
        //            {

        //                _context.ImpactDetails.RemoveRange(report.ImpactDetails);                    
        //                report.ImpactDetails.Clear();

        //                foreach (var impactDto in dto.ImpactDetails)
        //                {
        //                    int impactTypeId = impactDto.ImpactTypeId.GetValueOrDefault();
        //                    if (impactTypeId == 0 && !string.IsNullOrWhiteSpace(impactDto.ImpactTypeName))
        //                    {
        //                        var existingImpact = await _context.ImpactTypes
        //                            .FirstOrDefaultAsync(t => t.Name == impactDto.ImpactTypeName);

        //                        if (existingImpact != null)
        //                            impactTypeId = existingImpact.Id;
        //                        else
        //                        {
        //                            var newImpactType = new ImpactType
        //                            {
        //                                Name = impactDto.ImpactTypeName
        //                            };
        //                            _context.ImpactTypes.Add(newImpactType);
        //                            await _context.SaveChangesAsync();
        //                            impactTypeId = newImpactType.Id;
        //                        }
        //                    }

        //                    report.ImpactDetails.Add(new ImpactDetail
        //                    {
        //                        ReportId = report.Id,
        //                        ImpactTypeId = impactTypeId,
        //                        Description = impactDto.Description,
        //                        Severity = impactDto.Severity,
        //                        IsResolved = impactDto.IsResolved
        //                    });
        //                }
        //            }
        //            if (dto.NewPhotos != null && dto.NewPhotos.Any())
        //            {
        //                foreach (var file in dto.NewPhotos)
        //                {
        //                    var photo = await _photoService.UploadPhotoAsync(new CreatePhotoDto
        //                    {
        //                        File = file,
        //                        ReportId = report.Id
        //                    });
        //                    report.Photos.Add(photo);
        //                }
        //            }

        //            // 5️⃣ Handle Photo Deletions
        //            if (dto.RemovePhotoIds != null && dto.RemovePhotoIds.Any())
        //            {
        //                foreach (var photoId in dto.RemovePhotoIds)
        //                {
        //                    var photo = report.Photos.FirstOrDefault(p => p.Id == photoId);
        //                    if (photo != null)
        //                    {
        //                        await _photoService.DeletePhotoAsync(photoId);
        //                        report.Photos.Remove(photo);
        //                    }
        //                }
        //            }

        //            await _context.SaveChangesAsync();
        //            await transaction.CommitAsync();

        //            return new DisasterReportDto
        //            {
        //                Id = report.Id,
        //                Title = report.Title,
        //                Description = report.Description,
        //                Timestamp = report.Timestamp,
        //                Severity = report.Severity,
        //                Status = report.Status,
        //                DisasterEventId = report.DisasterEventId,
        //                DisasterEventName = report.DisasterEvent?.Name ?? string.Empty,
        //                DisasterTypeName = report.DisasterEvent?.DisasterType?.Name ?? string.Empty,

        //                UserId = report.UserId,
        //                ImpactDetails = report.ImpactDetails.Select(i => new ImpactDetailDto
        //                {
        //                    ImpactTypeNames = i.ImpactType != null ? new List<string> { i.ImpactType.Name } : new List<string>(),
        //                    Description = i.Description,
        //                    Severity = i.Severity,
        //                    IsResolved = i.IsResolved
        //                }).ToList(),
        //                PhotoUrls = report.Photos.Select(p => p.Url).ToList()
        //            };
        //        }
        //        catch (Exception ex)
        //        {
        //            await transaction.RollbackAsync();
        //            Console.WriteLine($"[ERROR] UpdateAsync failed: {ex}");
        //            throw;
        //        }
        //     }

        //public async Task<bool> DeleteAsync(Guid id)
        //{
        //    var report = await _context.DisasterReports
        //        .Include(r => r.ImpactDetails)
        //        .Include(r => r.Location)
        //        .Include(r => r.Photos)
        //        .FirstOrDefaultAsync(r => r.Id == id);

        //    if (report == null) return false;
        //    // 1️⃣ Delete related photos from Cloudinary and DB
        //    foreach (var photo in report.Photos.ToList())
        //    {
        //        await _photoService.DeletePhotoAsync(photo.Id);
        //        _context.Photos.Remove(photo);
        //    }


        //    _context.ImpactDetails.RemoveRange(report.ImpactDetails);
        //    if (report.Location != null)
        //        _context.Locations.Remove(report.Location);

        //    _context.DisasterReports.Remove(report);
        //    await _context.SaveChangesAsync();

        //    return true;
        //}
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

        public async Task<DisasterReportDto?> UpdateAsync(Guid id, DisasterReportUpdateDto dto, Guid userId)
        {
            var report = await _repository.GetByIdAsync(id);
            if(report == null || report.IsDeleted == true)
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

        public Task<bool> DeleteAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public async Task<object> GetStatisticsAsync()
        {
            var reports = await _repository.GetAllAsync();
            
            var totalReports = reports.Count();
            var reportsByStatus = reports.GroupBy(r => r.Status)
                .ToDictionary(g => g.Key, g => g.Count());
            var reportsBySeverity = reports.GroupBy(r => r.Severity)
                .ToDictionary(g => g.Key, g => g.Count());
            var recentReports = reports.Count(r => r.Timestamp >= DateTime.UtcNow.AddDays(-7));
            
            return new
            {
                TotalReports = totalReports,
                ReportsByStatus = reportsByStatus,
                ReportsBySeverity = reportsBySeverity,
                RecentReports = recentReports,
                LastUpdated = DateTime.UtcNow
            };
        }
    }
}
