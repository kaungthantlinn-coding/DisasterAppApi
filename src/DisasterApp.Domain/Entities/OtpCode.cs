using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DisasterApp.Domain.Entities;

/// <summary>
/// Entity representing a temporary OTP (One-Time Password) code for two-factor authentication
/// </summary>
[Table("OtpCode")]
public class OtpCode
{
    /// <summary>
    /// Unique identifier for the OTP code
    /// </summary>
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Foreign key reference to the user this OTP belongs to
    /// </summary>
    [Required]
    [Column("user_id")]
    public Guid UserId { get; set; }

    /// <summary>
    /// The 6-digit OTP code
    /// </summary>
    [Required]
    [StringLength(6, MinimumLength = 6)]
    [Column("code")]
    public string Code { get; set; } = null!;

    /// <summary>
    /// Type of OTP code (login, setup, disable, backup_generate)
    /// </summary>
    [Required]
    [StringLength(20)]
    [Column("type")]
    public string Type { get; set; } = null!;

    /// <summary>
    /// When this OTP code expires (typically 5 minutes from creation)
    /// </summary>
    [Required]
    [Column("expires_at")]
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// When this OTP code was successfully used (null if unused)
    /// </summary>
    [Column("used_at")]
    public DateTime? UsedAt { get; set; }

    /// <summary>
    /// When this OTP code was created
    /// </summary>
    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Number of verification attempts made with this code
    /// </summary>
    [Required]
    [Column("attempt_count")]
    public int AttemptCount { get; set; } = 0;

    /// <summary>
    /// Navigation property to the user this OTP belongs to
    /// </summary>
    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;

    /// <summary>
    /// Check if this OTP code is still valid (not expired and not used)
    /// </summary>
    public bool IsValid => UsedAt == null && ExpiresAt > DateTime.UtcNow;

    /// <summary>
    /// Check if this OTP code has expired
    /// </summary>
    public bool IsExpired => ExpiresAt <= DateTime.UtcNow;

    /// <summary>
    /// Check if this OTP code has been used
    /// </summary>
    public bool IsUsed => UsedAt != null;

    /// <summary>
    /// Check if this OTP code has reached maximum attempts
    /// </summary>
    public bool HasReachedMaxAttempts => AttemptCount >= 3;
}

/// <summary>
/// Constants for OTP code types
/// </summary>
public static class OtpCodeTypes
{
    public const string Login = "login";
    public const string Setup = "setup";
    public const string Disable = "disable";
    public const string BackupGenerate = "backup_generate";
}
