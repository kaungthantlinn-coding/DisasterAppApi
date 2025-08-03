using System.ComponentModel.DataAnnotations;

namespace DisasterApp.Application.DTOs;

/// <summary>
/// Request to send OTP code via email
/// </summary>
public class SendEmailOtpRequestDto
{
    /// <summary>
    /// Email address to send OTP to
    /// </summary>
    [Required]
    [EmailAddress]
    public string email { get; set; } = null!;

    /// <summary>
    /// Purpose of the OTP - "login", "signup", or "verification"
    /// </summary>
    public string purpose { get; set; } = "login";
}

/// <summary>
/// Response when OTP is sent via email
/// </summary>
public class SendEmailOtpResponseDto
{
    /// <summary>
    /// Success message to display to user
    /// </summary>
    public string message { get; set; } = null!;

    /// <summary>
    /// When the OTP expires
    /// </summary>
    public DateTime expiresAt { get; set; }

    /// <summary>
    /// Seconds until next request is allowed (optional)
    /// </summary>
    public int? retryAfter { get; set; }
}

/// <summary>
/// Request to verify OTP code
/// </summary>
public class VerifyEmailOtpRequestDto
{
    /// <summary>
    /// Email address
    /// </summary>
    [Required]
    [EmailAddress]
    public string email { get; set; } = null!;

    /// <summary>
    /// 6-digit OTP code
    /// </summary>
    [Required]
    [StringLength(6, MinimumLength = 6)]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "OTP must be 6 digits")]
    public string otp { get; set; } = null!;

    /// <summary>
    /// Purpose of the OTP - "login", "signup", or "verification"
    /// </summary>
    public string purpose { get; set; } = "login";
}

/// <summary>
/// Response when OTP is successfully verified
/// </summary>
public class VerifyEmailOtpResponseDto
{
    /// <summary>
    /// JWT access token
    /// </summary>
    public string accessToken { get; set; } = null!;

    /// <summary>
    /// JWT refresh token
    /// </summary>
    public string refreshToken { get; set; } = null!;

    /// <summary>
    /// When the access token expires
    /// </summary>
    public DateTime expiresAt { get; set; }

    /// <summary>
    /// User information
    /// </summary>
    public EmailOtpUserDto user { get; set; } = null!;

    /// <summary>
    /// Whether this is a new user account created during verification
    /// </summary>
    public bool isNewUser { get; set; }
}

/// <summary>
/// User information for email OTP response
/// </summary>
public class EmailOtpUserDto
{
    /// <summary>
    /// User ID
    /// </summary>
    public string userId { get; set; } = null!;

    /// <summary>
    /// User's full name
    /// </summary>
    public string name { get; set; } = null!;

    /// <summary>
    /// User's email address
    /// </summary>
    public string email { get; set; } = null!;

    /// <summary>
    /// User's photo URL (optional)
    /// </summary>
    public string? photoUrl { get; set; }

    /// <summary>
    /// User's roles
    /// </summary>
    public List<string> roles { get; set; } = new();
}