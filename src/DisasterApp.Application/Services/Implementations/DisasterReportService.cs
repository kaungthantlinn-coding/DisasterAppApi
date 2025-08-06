using DisasterApp.Application.DTOs;
using DisasterApp.Domain.Entities;
using DisasterApp.Infrastructure.Repositories;
using Google.Apis.Http;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static DisasterApp.Application.DTOs.DisasterReportDto;
using IHttpClientFactory = System.Net.Http.IHttpClientFactory;

namespace DisasterApp.Application.Services
{
    public class DisasterReportService : IDisasterReportService
    {
        private readonly IDisasterReportRepository _reportRepo;
        private readonly HttpClient _httpClient;
        private static readonly ConcurrentDictionary<(decimal, decimal), string> GeocodeCache = new();
        private static DateTime _lastGeocode=DateTime.MinValue;
        private static readonly object ThrottleLock = new();
        public DisasterReportService(IDisasterReportRepository reportRepo,IHttpClientFactory httpClientFactory)
        {
            _reportRepo = reportRepo;
            _httpClient = httpClientFactory.CreateClient("Nominatim");
        }

        private async Task ThrottleAsync()
        {
            lock (ThrottleLock)
            {
                var since = DateTime.UtcNow - _lastGeocode;
                if (since < TimeSpan.FromSeconds(1))
                    Task.Delay(TimeSpan.FromSeconds(1) - since).Wait();
                _lastGeocode = DateTime.UtcNow;
            }
        }
        public async Task<List<DisasterReportResponseDto>> GetAllAsync()
        {
           var reports=await _reportRepo.GetAllReportsAsync();
            var report=reports.
                Where(r=> r.IsDeleted == false || r.IsDeleted==null)
                .Select(r => new DisasterReportResponseDto
                {
                    Id = r.Id,
                    Title = r.Title,
                    Description = r.Description,
                    Timestamp = r.Timestamp,
                    Severity = r.Severity,
                    DisasterEventName = r.DisasterEvent.Name,
                    UserName = r.User.Name,
                    Latitude = r.Location.Latitude,
                    Longitude = r.Location.Longitude,
                    Address = r.Location.Address


                }).ToList();
            return report;
        }
        public async Task<Guid> CreateReportAsync(DisasterReportDto.ReportCreateDto dto)
        {
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
                Status = dto.Status,
                
                UserId = dto.UserId,
                DisasterEventId = dto.DisasterEventID,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
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

            return await _reportRepo.AddReportWithLocationAsync(report, location);
        }

        public async Task<bool> DeleteReportAsync(Guid id)
        {
            var report = await _reportRepo.GetByIdAsync(id);
            if (report == null || report.IsDeleted == true)
                return false;
            await _reportRepo.DeleteAsync(report);
            return true;
        }

        

        public async Task<bool> UpdateAsync(Guid id, DisasterReportUpdateDto dto)
        {
            var report = await _reportRepo.GetByIdAsync(id);
            if (report == null || report.IsDeleted == true)
                return false;

            report.Title = dto.Title;
            report.Description = dto.Description;
            report.Severity = dto.Severity;
            report.Timestamp = dto.Timestamp;

            if (report.Location != null)
            {
                report.Location.Latitude = dto.Latitude;
                report.Location.Longitude = dto.Longitude;

                var address = string.IsNullOrWhiteSpace(dto.Address)
                    ? await ReverseGeocodeAsync(dto.Latitude, dto.Longitude)
                    : dto.Address;

                report.Location.Address = address;
            }

            await _reportRepo.UpdateAsync(report);
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

        public async Task<IEnumerable<DisasterReportSearchDto>> SearchAsync(string keyword)
        {
            var reports=await _reportRepo.SearchAsync(keyword);
            return reports.Select(r => new DisasterReportSearchDto
            {
                Id = r.Id,
                Title = r.Title,
                Description = r.Description,
                Timestamp = r.Timestamp,
                Severity = r.Severity.ToString(),
                Status = r.Status.ToString(),
                DisasterEventName = r.DisasterEvent?.Name?? string.Empty,


                LocationAddress = r.Location?.Address?? string.Empty
            });
        }
    }
}
