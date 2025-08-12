using System;
using System.Collections.Generic;
using DisasterApp.Domain.Enums;

namespace DisasterApp.Domain.Entities;

public partial class ImpactDetail
{
    public int Id { get; set; }

    public Guid ReportId { get; set; }

    public string Description { get; set; } = null!;

    public SeverityLevel? Severity { get; set; }

    public bool? IsResolved { get; set; }

    public DateTime? ResolvedAt { get; set; }
   

    public virtual ICollection<ImpactType> ImpactTypes { get; set; } =new List<ImpactType>();

    public virtual DisasterReport Report { get; set; } = null!;
}
