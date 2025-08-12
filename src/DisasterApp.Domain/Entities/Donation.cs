using System;
using System.Collections.Generic;
using DisasterApp.Domain.Enums;

namespace DisasterApp.Domain.Entities;

public partial class Donation
{
    public int Id { get; set; }

    public Guid UserId { get; set; }

    public int OrganizationId { get; set; }

    public string DonorName { get; set; } = null!;

    public string? DonorContact { get; set; }

    public DonationType DonationType { get; set; }

    public decimal? Amount { get; set; }

    public string Description { get; set; } = null!;

    public DateTime ReceivedAt { get; set; }

    public DonationStatus Status { get; set; }

    public string? TransactionPhotoUrl { get; set; }

    public Guid? VerifiedBy { get; set; }

    public DateTime? VerifiedAt { get; set; }

    public virtual Organization Organization { get; set; } = null!;

    public virtual User User { get; set; } = null!;

    public virtual User? VerifiedByNavigation { get; set; }

}
