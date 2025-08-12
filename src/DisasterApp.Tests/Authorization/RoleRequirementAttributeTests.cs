using DisasterApp.WebApi.Authorization;
using Microsoft.AspNetCore.Authorization;
using Xunit;

namespace DisasterApp.Tests.Authorization;

public class RoleRequirementAttributeTests
{
    #region RoleRequirementAttribute Tests

    [Fact]
    public void RoleRequirementAttribute_WithSingleRole_SetsRolesCorrectly()
    {
        // Arrange & Act
        var attribute = new RoleRequirementAttribute("admin");

        // Assert
        Assert.Equal("admin", attribute.Roles);
    }

    [Fact]
    public void RoleRequirementAttribute_WithMultipleRoles_SetsRolesCorrectly()
    {
        // Arrange & Act
        var attribute = new RoleRequirementAttribute("admin", "user", "cj");

        // Assert
        Assert.Equal("admin,user,cj", attribute.Roles);
    }

    [Fact]
    public void RoleRequirementAttribute_WithEmptyRoles_SetsEmptyRoles()
    {
        // Arrange & Act
        var attribute = new RoleRequirementAttribute();

        // Assert
        Assert.Equal(string.Empty, attribute.Roles);
    }

    [Fact]
    public void RoleRequirementAttribute_WithNullRole_HandlesGracefully()
    {
        // Arrange & Act
        var attribute = new RoleRequirementAttribute("admin", null!, "user");

        // Assert
        Assert.Equal("admin,,user", attribute.Roles);
    }

    [Fact]
    public void RoleRequirementAttribute_InheritsFromAuthorizeAttribute()
    {
        // Arrange & Act
        var attribute = new RoleRequirementAttribute("admin");

        // Assert
        Assert.IsAssignableFrom<AuthorizeAttribute>(attribute);
    }

    [Fact]
    public void RoleRequirementAttribute_HasCorrectAttributeUsage()
    {
        // Arrange
        var attributeType = typeof(RoleRequirementAttribute);

        // Act
        var attributeUsage = (AttributeUsageAttribute?)Attribute.GetCustomAttribute(
            attributeType, typeof(AttributeUsageAttribute));

        // Assert
        Assert.NotNull(attributeUsage);
        Assert.True(attributeUsage.ValidOn.HasFlag(AttributeTargets.Class));
        Assert.True(attributeUsage.ValidOn.HasFlag(AttributeTargets.Method));
        Assert.True(attributeUsage.AllowMultiple);
    }

    #endregion

    #region AdminOnlyAttribute Tests

    [Fact]
    public void AdminOnlyAttribute_SetsAdminRole()
    {
        // Arrange & Act
        var attribute = new AdminOnlyAttribute();

        // Assert
        Assert.Equal("admin", attribute.Roles);
    }

    [Fact]
    public void AdminOnlyAttribute_InheritsFromRoleRequirementAttribute()
    {
        // Arrange & Act
        var attribute = new AdminOnlyAttribute();

        // Assert
        Assert.IsAssignableFrom<RoleRequirementAttribute>(attribute);
        Assert.IsAssignableFrom<AuthorizeAttribute>(attribute);
    }

    [Fact]
    public void AdminOnlyAttribute_CanBeInstantiated()
    {
        // Arrange & Act
        var attribute = new AdminOnlyAttribute();

        // Assert
        Assert.NotNull(attribute);
    }

    #endregion

    #region CjOnlyAttribute Tests

    [Fact]
    public void CjOnlyAttribute_SetsCjRole()
    {
        // Arrange & Act
        var attribute = new CjOnlyAttribute();

        // Assert
        Assert.Equal("cj", attribute.Roles);
    }

    [Fact]
    public void CjOnlyAttribute_InheritsFromRoleRequirementAttribute()
    {
        // Arrange & Act
        var attribute = new CjOnlyAttribute();

        // Assert
        Assert.IsAssignableFrom<RoleRequirementAttribute>(attribute);
        Assert.IsAssignableFrom<AuthorizeAttribute>(attribute);
    }

    [Fact]
    public void CjOnlyAttribute_CanBeInstantiated()
    {
        // Arrange & Act
        var attribute = new CjOnlyAttribute();

        // Assert
        Assert.NotNull(attribute);
    }

