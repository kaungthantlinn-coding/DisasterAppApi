using DisasterApp.Application.Services.Interfaces;
using DisasterApp.Domain.Entities;
using DisasterApp.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DisasterApp.Application.Services.Implementations;

public class RateLimitingService : IRateLimitingService
{
    private readonly IOtpAttemptRepository _otpAttemptRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<RateLimitingService> _logger;

    // Rate limiting configuration
    private readonly int _maxOtpSendPerHour;
    private readonly int _maxOtpVerifyPerHour;
    private readonly int _maxFailedAttemptsForLockout;
    private readonly int _lockoutDurationMinutes;
    private readonly int _maxIpAttemptsPerHour;

    public RateLimitingService(
        IOtpAttemptRepository otpAttemptRepository,
        IConfiguration configuration,
        ILogger<RateLimitingService> logger)
    {
        _otpAttemptRepository = otpAttemptRepository;
        _configuration = configuration;
        _logger = logger;

        // Load configuration with defaults
        _maxOtpSendPerHour = int.Parse(_configuration["TwoFactor:MaxOtpSendPerHour"] ?? "3");
        _maxOtpVerifyPerHour = int.Parse(_configuration["TwoFactor:MaxOtpVerifyPerHour"] ?? "10");
        _maxFailedAttemptsForLockout = int.Parse(_configuration["TwoFactor:MaxFailedAttemptsForLockout"] ?? "5");
        _lockoutDurationMinutes = int.Parse(_configuration["TwoFactor:LockoutDurationMinutes"] ?? "60");
        _maxIpAttemptsPerHour = int.Parse(_configuration["TwoFactor:MaxIpAttemptsPerHour"] ?? "20");
    }

