using System;
using System.Collections.Generic;
using DisasterApp.Domain.Enums;

namespace DisasterApp.Domain.Entities;

public partial class DisasterType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;


    public DisasterCategory Category { get; set; }

    public virtual ICollection<DisasterEvent> DisasterEvents { get; set; } = new List<DisasterEvent>();


}
