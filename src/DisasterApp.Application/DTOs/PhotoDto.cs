using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace DisasterApp.Application.DTOs
{
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
