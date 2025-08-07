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

    public string? Email { get; set; }
    public string? LoginToken { get; set; }

    [Required]
    public string Type { get; set; } = "login";
}


public class SendOtpResponseDto
{
    public bool Success { get; set; }

    public string Message { get; set; } = null!;

    public DateTime? ExpiresAt { get; set; }

    public string? LoginToken { get; set; }
}


public class VerifyOtpRequestDto
{

    [Required]
    [StringLength(6, MinimumLength = 6)]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "OTP code must be 6 digits")]
    public string Code { get; set; } = null!;

    public string? LoginToken { get; set; }


    public string? Email { get; set; }

    [Required]
    public string Type { get; set; } = "login";
}


public class VerifyBackupCodeRequestDto
{

    [Required]
    [StringLength(8, MinimumLength = 8)]
    public string BackupCode { get; set; } = null!;

    public string? LoginToken { get; set; }

    public string? Email { get; set; }
}


public class TwoFactorStatusDto
{
    public bool TwoFactorEnabled { get; set; }

    public int BackupCodesRemaining { get; set; }

    public DateTime? LastUsed { get; set; }

    public DateTime? EnabledAt { get; set; }
}


public class SetupTwoFactorRequestDto
{

    [Required]
    [MinLength(6)]
    public string CurrentPassword { get; set; } = null!;
}

public class SetupTwoFactorResponseDto
{
   
    public bool Success { get; set; }

    public string Message { get; set; } = null!;

    public string Instructions { get; set; } = null!;
}

public class VerifySetupRequestDto
{
 
    [Required]
    [StringLength(6, MinimumLength = 6)]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "OTP code must be 6 digits")]
    public string Code { get; set; } = null!;
}


public class VerifySetupResponseDto
{
    public bool Success { get; set; }


    public string Message { get; set; } = null!;

    public List<string> BackupCodes { get; set; } = new();
}


public class DisableTwoFactorRequestDto
{
   
    [Required]
    [MinLength(6)]
    public string CurrentPassword { get; set; } = null!;


    [StringLength(6, MinimumLength = 6)]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "OTP code must be 6 digits")]
    public string? OtpCode { get; set; }
}


public class GenerateBackupCodesResponseDto
{

    public bool Success { get; set; }

    public string Message { get; set; } = null!;

    public List<string> BackupCodes { get; set; } = new();

    public int CodesGenerated { get; set; }
}

public class EnhancedUserDto : UserDto
{

    public bool TwoFactorEnabled { get; set; }

    public int BackupCodesRemaining { get; set; }
}
