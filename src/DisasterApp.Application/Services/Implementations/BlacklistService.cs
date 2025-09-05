using DisasterApp.Application.DTOs;
using DisasterApp.Application.Services.Interfaces;
using DisasterApp.Domain.Entities;
using DisasterApp.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace DisasterApp.Application.Services.Implementations;//

public class BlacklistService : IBlacklistService
{
    private readonly IUserBlacklistRepository _blacklistRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<BlacklistService> _logger;

    public BlacklistService(
        IUserBlacklistRepository blacklistRepository,
        IUserRepository userRepository,
        ILogger<BlacklistService> logger)
    {
        _blacklistRepository = blacklistRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<BlacklistUserResponseDto> BlacklistUserAsync(Guid userId, BlacklistUserDto blacklistDto, Guid adminUserId)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return new BlacklistUserResponseDto
                {
                    Success = false,
                    Message = "User not found",
                    Data = null!
                };
            }

            var admin = await _userRepository.GetByIdAsync(adminUserId);
            if (admin == null)
            {
                return new BlacklistUserResponseDto
                {
                    Success = false,
                    Message = "Admin user not found",
                    Data = null!
                };
            }

            var existingBlacklist = await _blacklistRepository.GetActiveBlacklistAsync(userId);
            if (existingBlacklist != null)
            {
                return new BlacklistUserResponseDto
                {
                    Success = false,
                    Message = "User is already blacklisted",
                    Data = null!
                };
            }

            if (userId == adminUserId)
            {
                return new BlacklistUserResponseDto
                {
                    Success = false,
                    Message = "Cannot blacklist yourself",
                    Data = null!
                };
            }

            var blacklistRecord = new UserBlacklist
            {
                UserId = userId,
                Reason = blacklistDto.Reason,
                BlacklistedBy = adminUserId,
                BlacklistedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _blacklistRepository.CreateAsync(blacklistRecord);

            user.IsBlacklisted = true;
            await _userRepository.UpdateAsync(user);

            _logger.LogInformation("User {UserId} blacklisted by admin {AdminId} with reason: {Reason}", 
                userId, adminUserId, blacklistDto.Reason);

            return new BlacklistUserResponseDto
            {
                Success = true,
                Message = "User blacklisted successfully",
                Data = new BlacklistUserDataDto
                {
                    UserId = userId,
                    BlacklistedAt = blacklistRecord.BlacklistedAt,
                    Reason = blacklistRecord.Reason,
                    BlacklistedBy = adminUserId,
                    BlacklistedByName = admin.Name
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error blacklisting user {UserId}", userId);
            return new BlacklistUserResponseDto
            {
                Success = false,
                Message = "An error occurred while blacklisting the user",
                Data = null!
            };
        }
    }

    public async Task<UnblacklistUserResponseDto> UnblacklistUserAsync(Guid userId, UnblacklistUserDto? unblacklistDto, Guid adminUserId)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return new UnblacklistUserResponseDto
                {
                    Success = false,
                    Message = "User not found",
                    Data = null!
                };
            }

            var admin = await _userRepository.GetByIdAsync(adminUserId);
            if (admin == null)
            {
                return new UnblacklistUserResponseDto
                {
                    Success = false,
                    Message = "Admin user not found",
                    Data = null!
                };
            }

            var activeBlacklist = await _blacklistRepository.GetActiveBlacklistAsync(userId);
            if (activeBlacklist == null)
            {
                return new UnblacklistUserResponseDto
                {
                    Success = false,
                    Message = "User is not currently blacklisted",
                    Data = null!
                };
            }

            activeBlacklist.IsActive = false;
            activeBlacklist.UnblacklistedBy = adminUserId;
            activeBlacklist.UnblacklistedAt = DateTime.UtcNow;
            activeBlacklist.UpdatedAt = DateTime.UtcNow;

            await _blacklistRepository.UpdateAsync(activeBlacklist);

            user.IsBlacklisted = false;
            await _userRepository.UpdateAsync(user);

            _logger.LogInformation("User {UserId} unblacklisted by admin {AdminId}", userId, adminUserId);

            return new UnblacklistUserResponseDto
            {
                Success = true,
                Message = "User unblacklisted successfully",
                Data = new UnblacklistUserDataDto
                {
                    UserId = userId,
                    UnblacklistedAt = activeBlacklist.UnblacklistedAt.Value,
                    UnblacklistedBy = adminUserId,
                    UnblacklistedByName = admin.Name,
                    Reason = unblacklistDto?.Reason 
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unblacklisting user {UserId}", userId);
            return new UnblacklistUserResponseDto
            {
                Success = false,
                Message = "An error occurred while unblacklisting the user",
                Data = null!
            };
        }
    }

