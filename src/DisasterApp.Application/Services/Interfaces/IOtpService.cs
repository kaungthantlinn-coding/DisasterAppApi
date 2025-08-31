using DisasterApp.Application.DTOs;

namespace DisasterApp.Application.Services.Interfaces
{
    public interface IOtpService
    {
        Task<SendOtpResponseDto> SendOtpAsync(Guid userId, string email, string type);
        Task<SendOtpResponseDto> SendOtpAsync(Guid userId, string type);
        Task<bool> VerifyOtpAsync(Guid userId, string code, string type);
        Task<bool> MarkOtpAsUsedAsync(Guid userId, string code, string type);
        string GenerateOtpCode();//
        Task<int> CleanupExpiredCodesAsync();
        Task<int> GetActiveCodeCountAsync(Guid userId, string? type = null);
        Task<int> InvalidateCodesAsync(Guid userId, string? type = null);
    }
}
