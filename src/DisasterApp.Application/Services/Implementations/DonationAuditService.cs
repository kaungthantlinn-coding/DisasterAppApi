using DisasterApp.Application.Services.Interfaces;
using DisasterApp.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace DisasterApp.Application.Services.Implementations;
public class DonationAuditService : IDonationAuditService
{
    private readonly IAuditService _auditService;
    private readonly ILogger<DonationAuditService> _logger;

    public DonationAuditService(IAuditService auditService, ILogger<DonationAuditService> logger)
    {
        _auditService = auditService;
        _logger = logger;
    }

    public async Task LogDonationCreatedAsync(Guid donationId, decimal amount, Guid donorId, Guid organizationId, string paymentMethod, string? ipAddress = null, string? userAgent = null)
    {
        try
        {
            var metadata = new Dictionary<string, object>
            {
                ["donationId"] = donationId,
                ["amount"] = amount,
                ["organizationId"] = organizationId,
                ["paymentMethod"] = paymentMethod,
                ["targetType"] = AuditTargetType.Donation.ToString(),
                ["category"] = AuditCategory.Financial.ToString()
            };

            await _auditService.LogUserActionAsync(
                "DONATION_CREATED",
                AuditSeverity.Medium.ToString().ToLowerInvariant(),
                donorId,
                $"Donation of ${amount:F2} created for organization {organizationId} using {paymentMethod}",
                "donations",
                ipAddress,
                userAgent,
                metadata
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log donation creation audit for donation {DonationId}", donationId);
        }
    }

    public async Task LogDonationUpdatedAsync(Guid donationId, Guid updatedByUserId, string oldValues, string newValues, string? reason = null, string? ipAddress = null, string? userAgent = null)
    {
        try
        {
            var metadata = new Dictionary<string, object>
            {
                ["donationId"] = donationId,
                ["reason"] = reason ?? "Not specified",
                ["targetType"] = AuditTargetType.Donation.ToString(),
                ["category"] = AuditCategory.DataModification.ToString(),
                ["oldValues"] = oldValues,
                ["newValues"] = newValues
            };

            await _auditService.LogUserActionAsync(
                "DONATION_UPDATED",
                AuditSeverity.Medium.ToString().ToLowerInvariant(),
                updatedByUserId,
                $"Donation {donationId} updated. Reason: {reason ?? "Not specified"}",
                "donations",
                ipAddress,
                userAgent,
                metadata
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log donation update audit for donation {DonationId}", donationId);
        }
    }

    public async Task LogDonationProcessedAsync(Guid donationId, Guid processedByUserId, string status, string? transactionId = null, string? ipAddress = null, string? userAgent = null)
    {
        try
        {
            var metadata = new Dictionary<string, object>
            {
                ["donationId"] = donationId,
                ["newStatus"] = status,
                ["transactionId"] = transactionId ?? "Not provided",
                ["targetType"] = AuditTargetType.Donation.ToString(),
                ["category"] = AuditCategory.Financial.ToString()
            };

            var severity = status.ToLowerInvariant() switch
            {
                "completed" => AuditSeverity.Medium,
                "failed" => AuditSeverity.High,
                "cancelled" => AuditSeverity.Medium,
                _ => AuditSeverity.Low
            };

            await _auditService.LogUserActionAsync(
                "DONATION_PROCESSED",
                severity.ToString().ToLowerInvariant(),
                processedByUserId,
                $"Donation {donationId} processed with status: {status}. Transaction ID: {transactionId ?? "N/A"}",
                "donations",
                ipAddress,
                userAgent,
                metadata
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log donation processing audit for donation {DonationId}", donationId);
        }
    }

    public async Task LogDonationRefundedAsync(Guid donationId, Guid refundedByUserId, decimal refundAmount, string reason, string? refundTransactionId = null, string? ipAddress = null, string? userAgent = null)
    {
        try
        {
            var metadata = new Dictionary<string, object>
            {
                ["donationId"] = donationId,
                ["refundAmount"] = refundAmount,
                ["reason"] = reason,
                ["refundTransactionId"] = refundTransactionId ?? "Not provided",
                ["targetType"] = AuditTargetType.Donation.ToString(),
                ["category"] = AuditCategory.Financial.ToString()
            };

            await _auditService.LogUserActionAsync(
                "DONATION_REFUNDED",
                AuditSeverity.High.ToString().ToLowerInvariant(),
                refundedByUserId,
                $"Donation {donationId} refunded ${refundAmount:F2}. Reason: {reason}",
                "donations",
                ipAddress,
                userAgent,
                metadata
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log donation refund audit for donation {DonationId}", donationId);
        }
    }

    public async Task LogDonationVerifiedAsync(Guid donationId, Guid verifiedByUserId, string verificationStatus, string? notes = null, string? ipAddress = null, string? userAgent = null)
    {
        try
        {
            var metadata = new Dictionary<string, object>
            {
                ["donationId"] = donationId,
                ["verificationStatus"] = verificationStatus,
                ["notes"] = notes ?? "No notes provided",
                ["targetType"] = AuditTargetType.Donation.ToString(),
                ["category"] = AuditCategory.Compliance.ToString()
            };

            var severity = verificationStatus.ToLowerInvariant() switch
            {
                "verified" => AuditSeverity.Medium,
                "rejected" => AuditSeverity.High,
                "pending" => AuditSeverity.Low,
                _ => AuditSeverity.Medium
            };

            await _auditService.LogUserActionAsync(
                "DONATION_VERIFIED",
                severity.ToString().ToLowerInvariant(),
                verifiedByUserId,
                $"Donation {donationId} verification status: {verificationStatus}. Notes: {notes ?? "None"}",
                "donations",
                ipAddress,
                userAgent,
                metadata
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log donation verification audit for donation {DonationId}", donationId);
        }
    }

    public async Task LogSuspiciousDonationActivityAsync(Guid donationId, string suspiciousActivity, Guid? detectedByUserId = null, int? riskScore = null, string? ipAddress = null, string? userAgent = null)
    {
        try
        {
            var metadata = new Dictionary<string, object>
            {
                ["donationId"] = donationId,
                ["suspiciousActivity"] = suspiciousActivity,
                ["riskScore"] = riskScore ?? 0,
                ["detectionMethod"] = detectedByUserId.HasValue ? "Manual" : "Automated",
                ["targetType"] = AuditTargetType.Donation.ToString(),
                ["category"] = AuditCategory.Security.ToString()
            };

            var severity = riskScore switch
            {
                >= 80 => AuditSeverity.Critical,
                >= 60 => AuditSeverity.High,
                >= 40 => AuditSeverity.Medium,
                _ => AuditSeverity.Low
            };

            await _auditService.LogUserActionAsync(
                "DONATION_SUSPICIOUS_ACTIVITY",
                severity.ToString().ToLowerInvariant(),
                detectedByUserId,
                $"Suspicious activity detected for donation {donationId}: {suspiciousActivity}. Risk Score: {riskScore ?? 0}",
                "donations",
                ipAddress,
                userAgent,
                metadata
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log suspicious donation activity audit for donation {DonationId}", donationId);
        }
    }

    public async Task LogDonationReportGeneratedAsync(string reportType, Guid generatedByUserId, string dateRange, Guid? organizationId = null, string? ipAddress = null, string? userAgent = null)
    {
        try
        {
            var metadata = new Dictionary<string, object>
            {
                ["reportType"] = reportType,
                ["dateRange"] = dateRange,
                ["organizationId"] = organizationId?.ToString() ?? "All organizations",
                ["targetType"] = AuditTargetType.Donation.ToString(),
                ["category"] = AuditCategory.Compliance.ToString()
            };

            await _auditService.LogUserActionAsync(
                "DONATION_REPORT_GENERATED",
                AuditSeverity.Medium.ToString().ToLowerInvariant(),
                generatedByUserId,
                $"Donation report generated: {reportType} for period {dateRange}. Organization: {organizationId?.ToString() ?? "All"}",
                "donations",
                ipAddress,
                userAgent,
                metadata
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log donation report generation audit for report type {ReportType}", reportType);
        }
    }

    public async Task<(List<object> logs, int totalCount)> GetDonationAuditLogsAsync(Guid donationId, int pageNumber = 1, int pageSize = 50)
    {
        try
        {
            var filters = new DTOs.AuditLogFiltersDto
            {
                Page = pageNumber,
                PageSize = pageSize,
                Search = donationId.ToString(),
                Resource = "donations"
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
            _logger.LogError(ex, "Failed to get donation audit logs for donation {DonationId}", donationId);
            return (new List<object>(), 0);
        }
    }

    public async Task<object> GetDonationAuditStatsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var filters = new DTOs.AuditLogFiltersDto
            {
                Resource = "donations",
                DateFrom = startDate,
                DateTo = endDate,
                PageSize = 1000 // Get enough records for statistics
            };

            var result = await _auditService.GetLogsAsync(filters);
            var logs = result.Logs;

            var stats = new
            {
                totalDonationEvents = logs.Count,
                donationCreated = logs.Count(l => l.Action == "DONATION_CREATED"),
                donationProcessed = logs.Count(l => l.Action == "DONATION_PROCESSED"),
                donationRefunded = logs.Count(l => l.Action == "DONATION_REFUNDED"),
                suspiciousActivities = logs.Count(l => l.Action == "DONATION_SUSPICIOUS_ACTIVITY"),
                verificationEvents = logs.Count(l => l.Action == "DONATION_VERIFIED"),
                reportsGenerated = logs.Count(l => l.Action == "DONATION_REPORT_GENERATED"),
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
            _logger.LogError(ex, "Failed to get donation audit statistics");
            return new { error = "Failed to retrieve statistics" };
        }
    }
}
