using DisasterApp.Application.Services.Interfaces;
using DisasterApp.Domain.Entities;
using DisasterApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace DisasterApp.Application.Services.Implementations;

public class RoleService : IRoleService
{
    private readonly DisasterDbContext _context;
    private readonly ILogger<RoleService> _logger;
    private readonly IAuditService _auditService;
    private const string DefaultRoleName = "user";

    public RoleService(DisasterDbContext context, ILogger<RoleService> logger, IAuditService auditService)
    {
        _context = context;
        _logger = logger;
        _auditService = auditService;
    }

    public async Task<IEnumerable<Role>> GetAllRolesAsync()
    {
        return await _context.Roles.ToListAsync();
    }

    public async Task<Role?> GetRoleByNameAsync(string roleName)
    {
        return await _context.Roles
            .FirstOrDefaultAsync(r => r.Name.ToLower() == roleName.ToLower());
    }

    public async Task<Role?> GetDefaultRoleAsync()
    {
        return await GetRoleByNameAsync(DefaultRoleName);
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
                _logger.LogWarning("User with ID {UserId} not found", userId);
                throw new ArgumentException($"User with ID {userId} not found");
            }

            var role = await GetRoleByNameAsync(roleName);
            if (role == null)
            {
                _logger.LogWarning("Role {RoleName} not found", roleName);
                throw new ArgumentException($"Role {roleName} not found");
            }

            if (!user.Roles.Any(r => r.RoleId == role.RoleId))
            {
                user.Roles.Add(role);
                await _context.SaveChangesAsync();

                // Log the role assignment in the same transaction
                var auditLog = new AuditLog
                {
                    AuditLogId = Guid.NewGuid(),
                    Action = "ROLE_ASSIGNED",
                    EntityType = "UserRole",
                    EntityId = userId.ToString(),
                    OldValues = null,
                    NewValues = System.Text.Json.JsonSerializer.Serialize(new { RoleName = roleName }),
                    UserId = performedByUserId,
                    UserName = performedByUserName,
                    Timestamp = DateTime.UtcNow,
                    IpAddress = ipAddress,
                    UserAgent = userAgent
                };

                await _context.AuditLogs.AddAsync(auditLog);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Assigned role {RoleName} to user {UserId}", roleName, userId);
            }
            else
            {
                await transaction.CommitAsync();
                _logger.LogInformation("User {UserId} already has role {RoleName}", userId, roleName);
            }
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task AssignDefaultRoleToUserAsync(Guid userId)
    {
        await AssignRoleToUserAsync(userId, DefaultRoleName);
    }

    public async Task<IEnumerable<Role>> GetUserRolesAsync(Guid userId)
    {
        var user = await _context.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.UserId == userId);

        return user?.Roles ?? new List<Role>();
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
                _logger.LogWarning("User with ID {UserId} not found", userId);
                throw new ArgumentException($"User with ID {userId} not found");
            }

            // Validate role removal
            await ValidateRoleRemovalAsync(userId, roleName);

            var role = user.Roles.FirstOrDefault(r => r.Name.ToLower() == roleName.ToLower());
            if (role != null)
            {
                user.Roles.Remove(role);
                await _context.SaveChangesAsync();

                // Log the role removal in the same transaction
                var auditLog = new AuditLog
                {
                    AuditLogId = Guid.NewGuid(),
                    Action = "ROLE_REMOVED",
                    EntityType = "UserRole",
                    EntityId = userId.ToString(),
                    OldValues = JsonSerializer.Serialize(new { RoleName = roleName }),
                    NewValues = null,
                    UserId = performedByUserId,
                    UserName = performedByUserName,
                    Timestamp = DateTime.UtcNow,
                    IpAddress = ipAddress,
                    UserAgent = userAgent
                };

                await _context.AuditLogs.AddAsync(auditLog);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Removed role {RoleName} from user {UserId}", roleName, userId);
            }
            else
            {
                await transaction.CommitAsync();
                _logger.LogInformation("User {UserId} does not have role {RoleName}", userId, roleName);
            }
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private async Task ValidateRoleRemovalAsync(Guid userId, string roleName)
    {
        // Prevent removing admin role if this is the last admin
        if (roleName.ToLower() == "admin")
        {
            var adminCount = await _context.Users
                .Include(u => u.Roles)
                .Where(u => u.Roles.Any(r => r.Name.ToLower() == "admin"))
                .CountAsync();

            if (adminCount <= 1)
            {
                _logger.LogWarning("Attempted to remove admin role from last admin user {UserId}", userId);
                throw new InvalidOperationException("Cannot remove admin role from the last admin user. At least one admin must remain in the system.");
            }
        }

        // Prevent removing user role if it's the only role
        if (roleName.ToLower() == "user")
        {
            var user = await _context.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user?.Roles.Count == 1 && user.Roles.Any(r => r.Name.ToLower() == "user"))
            {
                _logger.LogWarning("Attempted to remove user role from user {UserId} who has no other roles", userId);
                throw new InvalidOperationException("Cannot remove user role when it's the only role assigned. Users must have at least one role.");
            }
        }
    }

    public async Task<bool> CanRemoveRoleAsync(Guid userId, string roleName)
    {
        try
        {
            await ValidateRoleRemovalAsync(userId, roleName);
            return true;
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
}