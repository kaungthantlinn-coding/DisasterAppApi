using DisasterApp.Application.DTOs;

namespace DisasterApp.Application.Services.Interfaces
{
    public interface IEmailOtpService
    {
        Task<SendEmailOtpResponseDto> SendOtpAsync(SendEmailOtpRequestDto request, string ipAddress);
        Task<VerifyEmailOtpResponseDto> VerifyOtpAsync(VerifyEmailOtpRequestDto request, string ipAddress);
    }
}
