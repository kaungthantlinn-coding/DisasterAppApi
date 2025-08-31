using DisasterApp.Application.DTOs;

namespace DisasterApp.Application.Services.Interfaces;

public interface IRoleManagementService
{
    Task<RoleManagementResponse> GetRolesAsync(string? search = null, string? filter = null);
    Task<RoleDto?> GetRoleByIdAsync(Guid id);
    Task<RoleDto> CreateRoleAsync(CreateRoleDto dto, string createdBy);
    Task<RoleDto> UpdateRoleAsync(Guid id, UpdateRoleDto dto, string updatedBy);
    Task<bool> DeleteRoleAsync(Guid id, string deletedBy = "System", Guid? userId = null);
    Task<List<RoleUserDto>> GetRoleUsersAsync(Guid id);
    Task<bool> RoleExistsAsync(string name, Guid? excludeId = null);
}
//