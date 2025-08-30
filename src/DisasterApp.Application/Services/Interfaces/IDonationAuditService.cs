using DisasterApp.Domain.Enums;

namespace DisasterApp.Application.Services.Interfaces;

public interface IDonationAuditService
{
    Task LogDonationCreatedAsync(Guid donationId, decimal amount, Guid donorId, Guid organizationId, string paymentMethod, string? ipAddress = null, string? userAgent = null);
    Task LogDonationUpdatedAsync(Guid donationId, Guid updatedByUserId, string oldValues, string newValues, string? reason = null, string? ipAddress = null, string? userAgent = null);
    Task LogDonationProcessedAsync(Guid donationId, Guid processedByUserId, string status, string? transactionId = null, string? ipAddress = null, string? userAgent = null);
    Task LogDonationRefundedAsync(Guid donationId, Guid refundedByUserId, decimal refundAmount, string reason, string? refundTransactionId = null, string? ipAddress = null, string? userAgent = null);
    Task LogDonationVerifiedAsync(Guid donationId, Guid verifiedByUserId, string verificationStatus, string? notes = null, string? ipAddress = null, string? userAgent = null);
    Task LogSuspiciousDonationActivityAsync(Guid donationId, string suspiciousActivity, Guid? detectedByUserId = null, int? riskScore = null, string? ipAddress = null, string? userAgent = null);
    Task LogDonationReportGeneratedAsync(string reportType, Guid generatedByUserId, string dateRange, Guid? organizationId = null, string? ipAddress = null, string? userAgent = null);
    Task<(List<object> logs, int totalCount)> GetDonationAuditLogsAsync(Guid donationId, int pageNumber = 1, int pageSize = 50);
    Task<object> GetDonationAuditStatsAsync(DateTime? startDate = null, DateTime? endDate = null);
}
