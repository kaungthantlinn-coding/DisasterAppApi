using DisasterApp.Application.DTOs;

namespace DisasterApp.Application.Services.Interfaces;

/// <summary>
/// Service interface for OTP (One-Time Password) operations
/// </summary>
public interface IOtpService
{
    /// <summary>
    /// Generate and send an OTP code to a user's email
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="email">Email address to send OTP to</param>
    /// <param name="type">Type of OTP (login, setup, disable, etc.)</param>
    /// <returns>Send OTP response</returns>
    Task<SendOtpResponseDto> SendOtpAsync(Guid userId, string email, string type);

    /// <summary>
    /// Generate and send an OTP code for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="type">Type of OTP</param>
    /// <returns>Send OTP response</returns>
    Task<SendOtpResponseDto> SendOtpAsync(Guid userId, string type);

    /// <summary>
    /// Verify an OTP code
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="code">OTP code to verify</param>
    /// <param name="type">Type of OTP being verified</param>
    /// <returns>True if code is valid</returns>
    Task<bool> VerifyOtpAsync(Guid userId, string code, string type);

    /// <summary>
    /// Mark an OTP code as used after successful authentication
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="code">OTP code to mark as used</param>
    /// <param name="type">Type of OTP code</param>
    /// <returns>True if successfully marked as used</returns>
    Task<bool> MarkOtpAsUsedAsync(Guid userId, string code, string type);



    /// <summary>
    /// Generate a 6-digit OTP code
    /// </summary>
    /// <returns>6-digit OTP code</returns>
    string GenerateOtpCode();

    /// <summary>
    /// Clean up expired OTP codes
    /// </summary>
    /// <returns>Number of codes cleaned up</returns>
    Task<int> CleanupExpiredCodesAsync();

    /// <summary>
    /// Get the number of active OTP codes for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="type">Type of OTP (optional)</param>
    /// <returns>Number of active codes</returns>
    Task<int> GetActiveCodeCountAsync(Guid userId, string? type = null);

    /// <summary>
    /// Invalidate all OTP codes for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="type">Type of OTP to invalidate (optional, null for all types)</param>
    /// <returns>Number of codes invalidated</returns>
    Task<int> InvalidateCodesAsync(Guid userId, string? type = null);
}
