namespace DisasterApp.Domain.Enums;

public enum AuditTargetType
{
    User = 1,
    Organization = 2,
    Donation = 3,
    Report = 4,
    System = 5,
    Role = 6,//
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
    DataAccess = 3,
    DataModification = 4,
    SystemAdmin = 5,
    UserManagement = 6,
    Financial = 7,
    Security = 8,
    Compliance = 9,
    Integration = 10,
    Emergency = 11,
    Error = 12
}

public enum AuditSeverity
{
    Info = 1,
    Low = 2,
    Medium = 3,
    High = 4,
    Critical = 5
}