    public async Task<IEnumerable<BlacklistHistoryDto>> GetBlacklistHistoryAsync(Guid userId)
    {
        try
        {
            var history = await _blacklistRepository.GetBlacklistHistoryAsync(userId);
            
            return history.Select(h => new BlacklistHistoryDto
            {
                Id = h.Id,
                Reason = h.Reason,
                BlacklistedBy = new UserSummaryDto
                {
                    UserId = h.BlacklistedByUser.UserId,
                    Name = h.BlacklistedByUser.Name,
                    Email = h.BlacklistedByUser.Email,
                    PhotoUrl = h.BlacklistedByUser.PhotoUrl
                },
                BlacklistedAt = h.BlacklistedAt,
                UnblacklistedBy = h.UnblacklistedByUser != null ? new UserSummaryDto
                {
                    UserId = h.UnblacklistedByUser.UserId,
                    Name = h.UnblacklistedByUser.Name,
                    Email = h.UnblacklistedByUser.Email,
                    PhotoUrl = h.UnblacklistedByUser.PhotoUrl
                } : null,
                UnblacklistedAt = h.UnblacklistedAt,
                IsActive = h.IsActive
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving blacklist history for user {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> IsUserBlacklistedAsync(Guid userId)
    {
        try
        {
            return await _blacklistRepository.HasActiveBlacklistAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if user {UserId} is blacklisted", userId);
            throw;
        }
    }

    public async Task<BlacklistHistoryDto?> GetActiveBlacklistAsync(Guid userId)
    {
        try
        {
            var activeBlacklist = await _blacklistRepository.GetActiveBlacklistAsync(userId);
            if (activeBlacklist == null)
                return null;

            return new BlacklistHistoryDto
            {
                Id = activeBlacklist.Id,
                Reason = activeBlacklist.Reason,
                BlacklistedBy = new UserSummaryDto
                {
                    UserId = activeBlacklist.BlacklistedByUser.UserId,
                    Name = activeBlacklist.BlacklistedByUser.Name,
                    Email = activeBlacklist.BlacklistedByUser.Email,
                    PhotoUrl = activeBlacklist.BlacklistedByUser.PhotoUrl
                },
                BlacklistedAt = activeBlacklist.BlacklistedAt,
                UnblacklistedBy = activeBlacklist.UnblacklistedByUser != null ? new UserSummaryDto
                {
                    UserId = activeBlacklist.UnblacklistedByUser.UserId,
                    Name = activeBlacklist.UnblacklistedByUser.Name,
                    Email = activeBlacklist.UnblacklistedByUser.Email,
                    PhotoUrl = activeBlacklist.UnblacklistedByUser.PhotoUrl
                } : null,
                UnblacklistedAt = activeBlacklist.UnblacklistedAt,
                IsActive = activeBlacklist.IsActive
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active blacklist for user {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<BlacklistHistoryDto>> GetRecentBlacklistsAsync(int count = 10)
    {
        try
        {
            var recentBlacklists = await _blacklistRepository.GetRecentBlacklistsAsync(count);
            
            return recentBlacklists.Select(b => new BlacklistHistoryDto
            {
                Id = b.Id,
                Reason = b.Reason,
                BlacklistedBy = new UserSummaryDto
                {
                    UserId = b.BlacklistedByUser.UserId,
                    Name = b.BlacklistedByUser.Name,
                    Email = b.BlacklistedByUser.Email,
                    PhotoUrl = b.BlacklistedByUser.PhotoUrl
                },
                BlacklistedAt = b.BlacklistedAt,
                UnblacklistedBy = b.UnblacklistedByUser != null ? new UserSummaryDto
                {
                    UserId = b.UnblacklistedByUser.UserId,
                    Name = b.UnblacklistedByUser.Name,
                    Email = b.UnblacklistedByUser.Email,
                    PhotoUrl = b.UnblacklistedByUser.PhotoUrl
                } : null,
                UnblacklistedAt = b.UnblacklistedAt,
                IsActive = b.IsActive
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving recent blacklists");
            throw;
        }
    }

    public async Task<int> GetBlacklistCountAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        try
        {
            return await _blacklistRepository.GetBlacklistCountAsync(fromDate, toDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting blacklist count");
            throw;
        }
    }
}