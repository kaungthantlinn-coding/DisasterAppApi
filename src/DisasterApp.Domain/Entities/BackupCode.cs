using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DisasterApp.Domain.Entities;

[Table("BackupCode")]
public class BackupCode
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column("user_id")]
    public Guid UserId { get; set; }

    [Required]
    [StringLength(255)]
    [Column("code_hash")]
    public string CodeHash { get; set; } = null!;

    [Column("used_at")]
    public DateTime? UsedAt { get; set; }

    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;

    public bool IsValid => UsedAt is null;
    public bool IsUsed => UsedAt is not null;

    public void MarkAsUsed() => UsedAt = DateTime.UtcNow;
}
