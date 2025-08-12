using DisasterApp.Domain.Entities;

namespace DisasterApp.Infrastructure.Repositories.Interfaces
{
    public interface IBackupCodeRepository
    {
        Task<BackupCode> CreateAsync(BackupCode backupCode);
        Task<List<BackupCode>> CreateManyAsync(List<BackupCode> backupCodes);
        Task<BackupCode?> GetByIdAsync(Guid id);
        Task<List<BackupCode>> GetUnusedCodesAsync(Guid userId);
        Task<BackupCode?> GetByUserAndHashAsync(Guid userId, string codeHash);
        Task<BackupCode> UpdateAsync(BackupCode backupCode);
        Task<bool> DeleteAsync(Guid id);
        Task<int> DeleteByUserAsync(Guid userId);
        Task<int> GetUnusedCountAsync(Guid userId);
        Task<bool> MarkAsUsedAsync(Guid id);
    }
}
