using Xunit;
using DisasterApp.Domain.Entities;

namespace DisasterApp.Tests.Entities;

public class LocationTests
{
    [Fact]
    public void Location_DefaultConstructor_SetsDefaultValues()
    {
        // Act
        var location = new Location();

        // Assert
        Assert.Equal(Guid.Empty, location.LocationId);
        Assert.Equal(Guid.Empty, location.ReportId);
        Assert.Equal(0m, location.Latitude);
        Assert.Equal(0m, location.Longitude);
        Assert.Null(location.Address);
        Assert.Null(location.FormattedAddress);
        Assert.Null(location.CoordinatePrecision);
    }

    [Fact]
    public void Location_SetProperties_SetsCorrectValues()
    {
        // Arrange
        var locationId = Guid.NewGuid();
        var reportId = Guid.NewGuid();
        var latitude = 40.7128m;
        var longitude = -74.0060m;
        var address = "123 Main Street";
        var formattedAddress = "123 Main Street, New York, NY 10001, USA";
        var coordinatePrecision = "ROOFTOP";

        // Act
        var location = new Location
        {
            LocationId = locationId,
            ReportId = reportId,
            Latitude = latitude,
            Longitude = longitude,
            Address = address,
            FormattedAddress = formattedAddress,
            CoordinatePrecision = coordinatePrecision
        };

        // Assert
        Assert.Equal(locationId, location.LocationId);
        Assert.Equal(reportId, location.ReportId);
        Assert.Equal(latitude, location.Latitude);
        Assert.Equal(longitude, location.Longitude);
        Assert.Equal(address, location.Address);
        Assert.Equal(formattedAddress, location.FormattedAddress);
        Assert.Equal(coordinatePrecision, location.CoordinatePrecision);
    }

    [Theory]
    [InlineData(40.7128, -74.0060)] // New York City
    [InlineData(34.0522, -118.2437)] // Los Angeles
    [InlineData(41.8781, -87.6298)] // Chicago
    [InlineData(29.7604, -95.3698)] // Houston
    [InlineData(33.4484, -112.0740)] // Phoenix
    public void Location_SetCoordinates_AcceptsValidCoordinates(double lat, double lng)
    {
        // Arrange
        var location = new Location();
        var latitude = (decimal)lat;
        var longitude = (decimal)lng;

        // Act
        location.Latitude = latitude;
        location.Longitude = longitude;

        // Assert
        Assert.Equal(latitude, location.Latitude);
        Assert.Equal(longitude, location.Longitude);
    }

    [Theory]
    [InlineData(90.0)] // North Pole
    [InlineData(-90.0)] // South Pole
    [InlineData(0.0)] // Equator
    [InlineData(45.0)] // Mid-latitude
    [InlineData(-45.0)] // Mid-latitude South
    public void Location_SetLatitude_AcceptsValidLatitudes(double lat)
    {
        // Arrange
        var location = new Location();
        var latitude = (decimal)lat;

        // Act
        location.Latitude = latitude;

        // Assert
        Assert.Equal(latitude, location.Latitude);
    }

    [Theory]
    [InlineData(180.0)] // International Date Line
    [InlineData(-180.0)] // International Date Line
    [InlineData(0.0)] // Prime Meridian
    [InlineData(90.0)] // Eastern Hemisphere
    [InlineData(-90.0)] // Western Hemisphere
    public void Location_SetLongitude_AcceptsValidLongitudes(double lng)
    {
        // Arrange
        var location = new Location();
        var longitude = (decimal)lng;

        // Act
        location.Longitude = longitude;

        // Assert
        Assert.Equal(longitude, location.Longitude);
    }

    [Theory]
    [InlineData("123 Main Street")]
    [InlineData("456 Oak Avenue, Apt 2B")]
    [InlineData("789 Pine Road, Suite 100")]
    [InlineData("1010 Elm Boulevard")]
    [InlineData("2020 Maple Drive, Unit 5")]
    public void Location_SetAddress_AcceptsValidAddresses(string address)
    {
        // Arrange
        var location = new Location();

        // Act
        location.Address = address;

        // Assert
        Assert.Equal(address, location.Address);
    }

    [Theory]
    [InlineData("123 Main Street, New York, NY 10001, USA")]
    [InlineData("456 Oak Avenue, Los Angeles, CA 90210, USA")]
    [InlineData("789 Pine Road, Chicago, IL 60601, USA")]
    [InlineData("321 Elm Street, Houston, TX 77001, USA")]
    [InlineData("654 Maple Drive, Phoenix, AZ 85001, USA")]
    public void Location_SetFormattedAddress_AcceptsValidFormattedAddresses(string formattedAddress)
    {
        // Arrange
        var location = new Location();

        // Act
        location.FormattedAddress = formattedAddress;

        // Assert
        Assert.Equal(formattedAddress, location.FormattedAddress);
    }

    [Theory]
    [InlineData("ROOFTOP")]
    [InlineData("RANGE_INTERPOLATED")]
    [InlineData("GEOMETRIC_CENTER")]
    [InlineData("APPROXIMATE")]
    [InlineData("UNKNOWN")]
    public void Location_SetCoordinatePrecision_AcceptsValidPrecisionValues(string precision)
    {
        // Arrange
        var location = new Location();

        // Act
        location.CoordinatePrecision = precision;

        // Assert
        Assert.Equal(precision, location.CoordinatePrecision);
    }

