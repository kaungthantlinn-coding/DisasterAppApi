using System.ComponentModel.DataAnnotations;

namespace DisasterApp.WebApi.DTOs
{
    public class CookieAuthResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public UserDto User { get; set; } = new UserDto();
    }

    public class UserDto
    {
        public Guid UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new List<string>();
    }

    public class LoginRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class SignupRequestDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class GoogleLoginRequestDto
    {
        [Required]
        public string IdToken { get; set; } = string.Empty;
        public string? DeviceInfo { get; set; }
    }

    public class RefreshTokenRequestDto
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class ForgotPasswordRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }

    public class ForgotPasswordResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class ResetPasswordRequestDto
    {
        [Required]
        public string Token { get; set; } = string.Empty;

        [Required]
        public string NewPassword { get; set; } = string.Empty;
    }

    public class VerifyResetTokenRequestDto
    {
        [Required]
        public string Token { get; set; } = string.Empty;
    }

    public class VerifyResetTokenResponseDto
    {
        public bool IsValid { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class ValidatePasswordRequestDto
    {
        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class SendOtpRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }

    public class SendOtpResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int ExpiresInMinutes { get; set; }
    }

    public class VerifyOtpRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Code { get; set; } = string.Empty;
    }

    public class VerifyBackupCodeRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Code { get; set; } = string.Empty;
    }

    public class SetupTwoFactorRequestDto
    {
        [Required]
        public string CurrentPassword { get; set; } = string.Empty;
    }

    public class VerifySetupRequestDto
    {
        [Required]
        public string Code { get; set; } = string.Empty;
    }

    public class DisableTwoFactorRequestDto
    {
        [Required]
        public string CurrentPassword { get; set; } = string.Empty;
        public string? OtpCode { get; set; }
    }

    public class SendEmailOtpRequestDto
    {
        [Required]
        [EmailAddress]
        public string email { get; set; } = string.Empty;
    }

    public class SendEmailOtpResponseDto
    {
        public bool success { get; set; }
        public string message { get; set; } = string.Empty;
        public int expiresAt { get; set; }
    }

    public class VerifyEmailOtpRequestDto
    {
        [Required]
        [EmailAddress]
        public string email { get; set; } = string.Empty;

        [Required]
        public string code { get; set; } = string.Empty;
    }

    public class VerifyEmailOtpResponseDto
    {
        public bool success { get; set; }
        public string message { get; set; } = string.Empty;
        public string? accessToken { get; set; }
        public DateTime? expiresAt { get; set; }
        public UserDto? user { get; set; }
    }
}