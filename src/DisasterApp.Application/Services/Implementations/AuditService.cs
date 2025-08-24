using DisasterApp.Application.DTOs;
using DisasterApp.Application.Services.Interfaces;
using DisasterApp.Domain.Entities;
using DisasterApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text;
using ClosedXML.Excel;

namespace DisasterApp.Application.Services.Implementations;

public class AuditService : IAuditService
{
    private readonly DisasterDbContext _context;
    private readonly ILogger<AuditService> _logger;
    private readonly IAuditDataSanitizer _dataSanitizer;
    private readonly IExportService _exportService;

    public AuditService(
        DisasterDbContext context, 
        ILogger<AuditService> logger,
        IAuditDataSanitizer dataSanitizer,
        IExportService exportService)
    {
        _context = context;
        _logger = logger;
        _dataSanitizer = dataSanitizer;
        _exportService = exportService;
    }

    public async Task LogRoleAssignmentAsync(Guid userId, string roleName, Guid? performedByUserId, string? performedByUserName, string? ipAddress, string? userAgent)
    {
        try
        {
            var auditLog = new AuditLog
            {
                AuditLogId = Guid.NewGuid(),
                Action = "ROLE_ASSIGNED",
                Severity = "info",
                EntityType = "UserRole",
                EntityId = userId.ToString(),
                Details = $"Role '{roleName}' assigned to user",
                OldValues = null,
                NewValues = JsonSerializer.Serialize(new { RoleName = roleName }),
                UserId = performedByUserId,
                UserName = performedByUserName,
                Timestamp = DateTime.UtcNow,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Resource = "user_management",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.AuditLogs.AddAsync(auditLog);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Logged role assignment: User {UserId} assigned role {RoleName} by {PerformedBy}", 
                userId, roleName, performedByUserName ?? "System");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log role assignment for user {UserId}", userId);
        }
    }

    public async Task LogRoleRemovalAsync(Guid userId, string roleName, Guid? performedByUserId, string? performedByUserName, string? ipAddress, string? userAgent)
    {
        try
        {
            var auditLog = new AuditLog
            {
                AuditLogId = Guid.NewGuid(),
                Action = "ROLE_REMOVED",
                Severity = "info",
                EntityType = "UserRole",
                EntityId = userId.ToString(),
                Details = $"Role '{roleName}' removed from user",
                OldValues = JsonSerializer.Serialize(new { RoleName = roleName }),
                NewValues = null,
                UserId = performedByUserId,
                UserName = performedByUserName,
                Timestamp = DateTime.UtcNow,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Resource = "user_management",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.AuditLogs.AddAsync(auditLog);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Logged role removal: User {UserId} removed role {RoleName} by {PerformedBy}", 
                userId, roleName, performedByUserName ?? "System");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log role removal for user {UserId}", userId);
        }
    }

    public async Task LogRoleUpdateAsync(Guid userId, List<string> oldRoles, List<string> newRoles, Guid? performedByUserId, string? performedByUserName, string? ipAddress, string? userAgent, string? reason = null)
    {
        try
        {
            var details = $"User roles updated from [{string.Join(", ", oldRoles)}] to [{string.Join(", ", newRoles)}]";
            if (!string.IsNullOrEmpty(reason))
            {
                details += $". Reason: {reason}";
            }
            
            var auditLog = new AuditLog
            {
                AuditLogId = Guid.NewGuid(),
                Action = "ROLES_UPDATED",
                Severity = "info",
                EntityType = "UserRole",
                EntityId = userId.ToString(),
                Details = details,
                OldValues = JsonSerializer.Serialize(new { Roles = oldRoles }),
                NewValues = JsonSerializer.Serialize(new { Roles = newRoles }),
                UserId = performedByUserId,
                UserName = performedByUserName,
                Timestamp = DateTime.UtcNow,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Resource = "user_management",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.AuditLogs.AddAsync(auditLog);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Logged role update: User {UserId} roles changed from [{OldRoles}] to [{NewRoles}] by {PerformedBy}", 
                userId, string.Join(", ", oldRoles), string.Join(", ", newRoles), performedByUserName ?? "System");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log role update for user {UserId}", userId);
        }
    }

