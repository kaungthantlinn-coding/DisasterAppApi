using System.ComponentModel.DataAnnotations;

namespace DisasterApp.Application.DTOs;

public class ExportAuditLogsRequest
{//
    public string Format { get; set; } = "csv";
    public List<string>? Fields { get; set; }
    public ExportAuditLogFilters? Filters { get; set; }
}

public class ExportAuditLogFilters
{
    public string? Action { get; set; }
    public string? TargetType { get; set; }
    public string? Category { get; set; }
    public string? Severity { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? UserId { get; set; }
    public string? Search { get; set; }
    public string? Resource { get; set; }
    public string? IpAddress { get; set; }
    public int MaxRecords { get; set; } = 10000;
    public bool IncludeMetadata { get; set; } = false;
    public bool SanitizeData { get; set; } = true;
}

public class ExportResult
{
    public byte[] Data { get; set; } = Array.Empty<byte>();
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public int RecordCount { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object> Metadata { get; set; } = new();
}
