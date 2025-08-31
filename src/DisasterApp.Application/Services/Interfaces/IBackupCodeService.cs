namespace DisasterApp.Application.Services.Interfaces
{
    public interface IBackupCodeService
    {
        Task<List<string>> GenerateBackupCodesAsync(Guid userId, int count = 8);
        Task<bool> VerifyAndUseBackupCodeAsync(Guid userId, string backupCode);
        Task<int> GetUnusedBackupCodeCountAsync(Guid userId);
        Task<int> InvalidateAllBackupCodesAsync(Guid userId);
        string GenerateBackupCode();
        string HashBackupCode(string backupCode);
        bool VerifyBackupCode(string backupCode, string hash);//
    }
}
