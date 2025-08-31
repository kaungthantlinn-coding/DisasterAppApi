using DisasterApp.Application.DTOs;
using DisasterApp.Application.Services.Interfaces;
using DisasterApp.Domain.Entities;
using DisasterApp.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using BCrypt.Net;
using System.Text;
using System.Text.Json;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Colors;
using iText.Kernel.Geom;//


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
                Roles = user.Roles.Select(r => new RoleDto { Id = r.RoleId, Name = r.Name }).ToList(),
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
                AuthProvider = "Email",
                AuthId = hashedPassword,
                IsBlacklisted = createUserDto.IsBlacklisted,
                CreatedAt = DateTime.UtcNow
            };

            // Create user
            var createdUser = await _userRepository.CreateAsync(user);

            // Assign roles
            var assignedRoles = new List<string>();
            if (createUserDto.Roles.Any())
            {
                foreach (var roleName in createUserDto.Roles)
                {
                    try
                    {
                        await _roleService.AssignRoleToUserAsync(createdUser.UserId, roleName);
                        assignedRoles.Add(roleName);
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
                assignedRoles.Add("user"); // Assuming default role is "user"
            }

            // Add audit logging for user creation
            try
            {
                await _auditService.LogUserActionAsync(
                    action: "CREATE_USER",
                    severity: "Info",
                    userId: null, // Will be set by audit service from current context
                    details: $"Created user '{createdUser.Name}' with email '{createdUser.Email}' and roles: {string.Join(", ", assignedRoles)}",
                    resource: "UserManagement",
                    metadata: new Dictionary<string, object>
                    {
                        ["targetUserId"] = createdUser.UserId.ToString(),
                        ["userEmail"] = createdUser.Email,
                        ["userName"] = createdUser.Name,
                        ["assignedRoles"] = assignedRoles,
                        ["authProvider"] = "Email"
                    }
                );
            }
            catch (Exception auditEx)
            {
                _logger.LogWarning(auditEx, "Failed to log user creation audit for user {UserId}", createdUser.UserId);
            }

            _logger.LogInformation("Created user {UserId} with email {Email} and roles {Roles}", 
                createdUser.UserId, createdUser.Email, string.Join(", ", assignedRoles));

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

            // Update user first
            await _userRepository.UpdateAsync(user);

            // Replace user roles atomically using ReplaceUserRolesAsync
            // This method handles all role validation and ensures atomic updates
            await _roleService.ReplaceUserRolesAsync(userId, updateUserDto.Roles);

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

            // Add audit logging for user suspension
            try
            {
                await _auditService.LogUserActionAsync(
                    action: "USER_SUSPENDED",
                    severity: "medium",
                    userId: null, // Will be set by audit service from current context
                    details: $"User '{user.Name}' (ID: {user.UserId}) has been suspended/blacklisted",
                    resource: "UserManagement",
                    metadata: new Dictionary<string, object>
                    {
                        ["targetUserId"] = user.UserId.ToString(),
                        ["userEmail"] = user.Email,
                        ["userName"] = user.Name,
                        ["action"] = "blacklist"
                    }
                );
            }
            catch (Exception auditEx)
            {
                _logger.LogWarning(auditEx, "Failed to log user suspension audit for user {UserId}", userId);
            }

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

            // Add audit logging for user reactivation
            try
            {
                await _auditService.LogUserActionAsync(
                    action: "USER_REACTIVATED",
                    severity: "medium",
                    userId: null, // Will be set by audit service from current context
                    details: $"User '{user.Name}' (ID: {user.UserId}) has been reactivated/unblacklisted",
                    resource: "UserManagement",
                    metadata: new Dictionary<string, object>
                    {
                        ["targetUserId"] = user.UserId.ToString(),
                        ["userEmail"] = user.Email,
                        ["userName"] = user.Name,
                        ["action"] = "unblacklist"
                    }
                );
            }
            catch (Exception auditEx)
            {
                _logger.LogWarning(auditEx, "Failed to log user reactivation audit for user {UserId}", userId);
            }

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
            if (user.AuthProvider != "Email")
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

    public async Task<int> BulkOperationAsync(BulkUserOperationDto bulkOperation, Guid? adminUserId = null)
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
                        // Prevent self-blacklisting
                        if (adminUserId.HasValue && user.UserId == adminUserId.Value)
                        {
                            _logger.LogWarning("Admin {AdminUserId} attempted to blacklist themselves, skipping", adminUserId.Value);
                            continue;
                        }
                        user.IsBlacklisted = true;
                        affectedCount++;

                        // Add audit logging for bulk user suspension
                        try
                        {
                            await _auditService.LogUserActionAsync(
                                action: "USER_SUSPENDED",
                                severity: "medium",
                                userId: adminUserId,
                                details: $"User '{user.Name}' (ID: {user.UserId}) has been suspended via bulk operation",
                                resource: "UserManagement",
                                metadata: new Dictionary<string, object>
                                {
                                    ["targetUserId"] = user.UserId.ToString(),
                                    ["userEmail"] = user.Email,
                                    ["userName"] = user.Name,
                                    ["action"] = "bulk_blacklist",
                                    ["operationType"] = "bulk"
                                }
                            );
                        }
                        catch (Exception auditEx)
                        {
                            _logger.LogWarning(auditEx, "Failed to log bulk user suspension audit for user {UserId}", user.UserId);
                        }
                    }
                    break;

                case "unblacklist":
                    foreach (var user in users)
                    {
                        user.IsBlacklisted = false;
                        affectedCount++;

                        // Add audit logging for bulk user reactivation
                        try
                        {
                            await _auditService.LogUserActionAsync(
                                action: "USER_REACTIVATED",
                                severity: "medium",
                                userId: adminUserId,
                                details: $"User '{user.Name}' (ID: {user.UserId}) has been reactivated via bulk operation",
                                resource: "UserManagement",
                                metadata: new Dictionary<string, object>
                                {
                                    ["targetUserId"] = user.UserId.ToString(),
                                    ["userEmail"] = user.Email,
                                    ["userName"] = user.Name,
                                    ["action"] = "bulk_unblacklist",
                                    ["operationType"] = "bulk"
                                }
                            );
                        }
                        catch (Exception auditEx)
                        {
                            _logger.LogWarning(auditEx, "Failed to log bulk user reactivation audit for user {UserId}", user.UserId);
                        }
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

            // if the user is the last admin, they cannot be deleted
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

            // Add new roles FIRST to ensure user always has at least one role
            foreach (var roleToAdd in rolesToAdd)
            {
                await _roleService.AssignRoleToUserAsync(userId, roleToAdd);
            }

            // Remove roles that are no longer assigned (after adding new ones)
            foreach (var roleToRemove in rolesToRemove)
            {
                // Use direct role removal without individual validation since we've already validated the final state
                await _roleService.RemoveRoleFromUserDirectAsync(userId, roleToRemove);
            }

            // Log the overall role update
            if (rolesToAdd.Any() || rolesToRemove.Any())
            {
                await _auditService.LogRoleUpdateAsync(userId, currentRoleNames, updateRolesDto.Roles, null, "System", null, null, updateRolesDto.Reason);
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

    public async Task<byte[]> ExportUsersAsync(UserExportRequestDto exportRequest)
    {
        try
        {
            // Get filtered users data
            var users = await GetFilteredUsersForExportAsync(exportRequest.Filters);

            // Convert to export format with field filtering
            var exportItems = new List<Dictionary<string, object?>>();
            foreach (var user in users)
            {
                // Get detailed user info for statistics
                var userDetails = await GetUserByIdAsync(user.UserId);
                
                var exportItem = new Dictionary<string, object?>();
                
                // Add fields based on request
                if (exportRequest.Fields.Contains("name") || !exportRequest.Fields.Any())
                    exportItem["name"] = user.Name;
                    
                if (exportRequest.Fields.Contains("email") || !exportRequest.Fields.Any())
                    exportItem["email"] = user.Email;
                    
                if (exportRequest.Fields.Contains("role") || !exportRequest.Fields.Any())
                    exportItem["role"] = string.Join(", ", user.RoleNames);
                    
                if (exportRequest.Fields.Contains("status") || !exportRequest.Fields.Any())
                    exportItem["status"] = user.Status;
                    
                if (exportRequest.Fields.Contains("createdAt") || !exportRequest.Fields.Any())
                    exportItem["createdAt"] = user.CreatedAt;
                    
                if (exportRequest.Fields.Contains("phoneNumber"))
                    exportItem["phoneNumber"] = user.PhoneNumber;
                    
                if (exportRequest.Fields.Contains("authProvider"))
                    exportItem["authProvider"] = user.AuthProvider;
                    
                if (exportRequest.Fields.Contains("disasterReports"))
                    exportItem["disasterReports"] = userDetails?.Statistics?.DisasterReportsCount ?? 0;
                    
                if (exportRequest.Fields.Contains("supportRequests"))
                    exportItem["supportRequests"] = userDetails?.Statistics?.SupportRequestsCount ?? 0;
                    
                if (exportRequest.Fields.Contains("donations"))
                    exportItem["donations"] = userDetails?.Statistics?.DonationsCount ?? 0;
                    
                if (exportRequest.Fields.Contains("organizations"))
                    exportItem["organizations"] = userDetails?.Statistics?.OrganizationsCount ?? 0;
                
                exportItems.Add(exportItem);
            }

            // Generate export based on format
            return exportRequest.Format.ToLower() switch
            {
                "json" => GenerateJsonExport(exportItems),
                "csv" => GenerateCsvExport(exportItems),
                "excel" => GenerateExcelExport(exportItems),
                "pdf" => GeneratePdfExport(exportItems),
                _ => GenerateCsvExport(exportItems) // Default to CSV
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting users");
            throw;
        }
    }

    private async Task<List<UserListItemDto>> GetFilteredUsersForExportAsync(ExportUsersFilters filters)
    {
        try
        {
            // Create a filter DTO for the existing GetUsersAsync method
            var filterDto = new UserFilterDto
            {
                PageSize = int.MaxValue,
                PageNumber = 1,
                Role = filters.Role?.Trim().ToLowerInvariant(),
                Status = filters.Status?.Trim().ToLowerInvariant()
            };

            // Map status values: frontend "suspended" -> backend "Suspended"
            if (!string.IsNullOrEmpty(filterDto.Status))
            {
                filterDto.Status = filterDto.Status switch
                {
                    "active" => "Active",
                    "suspended" => "Suspended", 
                    "inactive" => "Inactive",
                    _ => filterDto.Status
                };
            }

            var result = await GetUsersAsync(filterDto);
            return result.Users;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error filtering users for export");
            throw;
        }
    }

    private byte[] GenerateJsonExport(List<Dictionary<string, object?>> users)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        
        var json = JsonSerializer.Serialize(users, options);
        return Encoding.UTF8.GetBytes(json);
    }

    private byte[] GenerateCsvExport(List<Dictionary<string, object?>> users)
    {
        var csv = new StringBuilder();
        
        if (!users.Any()) return Encoding.UTF8.GetBytes("");
        
        // Add header based on available fields
        var headers = users.First().Keys.ToList();
        csv.AppendLine(string.Join(",", headers));
        
        // Add data rows
        foreach (var user in users)
        {
            var values = headers.Select(header => 
            {
                var value = user.ContainsKey(header) ? user[header]?.ToString() ?? "" : "";
                return $"\"{EscapeCsvField(value)}\"";
            });
            csv.AppendLine(string.Join(",", values));
        }
        
        return Encoding.UTF8.GetBytes(csv.ToString());
    }

    private byte[] GenerateExcelExport(List<Dictionary<string, object?>> users)
    {
        // For now, return CSV format as Excel implementation would require additional packages
        // In a real implementation, you would use libraries like EPPlus or ClosedXML
        return GenerateCsvExport(users);
    }

    private byte[] GeneratePdfExport(List<Dictionary<string, object?>> users)
    {
        using var memoryStream = new MemoryStream();
        var writer = new PdfWriter(memoryStream);
        var pdf = new PdfDocument(writer);
        var document = new Document(pdf, PageSize.A4.Rotate());
        document.SetMargins(10, 10, 10, 10);
        
        // Add title
        var title = new Paragraph($"Disaster Watch - Users Export - {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}")
            .SetFontSize(16)
            .SetBold()
            .SetTextAlignment(TextAlignment.CENTER)
            .SetMarginBottom(20);
        document.Add(title);
        
        if (!users.Any())
        {
            document.Add(new Paragraph("No users found matching the specified criteria."));
            document.Close();
            return memoryStream.ToArray();
        }
        
        // Get headers from first user
        var headers = users.First().Keys.ToList();
        var columnCount = headers.Count;
        
        // Create dynamic column widths
        var widths = new float[columnCount];
        for (int i = 0; i < columnCount; i++)
        {
            widths[i] = 100f / columnCount; // Equal width distribution
        }
        
        var table = new Table(widths).UseAllAvailableWidth();
        
        // Add headers
        foreach (var header in headers)
        {
            var cell = new Cell()
                .Add(new Paragraph(header.ToUpperInvariant()))
                .SetFontSize(8)
                .SetBold()
                .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPadding(5);
            table.AddCell(cell);
        }
        
        // Add data rows
        foreach (var user in users)
        {
            foreach (var header in headers)
            {
                var value = user.ContainsKey(header) ? user[header]?.ToString() ?? "" : "";
                
                // Format specific fields
                if (header == "createdAt" && DateTime.TryParse(value, out var date))
                {
                    value = date.ToString("yyyy-MM-dd");
                }
                
                var cell = new Cell()
                    .Add(new Paragraph(value))
                    .SetFontSize(7)
                    .SetPadding(3);
                    
                // Right-align numeric fields
                if (header.Contains("Reports") || header.Contains("Requests") || 
                    header.Contains("Donations") || header.Contains("Organizations"))
                {
                    cell.SetTextAlignment(TextAlignment.RIGHT);
                }
                
                table.AddCell(cell);
            }
        }
        
        document.Add(table);
        
        // Add footer
        var footer = new Paragraph($"Total Users: {users.Count} | Generated on: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC")
            .SetFontSize(8)
            .SetTextAlignment(TextAlignment.CENTER)
            .SetMarginTop(20);
        document.Add(footer);
        
        document.Close();
        writer.Close();
        
        return memoryStream.ToArray();
    }

    private string EscapeCsvField(string field)
    {
        if (string.IsNullOrEmpty(field))
            return string.Empty;
            
        // Escape quotes by doubling them
        return field.Replace("\"", "\"\"");
    }

    // Analytics methods implementation
    public async Task<UserStatisticsResponseDto> GetUserStatisticsAsync()
    {
        try
        {
            var totalUsers = await _userRepository.GetTotalUsersCountAsync();
            var activeUsers = await _userRepository.GetActiveUsersCountAsync();
            var suspendedUsers = await _userRepository.GetSuspendedUsersCountAsync();
            var adminUsers = await _userRepository.GetAdminUsersCountAsync();

            // Get new users this month and last month
            var thisMonth = DateTime.UtcNow.Date.AddDays(1 - DateTime.UtcNow.Day);
            var lastMonth = thisMonth.AddMonths(-1);
            var twoMonthsAgo = lastMonth.AddMonths(-1);

            var (thisMonthUsers, thisMonthCount) = await _userRepository.GetUsersAsync(
                1, int.MaxValue, createdAfter: thisMonth);
            var (lastMonthUsers, lastMonthCount) = await _userRepository.GetUsersAsync(
                1, int.MaxValue, createdAfter: lastMonth, createdBefore: thisMonth);

            return new UserStatisticsResponseDto
            {
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,
                SuspendedUsers = suspendedUsers,
                AdminUsers = adminUsers,
                NewUsersThisMonth = thisMonthCount,
                NewUsersLastMonth = lastMonthCount
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user statistics");
            throw;
        }
    }

    public async Task<UserActivityTrendsDto> GetUserActivityTrendsAsync(string period = "monthly", int months = 12)
    {
        try
        {
            var trends = new List<UserTrendDataDto>();
            var currentDate = DateTime.UtcNow.Date;
            
            for (int i = months - 1; i >= 0; i--)
            {
                var monthStart = currentDate.AddMonths(-i).AddDays(1 - currentDate.AddMonths(-i).Day);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);
                
                // Get new users for this month
                var (newUsers, newUsersCount) = await _userRepository.GetUsersAsync(
                    1, int.MaxValue, createdAfter: monthStart, createdBefore: monthEnd.AddDays(1));
                
                // Get active users (users who have logged in during this month)
                // For now, we'll use total users as active users since we don't have last_login tracking
                var (allUsers, totalCount) = await _userRepository.GetUsersAsync(
                    1, int.MaxValue, createdBefore: monthEnd.AddDays(1));
                
                // Get suspended users count for this month
                var suspendedCount = await _userRepository.GetSuspendedUsersCountAsync();
                
                trends.Add(new UserTrendDataDto
                {
                    Month = monthStart.ToString("yyyy-MM"),
                    NewUsers = newUsersCount,
                    ActiveUsers = totalCount, // This would ideally be users active in this month
                    SuspendedUsers = suspendedCount
                });
            }

            return new UserActivityTrendsDto
            {
                Period = period,
                Data = trends
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user activity trends");
            throw;
        }
    }

    public async Task<RoleDistributionDto> GetRoleDistributionAsync()
    {
        try
        {
            var totalUsers = await _userRepository.GetTotalUsersCountAsync();
            var roleDistribution = new List<RoleDistributionItemDto>();

            // Get all roles and calculate their user counts
            var allRoles = await _roleService.GetAllRolesAsync();
            
            foreach (var role in allRoles)
            {
                // Get users with this specific role
                var (users, userCount) = await _userRepository.GetUsersAsync(
                    1, int.MaxValue, role: role.Name);
                
                var percentage = totalUsers > 0 ? Math.Round((double)userCount / totalUsers * 100, 1) : 0;
                roleDistribution.Add(new RoleDistributionItemDto
                {
                    Role = role.Name ?? "Unknown",
                    Count = userCount,
                    Percentage = percentage
                });
            }

            return new RoleDistributionDto
            {
                Roles = roleDistribution
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving role distribution");
            throw;
        }
    }
}
