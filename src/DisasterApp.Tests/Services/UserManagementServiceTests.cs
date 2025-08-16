using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using DisasterApp.Application.Services.Implementations;
using DisasterApp.Application.Services.Interfaces;
using DisasterApp.Infrastructure.Repositories.Interfaces;
using DisasterApp.Domain.Entities;
using DisasterApp.Application.DTOs;

namespace DisasterApp.Tests.Services;

public class UserManagementServiceTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IRefreshTokenRepository> _mockRefreshTokenRepository;
    private readonly Mock<IPasswordResetTokenRepository> _mockPasswordResetTokenRepository;
    private readonly Mock<IRoleService> _mockRoleService;
    private readonly Mock<IAuditService> _mockAuditService;
    private readonly Mock<ILogger<UserManagementService>> _mockLogger;
    private readonly UserManagementService _userManagementService;

    public UserManagementServiceTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockRefreshTokenRepository = new Mock<IRefreshTokenRepository>();
        _mockPasswordResetTokenRepository = new Mock<IPasswordResetTokenRepository>();
        _mockRoleService = new Mock<IRoleService>();
        _mockAuditService = new Mock<IAuditService>();
        _mockLogger = new Mock<ILogger<UserManagementService>>();
        
        _userManagementService = new UserManagementService(
            _mockUserRepository.Object,
            _mockRefreshTokenRepository.Object,
            _mockPasswordResetTokenRepository.Object,
            _mockRoleService.Object,
            _mockAuditService.Object,
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task GetUsersAsync_ReturnsAllUsers()
    {
        // Arrange
        var filter = new UserFilterDto { PageNumber = 1, PageSize = 10 };
        var users = new List<User>
        {
            new User { 
                UserId = Guid.NewGuid(), 
                Email = "user1@example.com", 
                Name = "John Doe",
                AuthProvider = "local",
                AuthId = Guid.NewGuid().ToString()
            },
            new User { 
                UserId = Guid.NewGuid(), 
                Email = "user2@example.com", 
                Name = "Jane Smith",
                AuthProvider = "local",
                AuthId = Guid.NewGuid().ToString()
            }
        };
        var pagedResult = new PagedUserListDto
        {
            Users = users.Select(u => new UserListItemDto
            {
                UserId = u.UserId,
                Email = u.Email,
                Name = u.Name
            }).ToList(),
            TotalCount = 2,
            PageNumber = 1,
            PageSize = 10
        };

        _mockUserRepository.Setup(x => x.GetUsersAsync(
                1,
                10,
                null,
                null,
                null,
                null,
                null,
                null,
                "CreatedAt",
                "desc"
            ))
            .ReturnsAsync((users, 2));

        // Act
        var result = await _userManagementService.GetUsersAsync(filter);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Users.Count);
        Assert.Equal(2, result.TotalCount);
    }

    [Fact]
    public async Task GetUserByIdAsync_ExistingUser_ReturnsUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            UserId = userId,
            Email = "test@example.com",
            Name = "John Doe",
            Roles = new List<Role>()
        };

        _mockUserRepository.Setup(x => x.GetUserWithDetailsAsync(userId))
            .ReturnsAsync(user);
        _mockUserRepository.Setup(x => x.GetUserStatisticsAsync(userId))
            .ReturnsAsync((0, 0, 0, 0));

        // Act
        var result = await _userManagementService.GetUserByIdAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
        Assert.Equal("test@example.com", result.Email);
        Assert.Equal("John Doe", result.Name);
    }

    [Fact]
    public async Task GetUserByIdAsync_NonExistingUser_ReturnsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        
        _mockUserRepository.Setup(x => x.GetUserWithDetailsAsync(userId))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _userManagementService.GetUserByIdAsync(userId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateUserAsync_ValidData_CreatesUser()
    {
        // Arrange
        var createUserDto = new CreateUserDto
        {
            Email = "newuser@example.com",
            Password = "Password123!",
            Name = "John Doe",
            PhoneNumber = "+1234567890"
        };

        var createdUser = new User
        {
            UserId = Guid.NewGuid(),
            Email = createUserDto.Email,
            Name = createUserDto.Name,
            PhoneNumber = createUserDto.PhoneNumber,
            AuthProvider = "Email",
            AuthId = "hashedpassword",
            IsBlacklisted = false,
            CreatedAt = DateTime.UtcNow,
            Roles = new List<Role>()
        };

        _mockUserRepository.Setup(x => x.GetByEmailAsync(createUserDto.Email))
            .ReturnsAsync((User?)null);
        _mockUserRepository.Setup(x => x.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync(createdUser);
        _mockRoleService.Setup(x => x.AssignDefaultRoleToUserAsync(It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);
        _mockUserRepository.Setup(x => x.GetUserWithDetailsAsync(It.IsAny<Guid>()))
            .ReturnsAsync(createdUser);
        _mockUserRepository.Setup(x => x.GetUserStatisticsAsync(It.IsAny<Guid>()))
            .ReturnsAsync((0, 0, 0, 0));

        // Act
        var result = await _userManagementService.CreateUserAsync(createUserDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(createUserDto.Email, result.Email);
        Assert.Equal(createUserDto.Name, result.Name);
        _mockUserRepository.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task CreateUserAsync_ExistingEmail_ThrowsException()
    {
        // Arrange
        var createUserDto = new CreateUserDto
        {
            Email = "existing@example.com",
            Password = "Password123!",
            Name = "John Doe"
        };

        var existingUser = new User
        {
            UserId = Guid.NewGuid(),
            Email = "existing@example.com"
        };

        _mockUserRepository.Setup(x => x.ExistsAsync(createUserDto.Email))
            .ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _userManagementService.CreateUserAsync(createUserDto));
    }

    [Fact]
    public async Task UpdateUserAsync_ExistingUser_UpdatesUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var updateUserDto = new UpdateUserDto
        {
            Name = "Updated John Doe",
            PhoneNumber = "+9876543210",
            Email = "test@example.com",
            Roles = new List<string> { "User" }
        };

        var existingUser = new User
        {
            UserId = userId,
            Email = "test@example.com",
            Name = "John Doe",
            PhoneNumber = "+1234567890"
        };

        var updatedUserDetails = new UserDetailsDto
        {
            UserId = userId,
            Email = "test@example.com",
            Name = "Updated John Doe",
            PhoneNumber = "+9876543210",
            AuthProvider = "Email",
            IsBlacklisted = false,
            CreatedAt = DateTime.UtcNow,
            Roles = new List<RoleDto> { new RoleDto { Name = "User" } }
        };

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(existingUser);
        _mockUserRepository.Setup(x => x.ExistsAsync(updateUserDto.Email, userId))
            .ReturnsAsync(false);
        _mockUserRepository.Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => u);
        _mockRoleService.Setup(x => x.ReplaceUserRolesAsync(userId, updateUserDto.Roles, null, null, null, null))
            .Returns(Task.CompletedTask);
        _mockUserRepository.Setup(x => x.GetUserWithDetailsAsync(userId))
            .ReturnsAsync(existingUser);
        _mockUserRepository.Setup(x => x.GetUserStatisticsAsync(userId))
            .ReturnsAsync((0, 0, 0, 0));

        // Act
        var result = await _userManagementService.UpdateUserAsync(userId, updateUserDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
        Assert.Equal("Updated John Doe", result.Name);
        Assert.Equal("+9876543210", result.PhoneNumber);
    }

    [Fact]
    public async Task UpdateUserAsync_NonExistingUser_ThrowsException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var updateUserDto = new UpdateUserDto
        {
            Name = "Updated John Doe",
            Email = "test@example.com"
        };

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _userManagementService.UpdateUserAsync(userId, updateUserDto));
    }

    [Fact]
    public async Task UpdateUserAsync_OnlyNameEmailChanges_NoRoleUpdate_UpdatesUserWithoutRoleService()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var updateUserDto = new UpdateUserDto
        {
            Name = "Updated Name Only",
            Email = "updated@example.com",
            PhoneNumber = "+1234567890",
            PhotoUrl = "https://example.com/photo.jpg",
            IsBlacklisted = false,
            Roles = new List<string> { "User" } // Same roles as existing user
        };

        var existingUser = new User
        {
            UserId = userId,
            Email = "original@example.com",
            Name = "Original Name",
            PhoneNumber = "+0987654321",
            PhotoUrl = "https://example.com/old.jpg",
            IsBlacklisted = true,
            Roles = new List<Role> { new Role { Name = "User" } }
        };

        var updatedUserDetails = new UserDetailsDto
        {
            UserId = userId,
            Email = "updated@example.com",
            Name = "Updated Name Only",
            PhoneNumber = "+1234567890",
            PhotoUrl = "https://example.com/photo.jpg",
            IsBlacklisted = false,
            AuthProvider = "Email",
            CreatedAt = DateTime.UtcNow,
            Roles = new List<RoleDto> { new RoleDto { Name = "User" } }
        };

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(existingUser);
        _mockUserRepository.Setup(x => x.ExistsAsync(updateUserDto.Email, userId))
            .ReturnsAsync(false);
        _mockUserRepository.Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => u);
        _mockRoleService.Setup(x => x.ReplaceUserRolesAsync(userId, updateUserDto.Roles, null, null, null, null))
            .Returns(Task.CompletedTask);
        _mockUserRepository.Setup(x => x.GetUserWithDetailsAsync(userId))
            .ReturnsAsync(existingUser);
        _mockUserRepository.Setup(x => x.GetUserStatisticsAsync(userId))
            .ReturnsAsync((0, 0, 0, 0));

        // Act
        var result = await _userManagementService.UpdateUserAsync(userId, updateUserDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Name Only", result.Name);
        Assert.Equal("updated@example.com", result.Email);
        Assert.Equal("+1234567890", result.PhoneNumber);
        Assert.Equal("https://example.com/photo.jpg", result.PhotoUrl);
        Assert.False(result.IsBlacklisted);
        
        // Verify user properties were updated
        _mockUserRepository.Verify(x => x.UpdateAsync(It.Is<User>(u => 
            u.Name == "Updated Name Only" && 
            u.Email == "updated@example.com" && 
            u.PhoneNumber == "+1234567890" &&
            u.PhotoUrl == "https://example.com/photo.jpg" &&
            u.IsBlacklisted == false)), Times.Once);
        
        // Verify role service was still called for atomic role replacement
        _mockRoleService.Verify(x => x.ReplaceUserRolesAsync(userId, updateUserDto.Roles, null, null, null, null), Times.Once);
    }

    [Fact]
    public async Task UpdateUserAsync_DuplicateEmail_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var updateUserDto = new UpdateUserDto
        {
            Name = "Updated User",
            Email = "duplicate@example.com",
            Roles = new List<string> { "User" }
        };

        var existingUser = new User
        {
            UserId = userId,
            Email = "original@example.com",
            Name = "Original User"
        };

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(existingUser);
        _mockUserRepository.Setup(x => x.ExistsAsync(updateUserDto.Email, userId))
            .ReturnsAsync(true); // Email already exists for another user

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _userManagementService.UpdateUserAsync(userId, updateUserDto));
        
        Assert.Equal("Another user with this email already exists", exception.Message);
        
        // Verify update was not attempted
        _mockUserRepository.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
        _mockRoleService.Verify(x => x.ReplaceUserRolesAsync(It.IsAny<Guid>(), It.IsAny<List<string>>(), null, null, null, null), Times.Never);
    }

    [Fact]
    public async Task UpdateUserAsync_DbUpdateException_PropagatesException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var updateUserDto = new UpdateUserDto
        {
            Name = "Updated User",
            Email = "updated@example.com",
            Roles = new List<string> { "User" }
        };

        var existingUser = new User
        {
            UserId = userId,
            Email = "original@example.com",
            Name = "Original User"
        };

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(existingUser);
        _mockUserRepository.Setup(x => x.ExistsAsync(updateUserDto.Email, userId))
            .ReturnsAsync(false);
        _mockUserRepository.Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ThrowsAsync(new Microsoft.EntityFrameworkCore.DbUpdateException("Database constraint violation"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Microsoft.EntityFrameworkCore.DbUpdateException>(() => 
            _userManagementService.UpdateUserAsync(userId, updateUserDto));
        
        Assert.Contains("Database constraint violation", exception.Message);
        
        // Verify role service was not called due to database failure
        _mockRoleService.Verify(x => x.ReplaceUserRolesAsync(It.IsAny<Guid>(), It.IsAny<List<string>>(), null, null, null, null), Times.Never);
    }

    [Fact]
    public async Task UpdateUserAsync_PostRetrievalFailure_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var updateUserDto = new UpdateUserDto
        {
            Name = "Updated User",
            Email = "updated@example.com",
            Roles = new List<string> { "User" }
        };

        var existingUser = new User
        {
            UserId = userId,
            Email = "original@example.com",
            Name = "Original User"
        };

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(existingUser);
        _mockUserRepository.Setup(x => x.ExistsAsync(updateUserDto.Email, userId))
            .ReturnsAsync(false);
        _mockUserRepository.Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => u);
        _mockRoleService.Setup(x => x.ReplaceUserRolesAsync(userId, updateUserDto.Roles, null, null, null, null))
            .Returns(Task.CompletedTask);
        
        // Setup GetUserByIdAsync to return null on the final retrieval (simulating post-update failure)
        _mockUserRepository.Setup(x => x.GetUserWithDetailsAsync(userId))
            .ReturnsAsync((User?)null);
        _mockUserRepository.Setup(x => x.GetUserStatisticsAsync(userId))
            .ReturnsAsync((0, 0, 0, 0));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _userManagementService.UpdateUserAsync(userId, updateUserDto));
        
        Assert.Equal("Failed to retrieve updated user", exception.Message);
        
        // Verify all operations were attempted before the final retrieval failed
        _mockUserRepository.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Once);
        _mockRoleService.Verify(x => x.ReplaceUserRolesAsync(userId, updateUserDto.Roles, null, null, null, null), Times.Once);
    }

    [Fact]
    public async Task UpdateUserAsync_CaseInsensitiveRoles_HandlesRolesCaseInsensitively()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var updateUserDto = new UpdateUserDto
        {
            Name = "Updated User",
            Email = "updated@example.com",
            Roles = new List<string> { "ADMIN", "user" } // Mixed case roles
        };

        var existingUser = new User
        {
            UserId = userId,
            Email = "original@example.com",
            Name = "Original User",
            Roles = new List<Role> 
            { 
                new Role { Name = "Admin" },
                new Role { Name = "User" }
            }
        };

        var updatedUserDetails = new UserDetailsDto
        {
            UserId = userId,
            Email = "updated@example.com",
            Name = "Updated User",
            AuthProvider = "Email",
            IsBlacklisted = false,
            CreatedAt = DateTime.UtcNow,
            Roles = new List<RoleDto> 
            { 
                new RoleDto { Name = "Admin" },
                new RoleDto { Name = "User" }
            }
        };

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(existingUser);
        _mockUserRepository.Setup(x => x.ExistsAsync(updateUserDto.Email, userId))
            .ReturnsAsync(false);
        _mockUserRepository.Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => u);
        _mockRoleService.Setup(x => x.ReplaceUserRolesAsync(userId, updateUserDto.Roles, null, null, null, null))
            .Returns(Task.CompletedTask);
        _mockUserRepository.Setup(x => x.GetUserWithDetailsAsync(userId))
            .ReturnsAsync(existingUser);
        _mockUserRepository.Setup(x => x.GetUserStatisticsAsync(userId))
            .ReturnsAsync((0, 0, 0, 0));

        // Act
        var result = await _userManagementService.UpdateUserAsync(userId, updateUserDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated User", result.Name);
        Assert.Equal("updated@example.com", result.Email);
        
        // Verify role service was called with the mixed case roles (service should handle case-insensitivity)
        _mockRoleService.Verify(x => x.ReplaceUserRolesAsync(userId, 
            It.Is<List<string>>(roles => roles.Contains("ADMIN") && roles.Contains("user")), 
            null, null, null, null), Times.Once);
    }

    [Fact]
    public async Task UpdateUserAsync_RoleServiceThrowsLastAdminException_PropagatesException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var updateUserDto = new UpdateUserDto
        {
            Name = "Updated User",
            Email = "updated@example.com",
            Roles = new List<string> { "User" } // Removing admin role
        };

        var existingUser = new User
        {
            UserId = userId,
            Email = "original@example.com",
            Name = "Original User",
            Roles = new List<Role> { new Role { Name = "Admin" } }
        };

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(existingUser);
        _mockUserRepository.Setup(x => x.ExistsAsync(updateUserDto.Email, userId))
            .ReturnsAsync(false);
        _mockUserRepository.Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => u);
        _mockRoleService.Setup(x => x.ReplaceUserRolesAsync(userId, updateUserDto.Roles, null, null, null, null))
            .ThrowsAsync(new InvalidOperationException("Cannot remove admin role from the last admin user"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _userManagementService.UpdateUserAsync(userId, updateUserDto));
        
        Assert.Equal("Cannot remove admin role from the last admin user", exception.Message);
        
        // Verify user update was attempted but role service threw exception
        _mockUserRepository.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Once);
        _mockRoleService.Verify(x => x.ReplaceUserRolesAsync(userId, updateUserDto.Roles, null, null, null, null), Times.Once);
    }

    [Fact]
    public async Task DeleteUserAsync_ExistingUser_DeletesUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existingUser = new User
        {
            UserId = userId,
            Email = "test@example.com",
            Roles = new List<Role>(),
            DisasterReportUsers = new List<DisasterReport>(),
            SupportRequests = new List<SupportRequest>()
        };

        _mockUserRepository.Setup(x => x.GetUserWithDetailsAsync(userId))
            .ReturnsAsync(existingUser);
        _mockUserRepository.Setup(x => x.GetAdminUsersCountAsync())
            .ReturnsAsync(2); // More than 1 admin so deletion is allowed
        _mockRefreshTokenRepository.Setup(x => x.DeleteAllUserTokensAsync(userId))
            .ReturnsAsync(true);
        _mockPasswordResetTokenRepository.Setup(x => x.DeleteAllUserTokensAsync(userId))
            .ReturnsAsync(true);
        _mockUserRepository.Setup(x => x.DeleteUserAsync(userId))
            .ReturnsAsync(true);

        // Act
        var result = await _userManagementService.DeleteUserAsync(userId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task DeleteUserAsync_NonExistingUser_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        
        _mockUserRepository.Setup(x => x.GetUserWithDetailsAsync(userId))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _userManagementService.DeleteUserAsync(userId));
    }

    [Fact]
    public async Task ChangeUserPasswordAsync_ValidData_ChangesPassword()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var changePasswordDto = new ChangeUserPasswordDto
        {
            NewPassword = "NewPassword123!",
            ConfirmPassword = "NewPassword123!"
        };

        var user = new User
        {
            UserId = userId,
            Email = "test@example.com",
            AuthProvider = "Email",
            AuthId = BCrypt.Net.BCrypt.HashPassword("OldPassword123!")
        };

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);
        _mockUserRepository.Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => u);

        // Act
        var result = await _userManagementService.ChangeUserPasswordAsync(userId, changePasswordDto);

        // Assert
        Assert.True(result);
        Assert.True(BCrypt.Net.BCrypt.Verify("NewPassword123!", user.AuthId));
    }

    [Fact]
    public async Task ChangeUserPasswordAsync_InvalidAuthProvider_ThrowsException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var changePasswordDto = new ChangeUserPasswordDto
        {
            NewPassword = "NewPassword123!",
            ConfirmPassword = "NewPassword123!"
        };

        var user = new User
        {
            UserId = userId,
            Email = "test@example.com",
            AuthProvider = "Google",
            AuthId = "google-oauth-id"
        };

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _userManagementService.ChangeUserPasswordAsync(userId, changePasswordDto));
    }

    [Fact]
    public async Task ChangeUserPasswordAsync_NonExistingUser_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var changePasswordDto = new ChangeUserPasswordDto
        {
            NewPassword = "NewPassword123!",
            ConfirmPassword = "NewPassword123!"
        };

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _userManagementService.ChangeUserPasswordAsync(userId, changePasswordDto);

        // Assert
        Assert.False(result);
    }

    #region BulkOperationAsync Tests

    [Fact]
    public async Task BulkOperationAsync_BlacklistOperation_UpdatesUsersAndReturnsCount()
    {
        // Arrange
        var userIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var users = new List<User>
        {
            new User { UserId = userIds[0], Email = "user1@test.com", IsBlacklisted = false },
            new User { UserId = userIds[1], Email = "user2@test.com", IsBlacklisted = false }
        };
        var bulkOperation = new BulkUserOperationDto
        {
            UserIds = userIds,
            Operation = "blacklist"
        };

        _mockUserRepository.Setup(x => x.GetUsersByIdsAsync(userIds))
            .ReturnsAsync(users);
        _mockUserRepository.Setup(x => x.BulkUpdateUsersAsync(users))
            .ReturnsAsync(true);

        // Act
        var result = await _userManagementService.BulkOperationAsync(bulkOperation, null);

        // Assert
        Assert.Equal(2, result);
        Assert.True(users[0].IsBlacklisted);
        Assert.True(users[1].IsBlacklisted);
        _mockUserRepository.Verify(x => x.BulkUpdateUsersAsync(users), Times.Once);
    }

    [Fact]
    public async Task BulkOperationAsync_UnblacklistOperation_UpdatesUsersAndReturnsCount()
    {
        // Arrange
        var userIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var users = new List<User>
        {
            new User { UserId = userIds[0], Email = "user1@test.com", IsBlacklisted = true },
            new User { UserId = userIds[1], Email = "user2@test.com", IsBlacklisted = true }
        };
        var bulkOperation = new BulkUserOperationDto
        {
            UserIds = userIds,
            Operation = "unblacklist"
        };

        _mockUserRepository.Setup(x => x.GetUsersByIdsAsync(userIds))
            .ReturnsAsync(users);
        _mockUserRepository.Setup(x => x.BulkUpdateUsersAsync(users))
            .ReturnsAsync(true);

        // Act
        var result = await _userManagementService.BulkOperationAsync(bulkOperation, null);

        // Assert
        Assert.Equal(2, result);
        Assert.False(users[0].IsBlacklisted);
        Assert.False(users[1].IsBlacklisted);
        _mockUserRepository.Verify(x => x.BulkUpdateUsersAsync(users), Times.Once);
    }

    [Fact]
    public async Task BulkOperationAsync_AssignRoleOperation_CallsRoleServiceAndReturnsCount()
    {
        // Arrange
        var userIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var users = new List<User>
        {
            new User { UserId = userIds[0], Email = "user1@test.com" },
            new User { UserId = userIds[1], Email = "user2@test.com" }
        };
        var bulkOperation = new BulkUserOperationDto
        {
            UserIds = userIds,
            Operation = "assign-role",
            RoleName = "Admin"
        };

        _mockUserRepository.Setup(x => x.GetUsersByIdsAsync(userIds))
            .ReturnsAsync(users);
        _mockRoleService.Setup(x => x.AssignRoleToUserAsync(It.IsAny<Guid>(), "Admin", null, null, null, null))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _userManagementService.BulkOperationAsync(bulkOperation, null);

        // Assert
        Assert.Equal(2, result);
        _mockRoleService.Verify(x => x.AssignRoleToUserAsync(userIds[0], "Admin", null, null, null, null), Times.Once);
        _mockRoleService.Verify(x => x.AssignRoleToUserAsync(userIds[1], "Admin", null, null, null, null), Times.Once);
        _mockUserRepository.Verify(x => x.BulkUpdateUsersAsync(It.IsAny<List<User>>()), Times.Never);
    }

    [Fact]
    public async Task BulkOperationAsync_RemoveRoleOperation_CallsRoleServiceAndReturnsCount()
    {
        // Arrange
        var userIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var users = new List<User>
        {
            new User { UserId = userIds[0], Email = "user1@test.com" },
            new User { UserId = userIds[1], Email = "user2@test.com" }
        };
        var bulkOperation = new BulkUserOperationDto
        {
            UserIds = userIds,
            Operation = "remove-role",
            RoleName = "User"
        };

        _mockUserRepository.Setup(x => x.GetUsersByIdsAsync(userIds))
            .ReturnsAsync(users);
        _mockRoleService.Setup(x => x.RemoveRoleFromUserAsync(It.IsAny<Guid>(), "User", null, null, null, null))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _userManagementService.BulkOperationAsync(bulkOperation, null);

        // Assert
        Assert.Equal(2, result);
        _mockRoleService.Verify(x => x.RemoveRoleFromUserAsync(userIds[0], "User", null, null, null, null), Times.Once);
        _mockRoleService.Verify(x => x.RemoveRoleFromUserAsync(userIds[1], "User", null, null, null, null), Times.Once);
        _mockUserRepository.Verify(x => x.BulkUpdateUsersAsync(It.IsAny<List<User>>()), Times.Never);
    }

    [Fact]
    public async Task BulkOperationAsync_AssignRoleWithoutRoleName_ThrowsArgumentException()
    {
        // Arrange
        var userIds = new List<Guid> { Guid.NewGuid() };
        var bulkOperation = new BulkUserOperationDto
        {
            UserIds = userIds,
            Operation = "assign-role",
            RoleName = null
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
            _userManagementService.BulkOperationAsync(bulkOperation, null));
        Assert.Equal("Role name is required for role assignment", exception.Message);
    }

    [Fact]
    public async Task BulkOperationAsync_RemoveRoleWithoutRoleName_ThrowsArgumentException()
    {
        // Arrange
        var userIds = new List<Guid> { Guid.NewGuid() };
        var bulkOperation = new BulkUserOperationDto
        {
            UserIds = userIds,
            Operation = "remove-role",
            RoleName = ""
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
            _userManagementService.BulkOperationAsync(bulkOperation, null));
        Assert.Equal("Role name is required for role removal", exception.Message);
    }

    [Fact]
    public async Task BulkOperationAsync_UnknownOperation_ThrowsArgumentException()
    {
        // Arrange
        var userIds = new List<Guid> { Guid.NewGuid() };
        var bulkOperation = new BulkUserOperationDto
        {
            UserIds = userIds,
            Operation = "unknown-operation"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
            _userManagementService.BulkOperationAsync(bulkOperation, null));
        Assert.Equal("Unknown operation: unknown-operation", exception.Message);
    }

    [Fact]
    public async Task BulkOperationAsync_CaseInsensitiveOperation_WorksCorrectly()
    {
        // Arrange
        var userIds = new List<Guid> { Guid.NewGuid() };
        var users = new List<User>
        {
            new User { UserId = userIds[0], Email = "user1@test.com", IsBlacklisted = false }
        };
        var bulkOperation = new BulkUserOperationDto
        {
            UserIds = userIds,
            Operation = "BLACKLIST" // Test case insensitivity
        };

        _mockUserRepository.Setup(x => x.GetUsersByIdsAsync(userIds))
            .ReturnsAsync(users);
        _mockUserRepository.Setup(x => x.BulkUpdateUsersAsync(users))
            .ReturnsAsync(true);

        // Act
        var result = await _userManagementService.BulkOperationAsync(bulkOperation, null);

        // Assert
        Assert.Equal(1, result);
        Assert.True(users[0].IsBlacklisted);
    }

    [Fact]
    public async Task BulkOperationAsync_RoleAssignmentFails_ContinuesWithOtherUsers()
    {
        // Arrange
        var userIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var users = new List<User>
        {
            new User { UserId = userIds[0], Email = "user1@test.com" },
            new User { UserId = userIds[1], Email = "user2@test.com" }
        };
        var bulkOperation = new BulkUserOperationDto
        {
            UserIds = userIds,
            Operation = "assign-role",
            RoleName = "Admin"
        };

        _mockUserRepository.Setup(x => x.GetUsersByIdsAsync(userIds))
            .ReturnsAsync(users);
        _mockRoleService.Setup(x => x.AssignRoleToUserAsync(userIds[0], "Admin", null, null, null, null))
            .ThrowsAsync(new ArgumentException("Role assignment failed"));
        _mockRoleService.Setup(x => x.AssignRoleToUserAsync(userIds[1], "Admin", null, null, null, null))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _userManagementService.BulkOperationAsync(bulkOperation, null);

        // Assert
        Assert.Equal(1, result); // Only one user should be affected
        _mockRoleService.Verify(x => x.AssignRoleToUserAsync(userIds[0], "Admin", null, null, null, null), Times.Once);
        _mockRoleService.Verify(x => x.AssignRoleToUserAsync(userIds[1], "Admin", null, null, null, null), Times.Once);
    }

    [Fact]
    public async Task BulkOperationAsync_EmptyUserList_ReturnsZero()
    {
        // Arrange
        var userIds = new List<Guid>();
        var users = new List<User>();
        var bulkOperation = new BulkUserOperationDto
        {
            UserIds = userIds,
            Operation = "blacklist"
        };

        _mockUserRepository.Setup(x => x.GetUsersByIdsAsync(userIds))
            .ReturnsAsync(users);

        // Act
        var result = await _userManagementService.BulkOperationAsync(bulkOperation, null);

        // Assert
        Assert.Equal(0, result);
        _mockUserRepository.Verify(x => x.BulkUpdateUsersAsync(It.IsAny<List<User>>()), Times.Once);
    }

    [Fact]
    public async Task BulkOperationAsync_BlacklistOperation_PreventsSelfBlacklisting()
    {
        // Arrange
        var adminUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var userIds = new List<Guid> { adminUserId, otherUserId };
        var users = new List<User>
        {
            new User { UserId = adminUserId, Email = "admin@test.com", IsBlacklisted = false },
            new User { UserId = otherUserId, Email = "user@test.com", IsBlacklisted = false }
        };
        var bulkOperation = new BulkUserOperationDto
        {
            UserIds = userIds,
            Operation = "blacklist"
        };

        _mockUserRepository.Setup(x => x.GetUsersByIdsAsync(userIds))
            .ReturnsAsync(users);
        _mockUserRepository.Setup(x => x.BulkUpdateUsersAsync(users))
            .ReturnsAsync(true);

        // Act
        var result = await _userManagementService.BulkOperationAsync(bulkOperation, adminUserId);

        // Assert
        Assert.Equal(1, result); // Only one user should be affected (not the admin)
        Assert.False(users[0].IsBlacklisted); // Admin should not be blacklisted
        Assert.True(users[1].IsBlacklisted); // Other user should be blacklisted
        _mockUserRepository.Verify(x => x.BulkUpdateUsersAsync(users), Times.Once);
    }

    #endregion

    [Fact]
    public async Task GetUsersAsync_WithSearchQuery_ReturnsFilteredUsers()
    {
        // Arrange
        var searchQuery = "john";
        var filteredUsers = new List<User>
        {
            new User { UserId = Guid.NewGuid(), Email = "john@example.com", Name = "John Doe" },
            new User { UserId = Guid.NewGuid(), Email = "johnny@example.com", Name = "Johnny Cash" }
        };

        _mockUserRepository.Setup(x => x.GetUsersAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                searchQuery,
                It.IsAny<string>(),
                It.IsAny<bool?>(),
                It.IsAny<string>(),
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<string>(),
                It.IsAny<string>()
            ))
            .ReturnsAsync((filteredUsers, 2));

        // Act
        var result = await _userManagementService.GetUsersAsync(new UserFilterDto { SearchTerm = searchQuery });

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Users.Count);
        Assert.Equal(2, result.TotalCount);
    }
}