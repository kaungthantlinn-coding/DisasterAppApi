using DisasterApp.Application.DTOs;

namespace DisasterApp.Application.Services.Interfaces
{
    public interface ITwoFactorService
    {
        Task<TwoFactorStatusDto> GetTwoFactorStatusAsync(Guid userId);
        Task<SetupTwoFactorResponseDto> SetupTwoFactorAsync(Guid userId, string currentPassword);//
        Task<VerifySetupResponseDto> VerifySetupAsync(Guid userId, string otpCode);
        Task<bool> DisableTwoFactorAsync(Guid userId, string currentPassword, string? otpCode = null);
        Task<GenerateBackupCodesResponseDto> GenerateBackupCodesAsync(Guid userId, string currentPassword, string? otpCode = null);
        Task<bool> IsTwoFactorEnabledAsync(Guid userId);
        Task UpdateLastUsedAsync(Guid userId);
    }
}
