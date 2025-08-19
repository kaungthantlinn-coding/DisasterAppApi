using DisasterApp.Domain.Enums;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterApp.Application.DTOs
{
    public class DisasterReportCreateDto
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public SeverityLevel Severity { get; set; }


        public DisasterCategory? DisasterCategory { get; set; }
        public int DisasterTypeId { get; set; }
        public Guid? DisasterEventId { get; set; }

        public string? NewDisasterTypeName { get; set; }
        public string? DisasterEventName { get; set; }

        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string? CoordinatePrecision { get; set; }
        public string? Address { get; set; }
        public List<ImpactDetailCreateDto> ImpactDetails { get; set; } = new List<ImpactDetailCreateDto>();

        public List<IFormFile>? Photos { get; set; }
    }

    public class DisasterReportDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTime Timestamp { get; set; }
        public SeverityLevel Severity { get; set; }
        public ReportStatus Status { get; set; }
        public Guid UserId { get; set; }
        public string? UserName { get; set; }
        public int DisasterTypeId { get; set; }
        public string DisasterTypeName { get; set; }
        public string? NewDisasterTypeName { get; set; }

        public Guid DisasterEventId { get; set; }
        public string? DisasterEventName { get; set; }
        public DisasterCategory? DisasterCategory { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string? CoordinatePrecision { get; set; }
        public string? Address { get; set; }


        //public LocationDto? Location { get; set; }
        public List<ImpactDetailDto> ImpactDetails { get; set; } = new List<ImpactDetailDto>();
        // Photos (return as URLs for the frontend)
        public List<string> PhotoUrls { get; set; } = new();

    }
    public class DisasterReportUpdateDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime? Timestamp { get; set; }
        public SeverityLevel? Severity { get; set; }
        public ReportStatus? Status { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string Address { get; set; } = null!;
        public Guid? DisasterEventId { get; set; }
        public string? DisasterEventName { get; set; }
        public int? DisasterTypeId { get; set; }
        public string? NewDisasterTypeName { get; set; }
        public DisasterCategory? DisasterCategory { get; set; }


        public List<ImpactDetailUpdateDto>? ImpactDetails { get; set; }
        public List<IFormFile>? NewPhotos { get; set; } // For new photos to be added
        public List<int>? RemovePhotoIds { get; set; } // For photos to be removed

    }

    public class UpdateStatusDto
    {
        public ReportStatus Status { get; set; }
    }

}
