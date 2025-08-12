using Xunit;
using DisasterApp.Domain.Entities;
using DisasterApp.Domain.Enums;

namespace DisasterApp.Tests.Entities;

public class DisasterReportTests
{
    [Fact]
    public void DisasterReport_DefaultConstructor_SetsDefaultValues()
    {
        // Act
        var disasterReport = new DisasterReport();

        // Assert
        Assert.NotEqual(Guid.Empty, disasterReport.Id);
        Assert.Null(disasterReport.Title);
        Assert.Null(disasterReport.Description);
        Assert.Equal(DateTime.MinValue, disasterReport.Timestamp);
        Assert.Equal(SeverityLevel.Low, disasterReport.Severity);
        Assert.Equal(ReportStatus.Pending, disasterReport.Status);
        Assert.Null(disasterReport.VerifiedBy);
        Assert.Null(disasterReport.VerifiedAt);
        Assert.Null(disasterReport.IsDeleted);
        Assert.Equal(Guid.Empty, disasterReport.UserId);
        Assert.Null(disasterReport.CreatedAt);
        Assert.Null(disasterReport.UpdatedAt);
        Assert.Equal(Guid.Empty, disasterReport.DisasterEventId);
    }

    [Fact]
    public void DisasterReport_SetProperties_SetsCorrectValues()
    {
        // Arrange
        var id = Guid.NewGuid();
        var title = "Earthquake in City Center";
        var description = "Major earthquake causing building damage";
        var timestamp = DateTime.UtcNow;
        var severity = SeverityLevel.High;
        var status = ReportStatus.Verified;
        var verifiedBy = Guid.NewGuid();
        var verifiedAt = DateTime.UtcNow;
        var isDeleted = false;
        var userId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow.AddMinutes(-10);
        var updatedAt = DateTime.UtcNow;
        var disasterEventId = Guid.NewGuid();

        // Act
        var disasterReport = new DisasterReport
        {
            Id = id,
            Title = title,
            Description = description,
            Timestamp = timestamp,
            Severity = severity,
            Status = status,
            VerifiedBy = verifiedBy,
            VerifiedAt = verifiedAt,
            IsDeleted = isDeleted,
            UserId = userId,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
            DisasterEventId = disasterEventId
        };

        // Assert
        Assert.Equal(id, disasterReport.Id);
        Assert.Equal(title, disasterReport.Title);
        Assert.Equal(description, disasterReport.Description);
        Assert.Equal(timestamp, disasterReport.Timestamp);
        Assert.Equal(severity, disasterReport.Severity);
        Assert.Equal(status, disasterReport.Status);
        Assert.Equal(verifiedBy, disasterReport.VerifiedBy);
        Assert.Equal(verifiedAt, disasterReport.VerifiedAt);
        Assert.Equal(isDeleted, disasterReport.IsDeleted);
        Assert.Equal(userId, disasterReport.UserId);
        Assert.Equal(createdAt, disasterReport.CreatedAt);
        Assert.Equal(updatedAt, disasterReport.UpdatedAt);
        Assert.Equal(disasterEventId, disasterReport.DisasterEventId);
    }

    [Theory]
    [InlineData("Earthquake Alert")]
    [InlineData("Flood Warning")]
    [InlineData("Fire Emergency")]
    [InlineData("Storm Damage Report")]
    [InlineData("Landslide Incident")]
    public void DisasterReport_SetTitle_AcceptsValidTitles(string title)
    {
        // Arrange
        var disasterReport = new DisasterReport();

        // Act
        disasterReport.Title = title;

        // Assert
        Assert.Equal(title, disasterReport.Title);
    }

    [Theory]
    [InlineData(SeverityLevel.Low)]
    [InlineData(SeverityLevel.Medium)]
    [InlineData(SeverityLevel.High)]
    [InlineData(SeverityLevel.Critical)]
    public void DisasterReport_SetSeverity_AcceptsValidSeverityLevels(SeverityLevel severity)
    {
        // Arrange
        var disasterReport = new DisasterReport();

        // Act
        disasterReport.Severity = severity;

        // Assert
        Assert.Equal(severity, disasterReport.Severity);
    }

    [Theory]
    [InlineData(ReportStatus.Pending)]
    [InlineData(ReportStatus.Verified)]
    [InlineData(ReportStatus.Rejected)]
    public void DisasterReport_SetStatus_AcceptsValidStatuses(ReportStatus status)
    {
        // Arrange
        var disasterReport = new DisasterReport();

        // Act
        disasterReport.Status = status;

        // Assert
        Assert.Equal(status, disasterReport.Status);
    }

