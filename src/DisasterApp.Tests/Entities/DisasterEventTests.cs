using Xunit;
using DisasterApp.Domain.Entities;
using DisasterApp.Domain.Enums;

namespace DisasterApp.Tests.Entities;

public class DisasterEventTests
{
    [Fact]
    public void DisasterEvent_DefaultConstructor_SetsDefaultValues()
    {
        // Act
        var disasterEvent = new DisasterEvent();

        // Assert
        Assert.NotEqual(Guid.Empty, disasterEvent.Id);
        Assert.Null(disasterEvent.Name);
        Assert.Equal(0, disasterEvent.DisasterTypeId);
        Assert.NotNull(disasterEvent.DisasterReports);
        Assert.Empty(disasterEvent.DisasterReports);
    }

    [Fact]
    public void DisasterEvent_SetProperties_SetsCorrectValues()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "Hurricane Maria 2024";
        var disasterTypeId = 1;

        // Act
        var disasterEvent = new DisasterEvent
        {
            Id = id,
            Name = name,
            DisasterTypeId = disasterTypeId
        };

        // Assert
        Assert.Equal(id, disasterEvent.Id);
        Assert.Equal(name, disasterEvent.Name);
        Assert.Equal(disasterTypeId, disasterEvent.DisasterTypeId);
    }

    [Theory]
    [InlineData("Earthquake San Francisco 2024")]
    [InlineData("Wildfire Northern California")]
    [InlineData("Flood Mississippi River")]
    [InlineData("Tornado Oklahoma Plains")]
    [InlineData("Hurricane Atlantic Coast")]
    public void DisasterEvent_SetName_AcceptsValidNames(string name)
    {
        // Arrange
        var disasterEvent = new DisasterEvent();

        // Act
        disasterEvent.Name = name;

        // Assert
        Assert.Equal(name, disasterEvent.Name);
    }

    [Fact]
    public void DisasterEvent_SetDisasterTypeId_AcceptsValidId()
    {
        // Arrange
        var disasterEvent = new DisasterEvent();
        var typeId = 2;

        // Act
        disasterEvent.DisasterTypeId = typeId;

        // Assert
        Assert.Equal(typeId, disasterEvent.DisasterTypeId);
    }

    [Fact]
    public void DisasterEvent_SetName_LongName_SetsCorrectly()
    {
        // Arrange
        var disasterEvent = new DisasterEvent();
        var longName = new string('A', 200); // 200 character name

        // Act
        disasterEvent.Name = longName;

        // Assert
        Assert.Equal(longName, disasterEvent.Name);
        Assert.Equal(200, disasterEvent.Name.Length);
    }

    [Fact]
    public void DisasterEvent_SetName_EmptyString_SetsCorrectly()
    {
        // Arrange
        var disasterEvent = new DisasterEvent();

        // Act
        disasterEvent.Name = string.Empty;

        // Assert
        Assert.Equal(string.Empty, disasterEvent.Name);
    }

    [Fact]
    public void DisasterEvent_NavigationProperties_InitializeCorrectly()
    {
        // Arrange & Act
        var disasterEvent = new DisasterEvent();

        // Assert
        Assert.NotNull(disasterEvent.DisasterReports);
        Assert.Empty(disasterEvent.DisasterReports);
    }

    [Fact]
    public void DisasterEvent_WithDisasterType_SetsNavigationPropertyCorrectly()
    {
        // Arrange
        var disasterTypeId = 3;
        var disasterType = new DisasterType
        {
            Id = disasterTypeId,
            Name = "Hurricane",
            Category = DisasterCategory.Natural
        };

        // Act
        var disasterEvent = new DisasterEvent
        {
            DisasterTypeId = disasterTypeId,
            DisasterType = disasterType
        };

        // Assert
        Assert.Equal(disasterTypeId, disasterEvent.DisasterTypeId);
        Assert.Equal(disasterType, disasterEvent.DisasterType);
        Assert.Equal("Hurricane", disasterEvent.DisasterType.Name);
    }

    [Fact]
    public void DisasterEvent_WithDisasterReports_SetsCollectionCorrectly()
    {
        // Arrange
        var disasterEventId = Guid.NewGuid();
        var report1 = new DisasterReport { Id = Guid.NewGuid(), DisasterEventId = disasterEventId, Title = "Report 1" };
        var report2 = new DisasterReport { Id = Guid.NewGuid(), DisasterEventId = disasterEventId, Title = "Report 2" };
        
        var disasterEvent = new DisasterEvent { Id = disasterEventId };

        // Act
        disasterEvent.DisasterReports.Add(report1);
        disasterEvent.DisasterReports.Add(report2);

        // Assert
        Assert.Equal(2, disasterEvent.DisasterReports.Count);
        Assert.Contains(report1, disasterEvent.DisasterReports);
        Assert.Contains(report2, disasterEvent.DisasterReports);
        Assert.All(disasterEvent.DisasterReports, report => Assert.Equal(disasterEventId, report.DisasterEventId));
    }

    [Fact]
    public void DisasterEvent_CompleteEventData_AllPropertiesSet()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "Hurricane Katrina 2024";
        var disasterTypeId = 1;

        // Act
        var disasterEvent = new DisasterEvent
        {
            Id = id,
            Name = name,
            DisasterTypeId = disasterTypeId
        };

        // Assert
        Assert.NotEqual(Guid.Empty, disasterEvent.Id);
        Assert.False(string.IsNullOrEmpty(disasterEvent.Name));
        Assert.NotEqual(0, disasterEvent.DisasterTypeId);
        Assert.NotNull(disasterEvent.DisasterReports);
        Assert.Empty(disasterEvent.DisasterReports);
    }
}