using DisasterApp.Domain.Entities;

namespace DisasterApp.Infrastructure.Data;

public static class DefaultRoles
{
    public static readonly List<Role> InitialRoles = new()
    {
        new Role 
        { 
            RoleId = Guid.NewGuid(),
            Name = "SuperAdmin", 
            Description = "Full system administrator with complete access to all features and settings",
            IsActive = true,
            IsSystem = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = "System",
            UpdatedBy = "System"
        },
        new Role 
        { 
            RoleId = Guid.NewGuid(),
            Name = "Admin", 
            Description = "System administrator with user management and operational oversight capabilities",
            IsActive = true,
            IsSystem = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = "System",
            UpdatedBy = "System"
        },
        new Role 
        { 
            RoleId = Guid.NewGuid(),
            Name = "Manager", 
            Description = "Department manager with team oversight and reporting capabilities",
            IsActive = true,
            IsSystem = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = "System",
            UpdatedBy = "System"
        },
        new Role 
        { 
            RoleId = Guid.NewGuid(),
            Name = "User", 
            Description = "Standard user with basic system access and reporting capabilities",
            IsActive = true,
            IsSystem = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = "System",
            UpdatedBy = "System"
        }
    };
}
