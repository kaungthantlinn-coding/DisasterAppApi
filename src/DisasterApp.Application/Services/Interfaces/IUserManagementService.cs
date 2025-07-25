using DisasterApp.Application.DTOs;

namespace DisasterApp.Application.Services.Interfaces;

/// <summary>
/// Service interface for user management operations
/// </summary>
public interface IUserManagementService
{
    /// <summary>
    /// Get paginated list of users with filtering
    /// </summary>
    /// <param name="filter">Filter and pagination parameters</param>
    /// <returns>Paginated user list</returns>
    Task<PagedUserListDto> GetUsersAsync(UserFilterDto filter);

    /// <summary>
    /// Get detailed user information by ID
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>User details or null if not found</returns>
    Task<UserDetailsDto?> GetUserByIdAsync(Guid userId);

    /// <summary>
    /// Create a new user
    /// </summary>
    /// <param name="createUserDto">User creation data</param>
    /// <returns>Created user details</returns>
    Task<UserDetailsDto> CreateUserAsync(CreateUserDto createUserDto);

    /// <summary>
    /// Update an existing user
    /// </summary>
    /// <param name="userId">User ID to update</param>
    /// <param name="updateUserDto">Updated user data</param>
    /// <returns>Updated user details</returns>
    Task<UserDetailsDto> UpdateUserAsync(Guid userId, UpdateUserDto updateUserDto);

    /// <summary>
    /// Delete a user (soft delete by blacklisting)
    /// </summary>
    /// <param name="userId">User ID to delete</param>
    /// <returns>True if successful</returns>
    Task<bool> DeleteUserAsync(Guid userId);

    /// <summary>
    /// Blacklist/suspend a user
    /// </summary>
    /// <param name="userId">User ID to blacklist</param>
    /// <returns>True if successful</returns>
    Task<bool> BlacklistUserAsync(Guid userId);

    /// <summary>
    /// Remove blacklist/unsuspend a user
    /// </summary>
    /// <param name="userId">User ID to unblacklist</param>
    /// <returns>True if successful</returns>
    Task<bool> UnblacklistUserAsync(Guid userId);

    /// <summary>
    /// Change user password
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="changePasswordDto">Password change data</param>
    /// <returns>True if successful</returns>
    Task<bool> ChangeUserPasswordAsync(Guid userId, ChangeUserPasswordDto changePasswordDto);

    /// <summary>
    /// Perform bulk operations on multiple users
    /// </summary>
    /// <param name="bulkOperation">Bulk operation data</param>
    /// <returns>Number of users affected</returns>
    Task<int> BulkOperationAsync(BulkUserOperationDto bulkOperation);

    /// <summary>
    /// Get user management dashboard statistics
    /// </summary>
    /// <returns>Dashboard statistics</returns>
    Task<UserManagementStatsDto> GetDashboardStatsAsync();

    /// <summary>
    /// Validate if user can be deleted
    /// </summary>
    /// <param name="userId">User ID to validate</param>
    /// <returns>Validation result with reasons if invalid</returns>
    Task<UserDeletionValidationDto> ValidateUserDeletionAsync(Guid userId);

    /// <summary>
    /// Update user roles only
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="updateRolesDto">Role update data</param>
    /// <returns>Updated user details</returns>
    Task<UserDetailsDto> UpdateUserRolesAsync(Guid userId, UpdateUserRolesDto updateRolesDto);

    /// <summary>
    /// Validate role update before applying changes
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="newRoles">New roles to assign</param>
    /// <returns>Validation result</returns>
    Task<RoleUpdateValidationDto> ValidateRoleUpdateAsync(Guid userId, List<string> newRoles);
}


