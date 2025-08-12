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
        private readonly IImpactTypeRepository _impactTypeRepository = impactTypeRepository;
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
                    ImpactDetails = new List<ImpactDetail>(),
                    Photos = new List<Photo>(),
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

                // Process impact details
                foreach (var impactDto in dto.ImpactDetails)
                {
                    var impactDetail = new ImpactDetail
                    {
                        ReportId = report.Id,
                        Description = impactDto.Description,
                        Severity = impactDto.Severity,
                        ImpactTypes = new List<ImpactType>()
                    };

                    // Add impact types to the impact detail
                    foreach (var impactTypeId in impactDto.ImpactTypeIds)
                    {
                        var impactType = await _impactTypeRepository.GetByIdAsync(impactTypeId);
                        if (impactType == null)
                            throw new Exception($"ImpactType with ID {impactTypeId} not found");
                        
                        impactDetail.ImpactTypes.Add(impactType);
                    }

                    report.ImpactDetails.Add(impactDetail);
                }

                // Save the report
                await _repository.CreateAsync(report, location);

                // Handle photo uploads
                var uploadedPhotoUrls = new List<string>();
                if (dto.Photos != null && dto.Photos.Any())
                {
                    foreach (var file in dto.Photos)
                    {
                        var photo = await _photoService.UploadPhotoAsync(new CreatePhotoDto
                        {
                            File = file,
                            ReportId = report.Id
                        });
                        uploadedPhotoUrls.Add(photo.Url);
                        report.Photos.Add(photo);
                    }
                    await _repository.UpdateAsync(report);
                }

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
                    Address = address,
                    ImpactDetails = report.ImpactDetails.Select(i => new ImpactDetailDto
                    {
                        Id = i.Id,
                        Description = i.Description,
                        Severity = i.Severity,
                        IsResolved = i.IsResolved,
                        ImpactTypes = i.ImpactTypes.Select(t => new ImpactTypeDto
                        {
                            Id = t.Id,
                            Name = t.Name
                        }).ToList()
                    }).ToList(),
                    PhotoUrls = uploadedPhotoUrls
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
                Latitude = r.Location?.Latitude ?? 0m,
                Longitude = r.Location?.Longitude ?? 0m,
                Address = r.Location?.Address,
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

            return result;
        }

        public async Task<DisasterReportDto?> GetByIdAsync(Guid id)
        {
            var report = await _repository.GetByIdAsync(id);
            if (report == null || report.IsDeleted == true) 
                return null;

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
                Latitude = report.Location?.Latitude ?? 0m,
                Longitude = report.Location?.Longitude ?? 0m,
                Address = report.Location?.Address,
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

        public async Task<IEnumerable<DisasterReportSearchDto>> SearchAsync(string keyword)
        {
            var reports = await _repository.SearchAsync(keyword);
            return reports.Select(r => new DisasterReportSearchDto
            {
                Id = r.Id,
                Title = r.Title,
                Description = r.Description,
                Timestamp = r.Timestamp,
                Severity = r.Severity.ToString(),
                Status = r.Status.ToString(),
                DisasterEventName = r.DisasterEvent?.Name ?? string.Empty,
                LocationAddress = r.Location?.Address ?? string.Empty
            });
        }

        public async Task<DisasterReportDto?> UpdateAsync(Guid id, DisasterReportUpdateDto dto, Guid userId)
        {
            var report = await _repository.GetByIdAsync(id);
            if (report == null || report.IsDeleted == true)
                return null;

            if (report.UserId != userId)
                throw new UnauthorizedAccessException("You are not authorized to update this report.");

            // Update basic properties
            report.Title = dto.Title ?? report.Title;
            report.Description = dto.Description ?? report.Description;
            report.Severity = dto.Severity ?? report.Severity;
            report.Timestamp = dto.Timestamp ?? report.Timestamp;
            report.Status = dto.Status ?? report.Status;
            report.UpdatedAt = DateTime.UtcNow;

            // Update location if provided
            if (report.Location != null)
            {
                report.Location.Latitude = dto.Latitude;
                report.Location.Longitude = dto.Longitude;

                var address = string.IsNullOrWhiteSpace(dto.Address)
                    ? await ReverseGeocodeAsync(dto.Latitude, dto.Longitude)
                    : dto.Address;

                report.Location.Address = address;
            }

            // Update impact details
            if (dto.ImpactDetails != null)
            {
                report.ImpactDetails.Clear();
                foreach (var impactDto in dto.ImpactDetails)
                {
                    var impactDetail = new ImpactDetail
                    {
                        ReportId = report.Id,
                        ImpactTypes = new List<ImpactType>(),
                        Description = impactDto.Description,
                        Severity = impactDto.Severity,
                        IsResolved = impactDto.IsResolved
                    };

                    if (impactDto.ImpactTypeIds != null)
                    {
                        foreach (var impactTypeId in impactDto.ImpactTypeIds)
                        {
                            var impactType = await _impactTypeRepository.GetByIdAsync(impactTypeId);
                            if (impactType == null)
                            {
                                throw new Exception($"ImpactType with ID {impactTypeId} not found");
                            }
                            impactDetail.ImpactTypes.Add(impactType);
                        }
                    }
                    report.ImpactDetails.Add(impactDetail);
                }
            }

            // Handle new photos
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

            // Handle photo deletions
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

        public async Task<bool> DeleteAsync(Guid id)
        {
            var report = await _repository.GetByIdAsync(id);
            if (report == null)
                return false;

            // Delete related photos
            if (report.Photos != null && report.Photos.Any())
            {
                foreach (var photo in report.Photos.ToList())
                {
                    await _photoService.DeletePhotoAsync(photo.Id);
                }
            }

            // Soft delete the report
            report.IsDeleted = true;
            report.UpdatedAt = DateTime.UtcNow;
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

            GeocodeCache[(lat, lng)] = address!;
            return address;
        }
    }
}