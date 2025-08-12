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

public class OtpServiceTests
{
    private readonly Mock<IOtpCodeRepository> _mockOtpCodeRepository;
    private readonly Mock<IOtpAttemptRepository> _mockOtpAttemptRepository;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILogger<OtpService>> _mockLogger;
    private readonly OtpService _otpService;

    public OtpServiceTests()
    {
        _mockOtpCodeRepository = new Mock<IOtpCodeRepository>();
        _mockOtpAttemptRepository = new Mock<IOtpAttemptRepository>();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockEmailService = new Mock<IEmailService>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<OtpService>>();

        // Setup configuration
        _mockConfiguration.Setup(x => x["Otp:ExpiryMinutes"]).Returns("5");
        _mockConfiguration.Setup(x => x["Otp:MaxAttempts"]).Returns("3");
        _mockConfiguration.Setup(x => x["Otp:CodeLength"]).Returns("6");

        _otpService = new OtpService(
            _mockOtpCodeRepository.Object,
            _mockUserRepository.Object,
            _mockEmailService.Object,
            _mockConfiguration.Object,
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task SendOtpAsync_ValidUser_GeneratesOtp()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "test@example.com";
        var type = "EMAIL_VERIFICATION";
        var user = new User
        {
            UserId = userId,
            Email = email,
            IsBlacklisted = false
        };

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);
        _mockOtpCodeRepository.Setup(x => x.CreateAsync(It.IsAny<OtpCode>()))
            .ReturnsAsync(new OtpCode());
        _mockEmailService.Setup(x => x.SendOtpEmailAsync(email, It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        var result = await _otpService.SendOtpAsync(userId, email, type);

        // Assert
        Assert.True(result.Success);
        Assert.Equal($"OTP code sent to {email}. Code expires in 5 minutes.", result.Message);
        _mockOtpCodeRepository.Verify(x => x.CreateAsync(It.IsAny<OtpCode>()), Times.Once);
    }

    [Fact]
    public async Task SendOtpAsync_NonExistingUser_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "test@example.com";
        var type = "EMAIL_VERIFICATION";

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _otpService.SendOtpAsync(userId, email, type);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("User not found", result.Message);
    }

    [Fact]
    public async Task VerifyOtpAsync_ValidOtp_ReturnsSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var code = "123456";
        var type = "EMAIL_VERIFICATION";

        var otpCode = new OtpCode
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Code = code,
            Type = type,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            UsedAt = null,
            CreatedAt = DateTime.UtcNow
        };

        _mockOtpCodeRepository.Setup(x => x.GetByUserAndCodeAsync(userId, code, type))
            .ReturnsAsync(otpCode);
        _mockOtpCodeRepository.Setup(x => x.UpdateAsync(It.IsAny<OtpCode>()))
            .ReturnsAsync(otpCode);

        // Act
        var result = await _otpService.VerifyOtpAsync(userId, code, type);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task VerifyOtpAsync_InvalidOtp_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var code = "123456";
        var type = "EMAIL_VERIFICATION";

        _mockOtpCodeRepository.Setup(x => x.GetByUserAndCodeAsync(userId, code, type))
            .ReturnsAsync((OtpCode?)null);

        // Act
        var result = await _otpService.VerifyOtpAsync(userId, code, type);

        // Assert
        Assert.False(result);
    }

    // This test is removed as the actual OtpService doesn't implement rate limiting - it's handled by RateLimitingService

    [Fact]
    public async Task SendOtpAsync_ValidUserWithoutEmail_GeneratesOtp()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var type = "EMAIL_VERIFICATION";
        var user = new User
        {
            UserId = userId,
            Email = "test@example.com",
            IsBlacklisted = false
        };

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);
        _mockOtpCodeRepository.Setup(x => x.CreateAsync(It.IsAny<OtpCode>()))
            .ReturnsAsync(new OtpCode());
        _mockEmailService.Setup(x => x.SendOtpEmailAsync(user.Email, It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        var result = await _otpService.SendOtpAsync(userId, type);

        // Assert
        Assert.True(result.Success);
        _mockOtpCodeRepository.Verify(x => x.CreateAsync(It.IsAny<OtpCode>()), Times.Once);
    }


}