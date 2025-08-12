using Xunit;
using DisasterApp.Domain.Entities;

namespace DisasterApp.Tests.Entities;

public class PasswordResetTokenTests
{
    [Fact]
    public void PasswordResetToken_DefaultConstructor_SetsDefaultValues()
    {
        // Act
        var token = new PasswordResetToken();

        // Assert
        Assert.Equal(Guid.Empty, token.PasswordResetTokenId);
        Assert.Null(token.Token);
        Assert.Equal(Guid.Empty, token.UserId);
        Assert.Equal(DateTime.MinValue, token.ExpiredAt);
        Assert.Null(token.CreatedAt);
        Assert.False(token.IsUsed);
        Assert.Null(token.User);
    }

    [Fact]
    public void PasswordResetToken_SetProperties_SetsCorrectValues()
    {
        // Arrange
        var tokenId = Guid.NewGuid();
        var tokenValue = "reset-token-123456";
        var userId = Guid.NewGuid();
        var expiredAt = DateTime.UtcNow.AddHours(1);
        var createdAt = DateTime.UtcNow;
        var isUsed = true;
        var user = new User { UserId = userId, Email = "test@example.com", Name = "John Doe" };

        // Act
        var token = new PasswordResetToken
        {
            PasswordResetTokenId = tokenId,
            Token = tokenValue,
            UserId = userId,
            ExpiredAt = expiredAt,
            CreatedAt = createdAt,
            IsUsed = isUsed,
            User = user
        };

        // Assert
        Assert.Equal(tokenId, token.PasswordResetTokenId);
        Assert.Equal(tokenValue, token.Token);
        Assert.Equal(userId, token.UserId);
        Assert.Equal(expiredAt, token.ExpiredAt);
        Assert.Equal(createdAt, token.CreatedAt);
        Assert.Equal(isUsed, token.IsUsed);
        Assert.Contains("Doe", token.User.Name);
    }

    [Theory]
    [InlineData("simple-token")]
    [InlineData("complex-token-with-numbers-123456")]
    [InlineData("TOKEN_WITH_UNDERSCORES")]
    [InlineData("token.with.dots")]
    [InlineData("token-with-special-chars!@#")]
    [InlineData("very-long-token-string-that-might-be-used-in-production-environments-with-high-security-requirements")]
    public void PasswordResetToken_SetToken_AcceptsVariousTokenFormats(string tokenValue)
    {
        // Arrange
        var token = new PasswordResetToken();

        // Act
        token.Token = tokenValue;

        // Assert
        Assert.Equal(tokenValue, token.Token);
    }

    [Fact]
    public void PasswordResetToken_SetExpiredAt_AcceptsPastDate()
    {
        // Arrange
        var token = new PasswordResetToken();
        var pastDate = DateTime.UtcNow.AddHours(-1);

        // Act
        token.ExpiredAt = pastDate;

        // Assert
        Assert.Equal(pastDate, token.ExpiredAt);
    }

    [Fact]
    public void PasswordResetToken_SetExpiredAt_AcceptsFutureDate()
    {
        // Arrange
        var token = new PasswordResetToken();
        var futureDate = DateTime.UtcNow.AddHours(24);

        // Act
        token.ExpiredAt = futureDate;

        // Assert
        Assert.Equal(futureDate, token.ExpiredAt);
    }

    [Fact]
    public void PasswordResetToken_SetCreatedAt_AcceptsNullValue()
    {
        // Arrange
        var token = new PasswordResetToken();

        // Act
        token.CreatedAt = null;

        // Assert
        Assert.Null(token.CreatedAt);
    }

    [Fact]
    public void PasswordResetToken_SetCreatedAt_AcceptsDateTimeValue()
    {
        // Arrange
        var token = new PasswordResetToken();
        var createdDate = DateTime.UtcNow;

        // Act
        token.CreatedAt = createdDate;

        // Assert
        Assert.Equal(createdDate, token.CreatedAt);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void PasswordResetToken_SetIsUsed_AcceptsBooleanValues(bool isUsed)
    {
        // Arrange
        var token = new PasswordResetToken();

        // Act
        token.IsUsed = isUsed;

        // Assert
        Assert.Equal(isUsed, token.IsUsed);
    }

    [Fact]
    public void PasswordResetToken_UserNavigation_MaintainsReferenceIntegrity()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            UserId = userId,
            Email = "reset@example.com",
            Name = "John Doe"
        };
        var token = new PasswordResetToken
        {
            UserId = userId,
            User = user
        };

