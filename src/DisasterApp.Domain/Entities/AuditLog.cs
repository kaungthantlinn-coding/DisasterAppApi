namespace DisasterApp.Domain.Entities;

public partial class AuditLog
{
    public AuditLog()
    {
        AuditLogId = Guid.NewGuid();
        Timestamp = DateTime.UtcNow;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public Guid AuditLogId { get; set; }

    public string Action { get; set; } = null!;

    public string Severity { get; set; } = "info";

    public string EntityType { get; set; } = null!;

    public string? EntityId { get; set; }

    public string Details { get; set; } = null!;

    public string? OldValues { get; set; }

    public string? NewValues { get; set; }

    public Guid? UserId { get; set; }

    public string? UserName { get; set; }

    public DateTime Timestamp { get; set; }

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public string Resource { get; set; } = null!;

    public string? Metadata { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual User? User { get; set; }
}
