using DisasterApp.Application.Services.Interfaces;
using DisasterApp.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace DisasterApp.Application.Services.Implementations;

/// <summary>
/// Implementation of specialized audit service for organization-related operations
/// </summary>
public class OrganizationAuditService : IOrganizationAuditService
{
    private readonly IAuditService _auditService;
    private readonly ILogger<OrganizationAuditService> _logger;

    public OrganizationAuditService(IAuditService auditService, ILogger<OrganizationAuditService> logger)
    {
        _auditService = auditService;
        _logger = logger;
    }

    public async Task LogOrganizationRegisteredAsync(Guid organizationId, string organizationName, Guid registeredByUserId, string registrationType, string? ipAddress = null, string? userAgent = null)
    {
        try
        {
            var metadata = new Dictionary<string, object>
            {
                ["organizationId"] = organizationId,
                ["organizationName"] = organizationName,
                ["registrationType"] = registrationType,
                ["targetType"] = AuditTargetType.Organization.ToString(),
                ["category"] = AuditCategory.DataModification.ToString()
            };

            await _auditService.LogUserActionAsync(
                "ORGANIZATION_REGISTERED",
                AuditSeverity.Medium.ToString().ToLowerInvariant(),
                registeredByUserId,
                $"Organization '{organizationName}' registered via {registrationType}",
                "organizations",
                ipAddress,
                userAgent,
                metadata
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log organization registration audit for organization {OrganizationId}", organizationId);
        }
    }

    public async Task LogOrganizationUpdatedAsync(Guid organizationId, Guid updatedByUserId, string oldValues, string newValues, string? reason = null, string? ipAddress = null, string? userAgent = null)
    {
        try
        {
            var metadata = new Dictionary<string, object>
            {
                ["organizationId"] = organizationId,
                ["reason"] = reason ?? "Not specified",
                ["oldValues"] = oldValues,
                ["newValues"] = newValues,
                ["targetType"] = AuditTargetType.Organization.ToString(),
                ["category"] = AuditCategory.DataModification.ToString()
            };

            await _auditService.LogUserActionAsync(
                "ORGANIZATION_UPDATED",
                AuditSeverity.Medium.ToString().ToLowerInvariant(),
                updatedByUserId,
                $"Organization {organizationId} updated. Reason: {reason ?? "Not specified"}",
                "organizations",
                ipAddress,
                userAgent,
                metadata
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log organization update audit for organization {OrganizationId}", organizationId);
        }
    }

    public async Task LogOrganizationVerifiedAsync(Guid organizationId, Guid verifiedByUserId, string verificationStatus, string? verificationNotes = null, List<string>? documentsProvided = null, string? ipAddress = null, string? userAgent = null)
    {
        try
        {
            var metadata = new Dictionary<string, object>
            {
                ["organizationId"] = organizationId,
                ["verificationStatus"] = verificationStatus,
                ["verificationNotes"] = verificationNotes ?? "No notes provided",
                ["documentsProvided"] = documentsProvided ?? new List<string>(),
                ["targetType"] = AuditTargetType.Organization.ToString(),
                ["category"] = AuditCategory.Compliance.ToString()
            };

            var severity = verificationStatus.ToLowerInvariant() switch
            {
                "verified" => AuditSeverity.Medium,
                "rejected" => AuditSeverity.High,
                "pending" => AuditSeverity.Low,
                "suspended" => AuditSeverity.High,
                _ => AuditSeverity.Medium
            };

            await _auditService.LogUserActionAsync(
                "ORGANIZATION_VERIFIED",
                severity.ToString().ToLowerInvariant(),
                verifiedByUserId,
                $"Organization {organizationId} verification status: {verificationStatus}. Documents: {string.Join(", ", documentsProvided ?? new List<string>())}",
                "organizations",
                ipAddress,
                userAgent,
                metadata
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log organization verification audit for organization {OrganizationId}", organizationId);
        }
    }

    public async Task LogOrganizationStatusChangedAsync(Guid organizationId, Guid changedByUserId, string oldStatus, string newStatus, string? reason = null, string? ipAddress = null, string? userAgent = null)
    {
        try
        {
            var metadata = new Dictionary<string, object>
            {
                ["organizationId"] = organizationId,
                ["oldStatus"] = oldStatus,
                ["newStatus"] = newStatus,
                ["reason"] = reason ?? "Not specified",
                ["targetType"] = AuditTargetType.Organization.ToString(),
                ["category"] = AuditCategory.DataModification.ToString()
            };

            var severity = newStatus.ToLowerInvariant() switch
            {
                "suspended" or "banned" or "inactive" => AuditSeverity.High,
                "active" or "verified" => AuditSeverity.Medium,
                _ => AuditSeverity.Low
            };

            await _auditService.LogUserActionAsync(
                "ORGANIZATION_STATUS_CHANGED",
                severity.ToString().ToLowerInvariant(),
                changedByUserId,
                $"Organization {organizationId} status changed from {oldStatus} to {newStatus}. Reason: {reason ?? "Not specified"}",
                "organizations",
                ipAddress,
                userAgent,
                metadata
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log organization status change audit for organization {OrganizationId}", organizationId);
        }
    }

    public async Task LogOrganizationMemberAddedAsync(Guid organizationId, Guid addedUserId, Guid addedByUserId, string role, string? ipAddress = null, string? userAgent = null)
    {
        try
        {
            var metadata = new Dictionary<string, object>
            {
                ["organizationId"] = organizationId,
                ["addedUserId"] = addedUserId,
                ["role"] = role,
                ["targetType"] = AuditTargetType.Organization.ToString(),
                ["category"] = AuditCategory.UserManagement.ToString()
            };

            var severity = role.ToLowerInvariant() switch
            {
                "admin" or "owner" => AuditSeverity.High,
                "manager" => AuditSeverity.Medium,
                _ => AuditSeverity.Low
            };

            await _auditService.LogUserActionAsync(
                "ORGANIZATION_MEMBER_ADDED",
                severity.ToString().ToLowerInvariant(),
                addedByUserId,
                $"User {addedUserId} added to organization {organizationId} with role: {role}",
                "organizations",
                ipAddress,
                userAgent,
                metadata
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log organization member addition audit for organization {OrganizationId}", organizationId);
        }
    }

    public async Task LogOrganizationMemberRemovedAsync(Guid organizationId, Guid removedUserId, Guid removedByUserId, string? reason = null, string? ipAddress = null, string? userAgent = null)
    {
        try
        {
            var metadata = new Dictionary<string, object>
            {
                ["organizationId"] = organizationId,
                ["removedUserId"] = removedUserId,
                ["reason"] = reason ?? "Not specified",
                ["targetType"] = AuditTargetType.Organization.ToString(),
                ["category"] = AuditCategory.UserManagement.ToString()
            };

            await _auditService.LogUserActionAsync(
                "ORGANIZATION_MEMBER_REMOVED",
                AuditSeverity.Medium.ToString().ToLowerInvariant(),
                removedByUserId,
                $"User {removedUserId} removed from organization {organizationId}. Reason: {reason ?? "Not specified"}",
                "organizations",
                ipAddress,
                userAgent,
                metadata
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log organization member removal audit for organization {OrganizationId}", organizationId);
        }
    }

    public async Task LogOrganizationDataAccessedAsync(Guid organizationId, Guid accessedByUserId, string dataType, string? accessReason = null, string? ipAddress = null, string? userAgent = null)
    {
        try
        {
            var metadata = new Dictionary<string, object>
            {
                ["organizationId"] = organizationId,
                ["dataType"] = dataType,
                ["accessReason"] = accessReason ?? "Not specified",
                ["targetType"] = AuditTargetType.Organization.ToString(),
                ["category"] = AuditCategory.DataAccess.ToString()
            };

            var severity = dataType.ToLowerInvariant() switch
            {
                "financial" or "sensitive" or "personal" => AuditSeverity.Medium,
                "public" or "general" => AuditSeverity.Low,
                _ => AuditSeverity.Low
            };

            await _auditService.LogUserActionAsync(
                "ORGANIZATION_DATA_ACCESSED",
                severity.ToString().ToLowerInvariant(),
                accessedByUserId,
                $"Organization {organizationId} data accessed. Type: {dataType}. Reason: {accessReason ?? "Not specified"}",
                "organizations",
                ipAddress,
                userAgent,
                metadata
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log organization data access audit for organization {OrganizationId}", organizationId);
        }
    }

    public async Task LogOrganizationComplianceAuditAsync(Guid organizationId, Guid auditedByUserId, string complianceType, string result, string? findings = null, string? ipAddress = null, string? userAgent = null)
    {
        try
        {
            var metadata = new Dictionary<string, object>
            {
                ["organizationId"] = organizationId,
                ["complianceType"] = complianceType,
                ["result"] = result,
                ["findings"] = findings ?? "No findings",
                ["targetType"] = AuditTargetType.Organization.ToString(),
                ["category"] = AuditCategory.Compliance.ToString()
            };

            var severity = result.ToLowerInvariant() switch
            {
                "failed" or "non-compliant" => AuditSeverity.High,
                "passed" or "compliant" => AuditSeverity.Medium,
                "pending" => AuditSeverity.Low,
                _ => AuditSeverity.Medium
            };

            await _auditService.LogUserActionAsync(
                "ORGANIZATION_COMPLIANCE_AUDIT",
                severity.ToString().ToLowerInvariant(),
                auditedByUserId,
                $"Organization {organizationId} compliance audit: {complianceType} - {result}. Findings: {findings ?? "None"}",
                "organizations",
                ipAddress,
                userAgent,
                metadata
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log organization compliance audit for organization {OrganizationId}", organizationId);
        }
    }

    public async Task<(List<object> logs, int totalCount)> GetOrganizationAuditLogsAsync(Guid organizationId, int pageNumber = 1, int pageSize = 50)
    {
        try
        {
            var filters = new DTOs.AuditLogFiltersDto
            {
                Page = pageNumber,
                PageSize = pageSize,
                Search = organizationId.ToString(),
                Resource = "organizations"
            };

            var result = await _auditService.GetLogsAsync(filters);
            
            var logs = result.Logs.Select(log => new
            {
                id = log.Id,
                timestamp = log.Timestamp,
                action = log.Action,
                severity = log.Severity,
                userId = log.User?.Id,
                userName = log.User?.Name,
                details = log.Details,
                ipAddress = log.IpAddress,
                metadata = log.Metadata
            }).Cast<object>().ToList();

            return (logs, result.TotalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get organization audit logs for organization {OrganizationId}", organizationId);
            return (new List<object>(), 0);
        }
    }

    public async Task<object> GetOrganizationAuditStatsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var filters = new DTOs.AuditLogFiltersDto
            {
                Resource = "organizations",
                DateFrom = startDate,
                DateTo = endDate,
                PageSize = 1000 // Get enough records for statistics
            };

            var result = await _auditService.GetLogsAsync(filters);
            var logs = result.Logs;

            var stats = new
            {
                totalOrganizationEvents = logs.Count,
                organizationRegistered = logs.Count(l => l.Action == "ORGANIZATION_REGISTERED"),
                organizationUpdated = logs.Count(l => l.Action == "ORGANIZATION_UPDATED"),
                organizationVerified = logs.Count(l => l.Action == "ORGANIZATION_VERIFIED"),
                statusChanges = logs.Count(l => l.Action == "ORGANIZATION_STATUS_CHANGED"),
                memberAdditions = logs.Count(l => l.Action == "ORGANIZATION_MEMBER_ADDED"),
                memberRemovals = logs.Count(l => l.Action == "ORGANIZATION_MEMBER_REMOVED"),
                dataAccess = logs.Count(l => l.Action == "ORGANIZATION_DATA_ACCESSED"),
                complianceAudits = logs.Count(l => l.Action == "ORGANIZATION_COMPLIANCE_AUDIT"),
                criticalEvents = logs.Count(l => l.Severity == "critical"),
                highSeverityEvents = logs.Count(l => l.Severity == "high"),
                dateRange = new
                {
                    from = startDate?.ToString("yyyy-MM-dd") ?? "All time",
                    to = endDate?.ToString("yyyy-MM-dd") ?? "Present"
                }
            };

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get organization audit statistics");
            return new { error = "Failed to retrieve statistics" };
        }
    }
}
