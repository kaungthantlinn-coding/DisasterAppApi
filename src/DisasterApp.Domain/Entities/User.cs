using System;
using System.Collections.Generic;

namespace DisasterApp.Domain.Entities;

public partial class User
{
    public Guid UserId { get; set; }

    public string AuthProvider { get; set; } = null!;

    public string AuthId { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? PhotoUrl { get; set; }

    public string? PhoneNumber { get; set; }

    public bool? IsBlacklisted { get; set; }

    public DateTime? CreatedAt { get; set; }



    public virtual ICollection<Chat> ChatReceivers { get; set; } = new List<Chat>();

    public virtual ICollection<Chat> ChatSenders { get; set; } = new List<Chat>();

    public virtual ICollection<DisasterReport> DisasterReportUsers { get; set; } = new List<DisasterReport>();

    public virtual ICollection<DisasterReport> DisasterReportVerifiedByNavigations { get; set; } = new List<DisasterReport>();

    public virtual ICollection<Donation> DonationUsers { get; set; } = new List<Donation>();

    public virtual ICollection<Donation> DonationVerifiedByNavigations { get; set; } = new List<Donation>();

    public virtual ICollection<Organization> Organizations { get; set; } = new List<Organization>();

    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    public virtual ICollection<PasswordResetToken> PasswordResetTokens { get; set; } = new List<PasswordResetToken>();

    public virtual ICollection<SupportRequest> SupportRequests { get; set; } = new List<SupportRequest>();

    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
}
