using DisasterApp.Application.Services.Interfaces;
using DisasterApp.Domain.Enums;

namespace DisasterApp.Application.Services.Implementations;

public class AuditTargetValidator : IAuditTargetValidator
{
    private readonly Dictionary<AuditTargetType, List<AuditCategory>> _validCombinations;
    private readonly Dictionary<string, AuditSeverity> _actionSeverityMap;

    public AuditTargetValidator()
    {
        _validCombinations = InitializeValidCombinations();
        _actionSeverityMap = InitializeActionSeverityMap();
    }

    public bool IsValidTargetType(AuditTargetType targetType, string action)
    {
        if (string.IsNullOrWhiteSpace(action))
            return false;

        // Check for valid target type
        return Enum.IsDefined(typeof(AuditTargetType), targetType);
    }

    public bool IsValidCategoryForTarget(AuditCategory category, AuditTargetType targetType)
    {
        return _validCombinations.ContainsKey(targetType) && 
               _validCombinations[targetType].Contains(category);
    }

    public AuditCategory GetRecommendedCategory(AuditTargetType targetType, string action)
    {
        var actionLower = action.ToLowerInvariant();

        return targetType switch
        {
            AuditTargetType.User when actionLower.Contains("login") || actionLower.Contains("logout") || actionLower.Contains("password") => AuditCategory.Authentication,
            AuditTargetType.User when actionLower.Contains("role") || actionLower.Contains("permission") => AuditCategory.Authorization,
            AuditTargetType.User => AuditCategory.UserManagement,
            
            AuditTargetType.Organization => AuditCategory.DataModification,
            AuditTargetType.Donation => AuditCategory.Financial,
            AuditTargetType.Report => AuditCategory.DataModification,
            
            AuditTargetType.System => AuditCategory.SystemAdmin,
            AuditTargetType.Role or AuditTargetType.Permission => AuditCategory.Authorization,
            AuditTargetType.Authentication => AuditCategory.Authentication,
            AuditTargetType.Authorization => AuditCategory.Authorization,
            
            AuditTargetType.DataAccess when actionLower.Contains("export") || actionLower.Contains("audit") => AuditCategory.Compliance,
            AuditTargetType.DataAccess => AuditCategory.DataAccess,
            
            AuditTargetType.AuditLog => AuditCategory.Compliance,
            AuditTargetType.Emergency => AuditCategory.Emergency,
            AuditTargetType.Resource => AuditCategory.DataModification,
            AuditTargetType.Communication => AuditCategory.SystemAdmin,
            AuditTargetType.Integration => AuditCategory.Integration,
            
            _ => AuditCategory.DataModification
        };
    }

    public AuditSeverity GetRecommendedSeverity(string action, AuditTargetType targetType)
    {
        var actionLower = action.ToLowerInvariant();

        // Check for critical security actions
        if (actionLower.Contains("failed_login") || actionLower.Contains("security_violation") || 
            actionLower.Contains("unauthorized") || actionLower.Contains("breach"))
            return AuditSeverity.Critical;

        // Check for high priority actions
        if (actionLower.Contains("delete") || actionLower.Contains("admin") || 
            actionLower.Contains("role_change") || actionLower.Contains("permission_grant"))
            return AuditSeverity.High;

        // Check for medium priority actions
        if (actionLower.Contains("create") || actionLower.Contains("update") || 
            actionLower.Contains("export") || actionLower.Contains("login"))
            return AuditSeverity.Medium;

        // Check for low priority actions
        if (actionLower.Contains("view") || actionLower.Contains("search") || 
            actionLower.Contains("access"))
            return AuditSeverity.Low;

        // Check action severity map
        if (_actionSeverityMap.TryGetValue(actionLower, out var mappedSeverity))
            return mappedSeverity;

        // Target type specific defaults
        return targetType switch
        {
            AuditTargetType.System => AuditSeverity.High,
            AuditTargetType.Donation => AuditSeverity.Medium,
            AuditTargetType.Emergency => AuditSeverity.High,
            AuditTargetType.Authentication => AuditSeverity.Medium,
            AuditTargetType.Authorization => AuditSeverity.High,
            _ => AuditSeverity.Info
        };
    }

    public bool IsValidSeverity(AuditSeverity severity, string action, AuditTargetType targetType)
    {
        var recommended = GetRecommendedSeverity(action, targetType);
        
        // Allow the recommended severity or higher
        return (int)severity >= (int)recommended - 1; // Allow one level lower than recommended
    }

