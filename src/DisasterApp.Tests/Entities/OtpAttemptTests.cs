using Xunit;
using DisasterApp.Domain.Entities;

namespace DisasterApp.Tests.Entities;

public class OtpAttemptTests
{
    [Fact]
    public void OtpAttempt_DefaultConstructor_SetsDefaultValues()
    {
        // Act
        var otpAttempt = new OtpAttempt();

        // Assert
        Assert.NotEqual(Guid.Empty, otpAttempt.Id); // Should be auto-generated
        Assert.Null(otpAttempt.UserId);
        Assert.Null(otpAttempt.IpAddress);
        Assert.Null(otpAttempt.AttemptType);
        Assert.True(otpAttempt.AttemptedAt > DateTime.MinValue); // Should be set to UtcNow
        Assert.False(otpAttempt.Success);
        Assert.Null(otpAttempt.Email);
        Assert.Null(otpAttempt.User);
    }

    [Fact]
    public void OtpAttempt_SetProperties_SetsCorrectValues()
    {
        // Arrange
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var ipAddress = "192.168.1.1";
        var attemptType = "login";
        var attemptedAt = DateTime.UtcNow.AddMinutes(-5);
        var success = true;
        var email = "test@example.com";
        var user = new User { UserId = userId, Email = email };

        // Act
        var otpAttempt = new OtpAttempt
        {
            Id = id,
            UserId = userId,
            IpAddress = ipAddress,
            AttemptType = attemptType,
            AttemptedAt = attemptedAt,
            Success = success,
            Email = email,
            User = user
        };

        // Assert
        Assert.Equal(id, otpAttempt.Id);
        Assert.Equal(userId, otpAttempt.UserId);
        Assert.Equal(ipAddress, otpAttempt.IpAddress);
        Assert.Equal(attemptType, otpAttempt.AttemptType);
        Assert.Equal(attemptedAt, otpAttempt.AttemptedAt);
        Assert.Equal(success, otpAttempt.Success);
        Assert.Equal(email, otpAttempt.Email);
        Assert.Equal(user, otpAttempt.User);
    }

    [Theory]
    [InlineData("192.168.1.1")]
    [InlineData("10.0.0.1")]
    [InlineData("172.16.0.1")]
    [InlineData("127.0.0.1")]
    [InlineData("8.8.8.8")]
    public void OtpAttempt_SetIpAddress_AcceptsValidIPv4Addresses(string ipAddress)
    {
        // Arrange
        var otpAttempt = new OtpAttempt();

        // Act
        otpAttempt.IpAddress = ipAddress;

        // Assert
        Assert.Equal(ipAddress, otpAttempt.IpAddress);
    }

    [Theory]
    [InlineData("2001:0db8:85a3:0000:0000:8a2e:0370:7334")]
    [InlineData("2001:db8:85a3::8a2e:370:7334")]
    [InlineData("::1")]
    [InlineData("fe80::1")]
    [InlineData("2001:db8::1")]
    public void OtpAttempt_SetIpAddress_AcceptsValidIPv6Addresses(string ipAddress)
    {
        // Arrange
        var otpAttempt = new OtpAttempt();

        // Act
        otpAttempt.IpAddress = ipAddress;

        // Assert
        Assert.Equal(ipAddress, otpAttempt.IpAddress);
    }

    [Theory]
    [InlineData("send_otp")]
    [InlineData("verify_otp")]
    [InlineData("login")]
    [InlineData("setup")]
    [InlineData("disable")]
    public void OtpAttempt_SetAttemptType_AcceptsValidTypes(string attemptType)
    {
        // Arrange
        var otpAttempt = new OtpAttempt();

        // Act
        otpAttempt.AttemptType = attemptType;

        // Assert
        Assert.Equal(attemptType, otpAttempt.AttemptType);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void OtpAttempt_SetSuccess_AcceptsBooleanValues(bool success)
    {
        // Arrange
        var otpAttempt = new OtpAttempt();

        // Act
        otpAttempt.Success = success;

        // Assert
        Assert.Equal(success, otpAttempt.Success);
    }

    [Theory]
    [InlineData("user@example.com")]
    [InlineData("test.email+tag@domain.co.uk")]
    [InlineData("simple@test.org")]
    [InlineData(null)]
    public void OtpAttempt_SetEmail_AcceptsValidEmailFormats(string? email)
    {
        // Arrange
        var otpAttempt = new OtpAttempt();

        // Act
        otpAttempt.Email = email;

        // Assert
        Assert.Equal(email, otpAttempt.Email);
    }

    [Fact]
    public void OtpAttempt_UserId_CanBeNull()
    {
        // Arrange
        var otpAttempt = new OtpAttempt();

        // Act
        otpAttempt.UserId = null;

        // Assert
        Assert.Null(otpAttempt.UserId);
    }

    [Fact]
    public void OtpAttempt_UserId_CanBeSetToGuid()
    {
        // Arrange
        var otpAttempt = new OtpAttempt();
        var userId = Guid.NewGuid();

        // Act
        otpAttempt.UserId = userId;

        // Assert
        Assert.Equal(userId, otpAttempt.UserId);
    }

    [Fact]
    public void OtpAttempt_UserNavigation_MaintainsReferenceIntegrity()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            UserId = userId,
            Email = "otp@example.com",
            Name = "John Doe"
        };
        var otpAttempt = new OtpAttempt
        {
            UserId = userId,
            User = user
        };

