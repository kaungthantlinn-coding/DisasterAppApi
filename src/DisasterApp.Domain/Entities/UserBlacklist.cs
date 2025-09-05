using System;
using System.ComponentModel.DataAnnotations;

namespace DisasterApp.Domain.Entities;

public class UserBlacklist
{
    public UserBlacklist()
    {
        Id = Guid.NewGuid();
        BlacklistedAt = DateTime.UtcNow;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    [Required]
    [StringLength(1000)]
    public string Reason { get; set; } = null!;

    public Guid BlacklistedBy { get; set; }

    public DateTime BlacklistedAt { get; set; }

    public Guid? UnblacklistedBy { get; set; }

    public DateTime? UnblacklistedAt { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual User User { get; set; } = null!;
    public virtual User BlacklistedByUser { get; set; } = null!;
    public virtual User? UnblacklistedByUser { get; set; }
}