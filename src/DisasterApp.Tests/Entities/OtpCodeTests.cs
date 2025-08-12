using Xunit;
using DisasterApp.Domain.Entities;

namespace DisasterApp.Tests.Entities;

public class OtpCodeTests
{
    [Fact]
    public void OtpCode_DefaultConstructor_SetsDefaultValues()
    {
        // Act
        var otpCode = new OtpCode();

        // Assert
        Assert.NotEqual(Guid.Empty, otpCode.Id); // Should be auto-generated
        Assert.Equal(Guid.Empty, otpCode.UserId);
        Assert.Null(otpCode.Code);
        Assert.Null(otpCode.Type);
        Assert.Equal(DateTime.MinValue, otpCode.ExpiresAt);
        Assert.Null(otpCode.UsedAt);
        Assert.True(otpCode.CreatedAt > DateTime.MinValue); // Should be set to UtcNow
        Assert.Equal(0, otpCode.AttemptCount);
        Assert.Null(otpCode.User);
    }

    [Fact]
    public void OtpCode_SetProperties_SetsCorrectValues()
    {
        // Arrange
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var code = "123456";
        var type = "LOGIN";
        var expiresAt = DateTime.UtcNow.AddMinutes(5);
        var usedAt = DateTime.UtcNow;
        var createdAt = DateTime.UtcNow.AddMinutes(-1);
        var attemptCount = 2;
        var user = new User { UserId = userId, Email = "test@example.com" };

        // Act
        var otpCode = new OtpCode
        {
            Id = id,
            UserId = userId,
            Code = code,
            Type = type,
            ExpiresAt = expiresAt,
            UsedAt = usedAt,
            CreatedAt = createdAt,
            AttemptCount = attemptCount,
            User = user
        };

        // Assert
        Assert.Equal(id, otpCode.Id);
        Assert.Equal(userId, otpCode.UserId);
        Assert.Equal(code, otpCode.Code);
        Assert.Equal(type, otpCode.Type);
        Assert.Equal(expiresAt, otpCode.ExpiresAt);
        Assert.Equal(usedAt, otpCode.UsedAt);
        Assert.Equal(createdAt, otpCode.CreatedAt);
        Assert.Equal(attemptCount, otpCode.AttemptCount);
        Assert.Equal(user, otpCode.User);
    }

    [Theory]
    [InlineData("123456")]
    [InlineData("000000")]
    [InlineData("999999")]
    [InlineData("abcdef")]
    [InlineData("ABCDEF")]
    public void OtpCode_SetCode_AcceptsValidSixCharacterCodes(string code)
    {
        // Arrange
        var otpCode = new OtpCode();

        // Act
        otpCode.Code = code;

        // Assert
        Assert.Equal(code, otpCode.Code);
    }

    [Theory]
    [InlineData("LOGIN")]
    [InlineData("REGISTER")]
    [InlineData("PASSWORD_RESET")]
    [InlineData("2FA")]
    public void OtpCode_SetType_AcceptsValidTypes(string type)
    {
        // Arrange
        var otpCode = new OtpCode();

        // Act
        otpCode.Type = type;

        // Assert
        Assert.Equal(type, otpCode.Type);
    }

    [Fact]
    public void OtpCode_IsValid_ReturnsTrueWhenNotUsedAndNotExpired()
    {
        // Arrange
        var otpCode = new OtpCode
        {
            UsedAt = null,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)
        };

        // Act
        var isValid = otpCode.IsValid;

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void OtpCode_IsValid_ReturnsFalseWhenUsed()
    {
        // Arrange
        var otpCode = new OtpCode
        {
            UsedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)
        };

