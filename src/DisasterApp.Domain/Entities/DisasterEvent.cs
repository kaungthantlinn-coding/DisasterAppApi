using System;
using System.Collections.Generic;

namespace DisasterApp.Domain.Entities;

public partial class DisasterEvent
{
    public Guid Id { get; set; } // replace int

    public string Name { get; set; } = null!;

    public int DisasterTypeId { get; set; }

    public virtual ICollection<DisasterReport> DisasterReports { get; set; } = new List<DisasterReport>();

    public virtual DisasterType DisasterType { get; set; } = null!;
}
