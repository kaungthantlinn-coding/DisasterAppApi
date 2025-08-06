using DisasterApp.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterApp.Application.DTOs
{
    public class SupportRequestCreateDto
    {
        public Guid ReportId { get; set; }
        public string Description { get; set; } = null!;
        public byte Urgency { get; set; }
        public Guid UserId { get; set; }
        public string SupportTypeName { get; set; } = null!;
    }
    public class SupportRequestDto
    {
        public int Id { get; set; }
        public Guid ReportId { get; set; }
        public string Description { get; set; } = null!;
        public byte Urgency { get; set; }
        public SupportRequestStatus? Status { get; set; }
        public Guid UserId { get; set; }
        public string SupportTypeName { get; set; } = null!;
        public DateTime? CreatedAt { get; set; }
    }

    public class SupportRequestUpdateDto
    {
        public string Description { get; set; } = null!;
        public byte Urgency { get; set; }
        public string SupportTypeName { get; set; } = null!;
        public DateTime? UpdateAt { get; set; }
    }

}
