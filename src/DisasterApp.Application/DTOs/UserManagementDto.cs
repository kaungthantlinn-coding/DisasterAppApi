using System.ComponentModel.DataAnnotations;
using DisasterApp.Application.Validation;

namespace DisasterApp.Application.DTOs;//


public class CreateUserDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = null!;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = null!;

    [Url]
    public string? PhotoUrl { get; set; }

    [Phone]
    public string? PhoneNumber { get; set; }

    [ValidRoleNames]
    public List<string> Roles { get; set; } = new();

    public bool IsBlacklisted { get; set; } = false;
}




public class UpdateUserDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = null!;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Url]
    public string? PhotoUrl { get; set; }

    [Phone]
    public string? PhoneNumber { get; set; }

    [ValidRoleNames]
    public List<string> Roles { get; set; } = new();

    public bool IsBlacklisted { get; set; } = false;
}




public class UserDetailsDto
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? PhotoUrl { get; set; }
    public string? PhoneNumber { get; set; }
    public string AuthProvider { get; set; } = null!;
    public bool IsBlacklisted { get; set; }
    public DateTime? CreatedAt { get; set; }
    public List<RoleDto> Roles { get; set; } = new();
    public UserStatisticsDto Statistics { get; set; } = new();
}




public class UserListItemDto
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? PhotoUrl { get; set; }
    public string? PhoneNumber { get; set; }
    public string AuthProvider { get; set; } = null!;
    public bool IsBlacklisted { get; set; }
    public DateTime? CreatedAt { get; set; }
    public List<string> RoleNames { get; set; } = new();
    public string Status => IsBlacklisted ? "Suspended" : "Active";
}







public class UserStatisticsDto
{
    public int DisasterReportsCount { get; set; }
    public int SupportRequestsCount { get; set; }
    public int AssistanceProvidedCount { get; set; }
    public int DonationsCount { get; set; }
    public int OrganizationsCount { get; set; }
}


public class PagedUserListDto
{
    public List<UserListItemDto> Users { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}

public class UserFilterDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public string? Role { get; set; }
    public string? Status { get; set; } // "Active", "Suspended", "All"
    public bool? IsBlacklisted { get; set; }
    public string? AuthProvider { get; set; }
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
    public string SortBy { get; set; } = "CreatedAt";
    public string SortDirection { get; set; } = "desc";
}




public class BulkUserOperationDto
{
    [Required]
    public List<Guid> UserIds { get; set; } = new();

    [Required]
    public string Operation { get; set; } = null!; // "blacklist", "unblacklist", "delete", "assign-role", "remove-role"

    public string? RoleName { get; set; } // Required for role operations
}




public class ChangeUserPasswordDto
{
    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string NewPassword { get; set; } = null!;

    [Required]
    [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; } = null!;
}




public class UpdateUserRolesDto
{
    [Required]
    [MinLength(1, ErrorMessage = "At least one role must be specified")]
    [ValidRoleNames]
    public List<string> RoleNames { get; set; } = new();
    
    public string? Reason { get; set; }
    
    public List<string> Roles 
    { 
        get => RoleNames; 
        set => RoleNames = value; 
    }
}




public class RoleUpdateValidationDto
{
    public bool CanUpdate { get; set; }
    public List<string> Warnings { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public bool IsLastAdmin { get; set; }
    public int AdminCount { get; set; }
}




public class UserManagementStatsDto
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int SuspendedUsers { get; set; }
    public int AdminUsers { get; set; }
    public int NewUsersThisMonth { get; set; }
    public int NewUsersToday { get; set; }
    public int NewUsersLastMonth { get; set; }
}



public class UserStatisticsResponseDto
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int SuspendedUsers { get; set; }
    public int AdminUsers { get; set; }
    public int NewUsersThisMonth { get; set; }
    public int NewUsersLastMonth { get; set; }
}



public class UserActivityTrendsDto
{
    public string Period { get; set; } = null!;
    public List<UserTrendDataDto> Data { get; set; } = new();
}



public class UserTrendDataDto
{
    public string Month { get; set; } = null!;
    public int NewUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int SuspendedUsers { get; set; }
}



public class RoleDistributionDto
{
    public List<RoleDistributionItemDto> Roles { get; set; } = new();
}



public class RoleDistributionItemDto
{
    public string Role { get; set; } = null!;
    public int Count { get; set; }
    public double Percentage { get; set; }
}



public class UserDeletionValidationDto
{
    public bool CanDelete { get; set; }
    public List<string> Reasons { get; set; } = new();
    public bool HasActiveReports { get; set; }
    public bool HasActiveRequests { get; set; }
    public bool IsLastAdmin { get; set; }
}



public class UserExportRequestDto
{
    public string Format { get; set; } = "csv"; // csv, json, excel, pdf
    public List<string> Fields { get; set; } = new();
    public ExportUsersFilters Filters { get; set; } = new();
}

public class ExportUsersFilters
{
    public string? Role { get; set; }   // e.g., "admin" | "cj" | "user"
    public string? Status { get; set; } // "active" | "inactive" | "suspended"
}



public class UserExportResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? DownloadUrl { get; set; }
    public string? FileName { get; set; }
    public int TotalRecords { get; set; }
    public DateTime ExportedAt { get; set; }
}



public class UserExportItemDto
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public string AuthProvider { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string Roles { get; set; } = null!;
    public DateTime? CreatedAt { get; set; }
    public int DisasterReports { get; set; }
    public int SupportRequests { get; set; }
    public int Donations { get; set; }
    public int Organizations { get; set; }
}
