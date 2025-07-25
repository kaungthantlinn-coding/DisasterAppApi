using DisasterApp.Application.DTOs;
using DisasterApp.Application.Services.Interfaces;
using DisasterApp.Domain.Entities;
using DisasterApp.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using BCrypt.Net;

namespace DisasterApp.Application.Services.Implementations;

public class UserManagementService : IUserManagementService
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordResetTokenRepository _passwordResetTokenRepository;
    private readonly IRoleService _roleService;
    private readonly IAuditService _auditService;
    private readonly ILogger<UserManagementService> _logger;

    public UserManagementService(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordResetTokenRepository passwordResetTokenRepository,
        IRoleService roleService,
        IAuditService auditService,
        ILogger<UserManagementService> logger)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordResetTokenRepository = passwordResetTokenRepository;
        _roleService = roleService;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<PagedUserListDto> GetUsersAsync(UserFilterDto filter)
    {
        try
        {
            // Validate pagination parameters
            filter.PageNumber = Math.Max(1, filter.PageNumber);
            filter.PageSize = Math.Clamp(filter.PageSize, 1, 100);

            // Convert Status filter to IsBlacklisted
            bool? isBlacklistedFilter = filter.IsBlacklisted;
            if (!string.IsNullOrEmpty(filter.Status))
            {
                isBlacklistedFilter = filter.Status.ToLower() switch
                {
                    "active" => false,
                    "suspended" => true,
                    "all" => null,
                    _ => filter.IsBlacklisted
                };
            }

            var (users, totalCount) = await _userRepository.GetUsersAsync(
                filter.PageNumber,
                filter.PageSize,
                filter.SearchTerm,
                filter.Role,
                isBlacklistedFilter,
                filter.AuthProvider,
                filter.CreatedAfter,
                filter.CreatedBefore,
                filter.SortBy,
                filter.SortDirection);

            var userListItems = users.Select(u => new UserListItemDto
            {
                UserId = u.UserId,
                Name = u.Name,
                Email = u.Email,
                PhotoUrl = u.PhotoUrl,
                PhoneNumber = u.PhoneNumber,
                AuthProvider = u.AuthProvider,
                IsBlacklisted = u.IsBlacklisted ?? false,
                CreatedAt = u.CreatedAt,
                RoleNames = u.Roles.Select(r => r.Name).ToList()
            }).ToList();

            return new PagedUserListDto
            {
                Users = userListItems,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users with filter");
            throw;
        }
    }

    public async Task<UserDetailsDto?> GetUserByIdAsync(Guid userId)
    {
        try
        {
            var user = await _userRepository.GetUserWithDetailsAsync(userId);
            if (user == null) return null;

            var (disasterReports, supportRequests, donations, organizations) =
                await _userRepository.GetUserStatisticsAsync(userId);

            return new UserDetailsDto
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                PhotoUrl = user.PhotoUrl,
                PhoneNumber = user.PhoneNumber,
                AuthProvider = user.AuthProvider,
                IsBlacklisted = user.IsBlacklisted ?? false,
                CreatedAt = user.CreatedAt,
                Roles = user.Roles.Select(r => new RoleDto { RoleId = r.RoleId, Name = r.Name }).ToList(),
                Statistics = new UserStatisticsDto
                {
                    DisasterReportsCount = disasterReports,
                    SupportRequestsCount = supportRequests,
                    AssistanceProvidedCount = 0, // Removed AssistanceProvided entity
                    DonationsCount = donations,
                    OrganizationsCount = organizations
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user {UserId}", userId);
            throw;
        }
    }

    public async Task<UserDetailsDto> CreateUserAsync(CreateUserDto createUserDto)
    {
        try
        {
            // Validate email uniqueness
            if (await _userRepository.ExistsAsync(createUserDto.Email))
            {
                throw new InvalidOperationException("User with this email already exists");
            }

            // Hash password
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(createUserDto.Password);

            // Create user entity
            var user = new User
            {
                UserId = Guid.NewGuid(),
                Name = createUserDto.Name,
                Email = createUserDto.Email,
                PhotoUrl = createUserDto.PhotoUrl,
                PhoneNumber = createUserDto.PhoneNumber,
                AuthProvider = "local",
                AuthId = hashedPassword,
                IsBlacklisted = createUserDto.IsBlacklisted,
                CreatedAt = DateTime.UtcNow
            };

            // Create user
            var createdUser = await _userRepository.CreateAsync(user);

            // Assign roles
            if (createUserDto.Roles.Any())
            {
                foreach (var roleName in createUserDto.Roles)
                {
                    try
                    {
                        await _roleService.AssignRoleToUserAsync(createdUser.UserId, roleName);
                    }
                    catch (ArgumentException ex)
                    {
                        _logger.LogWarning("Failed to assign role {RoleName} to user {UserId}: {Message}",
                            roleName, createdUser.UserId, ex.Message);
                    }
                }
            }
            else
            {
                // Assign default role if no roles specified
                await _roleService.AssignDefaultRoleToUserAsync(createdUser.UserId);
            }

            _logger.LogInformation("Created user {UserId} with email {Email}", createdUser.UserId, createdUser.Email);

            // Return created user details
            return await GetUserByIdAsync(createdUser.UserId)
                ?? throw new InvalidOperationException("Failed to retrieve created user");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user with email {Email}", createUserDto.Email);
            throw;
        }
    }

    public async Task<UserDetailsDto> UpdateUserAsync(Guid userId, UpdateUserDto updateUserDto)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new ArgumentException($"User with ID {userId} not found");
            }

            // Validate email uniqueness (excluding current user)
            if (await _userRepository.ExistsAsync(updateUserDto.Email, userId))
            {
                throw new InvalidOperationException("Another user with this email already exists");
            }

            // Update user properties
            user.Name = updateUserDto.Name;
            user.Email = updateUserDto.Email;
            user.PhotoUrl = updateUserDto.PhotoUrl;
            user.PhoneNumber = updateUserDto.PhoneNumber;
            user.IsBlacklisted = updateUserDto.IsBlacklisted;

            // Update user
            await _userRepository.UpdateAsync(user);

            // Update roles with validation
            var currentRoles = await _roleService.GetUserRolesAsync(userId);
            var currentRoleNames = currentRoles.Select(r => r.Name).ToList();

            // Validate role changes before applying them
            var rolesToRemove = currentRoleNames.Where(r => !updateUserDto.Roles.Contains(r)).ToList();
            var rolesToAdd = updateUserDto.Roles.Where(r => !currentRoleNames.Contains(r)).ToList();

            // Validate role removals
            foreach (var roleToRemove in rolesToRemove)
            {
                if (!await _roleService.CanRemoveRoleAsync(userId, roleToRemove))
                {
                    throw new InvalidOperationException($"Cannot remove role '{roleToRemove}' from user. This would violate system constraints.");
                }
            }

            // Remove roles that are no longer assigned
            foreach (var currentRole in rolesToRemove)
            {
                try
                {
                    await _roleService.RemoveRoleFromUserAsync(userId, currentRole);
                }
                catch (InvalidOperationException ex)
                {
                    _logger.LogError("Failed to remove role {RoleName} from user {UserId}: {Message}",
                        currentRole, userId, ex.Message);
                    throw;
                }
            }

            // Add new roles
            foreach (var newRole in rolesToAdd)
            {
                try
                {
                    await _roleService.AssignRoleToUserAsync(userId, newRole);
                }
                catch (ArgumentException ex)
                {
                    _logger.LogWarning("Failed to assign role {RoleName} to user {UserId}: {Message}",
                        newRole, userId, ex.Message);
                    throw;
                }
            }

            _logger.LogInformation("Updated user {UserId}", userId);

            // Return updated user details
            return await GetUserByIdAsync(userId)
                ?? throw new InvalidOperationException("Failed to retrieve updated user");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> DeleteUserAsync(Guid userId)
    {
        try
        {
            // Validate user can be deleted
            var validation = await ValidateUserDeletionAsync(userId);
            if (!validation.CanDelete)
            {
                throw new InvalidOperationException($"Cannot delete user: {string.Join(", ", validation.Reasons)}");
            }

            // Clean up associated tokens before deleting the user
            // This prevents foreign key constraint violations
            _logger.LogInformation("Cleaning up tokens for user {UserId} before deletion", userId);

            // Delete all refresh tokens for the user
            try
            {
                await _refreshTokenRepository.DeleteAllUserTokensAsync(userId);
                _logger.LogDebug("Deleted refresh tokens for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete refresh tokens for user {UserId}, continuing with deletion", userId);
            }

            // Delete all password reset tokens for the user
            try
            {
                await _passwordResetTokenRepository.DeleteAllUserTokensAsync(userId);
                _logger.LogDebug("Deleted password reset tokens for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete password reset tokens for user {UserId}, continuing with deletion", userId);
            }

            // Now delete the user
            var result = await _userRepository.DeleteUserAsync(userId);
            if (result)
            {
                _logger.LogInformation("Successfully deleted user {UserId}", userId);
            }
            else
            {
                _logger.LogWarning("Failed to delete user {UserId} - user may not exist", userId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> BlacklistUserAsync(Guid userId)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return false;

            user.IsBlacklisted = true;
            await _userRepository.UpdateAsync(user);

            _logger.LogInformation("Blacklisted user {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error blacklisting user {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> UnblacklistUserAsync(Guid userId)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return false;

            user.IsBlacklisted = false;
            await _userRepository.UpdateAsync(user);

            _logger.LogInformation("Unblacklisted user {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unblacklisting user {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> ChangeUserPasswordAsync(Guid userId, ChangeUserPasswordDto changePasswordDto)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return false;

            // Only allow password change for local auth users
            if (user.AuthProvider != "local")
            {
                throw new InvalidOperationException("Cannot change password for non-local authentication users");
            }

            // Hash new password
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword);
            user.AuthId = hashedPassword;

            await _userRepository.UpdateAsync(user);

            _logger.LogInformation("Changed password for user {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user {UserId}", userId);
            throw;
        }
    }

    public async Task<int> BulkOperationAsync(BulkUserOperationDto bulkOperation)
    {
        try
        {
            var users = await _userRepository.GetUsersByIdsAsync(bulkOperation.UserIds);
            var affectedCount = 0;

            switch (bulkOperation.Operation.ToLower())
            {
                case "blacklist":
                    foreach (var user in users)
                    {
                        user.IsBlacklisted = true;
                        affectedCount++;
                    }
                    break;

                case "unblacklist":
                    foreach (var user in users)
                    {
                        user.IsBlacklisted = false;
                        affectedCount++;
                    }
                    break;

                case "assign-role":
                    if (string.IsNullOrEmpty(bulkOperation.RoleName))
                        throw new ArgumentException("Role name is required for role assignment");

                    foreach (var user in users)
                    {
                        try
                        {
                            await _roleService.AssignRoleToUserAsync(user.UserId, bulkOperation.RoleName);
                            affectedCount++;
                        }
                        catch (ArgumentException ex)
                        {
                            _logger.LogWarning("Failed to assign role {RoleName} to user {UserId}: {Message}",
                                bulkOperation.RoleName, user.UserId, ex.Message);
                        }
                    }
                    break;

                case "remove-role":
                    if (string.IsNullOrEmpty(bulkOperation.RoleName))
                        throw new ArgumentException("Role name is required for role removal");

                    foreach (var user in users)
                    {
                        try
                        {
                            await _roleService.RemoveRoleFromUserAsync(user.UserId, bulkOperation.RoleName);
                            affectedCount++;
                        }
                        catch (ArgumentException ex)
                        {
                            _logger.LogWarning("Failed to remove role {RoleName} from user {UserId}: {Message}",
                                bulkOperation.RoleName, user.UserId, ex.Message);
                        }
                    }
                    break;

                default:
                    throw new ArgumentException($"Unknown operation: {bulkOperation.Operation}");
            }

            if (bulkOperation.Operation.ToLower() is "blacklist" or "unblacklist")
            {
                await _userRepository.BulkUpdateUsersAsync(users);
            }

            _logger.LogInformation("Performed bulk operation {Operation} on {Count} users",
                bulkOperation.Operation, affectedCount);

            return affectedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing bulk operation {Operation}", bulkOperation.Operation);
            throw;
        }
    }

    public async Task<UserManagementStatsDto> GetDashboardStatsAsync()
    {
        try
        {
            var totalUsers = await _userRepository.GetTotalUsersCountAsync();
            var activeUsers = await _userRepository.GetActiveUsersCountAsync();
            var suspendedUsers = await _userRepository.GetSuspendedUsersCountAsync();
            var adminUsers = await _userRepository.GetAdminUsersCountAsync();

            // Get new users this month and today
            var thisMonth = DateTime.UtcNow.Date.AddDays(1 - DateTime.UtcNow.Day);
            var today = DateTime.UtcNow.Date;

            var (monthUsers, monthTotalCount) = await _userRepository.GetUsersAsync(
                1, int.MaxValue, createdAfter: thisMonth);
            var (todayUsers, todayTotalCount) = await _userRepository.GetUsersAsync(
                1, int.MaxValue, createdAfter: today);

            return new UserManagementStatsDto
            {
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,
                SuspendedUsers = suspendedUsers,
                AdminUsers = adminUsers,
                NewUsersThisMonth = monthTotalCount,
                NewUsersToday = todayTotalCount
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dashboard stats");
            throw;
        }
    }

    public async Task<UserDeletionValidationDto> ValidateUserDeletionAsync(Guid userId)
    {
        try
        {
            var validation = new UserDeletionValidationDto { CanDelete = true };
            var user = await _userRepository.GetUserWithDetailsAsync(userId);

            if (user == null)
            {
                validation.CanDelete = false;
                validation.Reasons.Add("User not found");
                return validation;
            }

            // Check if user has active disaster reports
            if (user.DisasterReportUsers.Any())
            {
                validation.HasActiveReports = true;
                validation.Reasons.Add("User has active disaster reports");
            }

            // Check if user has active support requests
            if (user.SupportRequests.Any())
            {
                validation.HasActiveRequests = true;
                validation.Reasons.Add("User has active support requests");
            }

            // Check if user is the last admin
            var isAdmin = user.Roles.Any(r => r.Name.ToLower() == "admin");
            if (isAdmin)
            {
                var adminCount = await _userRepository.GetAdminUsersCountAsync();
                if (adminCount <= 1)
                {
                    validation.IsLastAdmin = true;
                    validation.CanDelete = false;
                    validation.Reasons.Add("Cannot delete the last admin user");
                }
            }

            // For now, allow deletion even with active reports/requests (soft delete via blacklisting)
            // In production, you might want to set CanDelete = false for these cases
            validation.CanDelete = !validation.IsLastAdmin;

            return validation;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating user deletion for {UserId}", userId);
            throw;
        }
    }

    public async Task<UserDetailsDto> UpdateUserRolesAsync(Guid userId, UpdateUserRolesDto updateRolesDto)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new ArgumentException($"User with ID {userId} not found");
            }

            // Validate role update
            var validation = await ValidateRoleUpdateAsync(userId, updateRolesDto.Roles);
            if (!validation.CanUpdate)
            {
                throw new InvalidOperationException($"Cannot update roles: {string.Join(", ", validation.Errors)}");
            }

            // Update roles using the existing logic
            var currentRoles = await _roleService.GetUserRolesAsync(userId);
            var currentRoleNames = currentRoles.Select(r => r.Name).ToList();

            var rolesToRemove = currentRoleNames.Where(r => !updateRolesDto.Roles.Contains(r)).ToList();
            var rolesToAdd = updateRolesDto.Roles.Where(r => !currentRoleNames.Contains(r)).ToList();

            // Remove roles that are no longer assigned
            foreach (var roleToRemove in rolesToRemove)
            {
                await _roleService.RemoveRoleFromUserAsync(userId, roleToRemove);
            }

            // Add new roles
            foreach (var roleToAdd in rolesToAdd)
            {
                await _roleService.AssignRoleToUserAsync(userId, roleToAdd);
            }

            // Log the overall role update
            if (rolesToAdd.Any() || rolesToRemove.Any())
            {
                await _auditService.LogRoleUpdateAsync(userId, currentRoleNames, updateRolesDto.Roles, null, "System", null, null);
            }

            _logger.LogInformation("Updated roles for user {UserId}. Added: {AddedRoles}, Removed: {RemovedRoles}",
                userId, string.Join(", ", rolesToAdd), string.Join(", ", rolesToRemove));

            // Return updated user details
            return await GetUserByIdAsync(userId)
                ?? throw new InvalidOperationException("Failed to retrieve updated user");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating roles for user {UserId}", userId);
            throw;
        }
    }

    public async Task<RoleUpdateValidationDto> ValidateRoleUpdateAsync(Guid userId, List<string> newRoles)
    {
        try
        {
            var validation = new RoleUpdateValidationDto { CanUpdate = true };

            var user = await _userRepository.GetUserWithDetailsAsync(userId);
            if (user == null)
            {
                validation.CanUpdate = false;
                validation.Errors.Add("User not found");
                return validation;
            }

            var currentRoleNames = user.Roles.Select(r => r.Name).ToList();
            var rolesToRemove = currentRoleNames.Where(r => !newRoles.Contains(r)).ToList();

            // Check if removing admin role from last admin
            if (rolesToRemove.Contains("admin", StringComparer.OrdinalIgnoreCase))
            {
                var adminCount = await _roleService.GetAdminCountAsync();
                validation.AdminCount = adminCount;
                validation.IsLastAdmin = adminCount <= 1;

                if (validation.IsLastAdmin)
                {
                    validation.CanUpdate = false;
                    validation.Errors.Add("Cannot remove admin role from the last admin user");
                }
            }

            // Check if removing all roles
            if (!newRoles.Any())
            {
                validation.CanUpdate = false;
                validation.Errors.Add("User must have at least one role assigned");
            }

            // Validate that all new roles exist
            var allRoles = await _roleService.GetAllRolesAsync();
            var validRoleNames = allRoles.Select(r => r.Name).ToList();
            var invalidRoles = newRoles.Where(r => !validRoleNames.Contains(r, StringComparer.OrdinalIgnoreCase)).ToList();

            if (invalidRoles.Any())
            {
                validation.CanUpdate = false;
                validation.Errors.Add($"Invalid roles: {string.Join(", ", invalidRoles)}");
            }

            // Add warnings for significant role changes
            if (rolesToRemove.Contains("admin", StringComparer.OrdinalIgnoreCase) && !validation.IsLastAdmin)
            {
                validation.Warnings.Add("Removing admin privileges from user");
            }

            if (newRoles.Contains("admin", StringComparer.OrdinalIgnoreCase) && !currentRoleNames.Contains("admin", StringComparer.OrdinalIgnoreCase))
            {
                validation.Warnings.Add("Granting admin privileges to user");
            }

            return validation;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating role update for user {UserId}", userId);
            throw;
        }
    }
}
