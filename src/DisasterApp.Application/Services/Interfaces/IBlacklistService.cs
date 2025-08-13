using DisasterApp.Application.DTOs;

namespace DisasterApp.Application.Services.Interfaces;

public interface IBlacklistService
{
    Task<BlacklistUserResponseDto> BlacklistUserAsync(Guid userId, BlacklistUserDto blacklistDto, Guid adminUserId);
    Task<UnblacklistUserResponseDto> UnblacklistUserAsync(Guid userId, UnblacklistUserDto? unblacklistDto, Guid adminUserId);
    Task<IEnumerable<BlacklistHistoryDto>> GetBlacklistHistoryAsync(Guid userId);
    Task<bool> IsUserBlacklistedAsync(Guid userId);
    Task<BlacklistHistoryDto?> GetActiveBlacklistAsync(Guid userId);
    Task<IEnumerable<BlacklistHistoryDto>> GetRecentBlacklistsAsync(int count = 10);
    Task<int> GetBlacklistCountAsync(DateTime? fromDate = null, DateTime? toDate = null);
}