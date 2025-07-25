using DisasterApp.Domain.Entities;

namespace DisasterApp.Infrastructure.Repositories.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task<RefreshToken> CreateAsync(RefreshToken refreshToken);
    Task<bool> DeleteAsync(string token);
    Task<bool> DeleteAllUserTokensAsync(Guid userId);
    Task<bool> IsValidAsync(string token);
}