using DisasterApp.Application.Services.Interfaces;
using DisasterApp.Domain.Entities;
using DisasterApp.WebApi.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using Xunit;

namespace DisasterApp.Tests.Authorization;

public class RoleAuthorizationHandlerTests
{
    private readonly Mock<IRoleService> _mockRoleService;
    private readonly Mock<ILogger<RoleAuthorizationHandler>> _mockLogger;
    private readonly RoleAuthorizationHandler _handler;
    private readonly AuthorizationHandlerContext _context;
    private readonly ClaimsPrincipal _user;
    private readonly RoleRequirement _requirement;

    public RoleAuthorizationHandlerTests()
    {
        _mockRoleService = new Mock<IRoleService>();
        _mockLogger = new Mock<ILogger<RoleAuthorizationHandler>>();
        _handler = new RoleAuthorizationHandler(_mockRoleService.Object, _mockLogger.Object);
        
        _user = new ClaimsPrincipal();
        _requirement = new RoleRequirement("admin", "user");
        _context = new AuthorizationHandlerContext(new[] { _requirement }, _user, null);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        // Act & Assert
        Assert.NotNull(_handler);
    }

    [Fact]
    public void Constructor_WithNullRoleService_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new RoleAuthorizationHandler(null!, _mockLogger.Object));
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new RoleAuthorizationHandler(_mockRoleService.Object, null!));
    }

    #endregion

    #region HandleRequirementAsync Tests

    [Fact]
    public async Task HandleRequirementAsync_ValidUserWithRequiredRole_Succeeds()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userClaims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString())
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(userClaims));
        var context = new AuthorizationHandlerContext(new[] { _requirement }, user, null);

        var userRoles = new List<Role>
        {
            new() { RoleId = Guid.NewGuid(), Name = "Admin" },
            new() { RoleId = Guid.NewGuid(), Name = "User" }
        };

        _mockRoleService.Setup(x => x.GetUserRolesAsync(userId))
            .ReturnsAsync(userRoles);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        Assert.True(context.HasSucceeded);
        Assert.False(context.HasFailed);
        _mockRoleService.Verify(x => x.GetUserRolesAsync(userId), Times.Once);
    }

    [Fact]
    public async Task HandleRequirementAsync_ValidUserWithoutRequiredRole_Fails()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userClaims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString())
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(userClaims));
        var context = new AuthorizationHandlerContext(new[] { _requirement }, user, null);

        var userRoles = new List<Role>
        {
            new() { RoleId = Guid.NewGuid(), Name = "CJ" } // Different role
        };

        _mockRoleService.Setup(x => x.GetUserRolesAsync(userId))
            .ReturnsAsync(userRoles);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
        Assert.True(context.HasFailed);
        _mockRoleService.Verify(x => x.GetUserRolesAsync(userId), Times.Once);
    }

    [Fact]
    public async Task HandleRequirementAsync_UserWithCaseInsensitiveRole_Succeeds()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userClaims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString())
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(userClaims));
        var context = new AuthorizationHandlerContext(new[] { _requirement }, user, null);

        var userRoles = new List<Role>
        {
            new() { RoleId = Guid.NewGuid(), Name = "ADMIN" } // Different case
        };

        _mockRoleService.Setup(x => x.GetUserRolesAsync(userId))
            .ReturnsAsync(userRoles);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        Assert.True(context.HasSucceeded);
        Assert.False(context.HasFailed);
    }

    [Fact]
    public async Task HandleRequirementAsync_NoUserIdClaim_Fails()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity()); // No claims
        var context = new AuthorizationHandlerContext(new[] { _requirement }, user, null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
        Assert.True(context.HasFailed);
        _mockRoleService.Verify(x => x.GetUserRolesAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task HandleRequirementAsync_InvalidUserIdClaim_Fails()
    {
        // Arrange
        var userClaims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "invalid-guid")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(userClaims));
        var context = new AuthorizationHandlerContext(new[] { _requirement }, user, null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
        Assert.True(context.HasFailed);
        _mockRoleService.Verify(x => x.GetUserRolesAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task HandleRequirementAsync_EmptyUserIdClaim_Fails()
    {
        // Arrange
        var userClaims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, string.Empty)
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(userClaims));
        var context = new AuthorizationHandlerContext(new[] { _requirement }, user, null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
        Assert.True(context.HasFailed);
        _mockRoleService.Verify(x => x.GetUserRolesAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task HandleRequirementAsync_RoleServiceThrowsException_Fails()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userClaims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString())
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(userClaims));
        var context = new AuthorizationHandlerContext(new[] { _requirement }, user, null);

        _mockRoleService.Setup(x => x.GetUserRolesAsync(userId))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        await _handler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
        Assert.True(context.HasFailed);
        _mockRoleService.Verify(x => x.GetUserRolesAsync(userId), Times.Once);
    }

    [Fact]
    public async Task HandleRequirementAsync_UserWithMultipleRoles_OneMatching_Succeeds()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userClaims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString())
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(userClaims));
        var context = new AuthorizationHandlerContext(new[] { _requirement }, user, null);

        var userRoles = new List<Role>
        {
            new() { RoleId = Guid.NewGuid(), Name = "CJ" },
            new() { RoleId = Guid.NewGuid(), Name = "User" }, // This matches
            new() { RoleId = Guid.NewGuid(), Name = "Moderator" }
        };

        _mockRoleService.Setup(x => x.GetUserRolesAsync(userId))
            .ReturnsAsync(userRoles);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        Assert.True(context.HasSucceeded);
        Assert.False(context.HasFailed);
    }

    [Fact]
    public async Task HandleRequirementAsync_UserWithNoRoles_Fails()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userClaims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString())
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(userClaims));
        var context = new AuthorizationHandlerContext(new[] { _requirement }, user, null);

        var userRoles = new List<Role>(); // Empty roles

        _mockRoleService.Setup(x => x.GetUserRolesAsync(userId))
            .ReturnsAsync(userRoles);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
        Assert.True(context.HasFailed);
    }

    #endregion

    #region RoleRequirement Tests

    [Fact]
    public void RoleRequirement_WithValidRoles_CreatesInstance()
    {
        // Arrange & Act
        var requirement = new RoleRequirement("admin", "user", "cj");

        // Assert
        Assert.NotNull(requirement);
        Assert.Equal(3, requirement.AllowedRoles.Count());
        Assert.Contains("admin", requirement.AllowedRoles);
        Assert.Contains("user", requirement.AllowedRoles);
        Assert.Contains("cj", requirement.AllowedRoles);
    }

    [Fact]
    public void RoleRequirement_WithNullRoles_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new RoleRequirement(null!));
    }

    [Fact]
    public void RoleRequirement_WithEmptyRoles_CreatesInstanceWithEmptyCollection()
    {
        // Arrange & Act
        var requirement = new RoleRequirement();

        // Assert
        Assert.NotNull(requirement);
        Assert.Empty(requirement.AllowedRoles);
    }

    [Fact]
    public void RoleRequirement_WithSingleRole_CreatesInstance()
    {
        // Arrange & Act
        var requirement = new RoleRequirement("admin");

        // Assert
        Assert.NotNull(requirement);
        Assert.Single(requirement.AllowedRoles);
        Assert.Contains("admin", requirement.AllowedRoles);
    }

    #endregion
}