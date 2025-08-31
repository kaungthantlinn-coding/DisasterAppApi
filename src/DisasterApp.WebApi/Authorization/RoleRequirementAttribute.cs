using Microsoft.AspNetCore.Authorization;

namespace DisasterApp.WebApi.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RoleRequirementAttribute : AuthorizeAttribute
{
    public RoleRequirementAttribute(params string[] roles)
    {
        Roles = string.Join(",", roles);
    }
}

// Specific role attributes for convenience
public class AdminOnlyAttribute : RoleRequirementAttribute
{
    public AdminOnlyAttribute() : base("admin") { }
}

public class CjOnlyAttribute : RoleRequirementAttribute
{
    public CjOnlyAttribute() : base("cj") { }
}

public class UserOnlyAttribute : RoleRequirementAttribute
{
    public UserOnlyAttribute() : base("user") { }
}

public class AdminOrCjAttribute : RoleRequirementAttribute
{
    public AdminOrCjAttribute() : base("admin", "cj") { }
}

public class SuperAdminOrAdminAttribute : RoleRequirementAttribute
{
    public SuperAdminOrAdminAttribute() : base("SuperAdmin", "admin") { }
}