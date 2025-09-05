using DisasterApp.Application.DTOs;
using DisasterApp.Application.Services.Interfaces;
using DisasterApp.Domain.Entities;
using DisasterApp.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace DisasterApp.Application.Services.Implementations;

public class OtpService : IOtpService
{
    private readonly IOtpCodeRepository _otpCodeRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OtpService> _logger;

    public OtpService(
        IOtpCodeRepository otpCodeRepository,
        IUserRepository userRepository,
        IEmailService emailService,
        IConfiguration configuration,
        ILogger<OtpService> logger)
    {
        _otpCodeRepository = otpCodeRepository;
        _userRepository = userRepository;
        _emailService = emailService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<SendOtpResponseDto> SendOtpAsync(Guid userId, string email, string type)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return new SendOtpResponseDto
                {
                    Success = false,
                    Message = "User not found"
                };
            }

            await _otpCodeRepository.DeleteByUserAsync(userId, type);

            var otpCode = GenerateOtpCode();
            var expiryMinutes = int.Parse(_configuration["TwoFactor:OtpExpiryMinutes"] ?? "5");
            var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

            var otpEntity = new OtpCode
            {
                UserId = userId,
                Code = otpCode,
                Type = type,
                ExpiresAt = expiresAt
            };

            await _otpCodeRepository.CreateAsync(otpEntity);

            var emailSent = await _emailService.SendOtpEmailAsync(email, otpCode);

            if (!emailSent)
            {
                _logger.LogError("Failed to send OTP email to {Email}", email);
                return new SendOtpResponseDto
                {
                    Success = false,
                    Message = "Failed to send OTP email"
                };
            }

            _logger.LogInformation("OTP sent successfully to user {UserId} at {Email}", userId, email);

            return new SendOtpResponseDto
            {
                Success = true,
                Message = $"OTP code sent to {email}. Code expires in {expiryMinutes} minutes.",
                ExpiresAt = expiresAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending OTP to user {UserId}", userId);
            return new SendOtpResponseDto
            {
                Success = false,
                Message = "An error occurred while sending OTP"
            };
        }
    }

    public async Task<SendOtpResponseDto> SendOtpAsync(Guid userId, string type)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return new SendOtpResponseDto
                {
                    Success = false,
                    Message = "User not found"
                };
            }

            return await SendOtpAsync(userId, user.Email, type);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending OTP for user");
            return new SendOtpResponseDto
            {
                Success = false,
                Message = "An error occurred while sending OTP"
            };
        }
    }

    public async Task<bool> VerifyOtpAsync(Guid userId, string code, string type)
    {
        try
        {
            var otpCode = await _otpCodeRepository.GetByUserAndCodeAsync(userId, code, type);
            if (otpCode == null)
            {
                _logger.LogWarning("Invalid OTP code attempt for user {UserId}", userId);
                return false;
            }

            if (!otpCode.IsValid)
            {
                _logger.LogWarning("Expired or used OTP code attempt for user {UserId}", userId);
                return false;
            }

            if (otpCode.HasReachedMaxAttempts)
            {
                _logger.LogWarning("OTP code has reached maximum attempts for user {UserId}", userId);
                return false;
            }

            otpCode.AttemptCount++;
            await _otpCodeRepository.UpdateAsync(otpCode);

            _logger.LogInformation("OTP verified successfully for user {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying OTP for user {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> MarkOtpAsUsedAsync(Guid userId, string code, string type)
    {
        try
        {
            var otpCode = await _otpCodeRepository.GetByUserAndCodeAsync(userId, code, type);
            if (otpCode == null)
            {
                _logger.LogWarning("OTP code not found when trying to mark as used for user {UserId}", userId);
                return false;
            }

            otpCode.UsedAt = DateTime.UtcNow;
            await _otpCodeRepository.UpdateAsync(otpCode);

            _logger.LogInformation("OTP marked as used successfully for user {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking OTP as used for user {UserId}", userId);
            return false;
        }
    }

    public string GenerateOtpCode()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[4];
        rng.GetBytes(bytes);
        var randomNumber = BitConverter.ToUInt32(bytes, 0);
        return (randomNumber % 1000000).ToString("D6");
    }

    public async Task<int> CleanupExpiredCodesAsync()
    {
        try
        {
            var deletedCount = await _otpCodeRepository.DeleteExpiredAsync();
            _logger.LogInformation("Cleaned up {Count} expired OTP codes", deletedCount);
            return deletedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up expired OTP codes");
            return 0;
        }
    }

    public async Task<int> GetActiveCodeCountAsync(Guid userId, string? type = null)
    {
        return await _otpCodeRepository.GetActiveCountAsync(userId, type);
    }

    public async Task<int> InvalidateCodesAsync(Guid userId, string? type = null)
    {
        try
        {
            var deletedCount = await _otpCodeRepository.DeleteByUserAsync(userId, type);
            _logger.LogInformation("Invalidated {Count} OTP codes for user {UserId}", deletedCount, userId);
            return deletedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating OTP codes for user {UserId}", userId);
            return 0;
        }
    }
}
