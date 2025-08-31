using DisasterApp.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterApp.Application.DTOs
{
    public class SupportRequestDto
    {
        public class SupportRequestCreateDto
        {
            public Guid ReportId { get; set; }
            public string Description { get; set; } = null!;
            public byte Urgency { get; set; }
            //public Guid UserId { get; set; }
            public List<string> SupportTypeNames { get; set; } = new();
        }
        public class SupportRequestsDto
        {
            public int Id { get; set; }

            public Guid ReportId { get; set; }
            public string FullName { get; set; }
            public string Email { get; set; }
            public string? Location { get; set; }
            // public Guid UserId { get; set; }
            public string UrgencyLevel { get; set; }
            public string Description { get; set; }

            public DateTime? DateReported { get; set; }
            public string Status { get; set; }
            public List<int> SupportTypeIds { get; set; } = new();

            public string? AdminRemarks { get; set; } = "No remarks";

            public List<string>? SupportTypeNames { get; set; }
        }

        public class SupportRequestMetricsDto
        {
            public int TotalRequests { get; set; }
            public int PendingRequests { get; set; }
            public int VerifiedRequests { get; set; }
            public int RejectedRequests { get; set; }
        }
        public class SupportRequestResponseDto
        {
            public int Id { get; set; }
            public Guid ReportId { get; set; }
            public string UserName { get; set; } = null!;
            public string email { get; set; } = null!;
            public string Description { get; set; } = null!;
            public byte Urgency { get; set; }
            public List<int> SupportTypeIds { get; set; } = new();
            public SupportRequestStatus? Status { get; set; }
            public Guid UserId { get; set; }
            public List<string> SupportTypeNames { get; set; } = new();
            public DateTime? CreatedAt { get; set; }
            public DateTime? UpdatedAt { get; set; }
        }

        public class SupportRequestUpdateDto
        {
            public string Description { get; set; } = null!;
            public byte Urgency { get; set; }
            public string SupportTypeName { get; set; } = null!;
            public DateTime? UpdateAt { get; set; }
            public List<int> SupportTypeIds { get; set; } = new();
        }
    }
}
