using System.ComponentModel.DataAnnotations;

namespace DisasterApp.Application.DTOs;

public class RoleDto//
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsSystem { get; set; }
    public int UserCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;
}

public class CreateRoleDto
{
    [Required, StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;
    
    [Required, StringLength(500)]
    public string Description { get; set; } = string.Empty;
}

public class UpdateRoleDto
{
    [Required, StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;
    
    [Required, StringLength(500)]
    public string Description { get; set; } = string.Empty;
    
    public bool IsActive { get; set; }
}

public class RoleStatistics
{
    public int TotalRoles { get; set; }
    public int ActiveRoles { get; set; }
    public int SystemRoles { get; set; }
    public int CustomRoles { get; set; }
    public int TotalUsers { get; set; }
}

public class RoleManagementResponse
{
    public List<RoleDto> Roles { get; set; } = new();
    public RoleStatistics Statistics { get; set; } = new();
}

public class RoleUserDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
