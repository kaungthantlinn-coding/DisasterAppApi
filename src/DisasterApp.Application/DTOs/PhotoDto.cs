using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace DisasterApp.Application.DTOs
{

    public class PhotoDto
    {
        public int Id { get; set; }
        public string ReportId { get; set; } = string.Empty; // Guid string format
        public string Url { get; set; } = string.Empty;
        public string? Caption { get; set; }
        public string UploadedAt { get; set; } = string.Empty; // ISO 8601 string date/time
    }
    public class CreatePhotoDto
    {
        public Guid ReportId { get; set; }
        public IFormFile File { get; set; } = null!;
        public string? Caption { get; set; }
    }

    public class UpdatePhotoDto
    {
        public int Id { get; set; }
        public Guid ReportId { get; set; }
        public IFormFile? File { get; set; } // Optional new photo
        public string? Caption { get; set; }
    }


}
