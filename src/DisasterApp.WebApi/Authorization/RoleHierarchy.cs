namespace DisasterApp.WebApi.Authorization;

/// <summary>
/// Simple role-based authorization helper for role hierarchy
/// </summary>
public static class RoleHierarchy
{
    private static readonly Dictionary<string, int> _hierarchy = new()
    {
        { "User", 1 },
        { "Manager", 2 },
        { "Admin", 3 },
        { "SuperAdmin", 4 }
    };

    /// <summary>
    /// Check if user role has access to required role level
    /// </summary>
    /// <param name="userRole">User's current role</param>
    /// <param name="requiredRole">Required role for access</param>
    /// <returns>True if user has access, false otherwise</returns>
    public static bool HasAccess(string userRole, string requiredRole)
    {
        return _hierarchy.GetValueOrDefault(userRole, 0) >= 
               _hierarchy.GetValueOrDefault(requiredRole, 0);
    }

    /// <summary>
    /// Get role level for a given role name
    /// </summary>
    /// <param name="roleName">Role name</param>
    /// <returns>Role level (higher number = higher privilege)</returns>
    public static int GetRoleLevel(string roleName)
    {
        return _hierarchy.GetValueOrDefault(roleName, 0);
    }

    /// <summary>
    /// Check if role is a system role
    /// </summary>
    /// <param name="roleName">Role name</param>
    /// <returns>True if system role, false otherwise</returns>
    public static bool IsSystemRole(string roleName)
    {
        return roleName is "SuperAdmin" or "Admin";
    }
}
