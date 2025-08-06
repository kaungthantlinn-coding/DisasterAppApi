using DisasterApp.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterApp.Application.DTOs
{
    public class DisasterReportUpdateDto
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public SeverityLevel Severity { get; set; }
        public DateTime Timestamp { get; set; }

        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string Address { get; set; } = null!;
    }
}
