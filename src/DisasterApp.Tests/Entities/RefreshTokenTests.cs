using Xunit;
using DisasterApp.Domain.Entities;

namespace DisasterApp.Tests.Entities;

public class RefreshTokenTests
{
    [Fact]
    public void RefreshToken_DefaultConstructor_SetsDefaultValues()
    {
        // Act
        var refreshToken = new RefreshToken();

        // Assert
        Assert.Equal(Guid.Empty, refreshToken.RefreshTokenId);
        Assert.Null(refreshToken.Token);
        Assert.Equal(Guid.Empty, refreshToken.UserId);
        Assert.Equal(DateTime.MinValue, refreshToken.ExpiredAt);
        Assert.Null(refreshToken.CreatedAt);
        Assert.Null(refreshToken.User);
    }

    [Fact]
    public void RefreshToken_SetProperties_SetsCorrectValues()
    {
        // Arrange
        var refreshTokenId = Guid.NewGuid();
        var token = "sample-refresh-token-12345";
        var userId = Guid.NewGuid();
        var expiredAt = DateTime.UtcNow.AddDays(7);
        var createdAt = DateTime.UtcNow;
        var user = new User { UserId = userId, Email = "test@example.com" };

        // Act
        var refreshToken = new RefreshToken
        {
            RefreshTokenId = refreshTokenId,
            Token = token,
            UserId = userId,
            ExpiredAt = expiredAt,
            CreatedAt = createdAt,
            User = user
        };

        // Assert
        Assert.Equal(refreshTokenId, refreshToken.RefreshTokenId);
        Assert.Equal(token, refreshToken.Token);
        Assert.Equal(userId, refreshToken.UserId);
        Assert.Equal(expiredAt, refreshToken.ExpiredAt);
        Assert.Equal(createdAt, refreshToken.CreatedAt);
        Assert.Equal(user, refreshToken.User);
    }

    [Fact]
    public void RefreshToken_SetToken_AcceptsValidToken()
    {
        // Arrange
        var refreshToken = new RefreshToken();
        var token = "valid-refresh-token-abcdef123456";

        // Act
        refreshToken.Token = token;

        // Assert
        Assert.Equal(token, refreshToken.Token);
    }

    [Fact]
    public void RefreshToken_SetExpiredAt_AcceptsValidDateTime()
    {
        // Arrange
        var refreshToken = new RefreshToken();
        var expiredAt = DateTime.UtcNow.AddDays(30);

        // Act
        refreshToken.ExpiredAt = expiredAt;

        // Assert
        Assert.Equal(expiredAt, refreshToken.ExpiredAt);
    }

    [Fact]
    public void RefreshToken_SetCreatedAt_AcceptsValidDateTime()
    {
        // Arrange
        var refreshToken = new RefreshToken();
        var createdAt = DateTime.UtcNow;

        // Act
        refreshToken.CreatedAt = createdAt;

        // Assert
        Assert.Equal(createdAt, refreshToken.CreatedAt);
    }

    [Fact]
    public void RefreshToken_SetCreatedAt_AcceptsNull()
    {
        // Arrange
        var refreshToken = new RefreshToken
        {
            CreatedAt = DateTime.UtcNow
        };

        // Act
        refreshToken.CreatedAt = null;

        // Assert
        Assert.Null(refreshToken.CreatedAt);
    }

    [Fact]
    public void RefreshToken_SetUserId_AcceptsValidGuid()
    {
        // Arrange
        var refreshToken = new RefreshToken();
        var userId = Guid.NewGuid();

        // Act
        refreshToken.UserId = userId;

        // Assert
        Assert.Equal(userId, refreshToken.UserId);
    }

    [Fact]
    public void RefreshToken_SetUser_AcceptsValidUser()
    {
        // Arrange
        var refreshToken = new RefreshToken();
        var user = new User
        {
            UserId = Guid.NewGuid(),
            Email = "test@example.com",
            Name = "John Doe"
        };

        // Act
        refreshToken.User = user;

        // Assert
        Assert.Equal(user, refreshToken.User);
        Assert.Equal(user.UserId, refreshToken.User.UserId);
        Assert.Equal(user.Email, refreshToken.User.Email);
    }

