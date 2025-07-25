using System;
using System.Collections.Generic;

namespace DisasterApp.Domain.Entities;

public partial class Donation
{
    public int Id { get; set; }

    public Guid UserId { get; set; }

    public int OrganizationId { get; set; }

    public string DonorName { get; set; } = null!;

    public string? DonorContact { get; set; }

    public string DonationType { get; set; } = null!;

    public decimal? Amount { get; set; }

    public string Description { get; set; } = null!;

    public DateTime ReceivedAt { get; set; }

    public string? Status { get; set; }

    public Guid? VerifiedBy { get; set; }

    public DateTime? VerifiedAt { get; set; }

    public virtual Organization Organization { get; set; } = null!;

    public virtual User User { get; set; } = null!;

    public virtual User? VerifiedByNavigation { get; set; }

}
