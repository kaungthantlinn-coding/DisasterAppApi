using DisasterApp.Application.Services.Interfaces;
using DisasterApp.Application.DTOs;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace DisasterApp.WebApi.Middleware
{
    public class AuditLogMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuditLogMiddleware> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        // Actions that should be logged
        private readonly HashSet<string> _loggedActions = new()
        {
            "POST", "PUT", "DELETE", "PATCH"
        };

        // Paths that should be excluded from logging
        private readonly HashSet<string> _excludedPaths = new()
        {
            "/api/auth/refresh",
            "/api/health",
            "/api/audit-logs" // Avoid recursive logging
        };

        public AuditLogMiddleware(RequestDelegate next, ILogger<AuditLogMiddleware> logger, IServiceScopeFactory serviceScopeFactory)
        {
<<<<<<< HEAD
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
=======
            _next = next;
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
>>>>>>> f5beea9ebc339905ce51c40a6f67a3ce32e9d8be
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var originalBodyStream = context.Response.Body;
            
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            var startTime = DateTime.UtcNow;
            var requestBody = await GetRequestBodyAsync(context.Request);

            try
            {
                await _next(context);

                // Log successful actions
                if (ShouldLogAction(context))
                {
                    await LogActionAsync(context, requestBody, startTime, null);
                }
            }
            catch (Exception ex)
            {
                // Log failed actions
                if (ShouldLogAction(context))
                {
                    await LogActionAsync(context, requestBody, startTime, ex);
                }
                throw;
            }
            finally
            {
                responseBody.Seek(0, SeekOrigin.Begin);
                await responseBody.CopyToAsync(originalBodyStream);
            }
        }

        private bool ShouldLogAction(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower() ?? "";
            var method = context.Request.Method;

            // Skip excluded paths
            if (_excludedPaths.Any(excluded => path.StartsWith(excluded)))
                return false;

            // Log all methods for admin endpoints
            if (path.StartsWith("/api/admin"))
                return true;

            // Log specific HTTP methods
            return _loggedActions.Contains(method);
        }

        private async Task<string> GetRequestBodyAsync(HttpRequest request)
        {
            if (!request.HasFormContentType && request.ContentLength > 0)
            {
                request.EnableBuffering();
                var buffer = new byte[Convert.ToInt32(request.ContentLength)];
                await request.Body.ReadAsync(buffer, 0, buffer.Length);
                request.Body.Position = 0;
                return Encoding.UTF8.GetString(buffer);
            }
            return string.Empty;
        }

        private async Task LogActionAsync(HttpContext context, string requestBody, DateTime startTime, Exception? exception)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var auditService = scope.ServiceProvider.GetRequiredService<IAuditService>();

                var user = context.User;
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userName = user.FindFirst(ClaimTypes.Name)?.Value ?? "Anonymous";

                var action = GetActionName(context);
                var severity = exception != null ? "error" : "info";
                var details = GetActionDetails(context, requestBody, exception);
                var resource = GetResourceName(context);
                var metadata = GetMetadata(context, startTime);

                var createDto = new CreateAuditLogDto
                {
                    Action = action,
                    Severity = severity,
                    UserId = userId != null ? Guid.Parse(userId) : null,
<<<<<<< HEAD
                    UserName = userName,
=======
>>>>>>> f5beea9ebc339905ce51c40a6f67a3ce32e9d8be
                    Details = details,
                    IpAddress = GetClientIpAddress(context),
                    UserAgent = context.Request.Headers.UserAgent.ToString(),
                    Resource = resource,
                    Metadata = metadata,
                    EntityType = "HttpRequest",
                    EntityId = userId ?? "system" // Provide default value to avoid NULL constraint error
                };

                await auditService.CreateLogAsync(createDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create audit log entry");
            }
        }

        private string GetActionName(HttpContext context)
        {
            var method = context.Request.Method;
            var path = context.Request.Path.Value ?? "";

            // Extract meaningful action names
            if (path.Contains("/auth/login")) return "USER_LOGIN";
            if (path.Contains("/auth/logout")) return "USER_LOGOUT";
            if (path.Contains("/auth/register")) return "USER_REGISTER";
<<<<<<< HEAD
            
            // User Management endpoints
            if (path.Contains("/UserManagement") || path.Contains("/users"))
            {
                return method switch
                {
                    "POST" => "USER_CREATE",
                    "PUT" => path.Contains("/roles") ? "USER_ROLES_UPDATE" : "USER_UPDATE",
                    "DELETE" => "USER_DELETE",
                    "GET" => "USER_ACCESS",
                    _ => $"USER_{method}"
                };
            }
            
            // Role Management endpoints
            if (path.Contains("/RoleManagement") || (path.Contains("/api/Role") && !path.Contains("/api/RoleUser")))
            {
                return method switch
                {
                    "POST" => "ROLE_CREATE",
                    "PUT" => "ROLE_UPDATE",
                    "DELETE" => "ROLE_DELETE",
                    "GET" => "ROLE_ACCESS",
                    _ => $"ROLE_{method}"
                };
            }
            
            // Role assignment endpoints
            if (path.Contains("/Role/assign")) return "ROLE_ASSIGN";
            if (path.Contains("/Role/remove")) return "ROLE_REMOVE";
            if (path.Contains("/roles/assign")) return "ROLE_ASSIGN";
            if (path.Contains("/roles/remove")) return "ROLE_REMOVE";
            
            // Generic patterns
            if (path.Contains("/reports")) return $"REPORT_{method}";
            if (path.Contains("/admin/settings")) return "SYSTEM_SETTINGS_UPDATE";
            if (path.Contains("/admin")) return $"ADMIN_{method}";
            if (path.Contains("/audit-logs")) return $"AUDIT_{method}";
            if (path.Contains("/diagnostics")) return $"DIAGNOSTICS_{method}";

            // Extract controller name for better action names
            var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length >= 2 && segments[0] == "api")
            {
                var controller = segments[1].ToUpperInvariant();
                // Special handling for DELETE methods to make them more descriptive
                if (method == "DELETE")
                {
                    return $"{controller}_DELETE";
                }
                return $"{method}_{controller}";
            }

            // Default to method + generic for DELETE operations
            if (method == "DELETE")
            {
                return "RESOURCE_DELETED";
            }

            return $"{method}_UNKNOWN";
