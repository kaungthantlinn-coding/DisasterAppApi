using DisasterApp.Application.Services.Implementations;
using DisasterApp.Application.Services.Interfaces;
using DisasterApp.Domain.Entities;
using DisasterApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DisasterApp.Tests.Services;

public class RoleServiceTests : IDisposable
{
    private readonly DisasterDbContext _context;
    private readonly Mock<ILogger<RoleService>> _mockLogger;
    private readonly Mock<IAuditService> _mockAuditService;
    private readonly RoleService _roleService;

    public RoleServiceTests()
    {
        var options = new DbContextOptionsBuilder<DisasterDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new DisasterDbContext(options);
        _mockLogger = new Mock<ILogger<RoleService>>();
        _mockAuditService = new Mock<IAuditService>();
        _roleService = new RoleService(_context, _mockLogger.Object, _mockAuditService.Object);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    #region Super Admin Tests

    [Fact]
    public async Task GetSuperAdminRoleAsync_RoleExists_ReturnsRole()
    {
        // Arrange
        var superAdminRole = new Role { RoleId = Guid.NewGuid(), Name = "superadmin" };
        await _context.Roles.AddAsync(superAdminRole);
        await _context.SaveChangesAsync();

        // Act
        var result = await _roleService.GetSuperAdminRoleAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("superadmin", result.Name);
        Assert.Equal(superAdminRole.RoleId, result.RoleId);
    }

    [Fact]
    public async Task IsSuperAdminAsync_UserHasSuperAdminRole_ReturnsTrue()
    {
        // Arrange
        var superAdminRole = new Role { RoleId = Guid.NewGuid(), Name = "superadmin" };
        var user = new User
        {
            UserId = Guid.NewGuid(),
            Email = "superadmin@example.com",
            Name = "Super Admin",
            AuthProvider = "Email",
            Roles = new List<Role> { superAdminRole }
        };

        await _context.Roles.AddAsync(superAdminRole);
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _roleService.IsSuperAdminAsync(user.UserId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsSuperAdminAsync_UserDoesNotHaveSuperAdminRole_ReturnsFalse()
    {
        // Arrange
        var adminRole = new Role { RoleId = Guid.NewGuid(), Name = "admin" };
        var user = new User
        {
            UserId = Guid.NewGuid(),
            Email = "admin@example.com",
            Name = "Admin",
            AuthProvider = "Email",
            Roles = new List<Role> { adminRole }
        };

        await _context.Roles.AddAsync(adminRole);
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _roleService.IsSuperAdminAsync(user.UserId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetSuperAdminCountAsync_ReturnsCorrectCount()
    {
        // Arrange
        var superAdminRole = new Role { RoleId = Guid.NewGuid(), Name = "superadmin" };
        var adminRole = new Role { RoleId = Guid.NewGuid(), Name = "admin" };
        
        var superAdminUser = new User
        {
            UserId = Guid.NewGuid(),
            Email = "superadmin@example.com",
            Name = "Super Admin",
            AuthProvider = "Email",
            Roles = new List<Role> { superAdminRole }
        };
        
        var adminUser = new User
        {
            UserId = Guid.NewGuid(),
            Email = "admin@example.com",
            Name = "Admin",
            AuthProvider = "Email",
            Roles = new List<Role> { adminRole }
        };

        await _context.Roles.AddRangeAsync(superAdminRole, adminRole);
        await _context.Users.AddRangeAsync(superAdminUser, adminUser);
        await _context.SaveChangesAsync();

        // Act
        var result = await _roleService.GetSuperAdminCountAsync();

        // Assert
        Assert.Equal(1, result);
    }

    #endregion

    #region CleanupDuplicateUserRolesAsync Tests

    [Fact]
    public async Task CleanupDuplicateUserRolesAsync_NoDuplicates_ReturnsZero()
    {
        // Arrange
        var adminRole = new Role { RoleId = Guid.NewGuid(), Name = "Admin" };
        var userRole = new Role { RoleId = Guid.NewGuid(), Name = "User" };
        
        var user1 = new User
        {
            UserId = Guid.NewGuid(),
            Email = "user1@example.com",
            Name = "User One",
            AuthProvider = "Email",
            AuthId = "auth1",
            Roles = new List<Role> { adminRole }
        };
        
        var user2 = new User
        {
            UserId = Guid.NewGuid(),
            Email = "user2@example.com",
            Name = "User Two",
            AuthProvider = "Email",
            AuthId = "auth2",
            Roles = new List<Role> { userRole }
        };

        await _context.Roles.AddRangeAsync(adminRole, userRole);
        await _context.Users.AddRangeAsync(user1, user2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _roleService.CleanupDuplicateUserRolesAsync();

        // Assert
        Assert.Equal(0, result);
        
        // Verify no changes were made
        var updatedUser1 = await _context.Users.Include(u => u.Roles).FirstAsync(u => u.UserId == user1.UserId);
        var updatedUser2 = await _context.Users.Include(u => u.Roles).FirstAsync(u => u.UserId == user2.UserId);
        
        Assert.Single(updatedUser1.Roles);
        Assert.Single(updatedUser2.Roles);
        Assert.Equal("Admin", updatedUser1.Roles.First().Name);
        Assert.Equal("User", updatedUser2.Roles.First().Name);
    }

    [Fact]
    public async Task CleanupDuplicateUserRolesAsync_HasDuplicates_RemovesDuplicatesAndReturnsCount()
    {
        // Arrange
        var adminRole = new Role { RoleId = Guid.NewGuid(), Name = "Admin" };
        var userRole = new Role { RoleId = Guid.NewGuid(), Name = "User" };
        
        await _context.Roles.AddRangeAsync(adminRole, userRole);
        await _context.SaveChangesAsync();
        
        var user = new User
        {
            UserId = Guid.NewGuid(),
            Email = "user@example.com",
            Name = "Test User",
            AuthProvider = "Email",
            AuthId = "auth1"
        };
        
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        
        // Manually add duplicate role assignments by adding the same role multiple times
        user.Roles.Add(adminRole);
        user.Roles.Add(adminRole); // Duplicate
        user.Roles.Add(userRole);
        user.Roles.Add(userRole);  // Duplicate
        
        await _context.SaveChangesAsync();

        // Act
        var result = await _roleService.CleanupDuplicateUserRolesAsync();

        // Assert
        Assert.Equal(2, result); // Should remove 2 duplicates
        
        // Verify duplicates were removed
        var updatedUser = await _context.Users.Include(u => u.Roles).FirstAsync(u => u.UserId == user.UserId);
        Assert.Equal(2, updatedUser.Roles.Count); // Should have exactly 2 unique roles
        
        var roleNames = updatedUser.Roles.Select(r => r.Name).OrderBy(n => n).ToList();
        Assert.Equal(new[] { "Admin", "User" }, roleNames);
        
        // Verify each role appears only once
        var adminRoles = updatedUser.Roles.Where(r => r.Name == "Admin").ToList();
        var userRoles = updatedUser.Roles.Where(r => r.Name == "User").ToList();
        Assert.Single(adminRoles);
        Assert.Single(userRoles);
    }

    [Fact]
    public async Task CleanupDuplicateUserRolesAsync_Idempotency_SecondCallReturnsZero()
    {
        // Arrange
        var adminRole = new Role { RoleId = Guid.NewGuid(), Name = "Admin" };
        
        await _context.Roles.AddAsync(adminRole);
        await _context.SaveChangesAsync();
        
        var user = new User
        {
            UserId = Guid.NewGuid(),
            Email = "user@example.com",
            Name = "Test User",
            AuthProvider = "Email",
            AuthId = "auth1"
        };
        
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        
        // Add duplicate role assignments
        user.Roles.Add(adminRole);
        user.Roles.Add(adminRole); // Duplicate
        user.Roles.Add(adminRole); // Another duplicate
        
        await _context.SaveChangesAsync();

        // Act - First cleanup
        var firstResult = await _roleService.CleanupDuplicateUserRolesAsync();
        
        // Act - Second cleanup (should be idempotent)
        var secondResult = await _roleService.CleanupDuplicateUserRolesAsync();

        // Assert
        Assert.Equal(2, firstResult); // Should remove 2 duplicates on first run
        Assert.Equal(0, secondResult); // Should find no duplicates on second run
        
        // Verify final state
        var updatedUser = await _context.Users.Include(u => u.Roles).FirstAsync(u => u.UserId == user.UserId);
        Assert.Single(updatedUser.Roles);
        Assert.Equal("Admin", updatedUser.Roles.First().Name);
    }

    [Fact]
    public async Task CleanupDuplicateUserRolesAsync_MultipleUsersWithDuplicates_CleansAllUsers()
    {
        // Arrange
        var adminRole = new Role { RoleId = Guid.NewGuid(), Name = "Admin" };
        var userRole = new Role { RoleId = Guid.NewGuid(), Name = "User" };
        
        await _context.Roles.AddRangeAsync(adminRole, userRole);
        await _context.SaveChangesAsync();
        
        var user1 = new User
        {
            UserId = Guid.NewGuid(),
            Email = "user1@example.com",
            Name = "User One",
            AuthProvider = "Email",
            AuthId = "auth1"
        };
        
        var user2 = new User
        {
            UserId = Guid.NewGuid(),
            Email = "user2@example.com",
            Name = "User Two",
            AuthProvider = "Email",
            AuthId = "auth2"
        };

        await _context.Users.AddRangeAsync(user1, user2);
        await _context.SaveChangesAsync();
        
        // Add duplicate role assignments
        user1.Roles.Add(adminRole);
        user1.Roles.Add(adminRole); // Duplicate
        
        user2.Roles.Add(userRole);
        user2.Roles.Add(userRole); // Duplicate
        user2.Roles.Add(userRole); // Another duplicate
        
        await _context.SaveChangesAsync();

        // Act
        var result = await _roleService.CleanupDuplicateUserRolesAsync();

        // Assert
        Assert.Equal(3, result); // Should remove 1 + 2 = 3 duplicates total
        
        // Verify both users were cleaned
        var updatedUser1 = await _context.Users.Include(u => u.Roles).FirstAsync(u => u.UserId == user1.UserId);
        var updatedUser2 = await _context.Users.Include(u => u.Roles).FirstAsync(u => u.UserId == user2.UserId);
        
        Assert.Single(updatedUser1.Roles);
        Assert.Single(updatedUser2.Roles);
        Assert.Equal("Admin", updatedUser1.Roles.First().Name);
        Assert.Equal("User", updatedUser2.Roles.First().Name);
    }

    [Fact]
    public async Task CleanupDuplicateUserRolesAsync_SaveChangesThrows_LogsErrorAndRethrows()
    {
        // Arrange - Use a disposed context to force SaveChanges to throw
        var disposedContext = new DisasterDbContext(
            new DbContextOptionsBuilder<DisasterDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options);
        
        var adminRole = new Role { RoleId = Guid.NewGuid(), Name = "Admin" };
        await disposedContext.Roles.AddAsync(adminRole);
        await disposedContext.SaveChangesAsync();
        
        var user = new User
        {
            UserId = Guid.NewGuid(),
            Email = "user@example.com",
            Name = "Test User",
            AuthProvider = "Email",
            AuthId = "auth1"
        };
        
        await disposedContext.Users.AddAsync(user);
        await disposedContext.SaveChangesAsync();
        
        // Add duplicate role assignments
        user.Roles.Add(adminRole);
        user.Roles.Add(adminRole); // Duplicate
        
        await disposedContext.SaveChangesAsync();
        
        // Dispose the context to make SaveChanges throw
        disposedContext.Dispose();
        
        var serviceWithDisposedContext = new RoleService(disposedContext, _mockLogger.Object, _mockAuditService.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ObjectDisposedException>(
            () => serviceWithDisposedContext.CleanupDuplicateUserRolesAsync());
        
        // Verify error was logged
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error during duplicate role cleanup")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task CleanupDuplicateUserRolesAsync_LogsInformationMessages()
    {
        // Arrange
        var adminRole = new Role { RoleId = Guid.NewGuid(), Name = "Admin" };
        await _context.Roles.AddAsync(adminRole);
        await _context.SaveChangesAsync();
        
        var user = new User
        {
            UserId = Guid.NewGuid(),
            Email = "user@example.com",
            Name = "Test User",
            AuthProvider = "Email",
            AuthId = "auth1"
        };
        
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        
        // Add duplicate role assignments
        user.Roles.Add(adminRole);
        user.Roles.Add(adminRole); // Duplicate
        
        await _context.SaveChangesAsync();

        // Act
        var result = await _roleService.CleanupDuplicateUserRolesAsync();

        // Assert
        Assert.Equal(1, result);
        
        // Verify logging calls
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Starting cleanup of duplicate user roles")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
            
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Removed duplicate role Admin from user")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
            
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Cleanup completed. Removed 1 duplicate role assignments")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task CleanupDuplicateUserRolesAsync_NoDuplicatesFound_LogsNoDuplicatesMessage()
    {
        // Arrange
        var adminRole = new Role { RoleId = Guid.NewGuid(), Name = "Admin" };
        var user = new User
        {
            UserId = Guid.NewGuid(),
            Email = "user@example.com",
            Name = "Test User",
            AuthProvider = "Email",
            AuthId = "auth1",
            Roles = new List<Role> { adminRole } // No duplicates
        };

        await _context.Roles.AddAsync(adminRole);
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _roleService.CleanupDuplicateUserRolesAsync();

        // Assert
        Assert.Equal(0, result);
        
        // Verify appropriate logging
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No duplicate role assignments found")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    #endregion
}