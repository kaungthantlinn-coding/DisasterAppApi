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

    public int ImpactTypeId { get; set; }

    public virtual ImpactType ImpactType { get; set; } = null!;

    public virtual DisasterReport Report { get; set; } = null!;
}
