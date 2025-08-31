using DisasterApp.Application.DTOs;

namespace DisasterApp.Application.Services.Interfaces;
public interface IUserManagementService
{ 
    Task<PagedUserListDto> GetUsersAsync(UserFilterDto filter); 
    Task<UserDetailsDto?> GetUserByIdAsync(Guid userId);
    Task<UserDetailsDto> CreateUserAsync(CreateUserDto createUserDto);
    Task<UserDetailsDto> UpdateUserAsync(Guid userId, UpdateUserDto updateUserDto);//
    Task<bool> DeleteUserAsync(Guid userId);
    Task<bool> BlacklistUserAsync(Guid userId);
    Task<bool> UnblacklistUserAsync(Guid userId);
    Task<bool> ChangeUserPasswordAsync(Guid userId, ChangeUserPasswordDto changePasswordDto);
    Task<int> BulkOperationAsync(BulkUserOperationDto bulkOperation, Guid? adminUserId = null); // bulk operation on multiple users
    Task<UserManagementStatsDto> GetDashboardStatsAsync();
    Task<UserDeletionValidationDto> ValidateUserDeletionAsync(Guid userId);
    Task<UserDetailsDto> UpdateUserRolesAsync(Guid userId, UpdateUserRolesDto updateRolesDto);
    Task<RoleUpdateValidationDto> ValidateRoleUpdateAsync(Guid userId, List<string> newRoles);
    Task<byte[]> ExportUsersAsync(UserExportRequestDto exportRequest);
    
    // Analytics endpoints
    Task<UserStatisticsResponseDto> GetUserStatisticsAsync();
    Task<UserActivityTrendsDto> GetUserActivityTrendsAsync(string period = "monthly", int months = 12);
    Task<RoleDistributionDto> GetRoleDistributionAsync();
}


