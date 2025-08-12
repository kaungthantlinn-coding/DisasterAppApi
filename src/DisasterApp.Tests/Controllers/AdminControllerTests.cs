using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using DisasterApp.WebApi.Controllers;
using DisasterApp.Application.Services.Interfaces;
using DisasterApp.Application.DTOs;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace DisasterApp.Tests.Controllers;

public class AdminControllerTests
{
    private readonly Mock<ILogger<AdminController>> _mockLogger;
    private readonly Mock<IAuditService> _mockAuditService;
    private readonly AdminController _controller;

    public AdminControllerTests()
    {
        _mockLogger = new Mock<ILogger<AdminController>>();
        _mockAuditService = new Mock<IAuditService>();
        _controller = new AdminController(_mockLogger.Object, _mockAuditService.Object);
    }

    [Fact]
    public void GetAdminDashboard_ReturnsOkWithMessage()
    {
        // Act
        var result = _controller.GetAdminDashboard();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        
        var response = okResult.Value;
        var responseType = response.GetType();
        var messageProperty = responseType.GetProperty("message");
        var timestampProperty = responseType.GetProperty("timestamp");
        
        Assert.NotNull(messageProperty);
        Assert.NotNull(timestampProperty);
        
        var messageValue = messageProperty.GetValue(response) as string;
        var timestampValue = timestampProperty.GetValue(response);
        
        Assert.Equal("Welcome to Admin Dashboard", messageValue);
        Assert.IsType<DateTime>(timestampValue);
    }

    [Fact]
    public void GetAllUsers_ReturnsOkWithUserMessage()
    {
        // Act
        var result = _controller.GetAllUsers();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        
        var response = okResult.Value;
        var responseType = response.GetType();
        var messageProperty = responseType.GetProperty("message");
        var dataProperty = responseType.GetProperty("data");
        
        Assert.NotNull(messageProperty);
        Assert.NotNull(dataProperty);
        
        var messageValue = messageProperty.GetValue(response) as string;
        var dataValue = dataProperty.GetValue(response) as string;
        
        Assert.Equal("Admin can view all users", messageValue);
        Assert.Equal("User list would be here", dataValue);
    }

    [Fact]
    public void GetReports_ReturnsOkWithReportsMessage()
    {
        // Act
        var result = _controller.GetReports();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        
        var response = okResult.Value;
        var responseType = response.GetType();
        var messageProperty = responseType.GetProperty("message");
        var dataProperty = responseType.GetProperty("data");
        
        Assert.NotNull(messageProperty);
        Assert.NotNull(dataProperty);
        
        var messageValue = messageProperty.GetValue(response) as string;
        var dataValue = dataProperty.GetValue(response) as string;
        
        Assert.Equal("Admin or CJ can view reports", messageValue);
        Assert.Equal("Reports would be here", dataValue);
    }

    [Fact]
    public void UpdateSystemSettings_WithSettings_ReturnsOkWithMessage()
    {
        // Arrange
        var settings = new { setting1 = "value1", setting2 = "value2" };

        // Act
        var result = _controller.UpdateSystemSettings(settings);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        
        var response = okResult.Value;
        var responseType = response.GetType();
        var messageProperty = responseType.GetProperty("message");
        var timestampProperty = responseType.GetProperty("timestamp");
        
        Assert.NotNull(messageProperty);
        Assert.NotNull(timestampProperty);
        
        var messageValue = messageProperty.GetValue(response) as string;
        var timestampValue = timestampProperty.GetValue(response);
        
        Assert.Equal("System settings updated by admin", messageValue);
        Assert.IsType<DateTime>(timestampValue);
    }

    [Fact]
    public async Task GetAuditLogs_WithValidFilters_ReturnsOkWithLogs()
    {
        // Arrange
        var filters = new AuditLogFiltersDto();
        var expectedLogs = new List<AuditLogDto>
        {
            new AuditLogDto { Id = "1", User = new AuditLogUserDto { Id = "user1", Name = "User 1", Email = "user1@test.com" }, Action = "Login", Timestamp = DateTime.UtcNow, Severity = "info", Details = "Login successful", Resource = "Auth" },
            new AuditLogDto { Id = "2", User = new AuditLogUserDto { Id = "user2", Name = "User 2", Email = "user2@test.com" }, Action = "Create", Timestamp = DateTime.UtcNow, Severity = "info", Details = "Created item", Resource = "API" }
        };
        
        var paginatedResult = new PaginatedAuditLogsDto
        {
            Logs = expectedLogs,
            TotalCount = expectedLogs.Count,
            Page = 1,
            PageSize = 10,
            HasMore = false
        };
        
        _mockAuditService.Setup(x => x.GetLogsAsync(filters))
                        .ReturnsAsync(paginatedResult);

        // Act
        var result = await _controller.GetAuditLogs(filters);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedResult = Assert.IsType<PaginatedAuditLogsDto>(okResult.Value);
        Assert.Equal(2, returnedResult.TotalCount);
        Assert.Equal(2, returnedResult.Logs.Count());
    }

