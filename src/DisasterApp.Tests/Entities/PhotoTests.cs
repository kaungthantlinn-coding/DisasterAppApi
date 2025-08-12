using Xunit;
using DisasterApp.Domain.Entities;

namespace DisasterApp.Tests.Entities;

public class PhotoTests
{
    [Fact]
    public void Photo_DefaultConstructor_SetsDefaultValues()
    {
        // Act
        var photo = new Photo();

        // Assert
        Assert.Equal(0, photo.Id);
        Assert.Equal(Guid.Empty, photo.ReportId);
        Assert.Null(photo.Url);
        Assert.Null(photo.Caption);
        Assert.Null(photo.PublicId);
        Assert.Null(photo.UploadedAt);
    }

    [Fact]
    public void Photo_SetProperties_SetsCorrectValues()
    {
        // Arrange
        var id = 123;
        var reportId = Guid.NewGuid();
        var url = "https://example.com/photos/disaster123.jpg";
        var caption = "Damage to building after earthquake";
        var publicId = "cloudinary_public_id_123";
        var uploadedAt = DateTime.UtcNow;

        // Act
        var photo = new Photo
        {
            Id = id,
            ReportId = reportId,
            Url = url,
            Caption = caption,
            PublicId = publicId,
            UploadedAt = uploadedAt
        };

        // Assert
        Assert.Equal(id, photo.Id);
        Assert.Equal(reportId, photo.ReportId);
        Assert.Equal(url, photo.Url);
        Assert.Equal(caption, photo.Caption);
        Assert.Equal(publicId, photo.PublicId);
        Assert.Equal(uploadedAt, photo.UploadedAt);
    }

    [Theory]
    [InlineData("https://example.com/photo1.jpg")]
    [InlineData("https://cloudinary.com/image/upload/v123456/disaster_photo.png")]
    [InlineData("https://storage.googleapis.com/bucket/photos/emergency.jpeg")]
    [InlineData("https://aws.s3.amazonaws.com/disaster-photos/flood_damage.webp")]
    public void Photo_SetUrl_AcceptsValidUrls(string url)
    {
        // Arrange
        var photo = new Photo();

        // Act
        photo.Url = url;

        // Assert
        Assert.Equal(url, photo.Url);
    }

    [Theory]
    [InlineData("Building collapse on Main Street")]
    [InlineData("Flood water reaching second floor")]
    [InlineData("Fire damage to residential area")]
    [InlineData("Landslide blocking highway")]
    [InlineData("Storm damage to power lines")]
    public void Photo_SetCaption_AcceptsValidCaptions(string caption)
    {
        // Arrange
        var photo = new Photo();

        // Act
        photo.Caption = caption;

        // Assert
        Assert.Equal(caption, photo.Caption);
    }

    [Theory]
    [InlineData("cloudinary_id_123")]
    [InlineData("aws_s3_key_456")]
    [InlineData("google_storage_789")]
    [InlineData("azure_blob_abc")]
    public void Photo_SetPublicId_AcceptsValidPublicIds(string publicId)
    {
        // Arrange
        var photo = new Photo();

        // Act
        photo.PublicId = publicId;

        // Assert
        Assert.Equal(publicId, photo.PublicId);
    }

    [Fact]
    public void Photo_SetCaption_LongCaption_SetsCorrectly()
    {
        // Arrange
        var photo = new Photo();
        var longCaption = new string('A', 500); // 500 character caption

        // Act
        photo.Caption = longCaption;

        // Assert
        Assert.Equal(longCaption, photo.Caption);
        Assert.Equal(500, photo.Caption.Length);
    }

    [Fact]
    public void Photo_SetCaption_EmptyString_SetsCorrectly()
    {
        // Arrange
        var photo = new Photo();
        var emptyCaption = string.Empty;

        // Act
        photo.Caption = emptyCaption;

        // Assert
        Assert.Equal(emptyCaption, photo.Caption);
        Assert.True(string.IsNullOrEmpty(photo.Caption));
    }

    [Fact]
    public void Photo_SetUploadedAt_CurrentTime_SetsCorrectly()
    {
        // Arrange
        var photo = new Photo();
        var currentTime = DateTime.UtcNow;

        // Act
        photo.UploadedAt = currentTime;

        // Assert
        Assert.Equal(currentTime, photo.UploadedAt);
    }

