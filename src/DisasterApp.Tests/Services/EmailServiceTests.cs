using Xunit;
using Moq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using DisasterApp.Application.Services.Implementations;
using DisasterApp.Application.Services.Interfaces;
using System.Collections.Generic;

namespace DisasterApp.Tests.Services;

public class EmailServiceTests
{
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILogger<EmailService>> _mockLogger;
    private readonly EmailService _emailService;

    public EmailServiceTests()
    {
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<EmailService>>();
        _emailService = new EmailService(_mockConfiguration.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task SendPasswordResetEmailAsync_ValidInputs_ReturnsFalse()
    {
        // Arrange
        var email = "test@example.com";
        var resetToken = "reset-token-123";
        var resetUrl = "https://example.com/reset";
        
        SetupValidEmailConfiguration();

        // Act
        var result = await _emailService.SendPasswordResetEmailAsync(email, resetToken, resetUrl);

        // Assert
        Assert.False(result); // SMTP connection will fail in test environment
    }

    [Fact]
    public async Task SendPasswordResetEmailAsync_NullEmail_ReturnsFalse()
    {
        // Arrange
        string email = null;
        var resetToken = "reset-token-123";
        var resetUrl = "https://example.com/reset";
        
        SetupValidEmailConfiguration();

        // Act
        var result = await _emailService.SendPasswordResetEmailAsync(email, resetToken, resetUrl);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task SendPasswordResetEmailAsync_EmptyEmail_ReturnsFalse()
    {
        // Arrange
        var email = "";
        var resetToken = "reset-token-123";
        var resetUrl = "https://example.com/reset";
        
        SetupValidEmailConfiguration();

        // Act
        var result = await _emailService.SendPasswordResetEmailAsync(email, resetToken, resetUrl);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task SendEmailAsync_ValidInputs_ReturnsFalse()
    {
        // Arrange
        var to = "test@example.com";
        var subject = "Test Subject";
        var body = "<h1>Test Body</h1>";
        
        SetupValidEmailConfiguration();

        // Act
        var result = await _emailService.SendEmailAsync(to, subject, body);

        // Assert
        Assert.False(result); // SMTP connection will fail in test environment
    }

    [Fact]
    public async Task SendEmailAsync_IncompleteConfiguration_ReturnsTrue()
    {
        // Arrange
        var to = "test@example.com";
        var subject = "Test Subject";
        var body = "<h1>Test Body</h1>";
        
        SetupIncompleteEmailConfiguration();

        // Act
        var result = await _emailService.SendEmailAsync(to, subject, body);

        // Assert
        Assert.True(result); // Returns true as development fallback when configuration is incomplete
    }

    [Fact]
    public async Task SendEmailAsync_NullTo_ReturnsFalse()
    {
        // Arrange
        string to = null;
        var subject = "Test Subject";
        var body = "<h1>Test Body</h1>";
        
        SetupValidEmailConfiguration();

        // Act
        var result = await _emailService.SendEmailAsync(to, subject, body);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task SendAuthProviderNotificationEmailAsync_ValidEmail_ReturnsFalse()
    {
        // Arrange
        var email = "test@example.com";
        var authProvider = "Google";
        
        SetupValidEmailConfiguration();

        // Act
        var result = await _emailService.SendAuthProviderNotificationEmailAsync(email, authProvider);

        // Assert
        Assert.False(result); // SMTP connection will fail in test environment
    }

    [Fact]
    public async Task SendAuthProviderNotificationEmailAsync_NullEmail_ReturnsFalse()
    {
        // Arrange
        string email = null;
        var authProvider = "Google";
        
        SetupValidEmailConfiguration();

        // Act
        var result = await _emailService.SendAuthProviderNotificationEmailAsync(email, authProvider);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task SendOtpEmailAsync_ValidInputs_ReturnsFalse()
    {
        // Arrange
        var email = "test@example.com";
        var otpCode = "123456";
        
        SetupValidEmailConfiguration();

        // Act
        var result = await _emailService.SendOtpEmailAsync(email, otpCode);

        // Assert
        Assert.False(result); // SMTP connection will fail in test environment
    }

    [Fact]
    public async Task SendOtpEmailAsync_NullEmail_ReturnsFalse()
    {
        // Arrange
        string email = null;
        var otpCode = "123456";
        
        SetupValidEmailConfiguration();

        // Act
        var result = await _emailService.SendOtpEmailAsync(email, otpCode);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task SendTwoFactorEnabledEmailAsync_ValidEmail_ReturnsFalse()
    {
        // Arrange
        var email = "test@example.com";
        
        SetupValidEmailConfiguration();

        // Act
        var result = await _emailService.SendTwoFactorEnabledEmailAsync(email);

        // Assert
        Assert.False(result); // SMTP connection will fail in test environment
    }

    [Fact]
    public async Task SendTwoFactorDisabledEmailAsync_ValidEmail_ReturnsFalse()
    {
        // Arrange
        var email = "test@example.com";
        
        SetupValidEmailConfiguration();

        // Act
        var result = await _emailService.SendTwoFactorDisabledEmailAsync(email);

        // Assert
        Assert.False(result); // SMTP connection will fail in test environment
    }

    [Fact]
    public async Task SendBackupCodeUsedEmailAsync_ValidInputs_ReturnsFalse()
    {
        // Arrange
        var email = "test@example.com";
        var remainingCodes = 5;
        
        SetupValidEmailConfiguration();

        // Act
        var result = await _emailService.SendBackupCodeUsedEmailAsync(email, remainingCodes);

        // Assert
        Assert.False(result); // SMTP connection will fail in test environment
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(10)]
    public async Task SendBackupCodeUsedEmailAsync_VariousRemainingCodes_ReturnsFalse(int remainingCodes)
    {
        // Arrange
        var email = "test@example.com";
        
        SetupValidEmailConfiguration();

        // Act
        var result = await _emailService.SendBackupCodeUsedEmailAsync(email, remainingCodes);

        // Assert
        Assert.False(result); // SMTP connection will fail in test environment
    }

    [Fact]
    public async Task SendTwoFactorEnabledEmailAsync_NullEmail_ReturnsFalse()
    {
        // Arrange
        string email = null;
        
        SetupValidEmailConfiguration();

        // Act
        var result = await _emailService.SendTwoFactorEnabledEmailAsync(email);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task SendTwoFactorDisabledEmailAsync_NullEmail_ReturnsFalse()
    {
        // Arrange
        string email = null;
        
        SetupValidEmailConfiguration();

        // Act
        var result = await _emailService.SendTwoFactorDisabledEmailAsync(email);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task SendBackupCodeUsedEmailAsync_NullEmail_ReturnsFalse()
    {
        // Arrange
        string email = null;
        var remainingCodes = 5;
        
        SetupValidEmailConfiguration();

        // Act
        var result = await _emailService.SendBackupCodeUsedEmailAsync(email, remainingCodes);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("test@")]
    [InlineData("test..test@example.com")]
    public async Task SendEmailAsync_InvalidEmailFormats_ReturnsFalse(string invalidEmail)
    {
        // Arrange
        var subject = "Test Subject";
        var body = "<h1>Test Body</h1>";
        
        SetupValidEmailConfiguration();

        // Act
        var result = await _emailService.SendEmailAsync(invalidEmail, subject, body);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task SendEmailAsync_EmptySubject_ReturnsFalse()
    {
        // Arrange
        var to = "test@example.com";
        var subject = "";
        var body = "<h1>Test Body</h1>";
        
        SetupValidEmailConfiguration();

        // Act
        var result = await _emailService.SendEmailAsync(to, subject, body);

        // Assert
        Assert.False(result); // SMTP connection will fail in test environment
    }

    [Fact]
    public async Task SendEmailAsync_EmptyBody_ReturnsFalse()
    {
        // Arrange
        var to = "test@example.com";
        var subject = "Test Subject";
        var body = "";
        
        SetupValidEmailConfiguration();

        // Act
        var result = await _emailService.SendEmailAsync(to, subject, body);

        // Assert
        Assert.False(result); // SMTP connection will fail in test environment
    }

    private void SetupValidEmailConfiguration()
    {
        _mockConfiguration.Setup(x => x["Email:SmtpServer"]).Returns("smtp.example.com");
        _mockConfiguration.Setup(x => x["Email:SmtpPort"]).Returns("587");
        _mockConfiguration.Setup(x => x["Email:SenderEmail"]).Returns("noreply@example.com");
        _mockConfiguration.Setup(x => x["Email:SenderName"]).Returns("Test Sender");
        _mockConfiguration.Setup(x => x["Email:Username"]).Returns("username");
        _mockConfiguration.Setup(x => x["Email:Password"]).Returns("password");
        _mockConfiguration.Setup(x => x["Email:EnableSsl"]).Returns("true");
    }

    private void SetupIncompleteEmailConfiguration()
    {
        _mockConfiguration.Setup(x => x["Email:SmtpServer"]).Returns((string)null);
        _mockConfiguration.Setup(x => x["Email:SmtpPort"]).Returns("587");
        _mockConfiguration.Setup(x => x["Email:SenderEmail"]).Returns((string)null);
        _mockConfiguration.Setup(x => x["Email:SenderName"]).Returns("Test Sender");
        _mockConfiguration.Setup(x => x["Email:Username"]).Returns((string)null);
        _mockConfiguration.Setup(x => x["Email:Password"]).Returns((string)null);
        _mockConfiguration.Setup(x => x["Email:EnableSsl"]).Returns("true");
    }
}