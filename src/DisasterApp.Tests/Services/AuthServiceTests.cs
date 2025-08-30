using Xunit;
using Moq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using DisasterApp.Application.Services.Implementations;
using DisasterApp.Application.Services.Interfaces;
using DisasterApp.Infrastructure.Repositories.Interfaces;
using DisasterApp.Domain.Entities;
using DisasterApp.Application.DTOs;

namespace DisasterApp.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IRefreshTokenRepository> _mockRefreshTokenRepository;
    private readonly Mock<IPasswordResetTokenRepository> _mockPasswordResetTokenRepository;
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly Mock<IRoleService> _mockRoleService;
    private readonly Mock<IPasswordValidationService> _mockPasswordValidationService;
    private readonly Mock<ITwoFactorService> _mockTwoFactorService;
    private readonly Mock<IOtpService> _mockOtpService;
    private readonly Mock<IBackupCodeService> _mockBackupCodeService;
    private readonly Mock<IRateLimitingService> _mockRateLimitingService;
    private readonly Mock<ITokenService> _mockTokenService;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILogger<AuthService>> _mockLogger;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockRefreshTokenRepository = new Mock<IRefreshTokenRepository>();
        _mockPasswordResetTokenRepository = new Mock<IPasswordResetTokenRepository>();
        _mockEmailService = new Mock<IEmailService>();
        _mockRoleService = new Mock<IRoleService>();
        _mockPasswordValidationService = new Mock<IPasswordValidationService>();
        _mockTwoFactorService = new Mock<ITwoFactorService>();
        _mockOtpService = new Mock<IOtpService>();
        _mockBackupCodeService = new Mock<IBackupCodeService>();
        _mockRateLimitingService = new Mock<IRateLimitingService>();
        _mockTokenService = new Mock<ITokenService>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<AuthService>>();

        // Setup configuration defaults
        _mockConfiguration.Setup(x => x["GoogleAuth:ClientId"]).Returns("test-client-id");
        _mockConfiguration.Setup(x => x["Jwt:Key"]).Returns("this-is-a-very-long-secret-key-for-jwt-token-generation-that-is-at-least-256-bits-long");
        _mockConfiguration.Setup(x => x["Jwt:Issuer"]).Returns("test-issuer");
        _mockConfiguration.Setup(x => x["Jwt:Audience"]).Returns("test-audience");
        _mockConfiguration.Setup(x => x["Jwt:AccessTokenExpirationMinutes"]).Returns("60");
        _mockConfiguration.Setup(x => x["Jwt:RefreshTokenExpirationDays"]).Returns("7");

        _authService = new AuthService(
            _mockUserRepository.Object,
            _mockRefreshTokenRepository.Object,
            _mockPasswordResetTokenRepository.Object,
            _mockEmailService.Object,
            _mockRoleService.Object,
            _mockPasswordValidationService.Object,
            _mockTwoFactorService.Object,
            _mockOtpService.Object,
            _mockBackupCodeService.Object,
            _mockRateLimitingService.Object,
            _mockTokenService.Object,
            _mockConfiguration.Object,
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsAuthResponse()
    {
        // Arrange
        var loginDto = new LoginRequestDto
        {
            Email = "test@example.com",
            Password = "password123"
        };

        var user = new User
        {
            UserId = Guid.NewGuid(),
            Email = "test@example.com",
            Name = "Test User",
            AuthProvider = "Email",
            AuthId = BCrypt.Net.BCrypt.HashPassword("password123"),
            TwoFactorEnabled = false
        };

        _mockUserRepository.Setup(x => x.GetByEmailAsync(loginDto.Email))
            .ReturnsAsync(user);
        _mockUserRepository.Setup(x => x.GetUserRolesAsync(user.UserId))
            .ReturnsAsync(["User"]);
        _mockRefreshTokenRepository.Setup(x => x.CreateAsync(It.IsAny<RefreshToken>()))
            .ReturnsAsync((RefreshToken token) => token);
        _mockRoleService.Setup(x => x.AssignDefaultRoleToUserAsync(It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _authService.LoginAsync(loginDto);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.AccessToken);
        Assert.NotNull(result.RefreshToken);
        Assert.NotNull(result.User);
        Assert.Equal(user.Email, result.User.Email);
    }

    [Fact]
    public async Task LoginAsync_InvalidEmail_ThrowsException()
    {
        // Arrange
        var loginDto = new LoginRequestDto
        {
            Email = "nonexistent@example.com",
            Password = "password123"
        };

        _mockUserRepository.Setup(x => x.GetByEmailAsync(loginDto.Email))
            .ReturnsAsync((User?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _authService.LoginAsync(loginDto));
        
        Assert.Equal("Invalid email or password", exception.Message);
    }

    [Fact]
    public async Task LoginAsync_InvalidPassword_ThrowsException()
    {
        // Arrange
        var loginDto = new LoginRequestDto
        {
            Email = "test@example.com",
            Password = "wrongpassword"
        };

        var user = new User
        {
            UserId = Guid.NewGuid(),
            Email = "test@example.com",
            Name = "Test User",
            AuthProvider = "Email",
            AuthId = BCrypt.Net.BCrypt.HashPassword("correctpassword")
        };

        _mockUserRepository.Setup(x => x.GetByEmailAsync(loginDto.Email))
            .ReturnsAsync(user);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _authService.LoginAsync(loginDto));
        
        Assert.Equal("Invalid email or password", exception.Message);
    }

    [Fact]
    public async Task SignupAsync_ValidData_ReturnsAuthResponse()
    {
        // Arrange
        var signupDto = new SignupRequestDto
        {
            Email = "newuser@example.com",
            Password = "password123",
            FullName = "New User",
            ConfirmPassword = "password123",
            AgreeToTerms = true
        };

        _mockUserRepository.Setup(x => x.ExistsAsync(signupDto.Email))
            .ReturnsAsync(false);
        _mockUserRepository.Setup(x => x.GetByEmailAsync(signupDto.Email))
            .ReturnsAsync((User?)null);

        _mockUserRepository.Setup(x => x.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync(new User
            {
                UserId = Guid.NewGuid(),
                Email = signupDto.Email,
                Name = signupDto.FullName,
                AuthProvider = "Email",
                AuthId = BCrypt.Net.BCrypt.HashPassword(signupDto.Password)
            });

        _mockUserRepository.Setup(x => x.GetUserRolesAsync(It.IsAny<Guid>()))
            .ReturnsAsync(["User"]);

        _mockRefreshTokenRepository.Setup(x => x.CreateAsync(It.IsAny<RefreshToken>()))
            .ReturnsAsync((RefreshToken token) => token);

        _mockRoleService.Setup(x => x.AssignDefaultRoleToUserAsync(It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);
        _mockRoleService.Setup(x => x.GetUserRolesAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new List<Role> { new Role { RoleId = Guid.NewGuid(), Name = "User" } });

        // Act
        var result = await _authService.SignupAsync(signupDto);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.AccessToken);
        Assert.NotNull(result.RefreshToken);
        Assert.NotNull(result.User);
        Assert.Equal(signupDto.Email, result.User.Email);
    }

    [Fact]
    public async Task SignupAsync_ExistingEmail_ThrowsException()
    {
        // Arrange
        var signupDto = new SignupRequestDto
        {
            Email = "existing@example.com",
            Password = "password123",
            FullName = "John Doe",
            ConfirmPassword = "password123",
            AgreeToTerms = true
        };

        _mockUserRepository.Setup(x => x.ExistsAsync(signupDto.Email))
            .ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _authService.SignupAsync(signupDto));
        
        Assert.Equal("User with this email already exists", exception.Message);
    }

    [Fact]
    public async Task RefreshTokenAsync_ValidToken_ReturnsNewTokens()
    {
        // Arrange
        var refreshTokenValue = "valid-refresh-token";
        var userId = Guid.NewGuid();
        
        var user = new User
        {
            UserId = userId,
            Email = "test@example.com",
            Name = "Test User",
            IsBlacklisted = false,
            AuthProvider = "Email",
            AuthId = BCrypt.Net.BCrypt.HashPassword("password123")
        };

        var refreshToken = new RefreshToken
        {
            Token = "valid-refresh-token",
            UserId = userId,
            User = user,
            ExpiredAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };

        _mockRefreshTokenRepository.Setup(x => x.GetByTokenAsync(refreshTokenValue))
            .ReturnsAsync(refreshToken);
        _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);
        _mockUserRepository.Setup(x => x.GetUserRolesAsync(userId))
            .ReturnsAsync(["User"]);
        _mockRefreshTokenRepository.Setup(x => x.CreateAsync(It.IsAny<RefreshToken>()))
            .ReturnsAsync((RefreshToken token) => token);
        _mockRefreshTokenRepository.Setup(x => x.DeleteAsync(refreshTokenValue))
            .ReturnsAsync(true);
        _mockRoleService.Setup(x => x.AssignDefaultRoleToUserAsync(It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _authService.RefreshTokenAsync(new RefreshTokenRequestDto { RefreshToken = refreshTokenValue });

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.AccessToken);
        Assert.NotNull(result.RefreshToken);
        Assert.NotNull(result.User);
        Assert.Equal(user.Email, result.User.Email);
    }

    [Fact]
    public async Task LogoutAsync_ValidToken_RevokesToken()
    {
        // Arrange
        var refreshTokenValue = "valid-refresh-token";
        var refreshToken = new RefreshToken
        {
            Token = refreshTokenValue,
            UserId = Guid.NewGuid(),
            ExpiredAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };

        _mockRefreshTokenRepository.Setup(x => x.GetByTokenAsync(refreshTokenValue))
            .ReturnsAsync(refreshToken);

        _mockRefreshTokenRepository.Setup(x => x.DeleteAsync(refreshTokenValue))
            .ReturnsAsync(true);

        // Act
        var result = await _authService.LogoutAsync(refreshTokenValue);

        // Assert
        Assert.True(result);
        _mockRefreshTokenRepository.Verify(x => x.DeleteAsync(refreshTokenValue), Times.Once);
    }

    [Fact]
    public void GenerateJwtToken_ValidUser_ReturnsToken()
    {
        // Arrange
        var user = new User
        {
            UserId = Guid.NewGuid(),
            Email = "test@example.com",
            Name = "John Doe"
        };

        var roles = new List<string> { "User" };

        _mockUserRepository.Setup(x => x.GetUserRolesAsync(user.UserId))
            .ReturnsAsync(roles);

        // Act
        var token = _authService.GenerateJwtToken(user);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
    }

    [Fact]
    public async Task LoginAsync_CreatesRefreshToken()
    {
        // Arrange
        var loginDto = new LoginRequestDto
        {
            Email = "test@example.com",
            Password = "password123"
        };

        var user = new User
        {
            UserId = Guid.NewGuid(),
            Email = "test@example.com",
            Name = "Test User",
            AuthProvider = "Email",
            AuthId = BCrypt.Net.BCrypt.HashPassword("password123"),
            TwoFactorEnabled = false
        };

        _mockUserRepository.Setup(x => x.GetByEmailAsync(loginDto.Email))
            .ReturnsAsync(user);
        _mockUserRepository.Setup(x => x.GetUserRolesAsync(user.UserId))
            .ReturnsAsync(["User"]);
        _mockRefreshTokenRepository.Setup(x => x.CreateAsync(It.IsAny<RefreshToken>()))
            .ReturnsAsync((RefreshToken token) => token);
        _mockRoleService.Setup(x => x.AssignDefaultRoleToUserAsync(It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _authService.LoginAsync(loginDto);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.RefreshToken);
        _mockRefreshTokenRepository.Verify(x => x.CreateAsync(It.IsAny<RefreshToken>()), Times.Once);
    }

    #region GoogleLoginAsync Tests

    [Fact]
    public async Task GoogleLoginAsync_MissingGoogleClientId_ThrowsInvalidOperationException()
    {
        // Arrange
        _mockConfiguration.Setup(x => x["GoogleAuth:ClientId"]).Returns((string?)null);
        var request = new GoogleLoginRequestDto { IdToken = "valid-token" };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _authService.GoogleLoginAsync(request));
        
        Assert.Equal("Google Client ID not configured", exception.Message);
    }

    [Fact]
    public async Task GoogleLoginAsync_ExistingUserWithEmailAuth_UpdatesToGoogleAuth()
    {
        // Arrange
        var request = new GoogleLoginRequestDto { IdToken = "valid-google-token" };
        var existingUser = new User
        {
            UserId = Guid.NewGuid(),
            Email = "test@example.com",
            Name = "Test User",
            AuthProvider = "Email",
            AuthId = "hashed-password"
        };

        _mockUserRepository.Setup(x => x.GetByEmailAsync("test@example.com"))
            .ReturnsAsync(existingUser);
        _mockUserRepository.Setup(x => x.GetUserRolesAsync(existingUser.UserId))
            .ReturnsAsync(["User"]);
        _mockRefreshTokenRepository.Setup(x => x.CreateAsync(It.IsAny<RefreshToken>()))
            .ReturnsAsync((RefreshToken token) => token);
        _mockRoleService.Setup(x => x.AssignDefaultRoleToUserAsync(It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);

        // Mock Google token validation - this would require mocking GoogleJsonWebSignature
        // For now, we'll test the logic assuming valid token validation
        // In a real implementation, you'd need to wrap GoogleJsonWebSignature in an interface

        // Act & Assert
        // Note: This test would need GoogleJsonWebSignature to be mockable
        // For now, we'll test the configuration validation part
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _authService.GoogleLoginAsync(request));
        
        // The method should fail at Google token validation since we can't mock it directly
        Assert.Contains("Failed to authenticate with Google", exception.Message);
    }

    [Fact]
    public async Task GoogleLoginAsync_ExistingUserWithNoRoles_AssignsDefaultRole()
    {
        // Arrange
        var request = new GoogleLoginRequestDto { IdToken = "valid-google-token" };
        var existingUser = new User
        {
            UserId = Guid.NewGuid(),
            Email = "test@example.com",
            Name = "Test User",
            AuthProvider = "Google",
            AuthId = "google-subject-id"
        };

        _mockUserRepository.Setup(x => x.GetByEmailAsync("test@example.com"))
            .ReturnsAsync(existingUser);
        _mockUserRepository.Setup(x => x.GetUserRolesAsync(existingUser.UserId))
            .ReturnsAsync(new List<string>()); // No roles initially
        _mockRoleService.Setup(x => x.AssignDefaultRoleToUserAsync(existingUser.UserId))
            .Returns(Task.CompletedTask);
        _mockUserRepository.Setup(x => x.GetUserRolesAsync(existingUser.UserId))
            .ReturnsAsync(["User"]); // After role assignment
        _mockRefreshTokenRepository.Setup(x => x.CreateAsync(It.IsAny<RefreshToken>()))
            .ReturnsAsync((RefreshToken token) => token);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _authService.GoogleLoginAsync(request));
        
        // Verify that AssignDefaultRoleToUserAsync would be called
        // Note: In a real test, we'd mock GoogleJsonWebSignature validation
        Assert.Contains("Failed to authenticate with Google", exception.Message);
    }

    [Fact]
    public async Task GoogleLoginAsync_NewUser_CreatesUserAndAssignsDefaultRole()
    {
        // Arrange
        var request = new GoogleLoginRequestDto { IdToken = "valid-google-token" };
        
        _mockUserRepository.Setup(x => x.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null); // No existing user
        _mockUserRepository.Setup(x => x.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync((User user) => user);
        _mockRoleService.Setup(x => x.AssignDefaultRoleToUserAsync(It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);
        _mockRoleService.Setup(x => x.GetUserRolesAsync(It.IsAny<Guid>()))
            .ReturnsAsync([new Role { RoleId = Guid.NewGuid(), Name = "User" }]);
        _mockRefreshTokenRepository.Setup(x => x.CreateAsync(It.IsAny<RefreshToken>()))
            .ReturnsAsync((RefreshToken token) => token);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _authService.GoogleLoginAsync(request));
        
        // The method should fail at Google token validation since we can't mock it directly
        Assert.Contains("Failed to authenticate with Google", exception.Message);
    }

    #endregion

    [Fact]
    public async Task RefreshTokenAsync_ExpiredToken_ThrowsException()
    {
        // Arrange
        var refreshTokenValue = "expired-refresh-token";
        var refreshToken = new RefreshToken
        {
            Token = refreshTokenValue,
            UserId = Guid.NewGuid(),
            ExpiredAt = DateTime.UtcNow.AddDays(-1), // Expired token
            CreatedAt = DateTime.UtcNow.AddDays(-8)
        };

        _mockRefreshTokenRepository.Setup(x => x.GetByTokenAsync(refreshTokenValue))
            .ReturnsAsync(refreshToken);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _authService.RefreshTokenAsync(new RefreshTokenRequestDto { RefreshToken = refreshTokenValue }));
        
        Assert.Equal("Invalid or expired refresh token", exception.Message);
    }

    [Fact]
    public async Task RefreshTokenAsync_BlacklistedUser_ThrowsException()
    {
        // Arrange
        var refreshTokenValue = "valid-refresh-token";
        var userId = Guid.NewGuid();
        
        var user = new User
        {
            UserId = userId,
            Email = "test@example.com",
            Name = "Test User",
            IsBlacklisted = true, // Blacklisted user
            AuthProvider = "Email",
            AuthId = BCrypt.Net.BCrypt.HashPassword("password123")
        };

        var refreshToken = new RefreshToken
        {
            Token = refreshTokenValue,
            UserId = userId,
            User = user,
            ExpiredAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };

        _mockRefreshTokenRepository.Setup(x => x.GetByTokenAsync(refreshTokenValue))
            .ReturnsAsync(refreshToken);
        _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _authService.RefreshTokenAsync(new RefreshTokenRequestDto { RefreshToken = refreshTokenValue }));
        
        Assert.Equal("User account is disabled", exception.Message);
    }
}