    [Fact]
    public void DisasterReport_SetDescription_LongDescription_SetsCorrectly()
    {
        // Arrange
        var disasterReport = new DisasterReport();
        var longDescription = new string('A', 1000); // 1000 character description

        // Act
        disasterReport.Description = longDescription;

        // Assert
        Assert.Equal(longDescription, disasterReport.Description);
        Assert.Equal(1000, disasterReport.Description.Length);
    }

    [Fact]
    public void DisasterReport_SetTimestamp_FutureDate_SetsCorrectly()
    {
        // Arrange
        var disasterReport = new DisasterReport();
        var futureDate = DateTime.UtcNow.AddDays(1);

        // Act
        disasterReport.Timestamp = futureDate;

        // Assert
        Assert.Equal(futureDate, disasterReport.Timestamp);
    }

    [Fact]
    public void DisasterReport_SetTimestamp_PastDate_SetsCorrectly()
    {
        // Arrange
        var disasterReport = new DisasterReport();
        var pastDate = DateTime.UtcNow.AddDays(-30);

        // Act
        disasterReport.Timestamp = pastDate;

        // Assert
        Assert.Equal(pastDate, disasterReport.Timestamp);
    }

    [Fact]
    public void DisasterReport_VerificationWorkflow_SetsVerificationFieldsCorrectly()
    {
        // Arrange
        var disasterReport = new DisasterReport
        {
            Status = ReportStatus.Pending
        };
        var verifierId = Guid.NewGuid();
        var verificationTime = DateTime.UtcNow;

        // Act
        disasterReport.Status = ReportStatus.Verified;
        disasterReport.VerifiedBy = verifierId;
        disasterReport.VerifiedAt = verificationTime;

        // Assert
        Assert.Equal(ReportStatus.Verified, disasterReport.Status);
        Assert.Equal(verifierId, disasterReport.VerifiedBy);
        Assert.Equal(verificationTime, disasterReport.VerifiedAt);
    }

    [Fact]
    public void DisasterReport_SoftDelete_SetsIsDeletedCorrectly()
    {
        // Arrange
        var disasterReport = new DisasterReport
        {
            IsDeleted = false
        };

        // Act
        disasterReport.IsDeleted = true;

        // Assert
        Assert.True(disasterReport.IsDeleted);
    }

    [Fact]
    public void DisasterReport_NavigationProperties_InitializeCorrectly()
    {
        // Arrange & Act
        var disasterReport = new DisasterReport();

        // Assert
        Assert.NotNull(disasterReport.ImpactDetails);
        Assert.Empty(disasterReport.ImpactDetails);
        Assert.NotNull(disasterReport.Photos);
        Assert.Empty(disasterReport.Photos);
        Assert.NotNull(disasterReport.SupportRequests);
        Assert.Empty(disasterReport.SupportRequests);
    }

    [Fact]
    public void DisasterReport_WithRelatedEntities_SetsNavigationPropertiesCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var disasterEventId = Guid.NewGuid();
        var verifierId = Guid.NewGuid();
        
        var user = new User { UserId = userId, Email = "reporter@example.com" };
        var verifier = new User { UserId = verifierId, Email = "verifier@example.com" };
        var disasterEvent = new DisasterEvent { Id = disasterEventId, Name = "Test Event" };
        var location = new Location { LocationId = Guid.NewGuid(), Latitude = 40.7128m, Longitude = -74.0060m };

        // Act
        var disasterReport = new DisasterReport
        {
            UserId = userId,
            DisasterEventId = disasterEventId,
            VerifiedBy = verifierId,
            User = user,
            VerifiedByNavigation = verifier,
            DisasterEvent = disasterEvent,
            Location = location
        };

        // Assert
        Assert.Equal(user, disasterReport.User);
        Assert.Equal(verifier, disasterReport.VerifiedByNavigation);
        Assert.Equal(disasterEvent, disasterReport.DisasterEvent);
        Assert.Equal(location, disasterReport.Location);
    }

    [Fact]
    public void DisasterReport_AuditFields_TrackChangesCorrectly()
    {
        // Arrange
        var createdAt = DateTime.UtcNow.AddMinutes(-10);
        var updatedAt = DateTime.UtcNow;
        var disasterReport = new DisasterReport
        {
            CreatedAt = createdAt
        };

        // Act
        disasterReport.UpdatedAt = updatedAt;

        // Assert
        Assert.Equal(createdAt, disasterReport.CreatedAt);
        Assert.Equal(updatedAt, disasterReport.UpdatedAt);
        Assert.True(disasterReport.UpdatedAt > disasterReport.CreatedAt);
    }
}