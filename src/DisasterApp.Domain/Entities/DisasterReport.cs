using System;
using System.Collections.Generic;
using DisasterApp.Domain.Enums;

namespace DisasterApp.Domain.Entities;

public partial class DisasterReport
{
    public DisasterReport()
    {
        Id = Guid.NewGuid();
    }

    public Guid Id { get; set; }

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public DateTime Timestamp { get; set; }

    public SeverityLevel Severity { get; set; }

    public ReportStatus Status { get; set; }

    public Guid? VerifiedBy { get; set; }

    public DateTime? VerifiedAt { get; set; }

    public bool? IsDeleted { get; set; }

    public Guid UserId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid DisasterEventId { get; set; } //replace int

    public virtual DisasterEvent DisasterEvent { get; set; } = null!;



    public virtual ICollection<ImpactDetail> ImpactDetails { get; set; } = new List<ImpactDetail>();

    public virtual Location? Location { get; set; }

    public virtual ICollection<Photo> Photos { get; set; } = new List<Photo>();

    public virtual ICollection<SupportRequest> SupportRequests { get; set; } = new List<SupportRequest>();

    public virtual User User { get; set; } = null!;

    public virtual User? VerifiedByNavigation { get; set; }
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
