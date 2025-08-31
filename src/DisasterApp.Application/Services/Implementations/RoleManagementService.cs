using DisasterApp.Application.DTOs;
using DisasterApp.Application.Services.Interfaces;
using DisasterApp.Domain.Entities;
using DisasterApp.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace DisasterApp.Application.Services.Implementations;

public class RoleManagementService : IRoleManagementService
{
    private readonly IRoleRepository _roleRepository;
    private readonly IUserRepository _userRepository;
    private readonly IAuditService _auditService;
    private readonly ILogger<RoleManagementService> _logger;

    public RoleManagementService(
        IRoleRepository roleRepository,
        IUserRepository userRepository,//
        IAuditService auditService,
        ILogger<RoleManagementService> logger)
    {
        _roleRepository = roleRepository;
        _userRepository = userRepository;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<RoleManagementResponse> GetRolesAsync(string? search = null, string? filter = null)
    {
        try
        {
            var roles = await _roleRepository.GetAllRolesAsync();
            
            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                roles = roles.Where(r => r.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                                        r.Description.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            // Apply status filter
            if (!string.IsNullOrEmpty(filter))
            {
                switch (filter.ToLower())
                {
                    case "active":
                        roles = roles.Where(r => r.IsActive);
                        break;
                    case "inactive":
                        roles = roles.Where(r => !r.IsActive);
                        break;
                    case "system":
                        roles = roles.Where(r => r.IsSystem);
                        break;
                    case "custom":
                        roles = roles.Where(r => !r.IsSystem);
                        break;
                }
            }

            var roleList = roles.ToList();
            var roleDtos = new List<RoleDto>();

            foreach (var role in roleList)
            {
                var userCount = await _userRepository.GetUserCountByRoleAsync(role.RoleId);
                roleDtos.Add(new RoleDto
                {
                    Id = role.RoleId,
                    Name = role.Name,
                    Description = role.Description,
                    IsActive = role.IsActive,
                    IsSystem = role.IsSystem,
                    UserCount = userCount,
                    CreatedAt = role.CreatedAt,
                    UpdatedAt = role.UpdatedAt,
                    CreatedBy = role.CreatedBy,
                    UpdatedBy = role.UpdatedBy
                });
            }

            // Calculate statistics
            var statistics = new RoleStatistics
            {
                TotalRoles = roleList.Count,
                ActiveRoles = roleList.Count(r => r.IsActive),
                SystemRoles = roleList.Count(r => r.IsSystem),
                CustomRoles = roleList.Count(r => !r.IsSystem),
                TotalUsers = roleDtos.Sum(r => r.UserCount)
            };

            return new RoleManagementResponse
            {
                Roles = roleDtos,
                Statistics = statistics
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving roles with search: {Search}, filter: {Filter}", search, filter);
            throw;
        }
    }

    public async Task<RoleDto?> GetRoleByIdAsync(Guid id)
    {
        try
        {
            var role = await _roleRepository.GetRoleByIdAsync(id);
            if (role == null) return null;

            var userCount = await _userRepository.GetUserCountByRoleAsync(id);

            return new RoleDto
            {
                Id = role.RoleId,
                Name = role.Name,
                Description = role.Description,
                IsActive = role.IsActive,
                IsSystem = role.IsSystem,
                UserCount = userCount,
                CreatedAt = role.CreatedAt,
                UpdatedAt = role.UpdatedAt,
                CreatedBy = role.CreatedBy,
                UpdatedBy = role.UpdatedBy
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving role with ID: {RoleId}", id);
            throw;
        }
    }

    public async Task<RoleDto> CreateRoleAsync(CreateRoleDto dto, string createdBy)
    {
        try
        {
            // Check if role name already exists
            if (await RoleExistsAsync(dto.Name))
            {
                throw new ArgumentException($"Role with name '{dto.Name}' already exists");
            }

            var role = new Role
            {
                Name = dto.Name,
                Description = dto.Description,
                IsActive = true,
                IsSystem = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = createdBy,
                UpdatedBy = createdBy
            };

            var createdRole = await _roleRepository.CreateRoleAsync(role);

            // Log audit event
            await _auditService.LogUserActionAsync(
                action: "ROLE_CREATED",
                severity: "Info",
                userId: null,
                details: $"Role '{createdRole.Name}' created by {createdBy}",
                resource: "Role",
                metadata: new Dictionary<string, object> { 
                    { "RoleName", createdRole.Name }, 
                    { "Description", createdRole.Description },
                    { "CreatedBy", createdBy }
                }
            );

            _logger.LogInformation("Role created: {RoleName} by {CreatedBy}", createdRole.Name, createdBy);

            return new RoleDto
            {
                Id = createdRole.RoleId,
                Name = createdRole.Name,
                Description = createdRole.Description,
                IsActive = createdRole.IsActive,
                IsSystem = createdRole.IsSystem,
                UserCount = 0,
                CreatedAt = createdRole.CreatedAt,
                UpdatedAt = createdRole.UpdatedAt,
                CreatedBy = createdRole.CreatedBy,
                UpdatedBy = createdRole.UpdatedBy
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating role: {RoleName}", dto.Name);
            throw;
        }
    }

    public async Task<RoleDto> UpdateRoleAsync(Guid id, UpdateRoleDto dto, string updatedBy)
    {
        try
        {
            var existingRole = await _roleRepository.GetRoleByIdAsync(id);
            if (existingRole == null)
            {
                throw new ArgumentException($"Role with ID {id} not found");
            }

            if (existingRole.IsSystem)
            {
                throw new InvalidOperationException("Cannot modify system roles");
            }

            // Check if new name conflicts with existing roles (excluding current role)
            if (await RoleExistsAsync(dto.Name, id))
            {
                throw new ArgumentException($"Role with name '{dto.Name}' already exists");
            }

            var oldValues = new { existingRole.Name, existingRole.Description, existingRole.IsActive };

            existingRole.Name = dto.Name;
            existingRole.Description = dto.Description;
            existingRole.IsActive = dto.IsActive;
            existingRole.UpdatedAt = DateTime.UtcNow;
            existingRole.UpdatedBy = updatedBy;

            var updatedRole = await _roleRepository.UpdateRoleAsync(existingRole);
            var userCount = await _userRepository.GetUserCountByRoleAsync(id);

            // Log audit event
            await _auditService.LogUserActionAsync(
                action: "ROLE_UPDATED",
                severity: "Info",
                userId: null,
                details: $"Role '{updatedRole.Name}' updated by {updatedBy}",
                resource: "Role",
                metadata: new Dictionary<string, object> { 
                    { "RoleName", updatedRole.Name }, 
                    { "OldValues", oldValues },
                    { "NewValues", new { updatedRole.Name, updatedRole.Description, updatedRole.IsActive } },
                    { "UpdatedBy", updatedBy }
                }
            );

            _logger.LogInformation("Role updated: {RoleName} by {UpdatedBy}", updatedRole.Name, updatedBy);

            return new RoleDto
            {
                Id = updatedRole.RoleId,
                Name = updatedRole.Name,
                Description = updatedRole.Description,
                IsActive = updatedRole.IsActive,
                IsSystem = updatedRole.IsSystem,
                UserCount = userCount,
                CreatedAt = updatedRole.CreatedAt,
                UpdatedAt = updatedRole.UpdatedAt,
                CreatedBy = updatedRole.CreatedBy,
                UpdatedBy = updatedRole.UpdatedBy
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating role with ID: {RoleId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteRoleAsync(Guid id, string deletedBy = "System", Guid? userId = null)
    {
        try
        {
            var role = await _roleRepository.GetRoleByIdAsync(id);
            if (role == null)
            {
                throw new ArgumentException($"Role with ID {id} not found");
            }

            if (role.IsSystem)
            {
                throw new InvalidOperationException("Cannot delete system roles");
            }

            var userCount = await _userRepository.GetUserCountByRoleAsync(id);
            if (userCount > 0)
            {
                throw new InvalidOperationException($"Cannot delete role '{role.Name}' as it is assigned to {userCount} user(s)");
            }

            var deleted = await _roleRepository.DeleteRoleAsync(id);

            if (deleted)
            {
                // Log audit event with proper username information
                await _auditService.LogUserActionAsync(
                    action: "ROLE_DELETED",
                    severity: "Warning",
                    userId: userId,
                    details: $"Role '{role.Name}' deleted by {deletedBy}",
                    resource: "Role",
                    metadata: new Dictionary<string, object> { 
                        { "RoleName", role.Name },
                        { "RoleId", id },
                        { "DeletedBy", deletedBy }
                    }
                );

                _logger.LogInformation("Role deleted: {RoleName} by {DeletedBy}", role.Name, deletedBy);
            }

            return deleted;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting role with ID: {RoleId}", id);
            throw;
        }
    }

    public async Task<List<RoleUserDto>> GetRoleUsersAsync(Guid id)
    {
        try
        {
            var users = await _userRepository.GetUsersByRoleAsync(id);
            
            return users.Select(u => new RoleUserDto
            {
                Id = u.UserId,
                Name = u.Name,
                Email = u.Email,
                IsActive = !(u.IsBlacklisted ?? false),
                CreatedAt = u.CreatedAt ?? DateTime.MinValue
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users for role ID: {RoleId}", id);
            throw;
        }
    }

    public async Task<bool> RoleExistsAsync(string name, Guid? excludeId = null)
    {
        try
        {
            return await _roleRepository.RoleExistsAsync(name, excludeId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if role exists: {RoleName}", name);
            throw;
        }
    }
}
