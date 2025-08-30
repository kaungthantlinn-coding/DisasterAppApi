using DisasterApp.Domain.Enums;

namespace DisasterApp.Application.Services.Interfaces;

public interface IOrganizationAuditService
{
    Task LogOrganizationRegisteredAsync(Guid organizationId, string organizationName, Guid registeredByUserId, string registrationType, string? ipAddress = null, string? userAgent = null);
    Task LogOrganizationUpdatedAsync(Guid organizationId, Guid updatedByUserId, string oldValues, string newValues, string? reason = null, string? ipAddress = null, string? userAgent = null);
    Task LogOrganizationVerifiedAsync(Guid organizationId, Guid verifiedByUserId, string verificationStatus, string? verificationNotes = null, List<string>? documentsProvided = null, string? ipAddress = null, string? userAgent = null);
    Task LogOrganizationStatusChangedAsync(Guid organizationId, Guid changedByUserId, string oldStatus, string newStatus, string? reason = null, string? ipAddress = null, string? userAgent = null);
    Task LogOrganizationMemberAddedAsync(Guid organizationId, Guid addedUserId, Guid addedByUserId, string role, string? ipAddress = null, string? userAgent = null);
    Task LogOrganizationMemberRemovedAsync(Guid organizationId, Guid removedUserId, Guid removedByUserId, string? reason = null, string? ipAddress = null, string? userAgent = null);
    Task LogOrganizationDataAccessedAsync(Guid organizationId, Guid accessedByUserId, string dataType, string? accessReason = null, string? ipAddress = null, string? userAgent = null);
    Task LogOrganizationComplianceAuditAsync(Guid organizationId, Guid auditedByUserId, string complianceType, string result, string? findings = null, string? ipAddress = null, string? userAgent = null);
    Task<(List<object> logs, int totalCount)> GetOrganizationAuditLogsAsync(Guid organizationId, int pageNumber = 1, int pageSize = 50);
    Task<object> GetOrganizationAuditStatsAsync(DateTime? startDate = null, DateTime? endDate = null);
}