        // Act
        var isValid = otpCode.IsValid;

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void OtpCode_IsValid_ReturnsFalseWhenExpired()
    {
        // Arrange
        var otpCode = new OtpCode
        {
            UsedAt = null,
            ExpiresAt = DateTime.UtcNow.AddMinutes(-1)
        };

        // Act
        var isValid = otpCode.IsValid;

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void OtpCode_IsValid_ReturnsFalseWhenUsedAndExpired()
    {
        // Arrange
        var otpCode = new OtpCode
        {
            UsedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(-1)
        };

        // Act
        var isValid = otpCode.IsValid;

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void OtpCode_IsExpired_ReturnsTrueWhenExpired()
    {
        // Arrange
        var otpCode = new OtpCode
        {
            ExpiresAt = DateTime.UtcNow.AddMinutes(-1)
        };

        // Act
        var isExpired = otpCode.IsExpired;

        // Assert
        Assert.True(isExpired);
    }

    [Fact]
    public void OtpCode_IsExpired_ReturnsFalseWhenNotExpired()
    {
        // Arrange
        var otpCode = new OtpCode
        {
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)
        };

        // Act
        var isExpired = otpCode.IsExpired;

        // Assert
        Assert.False(isExpired);
    }

    [Fact]
    public void OtpCode_IsExpired_ReturnsTrueWhenExpiresAtEqualsCurrentTime()
    {
        // Arrange
        var currentTime = DateTime.UtcNow;
        var otpCode = new OtpCode
        {
            ExpiresAt = currentTime
        };

        // Act
        var isExpired = otpCode.IsExpired;

        // Assert
        Assert.True(isExpired);
    }

    [Fact]
    public void OtpCode_IsUsed_ReturnsTrueWhenUsedAtIsSet()
    {
        // Arrange
        var otpCode = new OtpCode
        {
            UsedAt = DateTime.UtcNow
        };

        // Act
        var isUsed = otpCode.IsUsed;

        // Assert
        Assert.True(isUsed);
    }

    [Fact]
    public void OtpCode_IsUsed_ReturnsFalseWhenUsedAtIsNull()
    {
        // Arrange
        var otpCode = new OtpCode
        {
            UsedAt = null
        };

        // Act
        var isUsed = otpCode.IsUsed;

        // Assert
        Assert.False(isUsed);
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, false)]
    [InlineData(2, false)]
    [InlineData(3, true)]
    [InlineData(4, true)]
    [InlineData(5, true)]
    public void OtpCode_HasReachedMaxAttempts_ReturnsCorrectValue(int attemptCount, bool expected)
    {
        // Arrange
        var otpCode = new OtpCode
        {
            AttemptCount = attemptCount
        };

        // Act
        var hasReachedMax = otpCode.HasReachedMaxAttempts;

        // Assert
        Assert.Equal(expected, hasReachedMax);
    }

    [Fact]
    public void OtpCode_AttemptCount_CanBeIncremented()
    {
        // Arrange
        var otpCode = new OtpCode
        {
            AttemptCount = 1
        };

        // Act
        otpCode.AttemptCount++;

        // Assert
        Assert.Equal(2, otpCode.AttemptCount);
    }

    [Fact]
    public void OtpCode_UserNavigation_MaintainsReferenceIntegrity()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            UserId = userId,
            Email = "otp@example.com"
        };
        var otpCode = new OtpCode
        {
            UserId = userId,
            User = user
        };

        // Act & Assert
        Assert.Equal(userId, otpCode.UserId);
        Assert.Equal(userId, otpCode.User.UserId);
        Assert.Equal(user.Email, otpCode.User.Email);
    }

    [Fact]
    public void OtpCode_CreatedAt_IsSetToRecentTime()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow.AddSeconds(-1);
        
        // Act
        var otpCode = new OtpCode();
        var afterCreation = DateTime.UtcNow.AddSeconds(1);

        // Assert
        Assert.True(otpCode.CreatedAt >= beforeCreation);
        Assert.True(otpCode.CreatedAt <= afterCreation);
    }

    [Fact]
    public void OtpCode_Id_IsUniqueForEachInstance()
    {
        // Act
        var otpCode1 = new OtpCode();
        var otpCode2 = new OtpCode();

        // Assert
        Assert.NotEqual(otpCode1.Id, otpCode2.Id);
        Assert.NotEqual(Guid.Empty, otpCode1.Id);
        Assert.NotEqual(Guid.Empty, otpCode2.Id);
    }

    [Fact]
    public void OtpCode_ComplexScenario_ValidUnusedCode()
    {
        // Arrange
        var otpCode = new OtpCode
        {
            Code = "123456",
            Type = "LOGIN",
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            UsedAt = null,
            AttemptCount = 1
        };

        // Act & Assert
        Assert.True(otpCode.IsValid);
        Assert.False(otpCode.IsExpired);
        Assert.False(otpCode.IsUsed);
        Assert.False(otpCode.HasReachedMaxAttempts);
    }

    [Fact]
    public void OtpCode_ComplexScenario_ExpiredUsedCodeWithMaxAttempts()
    {
        // Arrange
        var otpCode = new OtpCode
        {
            Code = "654321",
            Type = "2FA",
            ExpiresAt = DateTime.UtcNow.AddMinutes(-1),
            UsedAt = DateTime.UtcNow.AddMinutes(-2),
            AttemptCount = 5
        };

        // Act & Assert
        Assert.False(otpCode.IsValid);
        Assert.True(otpCode.IsExpired);
        Assert.True(otpCode.IsUsed);
        Assert.True(otpCode.HasReachedMaxAttempts);
    }

    [Fact]
    public void OtpCode_MarkAsUsed_UpdatesUsedAtProperty()
    {
        // Arrange
        var otpCode = new OtpCode
        {
            Code = "123456",
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            UsedAt = null
        };
        var beforeUsed = DateTime.UtcNow.AddSeconds(-1);

        // Act
        otpCode.UsedAt = DateTime.UtcNow;
        var afterUsed = DateTime.UtcNow.AddSeconds(1);

        // Assert
        Assert.NotNull(otpCode.UsedAt);
        Assert.True(otpCode.UsedAt >= beforeUsed);
        Assert.True(otpCode.UsedAt <= afterUsed);
        Assert.True(otpCode.IsUsed);
        Assert.False(otpCode.IsValid);
    }
}