    [Fact]
    public async Task GetAuditLogs_WithException_ReturnsInternalServerError()
    {
        // Arrange
        var filters = new AuditLogFiltersDto();
        var expectedException = new Exception("Database error");
        
        _mockAuditService.Setup(x => x.GetLogsAsync(filters))
                        .ThrowsAsync(expectedException);

        // Act
        var result = await _controller.GetAuditLogs(filters);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusResult.StatusCode);
        Assert.NotNull(statusResult.Value);
    }

    [Fact]
    public async Task GetAuditLogStatistics_WithValidData_ReturnsOkWithStats()
    {
        // Arrange
        var expectedStats = new AuditLogStatsDto
        {
            TotalLogs = 100,
            CriticalAlerts = 5,
            SecurityEvents = 15,
            SystemErrors = 3,
            RecentActivity = 10
        };
        
        _mockAuditService.Setup(x => x.GetStatisticsAsync())
                        .ReturnsAsync(expectedStats);

        // Act
        var result = await _controller.GetAuditLogStatistics();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedStats = Assert.IsType<AuditLogStatsDto>(okResult.Value);
        Assert.Equal(expectedStats.TotalLogs, returnedStats.TotalLogs);
        Assert.Equal(expectedStats.CriticalAlerts, returnedStats.CriticalAlerts);
        Assert.Equal(expectedStats.SecurityEvents, returnedStats.SecurityEvents);
        Assert.Equal(expectedStats.SystemErrors, returnedStats.SystemErrors);
        Assert.Equal(expectedStats.RecentActivity, returnedStats.RecentActivity);
    }

    [Fact]
    public async Task GetAuditLogStatistics_WithException_ReturnsInternalServerError()
    {
        // Arrange
        var expectedException = new Exception("Statistics error");
        
        _mockAuditService.Setup(x => x.GetStatisticsAsync())
                        .ThrowsAsync(expectedException);

        // Act
        var result = await _controller.GetAuditLogStatistics();

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusResult.StatusCode);
        Assert.NotNull(statusResult.Value);
    }

    [Fact]
    public async Task ExportAuditLogs_WithCsvFormat_ReturnsFileResult()
    {
        // Arrange
        var format = "csv";
        var filters = new AuditLogFiltersDto();
        var expectedData = new byte[] { 1, 2, 3, 4, 5 };
        
        _mockAuditService.Setup(x => x.ExportLogsAsync(format, filters))
                        .ReturnsAsync(expectedData);

        // Act
        var result = await _controller.ExportAuditLogs(format, filters);

        // Assert
        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal("text/csv", fileResult.ContentType);
        Assert.Contains(".csv", fileResult.FileDownloadName);
        Assert.Equal(expectedData, fileResult.FileContents);
    }

    [Fact]
    public async Task ExportAuditLogs_WithExcelFormat_ReturnsFileResult()
    {
        // Arrange
        var format = "excel";
        var filters = new AuditLogFiltersDto();
        var expectedData = new byte[] { 1, 2, 3, 4, 5 };
        
        _mockAuditService.Setup(x => x.ExportLogsAsync(format, filters))
                        .ReturnsAsync(expectedData);

        // Act
        var result = await _controller.ExportAuditLogs(format, filters);

        // Assert
        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileResult.ContentType);
        Assert.Contains(".xlsx", fileResult.FileDownloadName);
        Assert.Equal(expectedData, fileResult.FileContents);
    }

    [Fact]
    public async Task ExportAuditLogs_WithNullFilters_UsesDefaultFilters()
    {
        // Arrange
        var format = "csv";
        var expectedData = new byte[] { 1, 2, 3, 4, 5 };
        
        _mockAuditService.Setup(x => x.ExportLogsAsync(format, It.IsAny<AuditLogFiltersDto>()))
                        .ReturnsAsync(expectedData);

        // Act
        var result = await _controller.ExportAuditLogs(format, null!);

        // Assert
        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.NotNull(fileResult);
        _mockAuditService.Verify(x => x.ExportLogsAsync(format, It.IsAny<AuditLogFiltersDto>()), Times.Once);
    }

    [Fact]
    public async Task ExportAuditLogs_WithException_ReturnsInternalServerError()
    {
        // Arrange
        var format = "csv";
        var filters = new AuditLogFiltersDto();
        var expectedException = new Exception("Export error");
        
        _mockAuditService.Setup(x => x.ExportLogsAsync(format, filters))
                        .ThrowsAsync(expectedException);

        // Act
        var result = await _controller.ExportAuditLogs(format, filters);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusResult.StatusCode);
        Assert.NotNull(statusResult.Value);
    }

    [Fact]
    public void Constructor_WithValidParameters_InitializesSuccessfully()
    {
        // Arrange & Act
        var controller = new AdminController(_mockLogger.Object, _mockAuditService.Object);

        // Assert
        Assert.NotNull(controller);
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new AdminController(null!, _mockAuditService.Object));
    }

    [Fact]
    public void Constructor_WithNullAuditService_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new AdminController(_mockLogger.Object, null!));
    }
}