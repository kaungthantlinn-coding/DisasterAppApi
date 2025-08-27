using DisasterApp.Application.DTOs;
using DisasterApp.Application.Services.Interfaces;
using DisasterApp.WebApi.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace DisasterApp.WebApi.Controllers;

[ApiController]
[Route("api/audit-logs")]
[Authorize]
public class AuditLogsController : ControllerBase
{
    private readonly ILogger<AuditLogsController> _logger;
    private readonly IAuditService _auditService;
    private readonly IExportService _exportService;
    private readonly IAuditDataSanitizer _dataSanitizer;

    public AuditLogsController(
        ILogger<AuditLogsController> logger, 
        IAuditService auditService,
        IExportService exportService,
        IAuditDataSanitizer dataSanitizer)
    {
        _logger = logger;
        _auditService = auditService;
        _exportService = exportService;
        _dataSanitizer = dataSanitizer;
    }

    [HttpGet]
    [SuperAdminOrAdmin]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? severity = null,
        [FromQuery] string? action = null,
        [FromQuery] string? userId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        // Validate parameters
        if (page <= 0)
            return BadRequest(new { message = "Page must be greater than 0" });
        
        if (limit <= 0)
            return BadRequest(new { message = "Limit must be greater than 0" });
        
        try
        {
            var filters = new AuditLogFiltersDto
            {
                Page = page,
                PageSize = Math.Min(limit, 100), // Cap at 100 records per page
                Search = search,
                Severity = severity,
                Action = action,
                UserId = userId,
                DateFrom = startDate,
                DateTo = endDate
            };

            var result = await _auditService.GetLogsAsync(filters);
            
            // Transform to match the required response format
            var response = new
            {
                logs = result.Logs.Select(log => new
                {
                    id = log.Id,
                    timestamp = log.Timestamp,
                    userId = log.User?.Id,
                    userName = log.User?.Name,
                    action = log.Action,
                    severity = log.Severity,
                    details = log.Details,
                    ipAddress = log.IpAddress,
                    userAgent = log.UserAgent,
                    metadata = log.Metadata ?? new Dictionary<string, object>()
                }).ToList(),
                total = result.TotalCount,
                page = result.Page,
                limit = result.PageSize,
                totalPages = (int)Math.Ceiling((double)result.TotalCount / result.PageSize)
            };
            
            // Log admin access to audit logs
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(currentUserId, out var adminUserId))
            {
                await _auditService.LogUserActionAsync(
                    "AUDIT_LOGS_ACCESSED",
                    "info",
                    adminUserId,
                    "Admin accessed audit logs",
                    "audit",
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    Request.Headers.UserAgent.ToString()
                );
            }
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve audit logs");
            return StatusCode(500, new { message = "Failed to retrieve audit logs" });
        }
    }

    [HttpGet("stats")]
    [SuperAdminOrAdmin]
    public async Task<IActionResult> GetAuditLogStatistics()
    {
        try
        {
            var stats = await _auditService.GetStatisticsAsync();
            
            // Transform to match the required response format
            var response = new
            {
                totalLogs = stats.TotalLogs,
                todayLogs = stats.RecentActivity, // Using RecentActivity as proxy for today's logs
                criticalAlerts = stats.CriticalAlerts,
                activeUsers = await GetActiveUsersCountAsync()
            };
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve audit log statistics");
            return StatusCode(500, new { message = "Failed to retrieve audit log statistics" });
        }
    }

    [HttpGet("export")]
    [SuperAdminOrAdmin]
    public async Task<IActionResult> ExportAuditLogs(
        [FromQuery] string format = "csv",
        [FromQuery] int page = 1,
        [FromQuery] int limit = 1000,
        [FromQuery] string? search = null,
        [FromQuery] string? severity = null,
        [FromQuery] string? action = null,
        [FromQuery] string? userId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            if (string.IsNullOrEmpty(format) || (format.ToLower() != "csv" && format.ToLower() != "excel"))
            {
                return BadRequest(new { message = "Invalid format. Use 'csv' or 'excel'." });
            }

            var filters = new AuditLogFiltersDto
            {
                Page = page,
                PageSize = Math.Min(limit, 10000), // Cap at 10000 for exports
                Search = search,
                Severity = severity,
                Action = action,
                UserId = userId,
                DateFrom = startDate,
                DateTo = endDate
            };

            var exportData = await _auditService.ExportLogsAsync(format, filters);
            
            var contentType = format.ToLower() == "csv" ? "text/csv" : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            var fileName = $"audit-logs-{DateTime.UtcNow:yyyy-MM-dd-HH-mm-ss}.{format.ToLower()}";
            
            // Log export action
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(currentUserId, out var adminUserId))
            {
                await _auditService.LogUserActionAsync(
                    "AUDIT_LOGS_EXPORTED",
                    "info",
                    adminUserId,
                    $"Admin exported audit logs in {format.ToUpper()} format",
                    "audit",
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    Request.Headers.UserAgent.ToString(),
                    new Dictionary<string, object> { { "format", format }, { "filters", filters } }
                );
            }
            
            return File(exportData, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export audit logs in format {Format}", format);
            return StatusCode(500, new { message = "Failed to export audit logs" });
        }
    }

    [HttpPost("export")]
    [SuperAdminOrAdmin]
    public async Task<IActionResult> ExportAuditLogsAdvanced([FromBody] object requestBody)
    {
        try
        {
            // Log the raw request body for debugging
            _logger.LogInformation("Raw export request received: {@RequestBody}", requestBody);

            // Try to parse the request manually to handle property name mismatches
            var jsonElement = (JsonElement)requestBody;
            
            var format = jsonElement.TryGetProperty("format", out var formatProp) ? formatProp.GetString() : "csv";
            
            var fields = new List<string>();
            if (jsonElement.TryGetProperty("fields", out var fieldsProp) && fieldsProp.ValueKind == JsonValueKind.Array)
            {
                fields = fieldsProp.EnumerateArray().Select(x => x.GetString() ?? "").ToList();
            }

            // Handle both "filters" and "exportFilters" property names
            JsonElement filtersProp = default;
            bool hasFilters = jsonElement.TryGetProperty("filters", out filtersProp) || 
                             jsonElement.TryGetProperty("exportFilters", out filtersProp);

            var filters = new ExportAuditLogFilters();
            if (hasFilters && filtersProp.ValueKind == JsonValueKind.Object)
            {
                // Handle action (can be string or array)
                if (filtersProp.TryGetProperty("action", out var actionProp))
                {
                    if (actionProp.ValueKind == JsonValueKind.Array)
                    {
                        var actions = actionProp.EnumerateArray().Select(x => MapActionToDatabase(x.GetString() ?? string.Empty)).Where(x => !string.IsNullOrEmpty(x));
                        filters.Action = string.Join(",", actions);
                    }
                    else if (actionProp.ValueKind == JsonValueKind.String)
                    {
                        filters.Action = MapActionToDatabase(actionProp.GetString() ?? string.Empty);
                    }
                }

                // Handle targetType (can be string or array)
                if (filtersProp.TryGetProperty("targetType", out var targetTypeProp))
                {
                    if (targetTypeProp.ValueKind == JsonValueKind.Array)
                        filters.TargetType = string.Join(",", targetTypeProp.EnumerateArray().Select(x => x.GetString()));
                    else if (targetTypeProp.ValueKind == JsonValueKind.String)
                        filters.TargetType = targetTypeProp.GetString();
                }

                filters.Severity = filtersProp.TryGetProperty("severity", out var severityProp) ? severityProp.GetString() : null;
                filters.Search = filtersProp.TryGetProperty("search", out var searchProp) ? searchProp.GetString() : null;
                filters.UserId = filtersProp.TryGetProperty("userId", out var userIdProp) ? userIdProp.GetString() : null;
                filters.Resource = filtersProp.TryGetProperty("resource", out var resourceProp) ? resourceProp.GetString() : null;
                
                if (filtersProp.TryGetProperty("startDate", out var startDateProp) && startDateProp.ValueKind != JsonValueKind.Null)
                    filters.StartDate = startDateProp.GetDateTime();
                if (filtersProp.TryGetProperty("endDate", out var endDateProp) && endDateProp.ValueKind != JsonValueKind.Null)
                    filters.EndDate = endDateProp.GetDateTime();
                if (filtersProp.TryGetProperty("maxRecords", out var maxRecordsProp))
                    filters.MaxRecords = maxRecordsProp.GetInt32();
                if (filtersProp.TryGetProperty("sanitizeData", out var sanitizeProp))
                    filters.SanitizeData = sanitizeProp.GetBoolean();
            }

            var request = new ExportAuditLogsRequest
            {
                Format = format ?? "csv",
                Fields = fields,
                Filters = filters
            };

            _logger.LogInformation("Parsed export request: Format={Format}, FieldCount={FieldCount}, Action={Action}, TargetType={TargetType}", 
                request.Format, request.Fields?.Count ?? 0, request.Filters?.Action, request.Filters?.TargetType);

            // Validate request
            if (string.IsNullOrEmpty(request.Format))
            {
                return BadRequest(new { message = "Export format is required" });
            }

            // Validate fields if provided
            if (request.Fields?.Any() == true && !_exportService.ValidateFields(request.Fields))
            {
                var availableFields = _exportService.GetAvailableFields();
                var invalidFields = request.Fields.Where(f => !availableFields.Contains(f, StringComparer.OrdinalIgnoreCase)).ToList();
                _logger.LogWarning("Invalid export fields: {InvalidFields}. Available fields: {AvailableFields}", 
                    string.Join(", ", invalidFields), string.Join(", ", availableFields));
                return BadRequest(new { message = $"Invalid export fields: {string.Join(", ", invalidFields)}. Available fields: {string.Join(", ", availableFields)}" });
            }

            var supportedFormats = new[] { "csv", "excel", "pdf" };
            if (!supportedFormats.Contains(request.Format.ToLowerInvariant()))
            {
                return BadRequest(new { message = $"Unsupported format. Supported formats: {string.Join(", ", supportedFormats)}" });
            }

            // Get current user role for data sanitization
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "User";
            
            // Perform export
            var exportResult = await _auditService.ExportAuditLogsAsync(request, userRole);
            
            // Log export action
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(currentUserId, out var adminUserId))
            {
                await _auditService.LogUserActionAsync(
                    "AUDIT_LOGS_EXPORTED_ADVANCED",
                    "medium",
                    adminUserId,
                    $"Admin exported {exportResult.RecordCount} audit logs in {request.Format.ToUpper()} format with field selection",
                    "audit",
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    Request.Headers.UserAgent.ToString(),
                    exportResult.Metadata
                );
            }
            
            // Set response headers for immediate download
            Response.Headers.Append("Content-Disposition", $"attachment; filename=\"{exportResult.FileName}\"");
            Response.Headers.Append("X-Export-Record-Count", exportResult.RecordCount.ToString());
            Response.Headers.Append("X-Export-Generated-At", exportResult.GeneratedAt.ToString("yyyy-MM-ddTHH:mm:ssZ"));
            Response.Headers.Append("Access-Control-Expose-Headers", "Content-Disposition, X-Export-Record-Count, X-Export-Generated-At");
            Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
            Response.Headers.Append("Pragma", "no-cache");
            Response.Headers.Append("Expires", "0");
            
            return File(exportResult.Data, exportResult.ContentType, exportResult.FileName, enableRangeProcessing: false);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid export request: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export audit logs with advanced options");
            return StatusCode(500, new { message = "Export failed", error = ex.Message });
        }
    }

    private string MapActionToDatabase(string frontendAction)
    {
        if (string.IsNullOrEmpty(frontendAction))
            return frontendAction;

        return frontendAction.ToLowerInvariant() switch
        {
            // Frontend dropdown labels (exact match from image)
            "login" => "LOGIN_SUCCESS,LOGIN_FAILED",
            "logout" => "LOGOUT_SUCCESS", 
            "create" => "DONATION_CREATED,ORGANIZATION_CREATED,REPORT_POST,USER_CREATED",
            "edit" => "DONATION_UPDATED,ORGANIZATION_UPDATED,REPORT_PUT,USER_UPDATED",
            "delete" => "REPORT_DELETE,DONATION_DELETED,ORGANIZATION_DELETED",
            "suspend" => "USER_SUSPENDED,USER_DEACTIVATED",
            "reactivate" => "USER_REACTIVATED",
            "accessed audit logs" => "AUDIT_LOGS_ACCESSED,AUDIT_LOGS_EXPORTED_ADVANCED",
            "updated profile" => "PROFILE_UPDATED,USER_PROFILE_UPDATED",
            
            // Map to actual existing database actions
            "role assigned" => "ROLE_ASSIGNED",
            "role removed" => "ROLE_REMOVED",
            "roles updated" => "ROLES_UPDATED",
            "audit access" => "AUDIT_LOGS_ACCESSED",
            "export logs" => "AUDIT_LOGS_EXPORTED_ADVANCED",
            
            // Handle direct database values (backward compatibility)
            "user_suspended" => "USER_SUSPENDED",
            "user_deactivated" => "USER_SUSPENDED", // Map deactivated to suspended
            "user_reactivated" => "USER_REACTIVATED",
            "role_assigned" => "ROLE_ASSIGNED",
            "role_removed" => "ROLE_REMOVED", 
            "roles_updated" => "ROLES_UPDATED",
            "login_success" => "LOGIN_SUCCESS",
            "login_failed" => "LOGIN_FAILED",
            "audit_logs_accessed" => "AUDIT_LOGS_ACCESSED",
            "audit_logs_exported_advanced" => "AUDIT_LOGS_EXPORTED_ADVANCED",
            
            _ => frontendAction // Return as-is if no mapping found
        };
    }

    [HttpGet("filter-options")]
    [SuperAdminOrAdmin]
    public async Task<IActionResult> GetFilterOptions()
    {
        try
        {
            var filterOptions = await _auditService.GetFilterOptionsAsync();
            
            return Ok(filterOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve filter options");
            return StatusCode(500, new { message = "Failed to retrieve filter options" });
        }
    }

    private async Task<int> GetActiveUsersCountAsync()
    {
        try
        {
            // Get count of unique users who have logged actions in the last 24 hours
            var yesterday = DateTime.UtcNow.AddDays(-1);
            var filters = new AuditLogFiltersDto
            {
                DateFrom = yesterday,
                PageSize = 1000 // Get enough to count unique users
            };
            
            var result = await _auditService.GetLogsAsync(filters);
            return result.Logs
                .Where(log => log.User != null)
                .Select(log => log.User!.Id)
                .Distinct()
                .Count();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get active users count, returning 0");
            return 0;
        }
    }
}