    [Fact]
    public void Photo_SetUploadedAt_PastTime_SetsCorrectly()
    {
        // Arrange
        var photo = new Photo();
        var pastTime = DateTime.UtcNow.AddHours(-2);

        // Act
        photo.UploadedAt = pastTime;

        // Assert
        Assert.Equal(pastTime, photo.UploadedAt);
        Assert.True(photo.UploadedAt < DateTime.UtcNow);
    }

    [Fact]
    public void Photo_WithDisasterReport_SetsNavigationPropertyCorrectly()
    {
        // Arrange
        var reportId = Guid.NewGuid();
        var disasterReport = new DisasterReport
        {
            Id = reportId,
            Title = "Test Disaster Report",
            Description = "Test description"
        };

        // Act
        var photo = new Photo
        {
            ReportId = reportId,
            Report = disasterReport
        };

        // Assert
        Assert.Equal(reportId, photo.ReportId);
        Assert.Equal(disasterReport, photo.Report);
        Assert.Equal(reportId, photo.Report.Id);
    }

    [Fact]
    public void Photo_MultiplePhotosForSameReport_ShareSameReportId()
    {
        // Arrange
        var reportId = Guid.NewGuid();
        var photo1 = new Photo { ReportId = reportId, Url = "https://example.com/photo1.jpg" };
        var photo2 = new Photo { ReportId = reportId, Url = "https://example.com/photo2.jpg" };
        var photo3 = new Photo { ReportId = reportId, Url = "https://example.com/photo3.jpg" };

        // Assert
        Assert.Equal(reportId, photo1.ReportId);
        Assert.Equal(reportId, photo2.ReportId);
        Assert.Equal(reportId, photo3.ReportId);
        Assert.Equal(photo1.ReportId, photo2.ReportId);
        Assert.Equal(photo2.ReportId, photo3.ReportId);
    }

    [Fact]
    public void Photo_SetUrl_NullValue_SetsCorrectly()
    {
        // Arrange
        var photo = new Photo
        {
            Url = "https://example.com/photo.jpg"
        };

        // Act
        photo.Url = null;

        // Assert
        Assert.Null(photo.Url);
    }

    [Fact]
    public void Photo_SetCaption_NullValue_SetsCorrectly()
    {
        // Arrange
        var photo = new Photo
        {
            Caption = "Initial caption"
        };

        // Act
        photo.Caption = null;

        // Assert
        Assert.Null(photo.Caption);
    }

    [Fact]
    public void Photo_SetPublicId_NullValue_SetsCorrectly()
    {
        // Arrange
        var photo = new Photo
        {
            PublicId = "initial_public_id"
        };

        // Act
        photo.PublicId = null;

        // Assert
        Assert.Null(photo.PublicId);
    }

    [Fact]
    public void Photo_CompletePhotoData_AllPropertiesSet()
    {
        // Arrange
        var id = 456;
        var reportId = Guid.NewGuid();
        var url = "https://cloudinary.com/disaster_photos/earthquake_damage.jpg";
        var caption = "Severe structural damage to residential building after 7.2 magnitude earthquake";
        var publicId = "disaster_photos/earthquake_2024_001";
        var uploadedAt = DateTime.UtcNow;

        // Act
        var photo = new Photo
        {
            Id = id,
            ReportId = reportId,
            Url = url,
            Caption = caption,
            PublicId = publicId,
            UploadedAt = uploadedAt
        };

        // Assert
        Assert.Equal(id, photo.Id);
        Assert.Equal(reportId, photo.ReportId);
        Assert.Equal(url, photo.Url);
        Assert.Equal(caption, photo.Caption);
        Assert.Equal(publicId, photo.PublicId);
        Assert.Equal(uploadedAt, photo.UploadedAt);
        Assert.NotEqual(0, photo.Id);
        Assert.NotEqual(Guid.Empty, photo.ReportId);
        Assert.False(string.IsNullOrEmpty(photo.Url));
        Assert.False(string.IsNullOrEmpty(photo.Caption));
        Assert.False(string.IsNullOrEmpty(photo.PublicId));
        Assert.NotEqual(DateTime.MinValue, photo.UploadedAt);
    }

    [Fact]
    public void Photo_TimeComparison_UploadedAtBeforeNow()
    {
        // Arrange
        var photo = new Photo
        {
            UploadedAt = DateTime.UtcNow.AddMinutes(-5)
        };

        // Act & Assert
        Assert.True(photo.UploadedAt < DateTime.UtcNow);
        Assert.True(photo.UploadedAt > DateTime.UtcNow.AddMinutes(-10));
    }
}