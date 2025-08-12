using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using DisasterApp.WebApi.Controllers;
using System;
using System.Collections.Generic;

namespace DisasterApp.Tests.Controllers;

public class ConfigControllerTests
{
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly ConfigController _controller;

    public ConfigControllerTests()
    {
        _mockConfiguration = new Mock<IConfiguration>();
        _controller = new ConfigController(_mockConfiguration.Object);
    }

    [Fact]
    public void GetGoogleClientId_WithValidClientId_ReturnsOkWithClientId()
    {
        // Arrange
        var expectedClientId = "test-google-client-id";
        _mockConfiguration.Setup(x => x["GoogleAuth:ClientId"])
                         .Returns(expectedClientId);

        // Act
        var result = _controller.GetGoogleClientId();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        
        var response = okResult.Value;
        var responseType = response.GetType();
        var clientIdProperty = responseType.GetProperty("clientId");
        
        Assert.NotNull(clientIdProperty);
        var clientIdValue = clientIdProperty.GetValue(response) as string;
        Assert.Equal(expectedClientId, clientIdValue);
    }

    [Fact]
    public void GetGoogleClientId_WithNullClientId_ReturnsBadRequest()
    {
        // Arrange
        _mockConfiguration.Setup(x => x["GoogleAuth:ClientId"])
                         .Returns((string)null);

        // Act
        var result = _controller.GetGoogleClientId();

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Google Client ID not configured", badRequestResult.Value);
    }

    [Fact]
    public void GetGoogleClientId_WithEmptyClientId_ReturnsBadRequest()
    {
        // Arrange
        _mockConfiguration.Setup(x => x["GoogleAuth:ClientId"])
                         .Returns(string.Empty);

        // Act
        var result = _controller.GetGoogleClientId();

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Google Client ID not configured", badRequestResult.Value);
    }

    [Fact]
    public void GetGoogleClientId_WithWhitespaceClientId_ReturnsBadRequest()
    {
        // Arrange
        _mockConfiguration.Setup(x => x["GoogleAuth:ClientId"])
                         .Returns("   ");

        // Act
        var result = _controller.GetGoogleClientId();

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Google Client ID not configured", badRequestResult.Value);
    }

    [Fact]
    public void Constructor_WithConfiguration_InitializesSuccessfully()
    {
        // Arrange & Act
        var controller = new ConfigController(_mockConfiguration.Object);

        // Assert
        Assert.NotNull(controller);
    }

    [Fact]
    public void Constructor_WithNullConfiguration_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ConfigController(null));
    }

    [Theory]
    [InlineData("valid-client-id-123")]
    [InlineData("another-valid-id-456")]
    [InlineData("complex.client.id.with.dots")]
    public void GetGoogleClientId_WithVariousValidClientIds_ReturnsOkWithCorrectId(string clientId)
    {
        // Arrange
        _mockConfiguration.Setup(x => x["GoogleAuth:ClientId"])
                         .Returns(clientId);

        // Act
        var result = _controller.GetGoogleClientId();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value;
        var responseType = response.GetType();
        var clientIdProperty = responseType.GetProperty("clientId");
        var clientIdValue = clientIdProperty.GetValue(response) as string;
        
        Assert.Equal(clientId, clientIdValue);
    }

    [Fact]
    public void GetGoogleClientId_ConfigurationAccessedCorrectly_VerifiesConfigurationCall()
    {
        // Arrange
        var expectedClientId = "test-client-id";
        _mockConfiguration.Setup(x => x["GoogleAuth:ClientId"])
                         .Returns(expectedClientId)
                         .Verifiable();

        // Act
        var result = _controller.GetGoogleClientId();

        // Assert
        _mockConfiguration.Verify(x => x["GoogleAuth:ClientId"], Times.Once);
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }
}