    #endregion

    #region UserOnlyAttribute Tests

    [Fact]
    public void UserOnlyAttribute_SetsUserRole()
    {
        // Arrange & Act
        var attribute = new UserOnlyAttribute();

        // Assert
        Assert.Equal("user", attribute.Roles);
    }

    [Fact]
    public void UserOnlyAttribute_InheritsFromRoleRequirementAttribute()
    {
        // Arrange & Act
        var attribute = new UserOnlyAttribute();

        // Assert
        Assert.IsAssignableFrom<RoleRequirementAttribute>(attribute);
        Assert.IsAssignableFrom<AuthorizeAttribute>(attribute);
    }

    [Fact]
    public void UserOnlyAttribute_CanBeInstantiated()
    {
        // Arrange & Act
        var attribute = new UserOnlyAttribute();

        // Assert
        Assert.NotNull(attribute);
    }

    #endregion

    #region AdminOrCjAttribute Tests

    [Fact]
    public void AdminOrCjAttribute_SetsAdminAndCjRoles()
    {
        // Arrange & Act
        var attribute = new AdminOrCjAttribute();

        // Assert
        Assert.Equal("admin,cj", attribute.Roles);
    }

    [Fact]
    public void AdminOrCjAttribute_InheritsFromRoleRequirementAttribute()
    {
        // Arrange & Act
        var attribute = new AdminOrCjAttribute();

        // Assert
        Assert.IsAssignableFrom<RoleRequirementAttribute>(attribute);
        Assert.IsAssignableFrom<AuthorizeAttribute>(attribute);
    }

    [Fact]
    public void AdminOrCjAttribute_CanBeInstantiated()
    {
        // Arrange & Act
        var attribute = new AdminOrCjAttribute();

        // Assert
        Assert.NotNull(attribute);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void AllRoleAttributes_CanBeUsedTogether()
    {
        // Arrange & Act
        var adminAttribute = new AdminOnlyAttribute();
        var cjAttribute = new CjOnlyAttribute();
        var userAttribute = new UserOnlyAttribute();
        var adminOrCjAttribute = new AdminOrCjAttribute();

        // Assert
        Assert.NotEqual(adminAttribute.Roles, cjAttribute.Roles);
        Assert.NotEqual(adminAttribute.Roles, userAttribute.Roles);
        Assert.NotEqual(cjAttribute.Roles, userAttribute.Roles);
        Assert.Contains("admin", adminOrCjAttribute.Roles);
        Assert.Contains("cj", adminOrCjAttribute.Roles);
    }

    [Fact]
    public void RoleAttributes_HaveUniqueRoleValues()
    {
        // Arrange & Act
        var adminAttribute = new AdminOnlyAttribute();
        var cjAttribute = new CjOnlyAttribute();
        var userAttribute = new UserOnlyAttribute();

        var roles = new[] { adminAttribute.Roles, cjAttribute.Roles, userAttribute.Roles };

        // Assert
        Assert.Equal(3, roles.Distinct().Count());
        Assert.Contains("admin", roles);
        Assert.Contains("cj", roles);
        Assert.Contains("user", roles);
    }

    [Fact]
    public void RoleAttributes_CanBeAppliedToClasses()
    {
        // This test verifies that the attributes can be applied to classes
        // by checking their AttributeUsage settings
        var attributeTypes = new[]
        {
            typeof(AdminOnlyAttribute),
            typeof(CjOnlyAttribute),
            typeof(UserOnlyAttribute),
            typeof(AdminOrCjAttribute)
        };

        foreach (var attributeType in attributeTypes)
        {
            // Get the AttributeUsage from the base RoleRequirementAttribute
            var baseType = attributeType.BaseType; // RoleRequirementAttribute
            var attributeUsage = (AttributeUsageAttribute?)Attribute.GetCustomAttribute(
                baseType!, typeof(AttributeUsageAttribute));

            Assert.NotNull(attributeUsage);
            Assert.True(attributeUsage.ValidOn.HasFlag(AttributeTargets.Class));
            Assert.True(attributeUsage.ValidOn.HasFlag(AttributeTargets.Method));
        }
    }

    #endregion
}