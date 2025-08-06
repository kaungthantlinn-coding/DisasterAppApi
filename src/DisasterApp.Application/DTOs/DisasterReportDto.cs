using DisasterApp.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterApp.Application.DTOs
{
    public class DisasterReportDto
    {
        public class ReportCreateDto
        {
            public string Title { get; set; } = null!;
            public string Description { get; set; } = null!;
            public DateTime Timestamp { get; set; }
            public SeverityLevel Severity { get; set; }
            public ReportStatus Status { get; set; }
            

            public Guid UserId { get; set; }
            public Guid DisasterEventID { get; set; }

       
            public decimal Latitude { get; set; }
            public decimal Longitude { get; set; }
            public string? CoordinatePrecision { get; set; }
            public string? Address { get; set; }
            
           
        }
        public class DisasterReportResponseDto
        {
            public Guid Id { get; set; }
            public string Title { get; set; } = null!;
            public string Description { get; set; } = null!;
            public DateTime Timestamp { get; set; }
            public SeverityLevel Severity { get; set; }

            public string DisasterEventName { get; set; } = null!;
            public string UserName { get; set; } = null!;

            public decimal Latitude { get; set; }
            public decimal Longitude { get; set; }
            public string Address { get; set; } = null!;
        }
    }
}