    [Theory]
    [InlineData("short-token")]
    [InlineData("very-long-refresh-token-with-many-characters-1234567890-abcdefghijklmnopqrstuvwxyz")]
    [InlineData("token-with-special-chars-!@#$%^&*()_+-=")]
    public void RefreshToken_SetToken_AcceptsVariousTokenFormats(string token)
    {
        // Arrange
        var refreshToken = new RefreshToken();

        // Act
        refreshToken.Token = token;

        // Assert
        Assert.Equal(token, refreshToken.Token);
    }

    [Fact]
    public void RefreshToken_ExpiredAt_CanBeSetToPastDate()
    {
        // Arrange
        var refreshToken = new RefreshToken();
        var pastDate = DateTime.UtcNow.AddDays(-1);

        // Act
        refreshToken.ExpiredAt = pastDate;

        // Assert
        Assert.Equal(pastDate, refreshToken.ExpiredAt);
    }

    [Fact]
    public void RefreshToken_ExpiredAt_CanBeSetToFutureDate()
    {
        // Arrange
        var refreshToken = new RefreshToken();
        var futureDate = DateTime.UtcNow.AddDays(30);

        // Act
        refreshToken.ExpiredAt = futureDate;

        // Assert
        Assert.Equal(futureDate, refreshToken.ExpiredAt);
    }

    [Fact]
    public void RefreshToken_CreatedAt_CanBeSetToPastDate()
    {
        // Arrange
        var refreshToken = new RefreshToken();
        var pastDate = DateTime.UtcNow.AddHours(-1);

        // Act
        refreshToken.CreatedAt = pastDate;

        // Assert
        Assert.Equal(pastDate, refreshToken.CreatedAt);
    }

    [Fact]
    public void RefreshToken_AllProperties_CanBeSetIndependently()
    {
        // Arrange
        var refreshToken = new RefreshToken();
        var refreshTokenId = Guid.NewGuid();
        var token = "independent-token-test";
        var userId = Guid.NewGuid();
        var expiredAt = DateTime.UtcNow.AddDays(15);
        var createdAt = DateTime.UtcNow.AddMinutes(-5);

        // Act
        refreshToken.RefreshTokenId = refreshTokenId;
        refreshToken.Token = token;
        refreshToken.UserId = userId;
        refreshToken.ExpiredAt = expiredAt;
        refreshToken.CreatedAt = createdAt;

        // Assert
        Assert.Equal(refreshTokenId, refreshToken.RefreshTokenId);
        Assert.Equal(token, refreshToken.Token);
        Assert.Equal(userId, refreshToken.UserId);
        Assert.Equal(expiredAt, refreshToken.ExpiredAt);
        Assert.Equal(createdAt, refreshToken.CreatedAt);
    }

    [Fact]
    public void RefreshToken_UserNavigation_MaintainsReferenceIntegrity()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            UserId = userId,
            Email = "navigation@example.com",
            Name = "John Doe"
        };
        var refreshToken = new RefreshToken
        {
            UserId = userId,
            User = user
        };

        // Act & Assert
        Assert.Equal(userId, refreshToken.UserId);
        Assert.Equal("John Doe", refreshToken.User.Name);
        Assert.Equal(user.Email, refreshToken.User.Email);
    }

    [Fact]
    public void RefreshToken_MultipleInstances_AreIndependent()
    {
        // Arrange
        var token1 = new RefreshToken
        {
            RefreshTokenId = Guid.NewGuid(),
            Token = "token-1",
            UserId = Guid.NewGuid(),
            ExpiredAt = DateTime.UtcNow.AddDays(7)
        };

        var token2 = new RefreshToken
        {
            RefreshTokenId = Guid.NewGuid(),
            Token = "token-2",
            UserId = Guid.NewGuid(),
            ExpiredAt = DateTime.UtcNow.AddDays(14)
        };

        // Act & Assert
        Assert.NotEqual(token1.RefreshTokenId, token2.RefreshTokenId);
        Assert.NotEqual(token1.Token, token2.Token);
        Assert.NotEqual(token1.UserId, token2.UserId);
        Assert.NotEqual(token1.ExpiredAt, token2.ExpiredAt);
    }
}