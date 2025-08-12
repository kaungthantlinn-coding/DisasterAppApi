using Xunit;
using DisasterApp.Domain.Entities;

namespace DisasterApp.Tests.Entities;

public class AuditLogTests
{
    [Fact]
    public void AuditLog_Constructor_SetsPropertiesCorrectly()
    {
        // Arrange & Act
        var auditLog = new AuditLog();

        // Assert
        Assert.NotEqual(Guid.Empty, auditLog.AuditLogId);
        Assert.True(auditLog.Timestamp <= DateTime.UtcNow);
    }

    [Fact]
    public void AuditLog_SetUserId_SetsUserIdCorrectly()
    {
        // Arrange
        var auditLog = new AuditLog();
        var userId = Guid.NewGuid();

        // Act
        auditLog.UserId = userId;

        // Assert
        Assert.Equal(userId, auditLog.UserId);
    }

    [Fact]
    public void AuditLog_SetAction_ValidAction_SetsActionCorrectly()
    {
        // Arrange
        var auditLog = new AuditLog();
        var action = "Login";

        // Act
        auditLog.Action = action;

        // Assert
        Assert.Equal(action, auditLog.Action);
    }

    [Theory]
    [InlineData("Create")]
    [InlineData("Read")]
    [InlineData("Update")]
    [InlineData("Delete")]
    [InlineData("Login")]
    [InlineData("Logout")]
    public void AuditLog_SetAction_CommonActions_SetsActionCorrectly(string action)
    {
        // Arrange
        var auditLog = new AuditLog();

        // Act
        auditLog.Action = action;

        // Assert
        Assert.Equal(action, auditLog.Action);
    }

    [Fact]
    public void AuditLog_SetEntityType_ValidEntityType_SetsEntityTypeCorrectly()
    {
        // Arrange
        var auditLog = new AuditLog();
        var entityType = "User";

        // Act
        auditLog.EntityType = entityType;

        // Assert
        Assert.Equal(entityType, auditLog.EntityType);
    }

    [Theory]
    [InlineData("User")]
    [InlineData("DisasterReport")]
    [InlineData("Organization")]
    [InlineData("Role")]
    [InlineData("AuditLog")]
    public void AuditLog_SetEntityType_CommonEntityTypes_SetsEntityTypeCorrectly(string entityType)
    {
        // Arrange
        var auditLog = new AuditLog();

        // Act
        auditLog.EntityType = entityType;

        // Assert
        Assert.Equal(entityType, auditLog.EntityType);
    }

    [Fact]
    public void AuditLog_SetEntityId_ValidEntityId_SetsEntityIdCorrectly()
    {
        // Arrange
        var auditLog = new AuditLog();
        var entityId = Guid.NewGuid();

        // Act
        auditLog.EntityId = entityId.ToString();

        // Assert
        Assert.Equal(entityId.ToString(), auditLog.EntityId);
    }

    [Fact]
    public void AuditLog_SetOldValues_ValidJson_SetsOldValuesCorrectly()
    {
        // Arrange
        var auditLog = new AuditLog();
        var oldValues = "{\"name\":\"Old Name\",\"email\":\"old@example.com\"}";

        // Act
        auditLog.OldValues = oldValues;

        // Assert
        Assert.Equal(oldValues, auditLog.OldValues);
    }

    [Fact]
    public void AuditLog_SetNewValues_ValidJson_SetsNewValuesCorrectly()
    {
        // Arrange
        var auditLog = new AuditLog();
        var newValues = "{\"name\":\"New Name\",\"email\":\"new@example.com\"}";

        // Act
        auditLog.NewValues = newValues;

        // Assert
        Assert.Equal(newValues, auditLog.NewValues);
    }