        // Act & Assert
        Assert.Equal(userId, otpAttempt.UserId);
        Assert.Equal(userId, otpAttempt.User.UserId);
        Assert.Equal(user.Email, otpAttempt.User.Email);
        Assert.Equal(user.Name, otpAttempt.User.Name);
    }

    [Fact]
    public void OtpAttempt_UserNavigation_CanBeNull()
    {
        // Arrange
        var otpAttempt = new OtpAttempt
        {
            UserId = null,
            User = null
        };

        // Act & Assert
        Assert.Null(otpAttempt.UserId);
        Assert.Null(otpAttempt.User);
    }

    [Fact]
    public void OtpAttempt_AttemptedAt_IsSetToRecentTime()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow.AddSeconds(-1);
        
        // Act
        var otpAttempt = new OtpAttempt();
        var afterCreation = DateTime.UtcNow.AddSeconds(1);

        // Assert
        Assert.True(otpAttempt.AttemptedAt >= beforeCreation);
        Assert.True(otpAttempt.AttemptedAt <= afterCreation);
    }

    [Fact]
    public void OtpAttempt_Id_IsUniqueForEachInstance()
    {
        // Act
        var otpAttempt1 = new OtpAttempt();
        var otpAttempt2 = new OtpAttempt();

        // Assert
        Assert.NotEqual(otpAttempt1.Id, otpAttempt2.Id);
        Assert.NotEqual(Guid.Empty, otpAttempt1.Id);
        Assert.NotEqual(Guid.Empty, otpAttempt2.Id);
    }

    [Fact]
    public void OtpAttempt_PropertiesAreIndependent_CanBeSetSeparately()
    {
        // Arrange
        var otpAttempt = new OtpAttempt();
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var ipAddress = "192.168.1.100";
        var attemptType = "verify_otp";
        var email = "independent@example.com";

        // Act & Assert
        otpAttempt.Id = id;
        Assert.Equal(id, otpAttempt.Id);
        Assert.Null(otpAttempt.UserId);

        otpAttempt.UserId = userId;
        Assert.Equal(userId, otpAttempt.UserId);
        Assert.Null(otpAttempt.IpAddress);

        otpAttempt.IpAddress = ipAddress;
        Assert.Equal(ipAddress, otpAttempt.IpAddress);
        Assert.Null(otpAttempt.AttemptType);

        otpAttempt.AttemptType = attemptType;
        Assert.Equal(attemptType, otpAttempt.AttemptType);
        Assert.False(otpAttempt.Success);

        otpAttempt.Success = true;
        Assert.True(otpAttempt.Success);
        Assert.Null(otpAttempt.Email);

        otpAttempt.Email = email;
        Assert.Equal(email, otpAttempt.Email);
    }

    [Fact]
    public void OtpAttempt_MultipleInstances_AreIndependent()
    {
        // Arrange & Act
        var otpAttempt1 = new OtpAttempt
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            IpAddress = "192.168.1.1",
            AttemptType = "login",
            Success = true,
            Email = "user1@example.com"
        };

        var otpAttempt2 = new OtpAttempt
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            IpAddress = "10.0.0.1",
            AttemptType = "verify_otp",
            Success = false,
            Email = "user2@example.com"
        };

        // Assert
        Assert.NotEqual(otpAttempt1.Id, otpAttempt2.Id);
        Assert.NotEqual(otpAttempt1.UserId, otpAttempt2.UserId);
        Assert.NotEqual(otpAttempt1.IpAddress, otpAttempt2.IpAddress);
        Assert.NotEqual(otpAttempt1.AttemptType, otpAttempt2.AttemptType);
        Assert.NotEqual(otpAttempt1.Success, otpAttempt2.Success);
        Assert.NotEqual(otpAttempt1.Email, otpAttempt2.Email);
    }

    [Fact]
    public void OtpAttempt_ComplexScenario_SuccessfulLoginAttempt()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            UserId = userId,
            Email = "success@example.com"
        };

        var otpAttempt = new OtpAttempt
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            IpAddress = "203.0.113.1",
            AttemptType = OtpAttemptTypes.Login,
            AttemptedAt = DateTime.UtcNow.AddMinutes(-2),
            Success = true,
            Email = "success@example.com",
            User = user
        };

        // Act & Assert
        Assert.NotEqual(Guid.Empty, otpAttempt.Id);
        Assert.Equal(userId, otpAttempt.UserId);
        Assert.Equal("203.0.113.1", otpAttempt.IpAddress);
        Assert.Equal(OtpAttemptTypes.Login, otpAttempt.AttemptType);
        Assert.True(otpAttempt.AttemptedAt < DateTime.UtcNow);
        Assert.True(otpAttempt.Success);
        Assert.Equal("success@example.com", otpAttempt.Email);
        Assert.Equal(user.Email, otpAttempt.User.Email);
    }

    [Fact]
    public void OtpAttempt_ComplexScenario_FailedAnonymousAttempt()
    {
        // Arrange
        var otpAttempt = new OtpAttempt
        {
            Id = Guid.NewGuid(),
            UserId = null,
            IpAddress = "198.51.100.1",
            AttemptType = OtpAttemptTypes.VerifyOtp,
            AttemptedAt = DateTime.UtcNow.AddMinutes(-1),
            Success = false,
            Email = "anonymous@example.com",
            User = null
        };

        // Act & Assert
        Assert.NotEqual(Guid.Empty, otpAttempt.Id);
        Assert.Null(otpAttempt.UserId);
        Assert.Equal("198.51.100.1", otpAttempt.IpAddress);
        Assert.Equal(OtpAttemptTypes.VerifyOtp, otpAttempt.AttemptType);
        Assert.True(otpAttempt.AttemptedAt < DateTime.UtcNow);
        Assert.False(otpAttempt.Success);
        Assert.Equal("anonymous@example.com", otpAttempt.Email);
        Assert.Null(otpAttempt.User);
    }
}

