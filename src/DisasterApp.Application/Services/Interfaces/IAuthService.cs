using DisasterApp.Application.DTOs;

namespace DisasterApp.Application.Services.Interfaces;
//
public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
    Task<AuthResponseDto> SignupAsync(SignupRequestDto request);
    Task<AuthResponseDto> GoogleLoginAsync(GoogleLoginRequestDto request);
    Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request);
    Task<bool> LogoutAsync(string refreshToken);
    Task<bool> ValidateTokenAsync(string token);
    Task<ForgotPasswordResponseDto> ForgotPasswordAsync(ForgotPasswordRequestDto request);
    Task<ForgotPasswordResponseDto> ResetPasswordAsync(ResetPasswordRequestDto request);
    Task<VerifyResetTokenResponseDto> VerifyResetTokenAsync(VerifyResetTokenRequestDto request);

    // Two-Factor Authentication methods
    Task<EnhancedAuthResponseDto> LoginWithTwoFactorAsync(LoginRequestDto request);
    Task<SendOtpResponseDto> SendOtpAsync(SendOtpRequestDto request, string ipAddress);
    Task<AuthResponseDto> VerifyOtpAsync(VerifyOtpRequestDto request, string ipAddress);
    Task<AuthResponseDto> VerifyBackupCodeAsync(VerifyBackupCodeRequestDto request, string ipAddress);
}