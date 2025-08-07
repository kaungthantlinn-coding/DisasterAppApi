using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DisasterApp.Domain.Entities;
[Table("OtpCode")]
public class OtpCode
{

    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column("user_id")]
    public Guid UserId { get; set; }

    [Required]
    [StringLength(6, MinimumLength = 6)]
    [Column("code")]
    public string Code { get; set; } = null!;

    [Required]
    [StringLength(20)]
    [Column("type")]
    public string Type { get; set; } = null!;

    [Required]
    [Column("expires_at")]
    public DateTime ExpiresAt { get; set; }

    [Column("used_at")]
    public DateTime? UsedAt { get; set; }

    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    [Column("attempt_count")]
    public int AttemptCount { get; set; } = 0;

    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;

    public bool IsValid => UsedAt == null && ExpiresAt > DateTime.UtcNow;

    public bool IsExpired => ExpiresAt <= DateTime.UtcNow;
    public bool IsUsed => UsedAt != null;
    public bool HasReachedMaxAttempts => AttemptCount >= 3;
}


public static class OtpCodeTypes
{
    public const string Login = "login";
    public const string Setup = "setup";
    public const string Disable = "disable";
    public const string BackupGenerate = "backup_generate";
}