        // Act & Assert
        Assert.Equal(userId, token.UserId);
        Assert.Equal("John Doe", token.User.Name);
        Assert.Equal(user.Email, token.User.Email);
        Assert.Equal(user.Name, token.User.Name);
    }

    [Fact]
    public void PasswordResetToken_PropertiesAreIndependent_CanBeSetSeparately()
    {
        // Arrange
        var token = new PasswordResetToken();
        var tokenId = Guid.NewGuid();
        var tokenValue = "independent-token";
        var userId = Guid.NewGuid();
        var expiredAt = DateTime.UtcNow.AddMinutes(30);
        var createdAt = DateTime.UtcNow.AddMinutes(-5);

        // Act
        token.PasswordResetTokenId = tokenId;
        Assert.Equal(tokenId, token.PasswordResetTokenId);
        Assert.Null(token.Token);

        token.Token = tokenValue;
        Assert.Equal(tokenValue, token.Token);
        Assert.Equal(Guid.Empty, token.UserId);

        token.UserId = userId;
        Assert.Equal(userId, token.UserId);
        Assert.Equal(DateTime.MinValue, token.ExpiredAt);

        token.ExpiredAt = expiredAt;
        Assert.Equal(expiredAt, token.ExpiredAt);
        Assert.Null(token.CreatedAt);

        token.CreatedAt = createdAt;
        Assert.Equal(createdAt, token.CreatedAt);
        Assert.False(token.IsUsed);

        token.IsUsed = true;
        Assert.True(token.IsUsed);
    }

    [Fact]
    public void PasswordResetToken_MultipleInstances_AreIndependent()
    {
        // Arrange & Act
        var token1 = new PasswordResetToken
        {
            PasswordResetTokenId = Guid.NewGuid(),
            Token = "token-1",
            UserId = Guid.NewGuid(),
            IsUsed = false
        };

        var token2 = new PasswordResetToken
        {
            PasswordResetTokenId = Guid.NewGuid(),
            Token = "token-2",
            UserId = Guid.NewGuid(),
            IsUsed = true
        };

        // Assert
        Assert.NotEqual(token1.PasswordResetTokenId, token2.PasswordResetTokenId);
        Assert.NotEqual(token1.Token, token2.Token);
        Assert.NotEqual(token1.UserId, token2.UserId);
        Assert.NotEqual(token1.IsUsed, token2.IsUsed);
    }

    [Fact]
    public void PasswordResetToken_ComplexScenario_ValidUnusedToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            UserId = userId,
            Email = "user@example.com",
            Name = "John Doe"
        };

        var token = new PasswordResetToken
        {
            PasswordResetTokenId = Guid.NewGuid(),
            Token = "valid-reset-token-123",
            UserId = userId,
            ExpiredAt = DateTime.UtcNow.AddHours(1),
            CreatedAt = DateTime.UtcNow.AddMinutes(-5),
            IsUsed = false,
            User = user
        };

        // Act & Assert
        Assert.NotEqual(Guid.Empty, token.PasswordResetTokenId);
        Assert.Equal("valid-reset-token-123", token.Token);
        Assert.Equal(userId, token.UserId);
        Assert.True(token.ExpiredAt > DateTime.UtcNow);
        Assert.True(token.CreatedAt < DateTime.UtcNow);
        Assert.False(token.IsUsed);
        Assert.Equal(user.Email, token.User.Email);
    }

    [Fact]
    public void PasswordResetToken_ComplexScenario_ExpiredUsedToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            UserId = userId,
            Email = "expired@example.com",
            Name = "Jane Doe"
        };

        var token = new PasswordResetToken
        {
            PasswordResetTokenId = Guid.NewGuid(),
            Token = "expired-used-token-456",
            UserId = userId,
            ExpiredAt = DateTime.UtcNow.AddHours(-1),
            CreatedAt = DateTime.UtcNow.AddHours(-2),
            IsUsed = true,
            User = user
        };

        // Act & Assert
        Assert.NotEqual(Guid.Empty, token.PasswordResetTokenId);
        Assert.Equal("expired-used-token-456", token.Token);
        Assert.Equal(userId, token.UserId);
        Assert.True(token.ExpiredAt < DateTime.UtcNow);
        Assert.True(token.CreatedAt < DateTime.UtcNow);
        Assert.True(token.IsUsed);
        Assert.Equal(user.Email, token.User.Email);
    }

    [Fact]
    public void PasswordResetToken_MarkAsUsed_UpdatesIsUsedProperty()
    {
        // Arrange
        var token = new PasswordResetToken
        {
            Token = "test-token",
            IsUsed = false
        };

        // Act
        token.IsUsed = true;

        // Assert
        Assert.True(token.IsUsed);
    }

    [Fact]
    public void PasswordResetToken_GuidProperties_CanBeSetToNewGuid()
    {
        // Arrange
        var token = new PasswordResetToken();
        var newTokenId = Guid.NewGuid();
        var newUserId = Guid.NewGuid();

        // Act
        token.PasswordResetTokenId = newTokenId;
        token.UserId = newUserId;

        // Assert
        Assert.Equal(newTokenId, token.PasswordResetTokenId);
        Assert.Equal(newUserId, token.UserId);
        Assert.NotEqual(Guid.Empty, token.PasswordResetTokenId);
        Assert.NotEqual(Guid.Empty, token.UserId);
    }
}