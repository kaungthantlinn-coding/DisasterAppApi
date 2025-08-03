using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DisasterApp.Domain.Entities;

/// <summary>
/// Entity for tracking OTP attempts for rate limiting and security monitoring
/// </summary>
[Table("OtpAttempt")]
public class OtpAttempt
{
    /// <summary>
    /// Unique identifier for the attempt
    /// </summary>
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Foreign key reference to the user (can be null for failed login attempts)
    /// </summary>
    [Column("user_id")]
    public Guid? UserId { get; set; }

    /// <summary>
    /// IP address from which the attempt was made
    /// </summary>
    [Required]
    [StringLength(45)] // Supports both IPv4 and IPv6
    [Column("ip_address")]
    public string IpAddress { get; set; } = null!;

    /// <summary>
    /// Type of attempt (send_otp, verify_otp, login, setup, disable)
    /// </summary>
    [Required]
    [StringLength(20)]
    [Column("attempt_type")]
    public string AttemptType { get; set; } = null!;

    /// <summary>
    /// When the attempt was made
    /// </summary>
    [Required]
    [Column("attempted_at")]
    public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Whether the attempt was successful
    /// </summary>
    [Required]
    [Column("success")]
    public bool Success { get; set; } = false;

    /// <summary>
    /// Email address for tracking attempts when user_id is unknown
    /// </summary>
    [StringLength(255)]
    [Column("email")]
    public string? Email { get; set; }

    /// <summary>
    /// Navigation property to the user (if known)
    /// </summary>
    [ForeignKey("UserId")]
    public virtual User? User { get; set; }
}

/// <summary>
/// Constants for OTP attempt types
/// </summary>
public static class OtpAttemptTypes
{
    public const string SendOtp = "send_otp";
    public const string VerifyOtp = "verify_otp";
    public const string Login = "login";
    public const string Setup = "setup";
    public const string Disable = "disable";
}
