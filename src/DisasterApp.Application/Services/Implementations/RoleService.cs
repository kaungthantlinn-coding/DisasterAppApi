using DisasterApp.Application.Services.Interfaces;
using DisasterApp.Domain.Entities;
using DisasterApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DisasterApp.Application.Services.Implementations;

public class RoleService : IRoleService
{
    private readonly DisasterDbContext _context;
    private readonly ILogger<RoleService> _logger;//
    private readonly IAuditService _auditService;

    public RoleService(DisasterDbContext context, ILogger<RoleService> logger, IAuditService auditService)
    {
        _context = context;
        _logger = logger;
        _auditService = auditService;
    }

    public async Task<IEnumerable<Role>> GetAllRolesAsync()
    {
        try
        {
            var roles = await _context.Roles.ToListAsync();
            _logger.LogInformation("Retrieved {Count} roles from database", roles.Count);
            
            // Log any roles with missing names
            var rolesWithoutNames = roles.Where(r => string.IsNullOrEmpty(r.Name)).ToList();
            if (rolesWithoutNames.Any())
            {
                _logger.LogWarning("Found {Count} roles without names: {RoleIds}", 
                    rolesWithoutNames.Count, 
                    string.Join(", ", rolesWithoutNames.Select(r => r.RoleId)));
            }
            
            return roles;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all roles");
            throw;
        }
    }

    public async Task<Role?> GetRoleByNameAsync(string roleName)
    {
        return await _context.Roles
            .FirstOrDefaultAsync(r => r.Name.ToLower() == roleName.ToLower());
    }

    public async Task<Role?> GetDefaultRoleAsync()
    {
        return await _context.Roles
            .FirstOrDefaultAsync(r => r.Name.ToLower() == "user");
    }