    [Fact]
    public void AuditLog_SetIpAddress_ValidIpAddress_SetsIpAddressCorrectly()
    {
        // Arrange
        var auditLog = new AuditLog();
        var ipAddress = "192.168.1.1";

        // Act
        auditLog.IpAddress = ipAddress;

        // Assert
        Assert.Equal(ipAddress, auditLog.IpAddress);
    }

    [Theory]
    [InlineData("192.168.1.1")]
    [InlineData("10.0.0.1")]
    [InlineData("127.0.0.1")]
    [InlineData("2001:0db8:85a3:0000:0000:8a2e:0370:7334")]
    public void AuditLog_SetIpAddress_VariousIpAddresses_SetsIpAddressCorrectly(string ipAddress)
    {
        // Arrange
        var auditLog = new AuditLog();

        // Act
        auditLog.IpAddress = ipAddress;

        // Assert
        Assert.Equal(ipAddress, auditLog.IpAddress);
    }

    [Fact]
    public void AuditLog_SetUserAgent_ValidUserAgent_SetsUserAgentCorrectly()
    {
        // Arrange
        var auditLog = new AuditLog();
        var userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36";

        // Act
        auditLog.UserAgent = userAgent;

        // Assert
        Assert.Equal(userAgent, auditLog.UserAgent);
    }

    [Fact]
    public void AuditLog_SetTimestamp_ValidTimestamp_SetsTimestampCorrectly()
    {
        // Arrange
        var auditLog = new AuditLog();
        var timestamp = DateTime.UtcNow;

        // Act
        auditLog.Timestamp = timestamp;

        // Assert
        Assert.Equal(timestamp, auditLog.Timestamp);
    }

    [Fact]
    public void AuditLog_SetDetails_ValidDetails_SetsDetailsCorrectly()
    {
        // Arrange
        var auditLog = new AuditLog();
        var details = "User logged into the system";

        // Act
        auditLog.Details = details;

        // Assert
        Assert.Equal(details, auditLog.Details);
    }

    [Fact]
    public void AuditLog_SetSeverity_ValidSeverity_SetsSeverityCorrectly()
    {
        // Arrange
        var auditLog = new AuditLog();
        var severity = "High";

        // Act
        auditLog.Severity = severity;

        // Assert
        Assert.Equal(severity, auditLog.Severity);
    }

    [Theory]
    [InlineData("Low")]
    [InlineData("Medium")]
    [InlineData("High")]
    [InlineData("Critical")]
    public void AuditLog_SetSeverity_CommonSeverityLevels_SetsSeverityCorrectly(string severity)
    {
        // Arrange
        var auditLog = new AuditLog();

        // Act
        auditLog.Severity = severity;

        // Assert
        Assert.Equal(severity, auditLog.Severity);
    }



    [Fact]
    public void AuditLog_SetResource_ValidResource_SetsResourceCorrectly()
    {
        // Arrange
        var auditLog = new AuditLog();
        var resource = "WebAPI";

        // Act
        auditLog.Resource = resource;

        // Assert
        Assert.Equal(resource, auditLog.Resource);
    }

    [Theory]
    [InlineData("WebAPI")]
    [InlineData("MobileApp")]
    [InlineData("AdminPanel")]
    [InlineData("BackgroundService")]
    public void AuditLog_SetResource_CommonResources_SetsResourceCorrectly(string resource)
    {
        // Arrange
        var auditLog = new AuditLog();

        // Act
        auditLog.Resource = resource;

        // Assert
        Assert.Equal(resource, auditLog.Resource);
    }



    [Fact]
    public void AuditLog_CreateLoginAuditLog_CreatesCorrectAuditLog()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var ipAddress = "192.168.1.1";
        var userAgent = "Mozilla/5.0";

        // Act
        var auditLog = new AuditLog
        {
            UserId = userId,
            Action = "Login",
            EntityType = "User",
            EntityId = userId.ToString(),
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Details = "User successfully logged in",
            Severity = "Medium",
            Resource = "WebAPI"
        };

