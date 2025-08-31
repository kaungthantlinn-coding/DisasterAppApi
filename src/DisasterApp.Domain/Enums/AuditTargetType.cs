namespace DisasterApp.Domain.Enums;


public enum AuditTargetType
{
    User = 1,
    Organization = 2,
    Donation = 3,
    Report = 4,
    System = 5,
    Role = 6,
    Permission = 7,
    Authentication = 8,
    Authorization = 9,
    DataAccess = 10,
    AuditLog = 11,
    Emergency = 12,
    Resource = 13,
    Communication = 14,
    Integration = 15
}


public enum AuditCategory
{
    Authentication = 1,
    Authorization = 2,
    UserManagement = 3,
    DataAccess = 4,
    DataModification = 5,
    Security = 6,
    Financial = 7,
    Emergency = 8,
    SystemAdmin = 9,
    Compliance = 10,
    Integration = 11,
    Error = 12
}

/// <summary>
/// Defines the severity levels for audit events
/// </summary>
public enum AuditSeverity
{
    Info = 1,
    Low = 2,
    Medium = 3,
    High = 4,
    Critical = 5
}