namespace DisasterApp.WebApi.Authorization;

public static class RoleHierarchy
{
    private static readonly Dictionary<string, int> _hierarchy = new()
    {
        ["User"] = 1,
        ["CJ"] = 2,
        ["Admin"] = 3,
        ["SuperAdmin"] = 4
    };

    public static bool HasAccess(string userRole, string requiredRole) =>
        GetRoleLevel(userRole) >= GetRoleLevel(requiredRole);

    public static int GetRoleLevel(string roleName) =>
        _hierarchy.GetValueOrDefault(roleName, 0);

    public static bool IsSystemRole(string roleName) =>
        roleName is "SuperAdmin" or "Admin";
}
