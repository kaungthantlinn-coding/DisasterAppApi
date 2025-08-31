using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DisasterApp.Domain.Entities;

/// <summary>
/// Entity representing a backup code for two-factor authentication recovery
/// </summary>
[Table("BackupCode")]
public class BackupCode
{
    /// <summary>
    /// Unique identifier for the backup code
    /// </summary>
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Foreign key reference to the user this backup code belongs to
    /// </summary>
    [Required]
    [Column("user_id")]
    public Guid UserId { get; set; }

    /// <summary>
    /// Hashed version of the backup code for security
    /// </summary>
    [Required]
    [StringLength(255)]
    [Column("code_hash")]
    public string CodeHash { get; set; } = null!;

    /// <summary>
    /// When this backup code was used (null if unused)
    /// </summary>
    [Column("used_at")]
    public DateTime? UsedAt { get; set; }

    /// <summary>
    /// When this backup code was created
    /// </summary>
    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Navigation property to the user this backup code belongs to
    /// </summary>
    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;

    /// <summary>
    /// Check if this backup code is still valid (not used)
    /// </summary>
    public bool IsValid => UsedAt == null;

    /// <summary>
    /// Check if this backup code has been used
    /// </summary>
    public bool IsUsed => UsedAt != null;

    /// <summary>
    /// Mark this backup code as used
    /// </summary>
    public void MarkAsUsed()
    {
        UsedAt = DateTime.UtcNow;
    }
}
