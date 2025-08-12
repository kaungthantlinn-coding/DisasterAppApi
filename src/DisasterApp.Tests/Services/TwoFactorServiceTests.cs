using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using DisasterApp.Application.Services.Implementations;
using DisasterApp.Application.Services.Interfaces;
using DisasterApp.Infrastructure.Repositories.Interfaces;
using DisasterApp.Domain.Entities;
using DisasterApp.Application.DTOs;
using DisasterApp.Domain.Enums;

namespace DisasterApp.Tests.Services;

public class TwoFactorServiceTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IOtpService> _mockOtpService;
    private readonly Mock<IBackupCodeService> _mockBackupCodeService;
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly Mock<ILogger<TwoFactorService>> _mockLogger;
    private readonly TwoFactorService _twoFactorService;
    private readonly Guid _testUserId = Guid.NewGuid();

    public TwoFactorServiceTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockOtpService = new Mock<IOtpService>();
        _mockBackupCodeService = new Mock<IBackupCodeService>();
        _mockEmailService = new Mock<IEmailService>();
        _mockLogger = new Mock<ILogger<TwoFactorService>>();

        _twoFactorService = new TwoFactorService(
            _mockUserRepository.Object,
            _mockOtpService.Object,
            _mockBackupCodeService.Object,
            _mockEmailService.Object,
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task GetTwoFactorStatusAsync_WithValidUser_ReturnsStatus()
    {
        // Arrange
        var user = new User
        {
            UserId = _testUserId,
            TwoFactorEnabled = true,
            BackupCodesRemaining = 5,
            TwoFactorLastUsed = DateTime.UtcNow.AddDays(-1),
            CreatedAt = DateTime.UtcNow.AddMonths(-1)
        };
        _mockUserRepository.Setup(x => x.GetByIdAsync(_testUserId))
            .ReturnsAsync(user);

        // Act
        var result = await _twoFactorService.GetTwoFactorStatusAsync(_testUserId);

        // Assert
        Assert.True(result.TwoFactorEnabled);
        Assert.Equal(5, result.BackupCodesRemaining);
        Assert.NotNull(result.LastUsed);
        Assert.NotNull(result.EnabledAt);
    }

    [Fact]
    public async Task GetTwoFactorStatusAsync_WithNullUser_ThrowsException()
    {
        // Arrange
        _mockUserRepository.Setup(x => x.GetByIdAsync(_testUserId))
            .ReturnsAsync((User)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _twoFactorService.GetTwoFactorStatusAsync(_testUserId));
    }

    [Fact]
    public async Task SetupTwoFactorAsync_WithValidEmailUser_ReturnsSuccess()
    {
        // Arrange
        var user = new User
        {
            UserId = _testUserId,
            Email = "test@example.com",
            AuthProvider = "Email",
            AuthId = BCrypt.Net.BCrypt.HashPassword("password123"),
            TwoFactorEnabled = false
        };
        _mockUserRepository.Setup(x => x.GetByIdAsync(_testUserId))
            .ReturnsAsync(user);
        _mockOtpService.Setup(x => x.SendOtpAsync(_testUserId, user.Email, OtpCodeTypes.Setup))
            .ReturnsAsync(new SendOtpResponseDto { Success = true });

        // Act
        var result = await _twoFactorService.SetupTwoFactorAsync(_testUserId, "password123");

        // Assert
        Assert.True(result.Success);
        Assert.Contains("Setup verification code sent", result.Message);
        Assert.NotNull(result.Instructions);
    }

    [Fact]
    public async Task SetupTwoFactorAsync_WithInvalidPassword_ReturnsFailure()
    {
        // Arrange
        var user = new User
        {
            UserId = _testUserId,
            Email = "test@example.com",
            AuthProvider = "Email",
            AuthId = BCrypt.Net.BCrypt.HashPassword("password123"),
            TwoFactorEnabled = false
        };
        _mockUserRepository.Setup(x => x.GetByIdAsync(_testUserId))
            .ReturnsAsync(user);

        // Act
        var result = await _twoFactorService.SetupTwoFactorAsync(_testUserId, "wrongpassword");

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Invalid current password", result.Message);
    }

    [Fact]
    public async Task SetupTwoFactorAsync_WithAlreadyEnabled2FA_ReturnsFailure()
    {
        // Arrange
        var user = new User
        {
            UserId = _testUserId,
            Email = "test@example.com",
            AuthProvider = "Email",
            AuthId = BCrypt.Net.BCrypt.HashPassword("password123"),
            TwoFactorEnabled = true
        };
        _mockUserRepository.Setup(x => x.GetByIdAsync(_testUserId))
            .ReturnsAsync(user);

        // Act
        var result = await _twoFactorService.SetupTwoFactorAsync(_testUserId, "password123");

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Two-factor authentication is already enabled", result.Message);
    }

    [Fact]
    public async Task SetupTwoFactorAsync_WithNonEmailProvider_SkipsPasswordCheck()
    {
        // Arrange
        var user = new User
        {
            UserId = _testUserId,
            Email = "test@example.com",
            AuthProvider = "Google",
            TwoFactorEnabled = false
        };
        _mockUserRepository.Setup(x => x.GetByIdAsync(_testUserId))
            .ReturnsAsync(user);
        _mockOtpService.Setup(x => x.SendOtpAsync(_testUserId, user.Email, OtpCodeTypes.Setup))
            .ReturnsAsync(new SendOtpResponseDto { Success = true });

        // Act
        var result = await _twoFactorService.SetupTwoFactorAsync(_testUserId, "anypassword");

        // Assert
        Assert.True(result.Success);
    }

    [Fact]
    public async Task VerifySetupAsync_WithValidOtp_EnablesTwoFactor()
    {
        // Arrange
        var user = new User
        {
            UserId = _testUserId,
            Email = "test@example.com",
            TwoFactorEnabled = false
        };
        var backupCodes = new List<string> { "code1", "code2", "code3" };
        
        _mockUserRepository.Setup(x => x.GetByIdAsync(_testUserId))
            .ReturnsAsync(user);
        _mockOtpService.Setup(x => x.VerifyOtpAsync(_testUserId, "123456", OtpCodeTypes.Setup))
            .ReturnsAsync(true);
        _mockBackupCodeService.Setup(x => x.GenerateBackupCodesAsync(_testUserId, It.IsAny<int>()))
            .ReturnsAsync(backupCodes);
        _mockEmailService.Setup(x => x.SendTwoFactorEnabledEmailAsync(user.Email))
            .ReturnsAsync(true);

        // Act
        var result = await _twoFactorService.VerifySetupAsync(_testUserId, "123456");

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Two-factor authentication has been enabled successfully", result.Message);
        Assert.Equal(backupCodes, result.BackupCodes);
        _mockUserRepository.Verify(x => x.UpdateAsync(It.Is<User>(u => u.TwoFactorEnabled)), Times.Once);
    }

    [Fact]
    public async Task VerifySetupAsync_WithInvalidOtp_ReturnsFailure()
    {
        // Arrange
        var user = new User
        {
            UserId = _testUserId,
            Email = "test@example.com",
            TwoFactorEnabled = false
        };
        
        _mockUserRepository.Setup(x => x.GetByIdAsync(_testUserId))
            .ReturnsAsync(user);
        _mockOtpService.Setup(x => x.VerifyOtpAsync(_testUserId, "123456", OtpCodeTypes.Setup))
            .ReturnsAsync(false);

        // Act
        var result = await _twoFactorService.VerifySetupAsync(_testUserId, "123456");

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Invalid or expired verification code", result.Message);
    }

    [Fact]
    public async Task DisableTwoFactorAsync_WithValidCredentials_DisablesTwoFactor()
    {
        // Arrange
        var user = new User
        {
            UserId = _testUserId,
            Email = "test@example.com",
            AuthProvider = "Email",
            AuthId = BCrypt.Net.BCrypt.HashPassword("password123"),
            TwoFactorEnabled = true,
            BackupCodesRemaining = 5
        };
        
        _mockUserRepository.Setup(x => x.GetByIdAsync(_testUserId))
            .ReturnsAsync(user);
        _mockOtpService.Setup(x => x.VerifyOtpAsync(_testUserId, "123456", OtpCodeTypes.Disable))
            .ReturnsAsync(true);
        _mockBackupCodeService.Setup(x => x.InvalidateAllBackupCodesAsync(_testUserId))
            .ReturnsAsync(1);
        _mockOtpService.Setup(x => x.InvalidateCodesAsync(_testUserId, It.IsAny<string>()))
            .ReturnsAsync(1);
        _mockEmailService.Setup(x => x.SendTwoFactorDisabledEmailAsync(user.Email))
            .ReturnsAsync(true);

        // Act
        var result = await _twoFactorService.DisableTwoFactorAsync(_testUserId, "password123", "123456");

        // Assert
        Assert.True(result);
        _mockUserRepository.Verify(x => x.UpdateAsync(It.Is<User>(u => 
            !u.TwoFactorEnabled && 
            u.BackupCodesRemaining == 0 && 
            u.TwoFactorLastUsed == null)), Times.Once);
    }

    [Fact]
    public async Task DisableTwoFactorAsync_WithInvalidPassword_ReturnsFalse()
    {
        // Arrange
        var user = new User
        {
            UserId = _testUserId,
            Email = "test@example.com",
            AuthProvider = "Email",
            AuthId = BCrypt.Net.BCrypt.HashPassword("password123"),
            TwoFactorEnabled = true
        };
        
        _mockUserRepository.Setup(x => x.GetByIdAsync(_testUserId))
            .ReturnsAsync(user);

        // Act
        var result = await _twoFactorService.DisableTwoFactorAsync(_testUserId, "wrongpassword");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GenerateBackupCodesAsync_WithValidCredentials_ReturnsNewCodes()
    {
        // Arrange
        var user = new User
        {
            UserId = _testUserId,
            Email = "test@example.com",
            AuthProvider = "Email",
            AuthId = BCrypt.Net.BCrypt.HashPassword("password123"),
            TwoFactorEnabled = true
        };
        var backupCodes = new List<string> { "code1", "code2", "code3", "code4", "code5" };
        
        _mockUserRepository.Setup(x => x.GetByIdAsync(_testUserId))
            .ReturnsAsync(user);
        _mockOtpService.Setup(x => x.VerifyOtpAsync(_testUserId, "123456", OtpCodeTypes.BackupGenerate))
            .ReturnsAsync(true);
        _mockBackupCodeService.Setup(x => x.GenerateBackupCodesAsync(_testUserId, It.IsAny<int>()))
            .ReturnsAsync(backupCodes);

        // Act
        var result = await _twoFactorService.GenerateBackupCodesAsync(_testUserId, "password123", "123456");

        // Assert
        Assert.True(result.Success);
        Assert.Equal("New backup codes generated successfully", result.Message);
        Assert.Equal(backupCodes, result.BackupCodes);
        Assert.Equal(5, result.CodesGenerated);
    }

    [Fact]
    public async Task GenerateBackupCodesAsync_WithTwoFactorDisabled_ReturnsFailure()
    {
        // Arrange
        var user = new User
        {
            UserId = _testUserId,
            Email = "test@example.com",
            TwoFactorEnabled = false
        };
        
        _mockUserRepository.Setup(x => x.GetByIdAsync(_testUserId))
            .ReturnsAsync(user);

        // Act
        var result = await _twoFactorService.GenerateBackupCodesAsync(_testUserId, "password123");

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Two-factor authentication is not enabled", result.Message);
    }

    [Fact]
    public async Task IsTwoFactorEnabledAsync_WithEnabledUser_ReturnsTrue()
    {
        // Arrange
        var user = new User
        {
            UserId = _testUserId,
            TwoFactorEnabled = true
        };
        _mockUserRepository.Setup(x => x.GetByIdAsync(_testUserId))
            .ReturnsAsync(user);

        // Act
        var result = await _twoFactorService.IsTwoFactorEnabledAsync(_testUserId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsTwoFactorEnabledAsync_WithDisabledUser_ReturnsFalse()
    {
        // Arrange
        var user = new User
        {
            UserId = _testUserId,
            TwoFactorEnabled = false
        };
        _mockUserRepository.Setup(x => x.GetByIdAsync(_testUserId))
            .ReturnsAsync(user);

        // Act
        var result = await _twoFactorService.IsTwoFactorEnabledAsync(_testUserId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsTwoFactorEnabledAsync_WithNullUser_ReturnsFalse()
    {
        // Arrange
        _mockUserRepository.Setup(x => x.GetByIdAsync(_testUserId))
            .ReturnsAsync((User)null);

        // Act
        var result = await _twoFactorService.IsTwoFactorEnabledAsync(_testUserId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task UpdateLastUsedAsync_WithValidUser_UpdatesTimestamp()
    {
        // Arrange
        var user = new User
        {
            UserId = _testUserId,
            TwoFactorLastUsed = null
        };
        _mockUserRepository.Setup(x => x.GetByIdAsync(_testUserId))
            .ReturnsAsync(user);

        // Act
        await _twoFactorService.UpdateLastUsedAsync(_testUserId);

        // Assert
        _mockUserRepository.Verify(x => x.UpdateAsync(It.Is<User>(u => u.TwoFactorLastUsed != null)), Times.Once);
    }

    [Fact]
    public async Task UpdateLastUsedAsync_WithNullUser_DoesNotThrow()
    {
        // Arrange
        _mockUserRepository.Setup(x => x.GetByIdAsync(_testUserId))
            .ReturnsAsync((User)null);

        // Act & Assert
        await _twoFactorService.UpdateLastUsedAsync(_testUserId);
        // Should not throw exception
    }

    [Fact]
    public async Task SetupTwoFactorAsync_WithException_ReturnsErrorResponse()
    {
        // Arrange
        _mockUserRepository.Setup(x => x.GetByIdAsync(_testUserId))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _twoFactorService.SetupTwoFactorAsync(_testUserId, "password123");

        // Assert
        Assert.False(result.Success);
        Assert.Equal("An error occurred during 2FA setup", result.Message);
    }

    [Fact]
    public async Task VerifySetupAsync_WithException_ReturnsErrorResponse()
    {
        // Arrange
        _mockUserRepository.Setup(x => x.GetByIdAsync(_testUserId))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _twoFactorService.VerifySetupAsync(_testUserId, "123456");

        // Assert
        Assert.False(result.Success);
        Assert.Equal("An error occurred during 2FA setup verification", result.Message);
    }

    [Fact]
    public async Task DisableTwoFactorAsync_WithException_ReturnsFalse()
    {
        // Arrange
        _mockUserRepository.Setup(x => x.GetByIdAsync(_testUserId))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _twoFactorService.DisableTwoFactorAsync(_testUserId, "password123");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GenerateBackupCodesAsync_WithException_ReturnsErrorResponse()
    {
        // Arrange
        _mockUserRepository.Setup(x => x.GetByIdAsync(_testUserId))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _twoFactorService.GenerateBackupCodesAsync(_testUserId, "password123");

        // Assert
        Assert.False(result.Success);
        Assert.Equal("An error occurred while generating backup codes", result.Message);
    }

    [Fact]
    public async Task IsTwoFactorEnabledAsync_WithException_ReturnsFalse()
    {
        // Arrange
        _mockUserRepository.Setup(x => x.GetByIdAsync(_testUserId))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _twoFactorService.IsTwoFactorEnabledAsync(_testUserId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task UpdateLastUsedAsync_WithException_DoesNotThrow()
    {
        // Arrange
        _mockUserRepository.Setup(x => x.GetByIdAsync(_testUserId))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await _twoFactorService.UpdateLastUsedAsync(_testUserId);
        // Should not throw exception
    }
}