using System.ComponentModel.DataAnnotations;

namespace DisasterApp.Application.DTOs;
//
public class AuditLogDto
{
    public string Id { get; set; } = null!;
    public DateTime Timestamp { get; set; }
    public string Action { get; set; } = null!;
    public string Severity { get; set; } = null!;
    public AuditLogUserDto? User { get; set; }
    public string Details { get; set; } = null!;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string Resource { get; set; } = null!;
    public Dictionary<string, object>? Metadata { get; set; }
}

public class AuditLogUserDto
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
}

public class AuditLogFiltersDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Search { get; set; }
    public string? Severity { get; set; }
    public string? Action { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? UserId { get; set; }
    public string? Resource { get; set; }
    
    // Additional properties for API compatibility
    public DateTime? StartDate 
    { 
        get => DateFrom; 
        set => DateFrom = value; 
    }
    
    public DateTime? EndDate 
    { 
        get => DateTo; 
        set => DateTo = value; 
    }
}

public class PaginatedAuditLogsDto
{
    public List<AuditLogDto> Logs { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public bool HasMore { get; set; }
}

public class AuditLogStatsDto
{
    public int TotalLogs { get; set; }
    public int CriticalAlerts { get; set; }
    public int SecurityEvents { get; set; }
    public int SystemErrors { get; set; }
    public int RecentActivity { get; set; }
}

public class CreateAuditLogDto
{
    [Required]
    public string Action { get; set; } = null!;
    
    [Required]
    public string Severity { get; set; } = "info";
    
    public Guid? UserId { get; set; }
    
    [Required]
    public string Details { get; set; } = null!;
    
    public string? IpAddress { get; set; }
    
    public string? UserAgent { get; set; }
    
    [Required]
    public string Resource { get; set; } = null!;
    
    public Dictionary<string, object>? Metadata { get; set; }
    
    public string? EntityType { get; set; }
    
    public string? EntityId { get; set; }
    
    public string? OldValues { get; set; }
    
    public string? NewValues { get; set; }
}

public class AuditLogExportDto
{
    [Required]
    public string Format { get; set; } = "csv"; // csv or excel
    
    public AuditLogFiltersDto? Filters { get; set; }
}