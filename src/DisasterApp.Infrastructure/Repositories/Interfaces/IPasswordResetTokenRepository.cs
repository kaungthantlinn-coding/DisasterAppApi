using DisasterApp.Domain.Entities;

namespace DisasterApp.Infrastructure.Repositories.Interfaces;

public interface IPasswordResetTokenRepository
{
    Task<PasswordResetToken?> GetByTokenAsync(string token);
    Task<PasswordResetToken> CreateAsync(PasswordResetToken passwordResetToken);
    Task<bool> DeleteAsync(string token);
    Task<bool> DeleteAllUserTokensAsync(Guid userId);
    Task<bool> IsValidAsync(string token);
    Task<bool> MarkAsUsedAsync(string token);
}
