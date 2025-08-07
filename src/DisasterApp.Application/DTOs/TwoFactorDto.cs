using System.ComponentModel.DataAnnotations;

namespace DisasterApp.Application.DTOs;

/// <summary>
/// Enhanced authentication response that can indicate if 2FA is required
/// </summary>
public class EnhancedAuthResponseDto
{
    /// <summary>
    /// Whether OTP verification is required to complete login
    /// </summary>
    public bool RequiresOTP { get; set; } = false;

    /// <summary>
    /// Temporary login token for OTP verification (only present when RequiresOTP is true)
    /// </summary>
    public string? LoginToken { get; set; }

    /// <summary>
    /// Message to display to user
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Complete authentication response (only present when login is complete)
    /// </summary>
    public AuthResponseDto? AuthResponse { get; set; }
}

/// <summary>
/// Request to send OTP code
/// </summary>
public class SendOtpRequestDto
{
    /// <summary>
    /// Email address to send OTP to (used for initial login)
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Login token from initial authentication (alternative to email)
    /// </summary>
    public string? LoginToken { get; set; }

    /// <summary>
    /// Type of OTP being requested
    /// </summary>
    [Required]
    public string Type { get; set; } = "login";
}

/// <summary>
/// Response when OTP is sent
/// </summary>
public class SendOtpResponseDto
{
    /// <summary>
    /// Whether OTP was sent successfully
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Message to display to user
    /// </summary>
    public string Message { get; set; } = null!;

    /// <summary>
    /// When the OTP expires (for display purposes)
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Login token for verification (if applicable)
    /// </summary>
    public string? LoginToken { get; set; }
}

/// <summary>
/// Request to verify OTP code
/// </summary>
public class VerifyOtpRequestDto
{
    /// <summary>
    /// The 6-digit OTP code
    /// </summary>
    [Required]
    [StringLength(6, MinimumLength = 6)]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "OTP code must be 6 digits")]
    public string Code { get; set; } = null!;

    /// <summary>
    /// Login token from initial authentication
    /// </summary>
    public string? LoginToken { get; set; }

    /// <summary>
    /// Email address (alternative to login token)
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Type of OTP being verified
    /// </summary>
    [Required]
    public string Type { get; set; } = "login";
}

/// <summary>
/// Request to verify OTP using backup code
/// </summary>
public class VerifyBackupCodeRequestDto
{
    /// <summary>
    /// The 8-character backup code
    /// </summary>
    [Required]
    [StringLength(8, MinimumLength = 8)]
    public string BackupCode { get; set; } = null!;

    /// <summary>
    /// Login token from initial authentication
    /// </summary>
    public string? LoginToken { get; set; }

    /// <summary>
    /// Email address (alternative to login token)
    /// </summary>
    public string? Email { get; set; }
}

/// <summary>
/// User's two-factor authentication status
/// </summary>
public class TwoFactorStatusDto
{
    /// <summary>
    /// Whether 2FA is enabled for the user
    /// </summary>
    public bool TwoFactorEnabled { get; set; }

    /// <summary>
    /// Number of backup codes remaining
    /// </summary>
    public int BackupCodesRemaining { get; set; }

    /// <summary>
    /// When 2FA was last used
    /// </summary>
    public DateTime? LastUsed { get; set; }

    /// <summary>
    /// When 2FA was first enabled
    /// </summary>
    public DateTime? EnabledAt { get; set; }
}

/// <summary>
/// Request to setup two-factor authentication
/// </summary>
public class SetupTwoFactorRequestDto
{
    /// <summary>
    /// Current password for verification
    /// </summary>
    [Required]
    [MinLength(6)]
    public string CurrentPassword { get; set; } = null!;
}

/// <summary>
/// Response when starting 2FA setup
/// </summary>
public class SetupTwoFactorResponseDto
{
    /// <summary>
    /// Whether setup was initiated successfully
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Message to display to user
    /// </summary>
    public string Message { get; set; } = null!;

    /// <summary>
    /// Instructions for completing setup
    /// </summary>
    public string Instructions { get; set; } = null!;
}

/// <summary>
/// Request to complete 2FA setup
/// </summary>
public class VerifySetupRequestDto
{
    /// <summary>
    /// OTP code to verify setup
    /// </summary>
    [Required]
    [StringLength(6, MinimumLength = 6)]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "OTP code must be 6 digits")]
    public string Code { get; set; } = null!;
}

/// <summary>
/// Response when 2FA setup is completed
/// </summary>
public class VerifySetupResponseDto
{
    /// <summary>
    /// Whether setup was completed successfully
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Message to display to user
    /// </summary>
    public string Message { get; set; } = null!;

    /// <summary>
    /// Generated backup codes
    /// </summary>
    public List<string> BackupCodes { get; set; } = new();
}

/// <summary>
/// Request to disable two-factor authentication
/// </summary>
public class DisableTwoFactorRequestDto
{
    /// <summary>
    /// Current password for verification
    /// </summary>
    [Required]
    [MinLength(6)]
    public string CurrentPassword { get; set; } = null!;

    /// <summary>
    /// OTP code for additional verification (optional)
    /// </summary>
    [StringLength(6, MinimumLength = 6)]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "OTP code must be 6 digits")]
    public string? OtpCode { get; set; }
}

/// <summary>
/// Response when generating new backup codes
/// </summary>
public class GenerateBackupCodesResponseDto
{
    /// <summary>
    /// Whether backup codes were generated successfully
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Message to display to user
    /// </summary>
    public string Message { get; set; } = null!;

    /// <summary>
    /// New backup codes
    /// </summary>
    public List<string> BackupCodes { get; set; } = new();

    /// <summary>
    /// Number of codes generated
    /// </summary>
    public int CodesGenerated { get; set; }
}

/// <summary>
/// Enhanced user DTO with 2FA information
/// </summary>
public class EnhancedUserDto : UserDto
{
    /// <summary>
    /// Whether 2FA is enabled
    /// </summary>
    public bool TwoFactorEnabled { get; set; }

    /// <summary>
    /// Number of backup codes remaining
    /// </summary>
    public int BackupCodesRemaining { get; set; }
}