=======
            if (path.Contains("/users") && method == "POST") return "USER_CREATE";
            if (path.Contains("/users") && method == "PUT") return "USER_UPDATE";
            if (path.Contains("/users") && method == "DELETE") return "USER_DELETE";
            if (path.Contains("/roles")) return $"ROLE_{method}";
            if (path.Contains("/reports")) return $"REPORT_{method}";
            if (path.Contains("/admin/settings")) return "SYSTEM_SETTINGS_UPDATE";
            if (path.Contains("/admin")) return $"ADMIN_{method}";

            return $"{method}_{path.Split('/').LastOrDefault()?.ToUpper() ?? "UNKNOWN"}";
>>>>>>> f5beea9ebc339905ce51c40a6f67a3ce32e9d8be
        }

        private string GetActionDetails(HttpContext context, string requestBody, Exception? exception)
        {
            var method = context.Request.Method;
            var path = context.Request.Path.Value ?? "";
            var statusCode = context.Response.StatusCode;

            if (exception != null)
            {
                return $"{method} {path} failed with error: {exception.Message}";
            }

            var details = $"{method} {path} completed successfully (Status: {statusCode})";
            
            // Add request body for certain operations (excluding sensitive data)
            if (!string.IsNullOrEmpty(requestBody) && !path.Contains("/auth/"))
            {
                var truncatedBody = requestBody.Length > 500 ? requestBody.Substring(0, 500) + "..." : requestBody;
                details += $" | Request: {truncatedBody}";
            }

            return details;
        }

        private string GetResourceName(HttpContext context)
        {
            var path = context.Request.Path.Value ?? "";
            
            if (path.Contains("/auth")) return "authentication";
            if (path.Contains("/users")) return "user_management";
            if (path.Contains("/roles")) return "role_management";
            if (path.Contains("/reports")) return "reports";
            if (path.Contains("/admin")) return "admin";
            if (path.Contains("/disasters")) return "disasters";
            
            return "api";
        }

        private Dictionary<string, object> GetMetadata(HttpContext context, DateTime startTime)
        {
            var duration = DateTime.UtcNow - startTime;
            
            return new Dictionary<string, object>
            {
                ["method"] = context.Request.Method,
                ["path"] = context.Request.Path.Value ?? "",
                ["statusCode"] = context.Response.StatusCode,
                ["duration"] = duration.TotalMilliseconds,
                ["contentType"] = context.Request.ContentType ?? "",
                ["contentLength"] = context.Request.ContentLength ?? 0,
                ["referer"] = context.Request.Headers.Referer.ToString(),
                ["timestamp"] = startTime.ToString("O")
            };
        }

        private string GetClientIpAddress(HttpContext context)
        {
            // Check for forwarded IP first
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',')[0].Trim();
            }

            var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIp))
            {
                return realIp;
            }

            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }
    }

    // Extension method to register the middleware
    public static class AuditLogMiddlewareExtensions
    {
        public static IApplicationBuilder UseAuditLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuditLogMiddleware>();
        }
    }
}