    private Dictionary<AuditTargetType, List<AuditCategory>> InitializeValidCombinations()
    {
        return new Dictionary<AuditTargetType, List<AuditCategory>>
        {
            [AuditTargetType.User] = new List<AuditCategory>
            {
                AuditCategory.Authentication, AuditCategory.Authorization, AuditCategory.UserManagement,
                AuditCategory.DataAccess, AuditCategory.DataModification, AuditCategory.Security
            },
            [AuditTargetType.Organization] = new List<AuditCategory>
            {
                AuditCategory.DataAccess, AuditCategory.DataModification, AuditCategory.UserManagement,
                AuditCategory.Compliance
            },
            [AuditTargetType.Donation] = new List<AuditCategory>
            {
                AuditCategory.Financial, AuditCategory.DataAccess, AuditCategory.DataModification,
                AuditCategory.Compliance
            },
            [AuditTargetType.Report] = new List<AuditCategory>
            {
                AuditCategory.DataAccess, AuditCategory.DataModification, AuditCategory.Emergency,
                AuditCategory.Compliance
            },
            [AuditTargetType.System] = new List<AuditCategory>
            {
                AuditCategory.SystemAdmin, AuditCategory.Security, AuditCategory.Error,
                AuditCategory.Integration
            },
            [AuditTargetType.Role] = new List<AuditCategory>
            {
                AuditCategory.Authorization, AuditCategory.UserManagement, AuditCategory.Security
            },
            [AuditTargetType.Permission] = new List<AuditCategory>
            {
                AuditCategory.Authorization, AuditCategory.Security
            },
            [AuditTargetType.Authentication] = new List<AuditCategory>
            {
                AuditCategory.Authentication, AuditCategory.Security
            },
            [AuditTargetType.Authorization] = new List<AuditCategory>
            {
                AuditCategory.Authorization, AuditCategory.Security
            },
            [AuditTargetType.DataAccess] = new List<AuditCategory>
            {
                AuditCategory.DataAccess, AuditCategory.Compliance, AuditCategory.Security
            },
            [AuditTargetType.AuditLog] = new List<AuditCategory>
            {
                AuditCategory.Compliance, AuditCategory.DataAccess, AuditCategory.Security
            },
            [AuditTargetType.Emergency] = new List<AuditCategory>
            {
                AuditCategory.Emergency, AuditCategory.SystemAdmin, AuditCategory.Security
            },
            [AuditTargetType.Resource] = new List<AuditCategory>
            {
                AuditCategory.DataAccess, AuditCategory.DataModification, AuditCategory.SystemAdmin
            },
            [AuditTargetType.Communication] = new List<AuditCategory>
            {
                AuditCategory.SystemAdmin, AuditCategory.Integration, AuditCategory.Emergency
            },
            [AuditTargetType.Integration] = new List<AuditCategory>
            {
                AuditCategory.Integration, AuditCategory.SystemAdmin, AuditCategory.Security,
                AuditCategory.Error
            }
        };
    }

    private Dictionary<string, AuditSeverity> InitializeActionSeverityMap()
    {
        return new Dictionary<string, AuditSeverity>
        {
            // Critical actions
            ["system_shutdown"] = AuditSeverity.Critical,
            ["data_breach"] = AuditSeverity.Critical,
            ["security_violation"] = AuditSeverity.Critical,
            ["unauthorized_access"] = AuditSeverity.Critical,
            ["failed_login_multiple"] = AuditSeverity.Critical,
            
            // High severity actions
            ["user_delete"] = AuditSeverity.High,
            ["admin_role_assigned"] = AuditSeverity.High,
            ["permission_escalation"] = AuditSeverity.High,
            ["data_export_large"] = AuditSeverity.High,
            ["system_configuration_change"] = AuditSeverity.High,
            
            // Medium severity actions
            ["user_create"] = AuditSeverity.Medium,
            ["user_update"] = AuditSeverity.Medium,
            ["donation_create"] = AuditSeverity.Medium,
            ["report_create"] = AuditSeverity.Medium,
            ["login_success"] = AuditSeverity.Medium,
            
            // Low severity actions
            ["data_view"] = AuditSeverity.Low,
            ["search_performed"] = AuditSeverity.Low,
            ["report_view"] = AuditSeverity.Low,
            
            // Info level actions
            ["page_access"] = AuditSeverity.Info,
            ["api_call"] = AuditSeverity.Info,
            ["logout"] = AuditSeverity.Info
        };
    }
}
