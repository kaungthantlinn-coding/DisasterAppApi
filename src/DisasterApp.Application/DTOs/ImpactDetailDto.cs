using DisasterApp.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterApp.Application.DTOs
{
    public class ImpactDetailCreateDto
    {
        public List<int>? ImpactTypeIds { get; set; }
        public List<string>? ImpactTypeNames { get; set; }
        public string Description { get; set; } = null!;
        public SeverityLevel Severity { get; set; }

    }
    public class ImpactDetailDto
    {
        public string Description { get; set; } = null!;
        public SeverityLevel? Severity { get; set; }
        public bool? IsResolved { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public List<string> ImpactTypeNames { get; set; } = null!;
    }

    public class ImpactDetailUpdateDto
    {
        public int? Id { get; set; }
        public string? Description { get; set; }
        public SeverityLevel? Severity { get; set; }
        public bool? IsResolved { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public int? ImpactTypeId { get; set; }
        public string? ImpactTypeName { get; set; }
    }
}