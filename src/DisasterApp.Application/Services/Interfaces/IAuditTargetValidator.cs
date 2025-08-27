using DisasterApp.Domain.Enums;

namespace DisasterApp.Application.Services.Interfaces;

public interface IAuditTargetValidator
{
    bool IsValidTargetType(AuditTargetType targetType, string action);
    bool IsValidCategoryForTarget(AuditCategory category, AuditTargetType targetType);
    AuditCategory GetRecommendedCategory(AuditTargetType targetType, string action);
    AuditSeverity GetRecommendedSeverity(string action, AuditTargetType targetType);
    bool IsValidSeverity(AuditSeverity severity, string action, AuditTargetType targetType);
}
