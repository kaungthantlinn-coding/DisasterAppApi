using System.ComponentModel.DataAnnotations;

namespace DisasterApp.Application.Validation;

/// <summary>
/// Validation attribute to ensure role names are valid
/// </summary>
public class ValidRoleNamesAttribute : ValidationAttribute
{
    private static readonly string[] ValidRoles = { "user", "admin", "cj" };

    public override bool IsValid(object? value)
    {
        if (value is not IEnumerable<string> roles)
        {
            return false;
        }

        foreach (var role in roles)
        {
            if (string.IsNullOrWhiteSpace(role) || 
                !ValidRoles.Contains(role, StringComparer.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }

    public override string FormatErrorMessage(string name)
    {
        return $"The {name} field contains invalid role names. Valid roles are: {string.Join(", ", ValidRoles)}";
    }
}