    public async Task<bool> CanSendOtpAsync(Guid userId, string ipAddress)
    {
        try
        {
            var oneHourAgo = DateTime.UtcNow.AddHours(-1);

            // Check user-specific rate limit
            var userSendAttempts = await _otpAttemptRepository.CountUserAttemptsAsync(
                userId, oneHourAgo, OtpAttemptTypes.SendOtp);

            if (userSendAttempts >= _maxOtpSendPerHour)
            {
                _logger.LogWarning("User {UserId} exceeded OTP send rate limit: {Attempts}/{Max}", 
                    userId, userSendAttempts, _maxOtpSendPerHour);
                return false;
            }

            // Check IP-specific rate limit
            var ipAttempts = await _otpAttemptRepository.CountIpAttemptsAsync(
                ipAddress, oneHourAgo, OtpAttemptTypes.SendOtp);

            if (ipAttempts >= _maxIpAttemptsPerHour)
            {
                _logger.LogWarning("IP {IpAddress} exceeded OTP send rate limit: {Attempts}/{Max}", 
                    ipAddress, ipAttempts, _maxIpAttemptsPerHour);
                return false;
            }

            // Check if account is locked
            if (await IsAccountLockedAsync(userId))
            {
                _logger.LogWarning("User {UserId} account is locked", userId);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking OTP send rate limit for user {UserId}", userId);
            return false; // Fail safe
        }
    }

    public async Task<bool> CanVerifyOtpAsync(Guid userId, string ipAddress)
    {
        try
        {
            var oneHourAgo = DateTime.UtcNow.AddHours(-1);

            // Check user-specific rate limit
            var userVerifyAttempts = await _otpAttemptRepository.CountUserAttemptsAsync(
                userId, oneHourAgo, OtpAttemptTypes.VerifyOtp);

            if (userVerifyAttempts >= _maxOtpVerifyPerHour)
            {
                _logger.LogWarning("User {UserId} exceeded OTP verify rate limit: {Attempts}/{Max}", 
                    userId, userVerifyAttempts, _maxOtpVerifyPerHour);
                return false;
            }

            // Check IP-specific rate limit
            var ipAttempts = await _otpAttemptRepository.CountIpAttemptsAsync(
                ipAddress, oneHourAgo, OtpAttemptTypes.VerifyOtp);

            if (ipAttempts >= _maxIpAttemptsPerHour)
            {
                _logger.LogWarning("IP {IpAddress} exceeded OTP verify rate limit: {Attempts}/{Max}", 
                    ipAddress, ipAttempts, _maxIpAttemptsPerHour);
                return false;
            }

            // Check if account is locked
            if (await IsAccountLockedAsync(userId))
            {
                _logger.LogWarning("User {UserId} account is locked", userId);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking OTP verify rate limit for user {UserId}", userId);
            return false; // Fail safe
        }
    }

    public async Task<bool> CanSendOtpAsync(string email, string ipAddress)
    {
        try
        {
            var oneHourAgo = DateTime.UtcNow.AddHours(-1);

            // Check email-specific rate limit
            var emailAttempts = await _otpAttemptRepository.CountEmailAttemptsAsync(
                email, oneHourAgo, OtpAttemptTypes.SendOtp);

            if (emailAttempts >= _maxOtpSendPerHour)
            {
                _logger.LogWarning("Email {Email} exceeded OTP send rate limit: {Attempts}/{Max}", 
                    email, emailAttempts, _maxOtpSendPerHour);
                return false;
            }

            // Check IP-specific rate limit
            var ipAttempts = await _otpAttemptRepository.CountIpAttemptsAsync(
                ipAddress, oneHourAgo, OtpAttemptTypes.SendOtp);

            if (ipAttempts >= _maxIpAttemptsPerHour)
            {
                _logger.LogWarning("IP {IpAddress} exceeded OTP send rate limit: {Attempts}/{Max}", 
                    ipAddress, ipAttempts, _maxIpAttemptsPerHour);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking OTP send rate limit for email {Email}", email);
            return false; // Fail safe
        }
    }

    public async Task RecordAttemptAsync(Guid? userId, string? email, string ipAddress, string attemptType, bool success)
    {
        try
        {
            var attempt = new OtpAttempt
            {
                UserId = userId,
                Email = email,
                IpAddress = ipAddress,
                AttemptType = attemptType,
                Success = success,
                AttemptedAt = DateTime.UtcNow
            };

            await _otpAttemptRepository.CreateAsync(attempt);

            _logger.LogInformation("Recorded {AttemptType} attempt for user {UserId}/email {Email} from IP {IpAddress}: {Success}", 
                attemptType, userId, email, ipAddress, success ? "Success" : "Failed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording attempt for user {UserId}", userId);
        }
    }

    public async Task<bool> IsAccountLockedAsync(Guid userId)
    {
        try
        {
            var lockoutPeriod = DateTime.UtcNow.AddMinutes(-_lockoutDurationMinutes);

            var recentFailedAttempts = await _otpAttemptRepository.CountUserAttemptsAsync(
                userId, lockoutPeriod, successOnly: false);

            var isLocked = recentFailedAttempts >= _maxFailedAttemptsForLockout;

            if (isLocked)
            {
                _logger.LogWarning("User {UserId} account is locked due to {Attempts} failed attempts", 
                    userId, recentFailedAttempts);
            }

            return isLocked;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking account lockout for user {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> IsIpBlockedAsync(string ipAddress)
    {
        try
        {
            var oneHourAgo = DateTime.UtcNow.AddHours(-1);

            var ipAttempts = await _otpAttemptRepository.CountIpAttemptsAsync(
                ipAddress, oneHourAgo, successOnly: false);

            var isBlocked = ipAttempts >= _maxIpAttemptsPerHour * 2; // Double the normal limit for blocking

            if (isBlocked)
            {
                _logger.LogWarning("IP {IpAddress} is blocked due to {Attempts} attempts", 
                    ipAddress, ipAttempts);
            }

            return isBlocked;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking IP block status for {IpAddress}", ipAddress);
            return false;
        }
    }

    public async Task<TimeSpan?> GetOtpSendCooldownAsync(Guid userId)
    {
        try
        {
            var oneHourAgo = DateTime.UtcNow.AddHours(-1);
            var attempts = await _otpAttemptRepository.GetUserAttemptsAsync(
                userId, oneHourAgo, OtpAttemptTypes.SendOtp);

            if (attempts.Count >= _maxOtpSendPerHour)
            {
                var oldestAttempt = attempts.OrderBy(a => a.AttemptedAt).First();
                var cooldownEnd = oldestAttempt.AttemptedAt.AddHours(1);
                var remaining = cooldownEnd - DateTime.UtcNow;

                return remaining > TimeSpan.Zero ? remaining : null;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting OTP send cooldown for user {UserId}", userId);
            return null;
        }
    }

    public async Task<TimeSpan?> GetAccountLockoutTimeAsync(Guid userId)
    {
        try
        {
            if (!await IsAccountLockedAsync(userId))
                return null;

            var lockoutPeriod = DateTime.UtcNow.AddMinutes(-_lockoutDurationMinutes);
            var failedAttempts = await _otpAttemptRepository.GetFailedAttemptsAsync(userId, lockoutPeriod);

            if (failedAttempts.Any())
            {
                var latestFailedAttempt = failedAttempts.OrderByDescending(a => a.AttemptedAt).First();
                var lockoutEnd = latestFailedAttempt.AttemptedAt.AddMinutes(_lockoutDurationMinutes);
                var remaining = lockoutEnd - DateTime.UtcNow;

                return remaining > TimeSpan.Zero ? remaining : null;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting account lockout time for user {UserId}", userId);
            return null;
        }
    }

    public async Task<int> CleanupOldAttemptsAsync(TimeSpan? olderThan = null)
    {
        try
        {
            var cutoffTime = DateTime.UtcNow.Subtract(olderThan ?? TimeSpan.FromHours(24));
            var deletedCount = await _otpAttemptRepository.DeleteOldAttemptsAsync(cutoffTime);

            _logger.LogInformation("Cleaned up {Count} old OTP attempt records", deletedCount);
            return deletedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up old OTP attempts");
            return 0;
        }
    }
}
