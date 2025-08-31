using System.ComponentModel.DataAnnotations;

namespace DisasterApp.Application.DTOs;
//
public class SendEmailOtpRequestDto
{

    [Required]
    [EmailAddress]
    public string email { get; set; } = null!;

    public string purpose { get; set; } = "login";
}

public class SendEmailOtpResponseDto
{
 
    public string message { get; set; } = null!;

    public DateTime expiresAt { get; set; }

    public int? retryAfter { get; set; }
}

public class VerifyEmailOtpRequestDto
{

    [Required]
    [EmailAddress]
    public string email { get; set; } = null!;

    [Required]
    [StringLength(6, MinimumLength = 6)]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "OTP must be 6 digits")]
    public string otp { get; set; } = null!;

    public string purpose { get; set; } = "login";
}


public class VerifyEmailOtpResponseDto
{

    public string accessToken { get; set; } = null!;

    public string refreshToken { get; set; } = null!;

    public DateTime expiresAt { get; set; }

    public EmailOtpUserDto user { get; set; } = null!;

    public bool isNewUser { get; set; }
}


public class EmailOtpUserDto
{
  
    public string userId { get; set; } = null!;

    public string name { get; set; } = null!;

    public string email { get; set; } = null!;


    public string? photoUrl { get; set; }
    public List<string> roles { get; set; } = new();
}