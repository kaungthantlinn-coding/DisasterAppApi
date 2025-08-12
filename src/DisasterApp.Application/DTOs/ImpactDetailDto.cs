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
        public List<int> ImpactTypeIds { get; set; } = new List<int>();

        public string Description { get; set; } = null!;
        public SeverityLevel Severity { get; set; }

    }
    public class ImpactDetailDto
    {
        public int Id { get; set; }
        public string Description { get; set; } = null!;
        public SeverityLevel? Severity { get; set; }
        public bool? IsResolved { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public List<int> ImpactTypeIds { get; set; } = new List<int>(); // <-- for Id list
        public List<ImpactTypeDto> ImpactTypes { get; set; } = new List<ImpactTypeDto>();
    }

    public class ImpactDetailUpdateDto
    {
       
        public string? Description { get; set; }
        public SeverityLevel? Severity { get; set; }
        public bool? IsResolved { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public List<int>? ImpactTypeIds { get; set; } = new List<int>();


    }
}
