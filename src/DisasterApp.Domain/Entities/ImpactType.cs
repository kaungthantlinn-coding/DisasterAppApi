using System;
using System.Collections.Generic;

namespace DisasterApp.Domain.Entities;

public partial class ImpactType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;



    public virtual ICollection<ImpactDetail> ImpactDetails { get; set; } = new List<ImpactDetail>();
}
