using DisasterApp.Application.DTOs;

namespace DisasterApp.Application.Services.Interfaces;

/// <summary>
/// Service interface for Email OTP authentication
/// </summary>
public interface IEmailOtpService
{
    /// <summary>
    /// Send OTP code to email address
    /// </summary>
    /// <param name="request">Send OTP request</param>
    /// <param name="ipAddress">Client IP address for rate limiting</param>
    /// <returns>Send OTP response</returns>
    Task<SendEmailOtpResponseDto> SendOtpAsync(SendEmailOtpRequestDto request, string ipAddress);

    /// <summary>
    /// Verify OTP code and authenticate user
    /// </summary>
    /// <param name="request">Verify OTP request</param>
    /// <param name="ipAddress">Client IP address for auditing</param>
    /// <returns>Authentication response with tokens</returns>
    Task<VerifyEmailOtpResponseDto> VerifyOtpAsync(VerifyEmailOtpRequestDto request, string ipAddress);
}