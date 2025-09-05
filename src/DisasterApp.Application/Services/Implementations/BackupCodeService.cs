using DisasterApp.Application.Services.Interfaces;
using DisasterApp.Domain.Entities;
using DisasterApp.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace DisasterApp.Application.Services.Implementations;
public class BackupCodeService : IBackupCodeService
{
    private readonly IBackupCodeRepository _backupCodeRepository;
    private readonly IUserRepository _userRepository;//
    private readonly ILogger<BackupCodeService> _logger;

    public BackupCodeService(
        IBackupCodeRepository backupCodeRepository,
        IUserRepository userRepository,
        ILogger<BackupCodeService> logger)
    {
        _backupCodeRepository = backupCodeRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<List<string>> GenerateBackupCodesAsync(Guid userId, int count = 8)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }

            await _backupCodeRepository.DeleteByUserAsync(userId);

            var backupCodes = new List<string>();
            var backupCodeEntities = new List<BackupCode>();

            for (int i = 0; i < count; i++)
            {
                var code = GenerateBackupCode();
                var hash = HashBackupCode(code);

                backupCodes.Add(code);
                backupCodeEntities.Add(new BackupCode
                {
                    UserId = userId,
                    CodeHash = hash
                });
            }

            await _backupCodeRepository.CreateManyAsync(backupCodeEntities);

            user.BackupCodesRemaining = count;
            await _userRepository.UpdateAsync(user);

            _logger.LogInformation("Generated {Count} backup codes for user {UserId}", count, userId);
            return backupCodes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating backup codes for user {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> VerifyAndUseBackupCodeAsync(Guid userId, string backupCode)
    {
        try
        {
            var unusedCodes = await _backupCodeRepository.GetUnusedCodesAsync(userId);

            foreach (var storedCode in unusedCodes)
            {
                if (VerifyBackupCode(backupCode, storedCode.CodeHash))
                {
                    await _backupCodeRepository.MarkAsUsedAsync(storedCode.Id);

                    var user = await _userRepository.GetByIdAsync(userId);
                    if (user != null)
                    {
                        user.BackupCodesRemaining = Math.Max(0, user.BackupCodesRemaining - 1);
                        await _userRepository.UpdateAsync(user);
                    }

                    _logger.LogInformation("Backup code used successfully for user {UserId}. Remaining: {Remaining}", 
                        userId, user?.BackupCodesRemaining ?? 0);

                    return true;
                }
            }

            _logger.LogWarning("Invalid backup code attempt for user {UserId}", userId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying backup code for user {UserId}", userId);
            return false;
        }
    }

    public async Task<int> GetUnusedBackupCodeCountAsync(Guid userId)
    {
        try
        {
            return await _backupCodeRepository.GetUnusedCountAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unused backup code count for user {UserId}", userId);
            return 0;
        }
    }

    public async Task<int> InvalidateAllBackupCodesAsync(Guid userId)
    {
        try
        {
            var deletedCount = await _backupCodeRepository.DeleteByUserAsync(userId);

            var user = await _userRepository.GetByIdAsync(userId);
            if (user != null)
            {
                user.BackupCodesRemaining = 0;
                await _userRepository.UpdateAsync(user);
            }

            _logger.LogInformation("Invalidated {Count} backup codes for user {UserId}", deletedCount, userId);
            return deletedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating backup codes for user {UserId}", userId);
            return 0;
        }
    }

    public string GenerateBackupCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[8];
        rng.GetBytes(bytes);

        var result = new StringBuilder(8);
        for (int i = 0; i < 8; i++)
        {
            result.Append(chars[bytes[i] % chars.Length]);
        }

        return result.ToString();
    }

    public string HashBackupCode(string backupCode)
    {
        return BCrypt.Net.BCrypt.HashPassword(backupCode, BCrypt.Net.BCrypt.GenerateSalt());
    }

    public bool VerifyBackupCode(string backupCode, string hash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(backupCode, hash);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying backup code hash");
            return false;
        }
    }
}
