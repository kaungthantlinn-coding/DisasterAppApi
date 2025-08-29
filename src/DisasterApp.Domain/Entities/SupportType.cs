using System;
using System.Collections.Generic;

namespace DisasterApp.Domain.Entities;

public partial class SupportType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

   
    public virtual ICollection<SupportRequest> SupportRequests { get; set; } = new List<SupportRequest>();
}
