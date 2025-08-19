using System;
using System.Collections.Generic;
using DisasterApp.Domain.Enums;

namespace DisasterApp.Domain.Entities;

public partial class SupportRequest
{
    public int Id { get; set; }

    public Guid ReportId { get; set; }

    public string Description { get; set; } = null!;

    public byte Urgency { get; set; }

    public SupportRequestStatus? Status { get; set; }

    public Guid UserId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int SupportTypeId { get; set; } // Added SupportTypeId property

    public virtual DisasterReport Report { get; set; } = null!;

    public virtual User User { get; set; } = null!;

    public virtual SupportType SupportType { get; set; } = null!; // Added navigation property
    
    public virtual ICollection<SupportRequestSupportType> SupportRequestSupportTypes { get; set; } = new List<SupportRequestSupportType>();
}
