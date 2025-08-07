using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DisasterApp.Domain.Entities;

[Table("OtpAttempt")]
public class OtpAttempt
{

    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column("user_id")]
    public Guid? UserId { get; set; }

    [Required]
    [StringLength(45)] // Supports both IPv4 and IPv6
    [Column("ip_address")]
    public string IpAddress { get; set; } = null!;

    [Required]
    [StringLength(20)]
    [Column("attempt_type")]
    public string AttemptType { get; set; } = null!;

    [Required]
    [Column("attempted_at")]
    public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;

    [Required]
    [Column("success")]
    public bool Success { get; set; } = false;

    [StringLength(255)]
    [Column("email")]
    public string? Email { get; set; }


    [ForeignKey("UserId")]
    public virtual User? User { get; set; }
}


public static class OtpAttemptTypes
{
    public const string SendOtp = "send_otp";
    public const string VerifyOtp = "verify_otp";
    public const string Login = "login";
    public const string Setup = "setup";
    public const string Disable = "disable";
}
