namespace DisasterApp.WebApi.Authorization;

// Role Hierarchy
public static class RoleHierarchy
{
    private static readonly Dictionary<string, int> _hierarchy = new()
    {
        { "User", 1 },
        { "CJ", 2 },
        { "Admin", 3 },
        { "SuperAdmin", 4 }
    };

    // Check if user has access to a specific role
    public static bool HasAccess(string userRole, string requiredRole)
    {
        return _hierarchy.GetValueOrDefault(userRole, 0) >= 
               _hierarchy.GetValueOrDefault(requiredRole, 0);
    }

    // Get role level
    public static int GetRoleLevel(string roleName)
    {
        return _hierarchy.GetValueOrDefault(roleName, 0);
    }

   // Check if role is a system role
    public static bool IsSystemRole(string roleName)
    {
        return roleName is "SuperAdmin" or "Admin";
    }
}
