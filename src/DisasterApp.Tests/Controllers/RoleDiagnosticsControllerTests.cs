using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using DisasterApp.Application.Services.Interfaces;
using DisasterApp.Domain.Entities;
using DisasterApp.Infrastructure.Data;
using DisasterApp.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using DisasterApp.Application.DTOs;

namespace DisasterApp.Tests.Controllers;

public class RoleDiagnosticsControllerTests : IDisposable
{
    private readonly RoleDiagnosticsController _controller;
    private readonly Mock<ILogger<RoleDiagnosticsController>> _mockLogger;
    private readonly Mock<IRoleService> _mockRoleService;
    private readonly DisasterDbContext _context;

    public RoleDiagnosticsControllerTests()
    {
        var options = new DbContextOptionsBuilder<DisasterDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new DisasterDbContext(options);
        _mockRoleService = new Mock<IRoleService>();
        _mockLogger = new Mock<ILogger<RoleDiagnosticsController>>();
        _controller = new RoleDiagnosticsController(_context, _mockRoleService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetRolesStatus_WithValidData_ReturnsOkWithRoles()
    {
        // Arrange
        var roles = new List<Role>
        {
            new Role { RoleId = Guid.NewGuid(), Name = "Admin" },
            new Role { RoleId = Guid.NewGuid(), Name = "User" }
        };

        await _context.Roles.AddRangeAsync(roles);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetRolesStatus();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task GetRolesStatus_WithException_ReturnsInternalServerError()
    {
        // Arrange
        _context.Dispose(); // Force an exception

        // Act
        var result = await _controller.GetRolesStatus();

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusResult.StatusCode);
    }

    [Fact]
    public async Task GetUserRolesDiagnostics_WithValidUserId_ReturnsOkWithDiagnostics()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            UserId = userId,
            Email = "test@example.com",
            Name = "Test User"
        };
        var role = new Role { RoleId = Guid.NewGuid(), Name = "TestRole" };

        await _context.Users.AddAsync(user);
        await _context.Roles.AddAsync(role);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetUserRolesDiagnostics(userId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task GetUserRolesDiagnostics_WithNonExistentUser_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var result = await _controller.GetUserRolesDiagnostics(userId);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task GetUserRolesDiagnostics_WithException_ReturnsInternalServerError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _context.Dispose(); // Force an exception

        // Act
        var result = await _controller.GetUserRolesDiagnostics(userId);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusResult.StatusCode);
    }

    [Fact]
    public async Task FixRoleNames_WithValidData_ReturnsOkWithFixedRoles()
    {
        // Arrange
        var roles = new List<Role>
        {
            new Role { RoleId = Guid.NewGuid(), Name = "admin" }, // lowercase
            new Role { RoleId = Guid.NewGuid(), Name = "USER" }   // uppercase
        };

        await _context.Roles.AddRangeAsync(roles);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.FixRoleNames();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task FixRoleNames_WithValidRoles_ReturnsZeroFixed()
    {
        // Arrange - Create roles with valid names
        var roles = new List<Role>
        {
            new Role { RoleId = Guid.NewGuid(), Name = "admin" },
            new Role { RoleId = Guid.NewGuid(), Name = "user" },
            new Role { RoleId = Guid.NewGuid(), Name = "cj" }
        };

        await _context.Roles.AddRangeAsync(roles);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.FixRoleNames();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value;
        Assert.NotNull(response);
        
        // Verify no roles were "fixed" since they were already valid
        var updatedRoles = await _context.Roles.ToListAsync();
        Assert.Equal(3, updatedRoles.Count);
        Assert.All(updatedRoles, role => 
        {
            Assert.False(string.IsNullOrEmpty(role.Name));
            Assert.True(role.Name == "admin" || role.Name == "user" || role.Name == "cj");
        });
    }

    [Fact]
    public async Task FixRoleNames_WithNoRolesToFix_ReturnsZeroFixed()
    {
        // Arrange
        var roles = new List<Role>
        {
            new Role { RoleId = Guid.NewGuid(), Name = "admin" },
            new Role { RoleId = Guid.NewGuid(), Name = "user" }
        };

        await _context.Roles.AddRangeAsync(roles);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.FixRoleNames();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task FixRoleNames_WithException_ReturnsInternalServerError()
    {
        // Arrange
        var roles = new List<Role>
        {
            new Role { RoleId = Guid.NewGuid(), Name = "admin" }
        };

        await _context.Roles.AddRangeAsync(roles);
        await _context.SaveChangesAsync();
        _context.Dispose(); // Force an exception

        // Act
        var result = await _controller.FixRoleNames();

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusResult.StatusCode);
    }

    [Fact]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        // Act & Assert
        Assert.NotNull(_controller);
    }

    [Fact]
    public void Constructor_WithNullContext_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new RoleDiagnosticsController(null!, _mockRoleService.Object, _mockLogger.Object));
    }

    [Fact]
    public void Constructor_WithNullRoleService_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new RoleDiagnosticsController(_context, null!, _mockLogger.Object));
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new RoleDiagnosticsController(_context, _mockRoleService.Object, null!));
    }

    public void Dispose()
    {
        _context?.Dispose();
        GC.SuppressFinalize(this);
    }
}