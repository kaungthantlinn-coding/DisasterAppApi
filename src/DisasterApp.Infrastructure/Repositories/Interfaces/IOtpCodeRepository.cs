using DisasterApp.Domain.Entities;

namespace DisasterApp.Infrastructure.Repositories.Interfaces;
public interface IOtpCodeRepository
{
    Task<OtpCode> CreateAsync(OtpCode otpCode);
    Task<OtpCode?> GetByIdAsync(Guid id);//
    Task<OtpCode?> GetByUserAndCodeAsync(Guid userId, string code, string type);
    Task<List<OtpCode>> GetActiveCodesAsync(Guid userId, string? type = null);
    Task<OtpCode> UpdateAsync(OtpCode otpCode);
    Task<bool> DeleteAsync(Guid id);
    Task<int> DeleteExpiredAsync();
    Task<int> DeleteByUserAsync(Guid userId, string? type = null);
    Task<int> GetActiveCountAsync(Guid userId, string? type = null);
}
