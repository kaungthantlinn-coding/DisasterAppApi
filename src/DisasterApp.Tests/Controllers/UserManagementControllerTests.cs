using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using DisasterApp.WebApi.Controllers;
using DisasterApp.Application.Services.Interfaces;
using DisasterApp.Application.DTOs;
using DisasterApp.Domain.Entities;

namespace DisasterApp.Tests.Controllers;

public class UserManagementControllerTests
{
    private readonly Mock<IUserManagementService> _mockUserManagementService;
    private readonly Mock<IRoleService> _mockRoleService;
    private readonly Mock<IBlacklistService> _mockBlacklistService;
    private readonly Mock<ILogger<UserManagementController>> _mockLogger;
    private readonly UserManagementController _controller;

    public UserManagementControllerTests()
    {
        _mockUserManagementService = new Mock<IUserManagementService>();
        _mockRoleService = new Mock<IRoleService>();
        _mockBlacklistService = new Mock<IBlacklistService>();
        _mockLogger = new Mock<ILogger<UserManagementController>>();
        _controller = new UserManagementController(_mockUserManagementService.Object, _mockRoleService.Object, _mockBlacklistService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetUsers_ReturnsOkWithUsers()
    {
        // Arrange
        var pagedResult = new PagedUserListDto
        {
            Users = new List<UserListItemDto>
            {
                new UserListItemDto
                {
                    UserId = Guid.NewGuid(),
                    Email = "user1@example.com",
                    Name = "John Doe",
                    AuthProvider = "Email",
                    IsBlacklisted = false,
                    CreatedAt = DateTime.UtcNow,
                    RoleNames = new List<string> { "User" }
                },
                new UserListItemDto
                {
                    UserId = Guid.NewGuid(),
                    Email = "user2@example.com",
                    Name = "Jane Smith",
                    AuthProvider = "Email",
                    IsBlacklisted = false,
                    CreatedAt = DateTime.UtcNow,
                    RoleNames = new List<string> { "User" }
                }
            },
            TotalCount = 2,
            PageNumber = 1,
            PageSize = 10
        };

        _mockUserManagementService.Setup(x => x.GetUsersAsync(It.IsAny<UserFilterDto>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _controller.GetUsers(new UserFilterDto());

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsType<PagedUserListDto>(okResult.Value);
        Assert.Equal(2, returnValue.Users.Count);
    }

    [Fact]
    public async Task GetUsers_NoUsers_ReturnsOkWithEmptyList()
    {
        // Arrange
        var pagedResult = new PagedUserListDto
        {
            Users = new List<UserListItemDto>(),
            TotalCount = 0,
            PageNumber = 1,
            PageSize = 10
        };

        _mockUserManagementService.Setup(x => x.GetUsersAsync(It.IsAny<UserFilterDto>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _controller.GetUsers(new UserFilterDto());

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsType<PagedUserListDto>(okResult.Value);
        Assert.Empty(returnValue.Users);
    }

    [Fact]
    public async Task GetUser_ExistingUser_ReturnsOkWithUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new UserDetailsDto
        {
            UserId = userId,
            Email = "user@example.com",
            Name = "John Doe",
            AuthProvider = "Email",
            IsBlacklisted = false,
            CreatedAt = DateTime.UtcNow,
            Roles = new List<RoleDto>()
        };

        _mockUserManagementService.Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync(user);

        // Act
        var result = await _controller.GetUser(userId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsType<UserDetailsDto>(okResult.Value);
        Assert.Equal(userId, returnValue.UserId);
        Assert.Equal("user@example.com", returnValue.Email);
    }

    [Fact]
    public async Task GetUserById_NonExistingUser_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mockUserManagementService.Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync((UserDetailsDto)null);

        // Act
        var result = await _controller.GetUser(userId);

        // Assert
        var actionResult = Assert.IsType<ActionResult<UserDetailsDto>>(result);
        Assert.IsType<NotFoundObjectResult>(actionResult.Result);
    }

    [Fact]
    public async Task CreateUser_ValidUser_ReturnsCreatedWithUser()
    {
        // Arrange
        var createUserDto = new CreateUserDto
        {
            Email = "newuser@example.com",
            Name = "New User",
            Password = "password123"
        };

        var createdUser = new UserDetailsDto
        {
            UserId = Guid.NewGuid(),
            Email = "newuser@example.com",
            Name = "New User",
            AuthProvider = "Email",
            IsBlacklisted = false,
            CreatedAt = DateTime.UtcNow,
            Roles = new List<RoleDto>()
        };

        _mockUserManagementService.Setup(x => x.CreateUserAsync(createUserDto))
            .ReturnsAsync(createdUser);

        // Act
        var result = await _controller.CreateUser(createUserDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<UserDetailsDto>>(result);
        var createdResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
        var returnValue = Assert.IsType<UserDetailsDto>(createdResult.Value);
        Assert.Equal("newuser@example.com", returnValue.Email);
        Assert.Equal("New User", returnValue.Name);
    }

    [Fact]
    public async Task CreateUser_DuplicateEmail_ReturnsBadRequest()
    {
        // Arrange
        var createUserDto = new CreateUserDto
        {
            Email = "existing@example.com",
            Name = "New User",
            Password = "password123"
        };

        _mockUserManagementService.Setup(x => x.CreateUserAsync(createUserDto))
            .ThrowsAsync(new InvalidOperationException("Email already exists"));

        // Act
        var result = await _controller.CreateUser(createUserDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<UserDetailsDto>>(result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        Assert.Contains("Email already exists", badRequestResult.Value.ToString());
    }

    [Fact]
    public async Task CreateUser_NullCreateUserDto_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.CreateUser(null);

        // Assert
        var actionResult = Assert.IsType<ActionResult<UserDetailsDto>>(result);
        Assert.IsType<BadRequestObjectResult>(actionResult.Result);
    }

    [Fact]
    public async Task UpdateUser_ValidUser_ReturnsOkWithUpdatedUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var updateUserDto = new UpdateUserDto
        {
            Name = "Updated User",
            Email = "updated@example.com"
        };

        var updatedUser = new UserDetailsDto
        {
            UserId = userId,
            Email = "updated@example.com",
            Name = "Updated User",
            AuthProvider = "Email",
            IsBlacklisted = false,
            CreatedAt = DateTime.UtcNow,
            Roles = new List<RoleDto>()
        };

        _mockUserManagementService.Setup(x => x.UpdateUserAsync(userId, updateUserDto))
            .ReturnsAsync(updatedUser);

        // Act
        var result = await _controller.UpdateUser(userId, updateUserDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<UserDetailsDto>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnValue = Assert.IsType<UserDetailsDto>(okResult.Value);
        Assert.Equal("Updated User", returnValue.Name);
        Assert.Equal("updated@example.com", returnValue.Email);
    }

    [Fact]
    public async Task UpdateUser_NonExistingUser_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var updateUserDto = new UpdateUserDto
        {
            Name = "Updated User",
            Email = "updated@example.com"
        };

        _mockUserManagementService.Setup(x => x.UpdateUserAsync(userId, updateUserDto))
            .ThrowsAsync(new ArgumentException("User not found"));

        // Act
        var result = await _controller.UpdateUser(userId, updateUserDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<UserDetailsDto>>(result);
        Assert.IsType<NotFoundObjectResult>(actionResult.Result);
    }

    [Fact]
    public async Task UpdateUser_NullUpdateUserDto_ReturnsBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var result = await _controller.UpdateUser(userId, null);

        // Assert
        var actionResult = Assert.IsType<ActionResult<UserDetailsDto>>(result);
        Assert.IsType<BadRequestObjectResult>(actionResult.Result);
    }

    [Fact]
    public async Task DeleteUser_ExistingUser_ReturnsNoContent()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mockUserManagementService.Setup(x => x.DeleteUserAsync(userId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteUser(userId);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteUser_NonExistingUser_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mockUserManagementService.Setup(x => x.DeleteUserAsync(userId))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteUser(userId);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task ChangeUserPassword_ValidRequest_ReturnsOk()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var changePasswordDto = new ChangeUserPasswordDto
        {
            NewPassword = "newpassword123",
            ConfirmPassword = "newpassword123"
        };

        _mockUserManagementService.Setup(x => x.ChangeUserPasswordAsync(userId, changePasswordDto))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ChangeUserPassword(userId, changePasswordDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task ChangeUserPassword_InvalidRequest_ReturnsBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var changePasswordDto = new ChangeUserPasswordDto
        {
            NewPassword = "newpassword123",
            ConfirmPassword = "newpassword123"
        };

        _mockUserManagementService.Setup(x => x.ChangeUserPasswordAsync(userId, changePasswordDto))
            .ThrowsAsync(new InvalidOperationException("Cannot change password for non-local authentication users"));

        // Act
        var result = await _controller.ChangeUserPassword(userId, changePasswordDto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task ChangeUserPassword_NullChangePasswordDto_ReturnsBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var result = await _controller.ChangeUserPassword(userId, null);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }







    [Fact]
    public async Task GetUsers_ServiceThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        _mockUserManagementService.Setup(x => x.GetUsersAsync(It.IsAny<UserFilterDto>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _controller.GetUsers(new UserFilterDto());

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task CreateUser_ServiceThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        var createUserDto = new CreateUserDto
        {
            Email = "newuser@example.com",
            Name = "New User",
            Password = "password123"
        };

        _mockUserManagementService.Setup(x => x.CreateUserAsync(createUserDto))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _controller.CreateUser(createUserDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<UserDetailsDto>>(result);
        var statusCodeResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    #region UpdateUserRoles Tests

    [Fact]
    public async Task UpdateUserRoles_ValidRequest_ReturnsOkWithUpdatedUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var updateRolesDto = new UpdateUserRolesDto
        {
            RoleNames = ["User", "Admin"],
            Reason = "Promotion to admin"
        };

        var updatedUser = new UserDetailsDto
        {
            UserId = userId,
            Email = "test@example.com",
            Name = "Test User",
            AuthProvider = "Email",
            IsBlacklisted = false,
            CreatedAt = DateTime.UtcNow,
            Roles = new List<RoleDto>
            {
                new() { RoleId = Guid.NewGuid(), Name = "User" },
                new() { RoleId = Guid.NewGuid(), Name = "Admin" }
            }
        };

        _mockUserManagementService.Setup(x => x.UpdateUserRolesAsync(userId, updateRolesDto))
            .ReturnsAsync(updatedUser);

        // Act
        var result = await _controller.UpdateUserRoles(userId, updateRolesDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<UserDetailsDto>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnValue = Assert.IsType<UserDetailsDto>(okResult.Value);
        Assert.Equal(2, returnValue.Roles.Count);
        Assert.Contains(returnValue.Roles, r => r.Name == "User");
        Assert.Contains(returnValue.Roles, r => r.Name == "Admin");
    }

    [Fact]
    public async Task UpdateUserRoles_UserNotFound_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var updateRolesDto = new UpdateUserRolesDto
        {
            RoleNames = ["User"]
        };

        _mockUserManagementService.Setup(x => x.UpdateUserRolesAsync(userId, updateRolesDto))
            .ThrowsAsync(new ArgumentException($"User with ID {userId} not found"));

        // Act
        var result = await _controller.UpdateUserRoles(userId, updateRolesDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<UserDetailsDto>>(result);
        Assert.IsType<NotFoundObjectResult>(actionResult.Result);
    }

    [Fact]
    public async Task UpdateUserRoles_InvalidOperation_ReturnsBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var updateRolesDto = new UpdateUserRolesDto
        {
            RoleNames = [] // Empty roles list
        };

        _mockUserManagementService.Setup(x => x.UpdateUserRolesAsync(userId, updateRolesDto))
            .ThrowsAsync(new InvalidOperationException("Cannot update roles: User must have at least one role assigned"));

        // Act
        var result = await _controller.UpdateUserRoles(userId, updateRolesDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<UserDetailsDto>>(result);
        Assert.IsType<BadRequestObjectResult>(actionResult.Result);
    }

    [Fact]
    public async Task UpdateUserRoles_InvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var updateRolesDto = new UpdateUserRolesDto
        {
            RoleNames = [] // This should trigger validation error
        };

        _controller.ModelState.AddModelError("RoleNames", "At least one role must be specified");

        // Act
        var result = await _controller.UpdateUserRoles(userId, updateRolesDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<UserDetailsDto>>(result);
        Assert.IsType<BadRequestObjectResult>(actionResult.Result);
    }

    [Fact]
    public async Task UpdateUserRoles_ServiceThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var updateRolesDto = new UpdateUserRolesDto
        {
            RoleNames = ["User"]
        };

        _mockUserManagementService.Setup(x => x.UpdateUserRolesAsync(userId, updateRolesDto))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _controller.UpdateUserRoles(userId, updateRolesDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<UserDetailsDto>>(result);
        var statusCodeResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    #endregion

    #region ValidateRoleUpdate Tests

    [Fact]
    public async Task ValidateRoleUpdate_ValidRequest_ReturnsOkWithValidation()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var updateRolesDto = new UpdateUserRolesDto
        {
            RoleNames = ["User", "Admin"]
        };

        var validation = new RoleUpdateValidationDto
        {
            CanUpdate = true,
            Warnings = ["Granting admin privileges to user"],
            Errors = [],
            IsLastAdmin = false,
            AdminCount = 3
        };

        _mockUserManagementService.Setup(x => x.ValidateRoleUpdateAsync(userId, updateRolesDto.RoleNames))
            .ReturnsAsync(validation);

        // Act
        var result = await _controller.ValidateRoleUpdate(userId, updateRolesDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<RoleUpdateValidationDto>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnValue = Assert.IsType<RoleUpdateValidationDto>(okResult.Value);
        Assert.True(returnValue.CanUpdate);
        Assert.Single(returnValue.Warnings);
        Assert.Empty(returnValue.Errors);
        Assert.False(returnValue.IsLastAdmin);
        Assert.Equal(3, returnValue.AdminCount);
    }

    [Fact]
    public async Task ValidateRoleUpdate_CannotUpdate_ReturnsOkWithValidationErrors()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var updateRolesDto = new UpdateUserRolesDto
        {
            RoleNames = [] // Empty roles
        };

        var validation = new RoleUpdateValidationDto
        {
            CanUpdate = false,
            Warnings = [],
            Errors = ["User must have at least one role assigned"],
            IsLastAdmin = false,
            AdminCount = 2
        };

        _mockUserManagementService.Setup(x => x.ValidateRoleUpdateAsync(userId, updateRolesDto.RoleNames))
            .ReturnsAsync(validation);

        // Act
        var result = await _controller.ValidateRoleUpdate(userId, updateRolesDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<RoleUpdateValidationDto>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnValue = Assert.IsType<RoleUpdateValidationDto>(okResult.Value);
        Assert.False(returnValue.CanUpdate);
        Assert.Single(returnValue.Errors);
        Assert.Empty(returnValue.Warnings);
    }

    [Fact]
    public async Task ValidateRoleUpdate_LastAdminRemoval_ReturnsOkWithValidationErrors()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var updateRolesDto = new UpdateUserRolesDto
        {
            RoleNames = ["User"] // Removing admin role
        };

        var validation = new RoleUpdateValidationDto
        {
            CanUpdate = false,
            Warnings = [],
            Errors = ["Cannot remove admin role from the last admin user"],
            IsLastAdmin = true,
            AdminCount = 1
        };

        _mockUserManagementService.Setup(x => x.ValidateRoleUpdateAsync(userId, updateRolesDto.RoleNames))
            .ReturnsAsync(validation);

        // Act
        var result = await _controller.ValidateRoleUpdate(userId, updateRolesDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<RoleUpdateValidationDto>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnValue = Assert.IsType<RoleUpdateValidationDto>(okResult.Value);
        Assert.False(returnValue.CanUpdate);
        Assert.True(returnValue.IsLastAdmin);
        Assert.Equal(1, returnValue.AdminCount);
        Assert.Single(returnValue.Errors);
    }

    [Fact]
    public async Task ValidateRoleUpdate_InvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var updateRolesDto = new UpdateUserRolesDto
        {
            RoleNames = []
        };

        _controller.ModelState.AddModelError("RoleNames", "At least one role must be specified");

        // Act
        var result = await _controller.ValidateRoleUpdate(userId, updateRolesDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<RoleUpdateValidationDto>>(result);
        Assert.IsType<BadRequestObjectResult>(actionResult.Result);
    }

    [Fact]
    public async Task ValidateRoleUpdate_ServiceThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var updateRolesDto = new UpdateUserRolesDto
        {
            RoleNames = ["User"]
        };

        _mockUserManagementService.Setup(x => x.ValidateRoleUpdateAsync(userId, updateRolesDto.RoleNames))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _controller.ValidateRoleUpdate(userId, updateRolesDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<RoleUpdateValidationDto>>(result);
        var statusCodeResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    #endregion
}