        // Assert
        Assert.Equal(userId, auditLog.UserId);
        Assert.Equal("Login", auditLog.Action);
        Assert.Equal("User", auditLog.EntityType);
        Assert.Equal(userId.ToString(), auditLog.EntityId);
        Assert.Equal(ipAddress, auditLog.IpAddress);
        Assert.Equal(userAgent, auditLog.UserAgent);
        Assert.Equal("User successfully logged in", auditLog.Details);
        Assert.Equal("Medium", auditLog.Severity);
        Assert.Equal("WebAPI", auditLog.Resource);
    }

    [Fact]
    public void AuditLog_CreateDataModificationAuditLog_CreatesCorrectAuditLog()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var entityId = Guid.NewGuid();
        var oldValues = "{\"name\":\"Old Name\"}";
        var newValues = "{\"name\":\"New Name\"}";

        // Act
        var auditLog = new AuditLog
        {
            UserId = userId,
            Action = "Update",
            EntityType = "User",
            EntityId = entityId.ToString(),
            OldValues = oldValues,
            NewValues = newValues,
            Details = "User profile updated",
            Severity = "Low",
            Resource = "WebAPI"
        };

        // Assert
        Assert.Equal(userId, auditLog.UserId);
        Assert.Equal("Update", auditLog.Action);
        Assert.Equal("User", auditLog.EntityType);
        Assert.Equal(entityId.ToString(), auditLog.EntityId);
        Assert.Equal(oldValues, auditLog.OldValues);
        Assert.Equal(newValues, auditLog.NewValues);
        Assert.Equal("User profile updated", auditLog.Details);
        Assert.Equal("Low", auditLog.Severity);
        Assert.Equal("WebAPI", auditLog.Resource);
    }

    [Fact]
    public void AuditLog_CreateSecurityAuditLog_CreatesCorrectAuditLog()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var ipAddress = "192.168.1.100";

        // Act
        var auditLog = new AuditLog
        {
            UserId = userId,
            Action = "Failed Login",
            EntityType = "User",
            EntityId = userId.ToString(),
            IpAddress = ipAddress,
            Details = "Multiple failed login attempts detected",
            Severity = "High",
            Resource = "WebAPI"
        };

        // Assert
        Assert.Equal(userId, auditLog.UserId);
        Assert.Equal("Failed Login", auditLog.Action);
        Assert.Equal("User", auditLog.EntityType);
        Assert.Equal(userId.ToString(), auditLog.EntityId);
        Assert.Equal(ipAddress, auditLog.IpAddress);
        Assert.Equal("Multiple failed login attempts detected", auditLog.Details);
        Assert.Equal("High", auditLog.Severity);
        Assert.Equal("WebAPI", auditLog.Resource);
    }

    [Fact]
    public void AuditLog_TwoAuditLogsWithSameId_AreEqual()
    {
        // Arrange
        var auditLogId = Guid.NewGuid();
        var auditLog1 = new AuditLog { AuditLogId = auditLogId };
        var auditLog2 = new AuditLog { AuditLogId = auditLogId };

        // Act & Assert
        Assert.Equal(auditLog1.AuditLogId, auditLog2.AuditLogId);
    }

    [Fact]
    public void AuditLog_TwoAuditLogsWithDifferentIds_AreNotEqual()
    {
        // Arrange
        var auditLog1 = new AuditLog();
        var auditLog2 = new AuditLog();

        // Act & Assert
        Assert.NotEqual(auditLog1.AuditLogId, auditLog2.AuditLogId);
    }

    [Fact]
    public void AuditLog_TimestampIsUtc_ReturnsTrue()
    {
        // Arrange
        var auditLog = new AuditLog();
        var utcTimestamp = DateTime.UtcNow;

        // Act
        auditLog.Timestamp = utcTimestamp;

        // Assert
        Assert.Equal(DateTimeKind.Utc, auditLog.Timestamp.Kind);
    }
}