    public async Task AssignRoleToUserAsync(Guid userId, string roleName, Guid? performedByUserId = null, string? performedByUserName = null, string? ipAddress = null, string? userAgent = null)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var user = await _context.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                throw new InvalidOperationException($"User with ID {userId} not found.");
            }

            var role = await GetRoleByNameAsync(roleName);
            if (role == null)
            {
                throw new InvalidOperationException($"Role '{roleName}' not found.");
            }

            if (user.Roles.Any(r => r.RoleId == role.RoleId))
            {
                _logger.LogInformation("User {UserId} already has role {RoleName}", userId, roleName);
                return;
            }

            user.Roles.Add(role);
            await _context.SaveChangesAsync();

            // Log the role assignment
            await _auditService.LogRoleAssignmentAsync(
                userId,
                roleName,
                performedByUserId,
                performedByUserName,
                ipAddress,
                userAgent
            );

            await transaction.CommitAsync();
            _logger.LogInformation("Assigned role {RoleName} to user {UserId}", roleName, userId);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error assigning role {RoleName} to user {UserId}", roleName, userId);
            throw;
        }
    }

    public async Task AssignDefaultRoleToUserAsync(Guid userId)
    {
        var defaultRole = await GetDefaultRoleAsync();
        if (defaultRole != null)
        {
            await AssignRoleToUserAsync(userId, defaultRole.Name);
        }
    }

    public async Task<IEnumerable<Role>> GetUserRolesAsync(Guid userId)
    {
        try
        {
            var user = await _context.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found when getting user roles", userId);
                return new List<Role>();
            }

            var roles = user.Roles.ToList();
            _logger.LogInformation("Retrieved {Count} roles for user {UserId}: {RoleNames}", 
                roles.Count, userId, string.Join(", ", roles.Select(r => r.Name ?? "[NO NAME]")));

            // Log any roles with missing names
            var rolesWithoutNames = roles.Where(r => string.IsNullOrEmpty(r.Name)).ToList();
            if (rolesWithoutNames.Any())
            {
                _logger.LogWarning("User {UserId} has {Count} roles without names: {RoleIds}", 
                    userId, rolesWithoutNames.Count, 
                    string.Join(", ", rolesWithoutNames.Select(r => r.RoleId)));
            }

            return roles;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving roles for user {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> UserHasRoleAsync(Guid userId, string roleName)
    {
        var user = await _context.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.UserId == userId);

        return user?.Roles.Any(r => r.Name.ToLower() == roleName.ToLower()) ?? false;
    }

    public async Task RemoveRoleFromUserAsync(Guid userId, string roleName, Guid? performedByUserId = null, string? performedByUserName = null, string? ipAddress = null, string? userAgent = null)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var user = await _context.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                throw new InvalidOperationException($"User with ID {userId} not found.");
            }

            // Validate role removal
            var canRemove = await CanRemoveRoleAsync(userId, roleName);
            if (!canRemove)
            {
                throw new InvalidOperationException($"Cannot remove role '{roleName}' from user {userId}.");
            }

            var roleToRemove = user.Roles.FirstOrDefault(r => r.Name.ToLower() == roleName.ToLower());
            if (roleToRemove != null)
            {
                user.Roles.Remove(roleToRemove);
                await _context.SaveChangesAsync();

                // Log the role removal
                await _auditService.LogRoleRemovalAsync(
                    userId,
                    roleName,
                    performedByUserId,
                    performedByUserName,
                    ipAddress,
                    userAgent
                );

                await transaction.CommitAsync();
                _logger.LogInformation("Removed role {RoleName} from user {UserId}", roleName, userId);
            }
            else
            {
                _logger.LogInformation("User {UserId} does not have role {RoleName}", userId, roleName);
            }
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error removing role {RoleName} from user {UserId}", roleName, userId);
            throw;
        }
    }

    public async Task RemoveRoleFromUserDirectAsync(Guid userId, string roleName, Guid? performedByUserId = null, string? performedByUserName = null, string? ipAddress = null, string? userAgent = null)
    {
        try
        {
            var user = await _context.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                _logger.LogError("User with ID {UserId} not found", userId);
                return;
            }

            var roleToRemove = user.Roles.FirstOrDefault(r => r.Name.ToLower() == roleName.ToLower());
            if (roleToRemove != null)
            {
                user.Roles.Remove(roleToRemove);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Removed role {RoleName} from user {UserId} (direct)", roleName, userId);
            }
            else
            {
                _logger.LogInformation("User {UserId} does not have role {RoleName}", userId, roleName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing role {RoleName} from user {UserId} (direct)", roleName, userId);
        }
    }

    public async Task<bool> CanRemoveRoleAsync(Guid userId, string roleName)
    {
        try
        {
            // Super Admins cannot have their role removed (ultimate protection)
            if (roleName.ToLower() == "superadmin")
            {
                return false;
            }

            // Check if removing admin role would leave no admins (but Super Admins can still manage)
            if (roleName.ToLower() == "admin")
            {
                var isLastAdmin = await IsLastAdminAsync(userId);
                if (isLastAdmin)
                {
                    // Allow removal if there are Super Admins in the system
                    var superAdminCount = await GetSuperAdminCountAsync();
                    if (superAdminCount == 0)
                    {
                        return false;
                    }
                }
            }

            // Check if user would have no roles left
            var user = await _context.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null) return false;

            var remainingRoles = user.Roles.Where(r => r.Name.ToLower() != roleName.ToLower()).Count();
            return remainingRoles > 0;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    public async Task<int> GetAdminCountAsync()
    {
        return await _context.Users
            .Include(u => u.Roles)
            .Where(u => u.Roles.Any(r => r.Name.ToLower() == "admin"))
            .CountAsync();
    }

    public async Task<bool> IsLastAdminAsync(Guid userId)
    {
        var adminCount = await GetAdminCountAsync();
        if (adminCount > 1) return false;

        var user = await _context.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.UserId == userId);

        return user?.Roles.Any(r => r.Name.ToLower() == "admin") ?? false;
    }

    public async Task<int> CleanupDuplicateUserRolesAsync()
    {
        try
        {
            _logger.LogInformation("Starting cleanup of duplicate user roles");
            
            // Get all users with their roles
            var users = await _context.Users
                .Include(u => u.Roles)
                .ToListAsync();

            var duplicatesRemoved = 0;
            
            foreach (var user in users)
            {
                // Group roles by Id to find duplicates
                var roleGroups = user.Roles.GroupBy(r => r.RoleId).ToList();
                
                foreach (var roleGroup in roleGroups)
                {
                    if (roleGroup.Count() > 1)
                    {
                        // Keep only the first occurrence, remove the rest
                        var rolesToRemove = roleGroup.Skip(1).ToList();
                        foreach (var duplicateRole in rolesToRemove)
                        {
                            user.Roles.Remove(duplicateRole);
                            duplicatesRemoved++;
                            _logger.LogInformation("Removed duplicate role {RoleName} from user {UserId}", duplicateRole.Name, user.UserId);
                        }
                    }
                }
            }
            
            if (duplicatesRemoved > 0)
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Cleanup completed. Removed {Count} duplicate role assignments", duplicatesRemoved);
            }
            else
            {
                _logger.LogInformation("No duplicate role assignments found");
            }

            return duplicatesRemoved;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during duplicate role cleanup");
            throw;
        }
    }

    public async Task<bool> FixRoleNamesAsync()
    {
        try
        {
            _logger.LogInformation("Starting role name fix");
            
            var rolesWithoutNames = await _context.Roles
                .Where(r => string.IsNullOrEmpty(r.Name))
                .ToListAsync();

            if (!rolesWithoutNames.Any())
            {
                _logger.LogInformation("No roles with missing names found");
                return true;
            }

            foreach (var role in rolesWithoutNames)
            {
                // Try to identify role by common IDs
                if (role.RoleId.ToString().ToUpper() == "AB4F1B2A-7227-4EAE-B368-32AA5E0A6F4D")
                {
                    role.Name = "admin";
                    _logger.LogInformation("Fixed role {RoleId} to 'admin'", role.RoleId);
                }
                else if (role.RoleId.ToString().ToUpper() == "2DB3E17D-A6B8-49C1-94D5-8458AAE284AA")
                {
                    role.Name = "user";
                    _logger.LogInformation("Fixed role {RoleId} to 'user'", role.RoleId);
                }
                else
                {
                    role.Name = "user"; // Default to user role
                    _logger.LogInformation("Fixed role {RoleId} to default 'user'", role.RoleId);
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Fixed {Count} roles with missing names", rolesWithoutNames.Count);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fixing role names");
            return false;
        }
    }

    public async Task ReplaceUserRolesAsync(Guid userId, IEnumerable<string> roleNames, Guid? performedByUserId = null, string? performedByUserName = null, string? ipAddress = null, string? userAgent = null)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var user = await _context.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                throw new InvalidOperationException($"User with ID {userId} not found.");
            }

            // Validate all roles exist before making any changes
            var rolesToAssign = new List<Role>();
            foreach (var roleName in roleNames)
            {
                var role = await GetRoleByNameAsync(roleName);
                if (role == null)
                {
                    throw new InvalidOperationException($"Role '{roleName}' not found.");
                }
                rolesToAssign.Add(role);
            }

            // Check if removing Super Admin role (not allowed)
            var currentSuperAdminRole = user.Roles.FirstOrDefault(r => r.Name.ToLower() == "superadmin");
            var newSuperAdminRole = rolesToAssign.FirstOrDefault(r => r.Name.ToLower() == "superadmin");
            
            if (currentSuperAdminRole != null && newSuperAdminRole == null)
            {
                throw new InvalidOperationException("Cannot remove Super Admin role from user.");
            }

            // Check if removing admin role would leave no admins (but Super Admins can still manage)
            var currentAdminRole = user.Roles.FirstOrDefault(r => r.Name.ToLower() == "admin");
            var newAdminRole = rolesToAssign.FirstOrDefault(r => r.Name.ToLower() == "admin");
            
            if (currentAdminRole != null && newAdminRole == null)
            {
                var isLastAdmin = await IsLastAdminAsync(userId);
                if (isLastAdmin)
                {
                    // Allow removal if there are Super Admins in the system
                    var superAdminCount = await GetSuperAdminCountAsync();
                    if (superAdminCount == 0)
                    {
                        throw new InvalidOperationException("Cannot remove admin role from the last admin user when no Super Admins exist.");
                    }
                }
            }

            // Store current roles for audit logging
            var currentRoles = user.Roles.ToList();
            var currentRoleNames = currentRoles.Select(r => r.Name).ToList();

            // Replace all roles atomically
            user.Roles.Clear();
            foreach (var role in rolesToAssign)
            {
                user.Roles.Add(role);
            }

            await _context.SaveChangesAsync();

            // Log the role replacement
            await _auditService.LogRoleUpdateAsync(
                userId,
                currentRoleNames,
                roleNames.ToList(),
                performedByUserId,
                performedByUserName,
                ipAddress,
                userAgent,
                "Role replacement operation"
            );

            await transaction.CommitAsync();
            _logger.LogInformation("Replaced roles for user {UserId}. Old roles: [{OldRoles}], New roles: [{NewRoles}]", 
                userId, 
                string.Join(", ", currentRoleNames), 
                string.Join(", ", roleNames));
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error replacing roles for user {UserId}", userId);
            throw;
        }
    }

    public async Task<Role?> GetSuperAdminRoleAsync()
    {
        return await _context.Roles
            .FirstOrDefaultAsync(r => r.Name.ToLower() == "superadmin");
    }

    public async Task<bool> IsSuperAdminAsync(Guid userId)
    {
        var user = await _context.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.UserId == userId);

        return user?.Roles.Any(r => r.Name.ToLower() == "superadmin") ?? false;
    }

    public async Task<int> GetSuperAdminCountAsync()
    {
        return await _context.Users
            .Include(u => u.Roles)
            .Where(u => u.Roles.Any(r => r.Name.ToLower() == "superadmin"))
            .CountAsync();
    }

    public async Task AssignSuperAdminRoleAsync(Guid userId, Guid? performedByUserId = null, string? performedByUserName = null, string? ipAddress = null, string? userAgent = null)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var user = await _context.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                throw new InvalidOperationException($"User with ID {userId} not found.");
            }

            var superAdminRole = await GetSuperAdminRoleAsync();
            if (superAdminRole == null)
            {
                throw new InvalidOperationException("Super Admin role not found in the system.");
            }

            if (user.Roles.Any(r => r.RoleId == superAdminRole.RoleId))
            {
                _logger.LogInformation("User {UserId} already has Super Admin role", userId);
                return;
            }

            // Super Admin role assignment with value '1' (indicating active/primary status)
            user.Roles.Add(superAdminRole);
            await _context.SaveChangesAsync();

            // Log the Super Admin role assignment
            await _auditService.LogRoleAssignmentAsync(
                userId,
                "superadmin",
                performedByUserId,
                performedByUserName,
                ipAddress,
                userAgent
            );

            await transaction.CommitAsync();
            _logger.LogInformation("Assigned Super Admin role to user {UserId} with assignment value '1' (active/primary status)", userId);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error assigning Super Admin role to user {UserId}", userId);
            throw;
        }
    }
}