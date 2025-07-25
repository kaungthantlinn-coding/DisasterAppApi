using DisasterApp.Domain.Entities;

namespace DisasterApp.Application.Services.Interfaces;

public interface IAuditService
{
    /// <summary>
    /// Log a role assignment action
    /// </summary>
    /// <param name="userId">User ID who received the role</param>
    /// <param name="roleName">Role name that was assigned</param>
    /// <param name="performedByUserId">User ID who performed the action</param>
    /// <param name="performedByUserName">User name who performed the action</param>
    /// <param name="ipAddress">IP address of the request</param>
    /// <param name="userAgent">User agent of the request</param>
    Task LogRoleAssignmentAsync(Guid userId, string roleName, Guid? performedByUserId, string? performedByUserName, string? ipAddress, string? userAgent);

    /// <summary>
    /// Log a role removal action
    /// </summary>
    /// <param name="userId">User ID who lost the role</param>
    /// <param name="roleName">Role name that was removed</param>
    /// <param name="performedByUserId">User ID who performed the action</param>
    /// <param name="performedByUserName">User name who performed the action</param>
    /// <param name="ipAddress">IP address of the request</param>
    /// <param name="userAgent">User agent of the request</param>
    Task LogRoleRemovalAsync(Guid userId, string roleName, Guid? performedByUserId, string? performedByUserName, string? ipAddress, string? userAgent);

    /// <summary>
    /// Log a role update action (multiple roles changed)
    /// </summary>
    /// <param name="userId">User ID whose roles were updated</param>
    /// <param name="oldRoles">Previous roles</param>
    /// <param name="newRoles">New roles</param>
    /// <param name="performedByUserId">User ID who performed the action</param>
    /// <param name="performedByUserName">User name who performed the action</param>
    /// <param name="ipAddress">IP address of the request</param>
    /// <param name="userAgent">User agent of the request</param>
    Task LogRoleUpdateAsync(Guid userId, List<string> oldRoles, List<string> newRoles, Guid? performedByUserId, string? performedByUserName, string? ipAddress, string? userAgent);

    /// <summary>
    /// Get audit logs for a specific user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="pageNumber">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>Paginated audit logs</returns>
    Task<(List<AuditLog> logs, int totalCount)> GetUserAuditLogsAsync(Guid userId, int pageNumber = 1, int pageSize = 50);

    /// <summary>
    /// Get all role-related audit logs
    /// </summary>
    /// <param name="pageNumber">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>Paginated audit logs</returns>
    Task<(List<AuditLog> logs, int totalCount)> GetRoleAuditLogsAsync(int pageNumber = 1, int pageSize = 50);
}