    [Fact]
    public void Location_SetReportId_AcceptsValidReportId()
    {
        // Arrange
        var location = new Location();
        var reportId = Guid.NewGuid();

        // Act
        location.ReportId = reportId;

        // Assert
        Assert.Equal(reportId, location.ReportId);
    }

    [Fact]
    public void Location_SetAddress_LongAddress_SetsCorrectly()
    {
        // Arrange
        var location = new Location();
        var longAddress = "1234 Very Long Street Name With Multiple Words And Numbers, Apartment Complex Building A, Unit 123B, Floor 5";

        // Act
        location.Address = longAddress;

        // Assert
        Assert.Equal(longAddress, location.Address);
        Assert.True(location.Address.Length > 50);
    }

    [Fact]
    public void Location_SetCoordinates_HighPrecision_SetsCorrectly()
    {
        // Arrange
        var location = new Location();
        var latitude = 40.712776m; // High precision latitude
        var longitude = -74.005974m; // High precision longitude

        // Act
        location.Latitude = latitude;
        location.Longitude = longitude;

        // Assert
        Assert.Equal(latitude, location.Latitude);
        Assert.Equal(longitude, location.Longitude);
    }

    [Fact]
    public void Location_WithDisasterReport_SetsNavigationPropertyCorrectly()
    {
        // Arrange
        var locationId = Guid.NewGuid();
        var disasterReport = new DisasterReport
        {
            Id = Guid.NewGuid(),
            Title = "Test Disaster Report",
            Description = "Test description"
        };

        // Act
        var location = new Location
        {
            LocationId = locationId,
            Report = disasterReport
        };

        // Assert
        Assert.Equal(locationId, location.LocationId);
        Assert.Equal(disasterReport, location.Report);
    }

    [Fact]
    public void Location_SetNullValues_SetsCorrectly()
    {
        // Arrange
        var location = new Location
        {
            LocationId = Guid.NewGuid(),
            ReportId = Guid.NewGuid(),
            Latitude = 40.7589m,
            Longitude = -73.9851m,
            Address = "Times Square",
            FormattedAddress = "Times Square, New York, NY 10001, USA",
            CoordinatePrecision = "ROOFTOP"
        };

        // Act
        location.FormattedAddress = null;
        location.CoordinatePrecision = null;

        // Assert
        Assert.Null(location.FormattedAddress);
        Assert.Null(location.CoordinatePrecision);
    }

    [Fact]
    public void Location_SetEmptyStringValues_SetsCorrectly()
    {
        // Arrange
        var location = new Location();

        // Act
        location.Address = string.Empty;
        location.FormattedAddress = string.Empty;
        location.CoordinatePrecision = string.Empty;

        // Assert
        Assert.Equal(string.Empty, location.Address);
        Assert.Equal(string.Empty, location.FormattedAddress);
        Assert.Equal(string.Empty, location.CoordinatePrecision);
    }

    [Fact]
    public void Location_CoordinateComparison_WorksCorrectly()
    {
        // Arrange
        var location1 = new Location { Latitude = 40.7128m, Longitude = -74.0060m };
        var location2 = new Location { Latitude = 40.7128m, Longitude = -74.0060m };
        var location3 = new Location { Latitude = 34.0522m, Longitude = -118.2437m };

        // Assert
        Assert.Equal(location1.Latitude, location2.Latitude);
        Assert.Equal(location1.Longitude, location2.Longitude);
        Assert.NotEqual(location1.Latitude, location3.Latitude);
        Assert.NotEqual(location1.Longitude, location3.Longitude);
    }

    [Fact]
    public void Location_CompleteLocationData_AllPropertiesSet()
    {
        // Arrange
        var locationId = Guid.NewGuid();
        var latitude = 40.7589m;
        var longitude = -73.9851m;
        var address = "Times Square, Manhattan";

        // Act
        var location = new Location
        {
            LocationId = locationId,
            Latitude = latitude,
            Longitude = longitude,
            Address = address,
        };

        // Assert
        Assert.NotEqual(Guid.Empty, location.LocationId);
        Assert.NotEqual(0m, location.Latitude);
        Assert.NotEqual(0m, location.Longitude);
        Assert.False(string.IsNullOrEmpty(location.Address));
    }

    [Fact]
    public void Location_ExtremeCoordinates_SetsCorrectly()
    {
        // Arrange
        var location = new Location();
        var maxLatitude = 90m;
        var minLatitude = -90m;
        var maxLongitude = 180m;
        var minLongitude = -180m;

        // Act & Assert for maximum values
        location.Latitude = maxLatitude;
        location.Longitude = maxLongitude;
        Assert.Equal(maxLatitude, location.Latitude);
        Assert.Equal(maxLongitude, location.Longitude);

        // Act & Assert for minimum values
        location.Latitude = minLatitude;
        location.Longitude = minLongitude;
        Assert.Equal(minLatitude, location.Latitude);
        Assert.Equal(minLongitude, location.Longitude);
    }
}