public class OtpAttemptTypesTests
{
    [Fact]
    public void OtpAttemptTypes_SendOtp_HasCorrectValue()
    {
        // Act & Assert
        Assert.Equal("send_otp", OtpAttemptTypes.SendOtp);
    }

    [Fact]
    public void OtpAttemptTypes_VerifyOtp_HasCorrectValue()
    {
        // Act & Assert
        Assert.Equal("verify_otp", OtpAttemptTypes.VerifyOtp);
    }

    [Fact]
    public void OtpAttemptTypes_Login_HasCorrectValue()
    {
        // Act & Assert
        Assert.Equal("login", OtpAttemptTypes.Login);
    }

    [Fact]
    public void OtpAttemptTypes_Setup_HasCorrectValue()
    {
        // Act & Assert
        Assert.Equal("setup", OtpAttemptTypes.Setup);
    }

    [Fact]
    public void OtpAttemptTypes_Disable_HasCorrectValue()
    {
        // Act & Assert
        Assert.Equal("disable", OtpAttemptTypes.Disable);
    }

    [Fact]
    public void OtpAttemptTypes_AllConstants_AreUnique()
    {
        // Arrange
        var constants = new[]
        {
            OtpAttemptTypes.SendOtp,
            OtpAttemptTypes.VerifyOtp,
            OtpAttemptTypes.Login,
            OtpAttemptTypes.Setup,
            OtpAttemptTypes.Disable
        };

        // Act & Assert
        Assert.Equal(constants.Length, constants.Distinct().Count());
    }

    [Fact]
    public void OtpAttemptTypes_CanBeUsedInOtpAttemptEntity()
    {
        // Arrange & Act
        var otpAttempt = new OtpAttempt
        {
            AttemptType = OtpAttemptTypes.SendOtp
        };

        // Assert
        Assert.Equal(OtpAttemptTypes.SendOtp, otpAttempt.AttemptType);
        Assert.Equal("send_otp", otpAttempt.AttemptType);
    }
}