    public async Task<(List<AuditLog> logs, int totalCount)> GetUserAuditLogsAsync(Guid userId, int pageNumber = 1, int pageSize = 50)
    {
        try
        {
            var query = _context.AuditLogs
                .Where(a => a.EntityId == userId.ToString() && a.EntityType == "UserRole")
                .OrderByDescending(a => a.Timestamp);

            var totalCount = await query.CountAsync();
            var logs = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (logs, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get audit logs for user {UserId}", userId);
            throw;
        }
    }

    public async Task<(List<AuditLog> logs, int totalCount)> GetRoleAuditLogsAsync(int pageNumber = 1, int pageSize = 50)
    {
        try
        {
            var query = _context.AuditLogs
                .Where(a => a.EntityType == "UserRole")
                .OrderByDescending(a => a.Timestamp);

            var totalCount = await query.CountAsync();
            var logs = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (logs, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get role audit logs");
            throw;
        }
    }

    public async Task<AuditLog> CreateLogAsync(CreateAuditLogDto data)
    {
        try
        {
            var auditLog = new AuditLog
            {
                AuditLogId = Guid.NewGuid(),
                Action = data.Action,
                Severity = data.Severity,
                EntityType = data.EntityType ?? "General",
                EntityId = data.EntityId,
                Details = data.Details,
                OldValues = data.OldValues,
                NewValues = data.NewValues,
                UserId = data.UserId,
                UserName = data.UserId.HasValue ? await GetUserNameAsync(data.UserId.Value) : null,
                Timestamp = DateTime.UtcNow,
                IpAddress = data.IpAddress,
                UserAgent = data.UserAgent,
                Resource = data.Resource,
                Metadata = data.Metadata != null ? JsonSerializer.Serialize(data.Metadata) : null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.AuditLogs.AddAsync(auditLog);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created audit log: {Action} for resource {Resource}", data.Action, data.Resource);
            return auditLog;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create audit log for action {Action}", data.Action);
            throw;
        }
    }

    public async Task<PaginatedAuditLogsDto> GetLogsAsync(AuditLogFiltersDto filters)
    {
        try
        {
            _logger.LogInformation("GetLogsAsync called with filters: {Filters}", System.Text.Json.JsonSerializer.Serialize(filters));
            
            // Use AsNoTracking for better performance on read-only queries
            var query = _context.AuditLogs.AsNoTracking()
                .Include(a => a.User)
                .AsQueryable();

            var originalCount = await query.CountAsync();
            _logger.LogInformation("Total audit logs in database: {Count}", originalCount);

            // Apply filters in order of selectivity (most selective first)
            if (!string.IsNullOrEmpty(filters.UserId) && Guid.TryParse(filters.UserId, out var userId))
            {
                query = query.Where(a => a.UserId == userId);
                var countAfterUserId = await query.CountAsync();
                _logger.LogInformation("Records after UserId filter: {Count}", countAfterUserId);
            }

            if (filters.DateFrom.HasValue)
            {
                query = query.Where(a => a.Timestamp >= filters.DateFrom.Value);
                var countAfterDateFrom = await query.CountAsync();
                _logger.LogInformation("Records after DateFrom filter: {Count}", countAfterDateFrom);
            }

            if (filters.DateTo.HasValue)
            {
                query = query.Where(a => a.Timestamp <= filters.DateTo.Value);
                var countAfterDateTo = await query.CountAsync();
                _logger.LogInformation("Records after DateTo filter: {Count}", countAfterDateTo);
            }

            if (!string.IsNullOrEmpty(filters.Severity))
            {
                query = query.Where(a => a.Severity == filters.Severity);
                var countAfterSeverity = await query.CountAsync();
                _logger.LogInformation("Records after Severity filter: {Count}", countAfterSeverity);
            }

            if (!string.IsNullOrEmpty(filters.Action))
            {
                var actions = filters.Action.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(a => a.Trim().ToUpperInvariant()).ToList();
                query = query.Where(a => actions.Contains(a.Action.ToUpper()));
            }

            if (!string.IsNullOrEmpty(filters.TargetType))
            {
                var targetTypes = filters.TargetType.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim().ToLower()).ToList();

                if (targetTypes.Any())
                {
                    query = query.Where(a => a.EntityType != null && targetTypes.Contains(a.EntityType.ToLower()));
                }
            }

            if (!string.IsNullOrEmpty(filters.Resource))
            {
                query = query.Where(a => a.Resource == filters.Resource);
                var countAfterResource = await query.CountAsync();
                _logger.LogInformation("Records after Resource filter: {Count}", countAfterResource);
            }

            // Apply text search last as it's the most expensive
            if (!string.IsNullOrEmpty(filters.Search))
            {
                query = query.Where(a => a.Details.Contains(filters.Search) || 
                                        a.Action.Contains(filters.Search) ||
                                        (a.UserName != null && a.UserName.Contains(filters.Search)));
            }

            // Order by timestamp descending (using index)
            query = query.OrderByDescending(a => a.Timestamp);

            // Get total count with timeout handling
            var totalCount = 0;
            try
            {
                totalCount = await query.CountAsync();
            }
            catch (Exception countEx)
            {
                _logger.LogWarning(countEx, "Failed to get exact count, using estimated count");
                // Fallback to a reasonable estimate if count times out
                totalCount = filters.PageSize * 10; // Estimate for pagination
            }

            // Get paginated results with explicit timeout and include User data
            var logs = await query
                .Skip((filters.Page - 1) * filters.PageSize)
                .Take(filters.PageSize)
                .Select(a => new
                {
                    a.AuditLogId,
                    a.Timestamp,
                    a.Action,
                    a.Severity,
                    a.Details,
                    a.IpAddress,
                    a.UserAgent,
                    a.Resource,
                    a.Metadata,
                    a.UserId,
                    a.UserName,
                    User = a.User != null ? new { a.User.UserId, a.User.Name, a.User.Email } : null
                })
                .ToListAsync();

            var auditLogDtos = logs.Select(log => new AuditLogDto
            {
                Id = log.AuditLogId.ToString(),
                Timestamp = log.Timestamp,
                Action = log.Action,
                Severity = log.Severity,
                User = log.User != null ? new AuditLogUserDto
                {
                    Id = log.User.UserId.ToString(),
                    Name = log.User.Name,
                    Email = log.User.Email
                } : (log.UserName != null ? new AuditLogUserDto
                {
                    Id = log.UserId?.ToString() ?? "",
                    Name = log.UserName,
                    Email = ""
                } : null),
                Details = log.Details,
                IpAddress = log.IpAddress,
                UserAgent = log.UserAgent,
                Resource = log.Resource,
                Metadata = !string.IsNullOrEmpty(log.Metadata) ? 
                    JsonSerializer.Deserialize<Dictionary<string, object>>(log.Metadata) : null
            }).ToList();

            return new PaginatedAuditLogsDto
            {
                Logs = auditLogDtos,
                TotalCount = totalCount,
                Page = filters.Page,
                PageSize = filters.PageSize,
                HasMore = (filters.Page * filters.PageSize) < totalCount
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get audit logs with filters");
            throw;
        }
    }

    public async Task<AuditLogStatsDto> GetStatisticsAsync()
    {
        try
        {
            var now = DateTime.UtcNow;
            var last24Hours = now.AddHours(-24);
            var last7Days = now.AddDays(-7);

            var totalLogs = await _context.AuditLogs.CountAsync();
            var criticalAlerts = await _context.AuditLogs.CountAsync(a => a.Severity == "critical");
            var securityEvents = await _context.AuditLogs.CountAsync(a => 
                a.Action.Contains("LOGIN") || a.Action.Contains("LOGOUT") || 
                a.Action.Contains("FAILED_LOGIN") || a.Action.Contains("SECURITY"));
            var systemErrors = await _context.AuditLogs.CountAsync(a => a.Severity == "error");
            var recentActivity = await _context.AuditLogs.CountAsync(a => a.Timestamp >= last24Hours);

            return new AuditLogStatsDto
            {
                TotalLogs = totalLogs,
                CriticalAlerts = criticalAlerts,
                SecurityEvents = securityEvents,
                SystemErrors = systemErrors,
                RecentActivity = recentActivity
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get audit log statistics");
            throw;
        }
    }

    public async Task<byte[]> ExportLogsAsync(string format, AuditLogFiltersDto filters)
    {
        try
        {
            _logger.LogInformation("Starting export with filters: {Filters}", System.Text.Json.JsonSerializer.Serialize(filters));
            
            var logsResult = await GetLogsAsync(new AuditLogFiltersDto
            {
                Page = 1,
                PageSize = int.MaxValue, // Get all logs for export
                Search = filters.Search,
                Severity = filters.Severity,
                Action = filters.Action,
                TargetType = filters.TargetType, // Fix: Include TargetType filter
                DateFrom = filters.DateFrom,
                DateTo = filters.DateTo,
                UserId = filters.UserId,
                Resource = filters.Resource
            });
            
            _logger.LogInformation("Export retrieved {LogCount} logs for format {Format}", logsResult.Logs.Count, format);

            if (format.ToLower() == "csv")
            {
                return GenerateCsvExport(logsResult.Logs);
            }
            else if (format.ToLower() == "excel")
            {
                return GenerateExcelExport(logsResult.Logs);
            }
            else
            {
                throw new ArgumentException("Unsupported export format. Use 'csv' or 'excel'.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export audit logs in format {Format}", format);
            throw;
        }
    }

    public async Task LogUserActionAsync(string action, string severity, Guid? userId, string details, string resource, string? ipAddress = null, string? userAgent = null, Dictionary<string, object>? metadata = null)
    {
        // Determine EntityType based on resource and action
        var entityType = DetermineEntityType(resource, action, metadata);
        
        await CreateLogAsync(new CreateAuditLogDto
        {
            Action = action,
            Severity = severity,
            UserId = userId,
            Details = details,
            Resource = resource,
            EntityType = entityType,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Metadata = metadata
        });
    }

    public async Task LogSystemEventAsync(string action, string severity, string details, string resource, Dictionary<string, object>? metadata = null)
    {
        await CreateLogAsync(new CreateAuditLogDto
        {
            Action = action,
            Severity = severity,
            Details = details,
            Resource = resource,
            EntityType = "System",
            Metadata = metadata
        });
    }

    public async Task LogSecurityEventAsync(string action, string details, Guid? userId = null, string? ipAddress = null, string? userAgent = null, Dictionary<string, object>? metadata = null)
    {
        await CreateLogAsync(new CreateAuditLogDto
        {
            Action = action,
            Severity = "warning",
            UserId = userId,
            Details = details,
            Resource = "security",
            EntityType = "Security",
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Metadata = metadata
        });
    }

    public async Task LogErrorAsync(string action, string details, Exception? exception = null, Guid? userId = null, string? resource = null)
    {
        var metadata = new Dictionary<string, object>();
        if (exception != null)
        {
            metadata["exception"] = new
            {
                Message = exception.Message,
                StackTrace = exception.StackTrace,
                Type = exception.GetType().Name
            };
        }

        await CreateLogAsync(new CreateAuditLogDto
        {
            Action = action,
            Severity = "error",
            UserId = userId,
            Details = details,
            Resource = resource ?? "system",
            EntityType = "Error",
            Metadata = metadata
        });
    }

    private async Task<string?> GetUserNameAsync(Guid userId)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            return user?.Name;
        }
        catch
        {
            return null;
        }
    }

    private byte[] GenerateCsvExport(List<AuditLogDto> logs)
    {
        var csv = new StringBuilder();
        csv.AppendLine("Timestamp,Action,Severity,User,Details,IP Address,Resource");

        foreach (var log in logs)
        {
            csv.AppendLine($"{log.Timestamp:yyyy-MM-dd HH:mm:ss},{log.Action},{log.Severity},{log.User?.Name ?? "System"},{EscapeCsvField(log.Details)},{log.IpAddress},{log.Resource}");
        }

        return Encoding.UTF8.GetBytes(csv.ToString());
    }

    private byte[] GenerateExcelExport(List<AuditLogDto> logs)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Audit Logs");

        // Headers
        worksheet.Cell(1, 1).Value = "Timestamp";
        worksheet.Cell(1, 2).Value = "Action";
        worksheet.Cell(1, 3).Value = "Severity";
        worksheet.Cell(1, 4).Value = "User";
        worksheet.Cell(1, 5).Value = "Details";
        worksheet.Cell(1, 6).Value = "IP Address";
        worksheet.Cell(1, 7).Value = "Resource";

        // Data
        for (int i = 0; i < logs.Count; i++)
        {
            var log = logs[i];
            var row = i + 2;
            worksheet.Cell(row, 1).Value = log.Timestamp;
            worksheet.Cell(row, 2).Value = log.Action;
            worksheet.Cell(row, 3).Value = log.Severity;
            worksheet.Cell(row, 4).Value = log.User?.Name ?? "System";
            worksheet.Cell(row, 5).Value = log.Details;
            worksheet.Cell(row, 6).Value = log.IpAddress;
            worksheet.Cell(row, 7).Value = log.Resource;
        }

        worksheet.ColumnsUsed().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public async Task<ExportResult> ExportAuditLogsAsync(ExportAuditLogsRequest request, string? userRole = null)
    {
        try
        {
            _logger.LogInformation("Starting export with format: {Format}, Fields: {Fields}", 
                request.Format, request.Fields?.Count ?? 0);

            // Handle null filters
            var requestFilters = request.Filters ?? new ExportAuditLogFilters();

            // Convert request filters to internal DTO
            var filters = new AuditLogFiltersDto
            {
                Page = 1,
                PageSize = requestFilters.MaxRecords,
                Search = requestFilters.Search,
                Severity = requestFilters.Severity,
                Action = requestFilters.Action,
                TargetType = requestFilters.TargetType,
                UserId = requestFilters.UserId,
                Resource = requestFilters.Resource,
                DateFrom = requestFilters.StartDate,
                DateTo = requestFilters.EndDate,
                StartDate = requestFilters.StartDate,
                EndDate = requestFilters.EndDate
            };

            // Only use fallback if NO filters are specified at all
            bool hasAnyFilters = !string.IsNullOrEmpty(filters.Search) || 
                                !string.IsNullOrEmpty(filters.Severity) || 
                                !string.IsNullOrEmpty(filters.Action) || 
                                !string.IsNullOrEmpty(filters.TargetType) ||
                                !string.IsNullOrEmpty(filters.UserId) || 
                                !string.IsNullOrEmpty(filters.Resource) ||
                                filters.DateFrom.HasValue || 
                                filters.DateTo.HasValue;

            if (!hasAnyFilters)
            {
                // Only apply fallback when no filters are specified
                _logger.LogInformation("No filters specified, using default date range for export");
                filters.DateFrom = DateTime.UtcNow.AddDays(-30);
            }

            // Get filtered audit logs
            var result = await GetLogsAsync(filters);
            var logs = result.Logs;
            
            _logger.LogInformation("Export retrieved {LogCount} logs with filters: {Filters}", 
                logs.Count, System.Text.Json.JsonSerializer.Serialize(filters));

            // Apply data sanitization if requested
            if (request.Filters.SanitizeData && !string.IsNullOrEmpty(userRole))
            {
                logs = logs.Select(log => SanitizeAuditLogForRole(log, userRole)).ToList();
            }

            // Use default fields if none specified
            var fieldsToExport = request.Fields?.Any() == true ? request.Fields : GetDefaultExportFields();

            // Validate fields
            if (!_exportService.ValidateFields(fieldsToExport))
            {
                throw new ArgumentException("Invalid export fields specified");
            }

            // Generate export based on format
            byte[] exportData = request.Format.ToLowerInvariant() switch
            {
                "csv" => await _exportService.ExportToCsvAsync(logs, fieldsToExport),
                "excel" => await _exportService.ExportToExcelAsync(logs, fieldsToExport),
                "pdf" => await _exportService.ExportToPdfAsync(logs, fieldsToExport),
                _ => throw new ArgumentException($"Unsupported export format: {request.Format}")
            };

            var fileName = GenerateExportFileName(request.Format, request.Filters);
            var contentType = _exportService.GetMimeType(request.Format);

            return new ExportResult
            {
                Data = exportData,
                FileName = fileName,
                ContentType = contentType,
                RecordCount = logs.Count,
                GeneratedAt = DateTime.UtcNow,
                Metadata = new Dictionary<string, object>
                {
                    ["format"] = request.Format,
                    ["fields"] = fieldsToExport,
                    ["filters"] = request.Filters,
                    ["sanitized"] = request.Filters.SanitizeData,
                    ["userRole"] = userRole ?? "Unknown"
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export audit logs with format {Format}", request.Format);
            throw;
        }
    }

    private AuditLogDto SanitizeAuditLogForRole(AuditLogDto log, string userRole)
    {
        return new AuditLogDto
        {
            Id = log.Id,
            Timestamp = log.Timestamp,
            Action = log.Action,
            Severity = log.Severity,
            User = log.User,
            Details = _dataSanitizer.SanitizeForRole(log.Details, userRole),
            IpAddress = _dataSanitizer.RedactIpAddresses(log.IpAddress ?? "", userRole),
            UserAgent = _dataSanitizer.HasSensitiveDataAccess(userRole) ? log.UserAgent : "[REDACTED]",
            Resource = log.Resource,
            Metadata = _dataSanitizer.SanitizeMetadata(log.Metadata, userRole)
        };
    }

    private List<string> GetDefaultExportFields()
    {
        return new List<string>
        {
            "Timestamp", "Action", "Severity", "UserName", "Details", "IpAddress", "Resource"
        };
    }

    private string GenerateExportFileName(string format, ExportAuditLogFilters filters)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss");
        var dateRange = "";
        
        if (filters.StartDate.HasValue || filters.EndDate.HasValue)
        {
            var start = filters.StartDate?.ToString("yyyy-MM-dd") ?? "start";
            var end = filters.EndDate?.ToString("yyyy-MM-dd") ?? "end";
            dateRange = $"-{start}-to-{end}";
        }

        var extension = _exportService.GetFileExtension(format);
        return $"audit-logs{dateRange}-{timestamp}.{extension}";
    }

    private string EscapeCsvField(string field)
    {
        if (string.IsNullOrEmpty(field)) return "";
        if (field.Contains(',') || field.Contains('"') || field.Contains('\n'))
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }
        return field;
    }

    private string DetermineEntityType(string resource, string action, Dictionary<string, object>? metadata)
    {
        // Check metadata first for explicit targetType
        if (metadata?.ContainsKey("targetType") == true)
        {
            var targetType = metadata["targetType"]?.ToString();
            if (!string.IsNullOrEmpty(targetType))
            {
                return targetType;
            }
        }

        // Determine by resource
        return resource?.ToLowerInvariant() switch
        {
            "donations" => "Donation",
            "organizations" => "Organization", 
            "reports" => "Report",
            "users" => "User",
            "user_management" => "User",
            "security" => "Security",
            "system" => "System",
            "audit" => "Audit",
            _ when action?.Contains("ROLE") == true => "UserRole",
            _ when action?.Contains("USER") == true => "User",
            _ when action?.Contains("ORGANIZATION") == true => "Organization",
            _ when action?.Contains("DONATION") == true => "Donation",
            _ when action?.Contains("REPORT") == true => "Report",
            _ => "General"
        };
    }

    public async Task<FilterOptionsDto> GetFilterOptionsAsync()
    {
        try
        {
            var actions = await _context.AuditLogs
                .Where(a => !string.IsNullOrEmpty(a.Action))
                .Select(a => a.Action)
                .Distinct()
                .OrderBy(a => a)
                .ToListAsync();

            var targetTypes = await _context.AuditLogs
                .Where(a => !string.IsNullOrEmpty(a.EntityType))
                .Select(a => a.EntityType)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();

            return new FilterOptionsDto
            {
                Actions = actions,
                TargetTypes = targetTypes
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving filter options");
            return new FilterOptionsDto();
        }
    }
}
