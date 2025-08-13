using System;
using System.Collections.Generic;

namespace DisasterApp.Domain.Entities;

public partial class User
{
    public User()
    {
        UserId = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        // Initialize required auth fields to safe defaults for test entities
        AuthProvider = "local";
        AuthId = Guid.NewGuid().ToString();
    }

    public Guid UserId { get; set; }

    public string AuthProvider { get; set; } = null!;

    public string AuthId { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? PhotoUrl { get; set; }

    public string? PhoneNumber { get; set; }

    public bool? IsBlacklisted { get; set; }

    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// Whether two-factor authentication is enabled for this user
    /// </summary>
    public bool TwoFactorEnabled { get; set; } = false;

    /// <summary>
    /// Number of unused backup codes remaining for this user
    /// </summary>
    public int BackupCodesRemaining { get; set; } = 0;

    /// <summary>
    /// When two-factor authentication was last used by this user
    /// </summary>
    public DateTime? TwoFactorLastUsed { get; set; }



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

    public virtual ICollection<OtpCode> OtpCodes { get; set; } = new List<OtpCode>();

    public virtual ICollection<BackupCode> BackupCodes { get; set; } = new List<BackupCode>();

    public virtual ICollection<OtpAttempt> OtpAttempts { get; set; } = new List<OtpAttempt>();

    public virtual ICollection<UserBlacklist> BlacklistHistory { get; set; } = new List<UserBlacklist>();
}
