using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using DisasterApp.WebApi.Controllers;
using DisasterApp.Application.Services.Interfaces;
using DisasterApp.Application.DTOs;
using DisasterApp.Domain.Entities;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace DisasterApp.Tests.Controllers;

public class AuditLogsControllerTests
{
    private readonly Mock<IAuditService> _mockAuditService;
    private readonly Mock<ILogger<AuditLogsController>> _mockLogger;
    private readonly AuditLogsController _controller;

    public AuditLogsControllerTests()
    {
        _mockAuditService = new Mock<IAuditService>();
        _mockLogger = new Mock<ILogger<AuditLogsController>>();
        _controller = new AuditLogsController(_mockLogger.Object, _mockAuditService.Object);
        
        // Set up HttpContext and User claims
        var httpContext = new DefaultHttpContext();
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        httpContext.User = principal;
        
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    [Fact]
    public async Task GetAuditLogs_ReturnsOkWithAuditLogs()
    {
        // Arrange
        var auditLogs = new List<AuditLogDto>
        {
            new()
            {
                Id = Guid.NewGuid().ToString(),
                User = new AuditLogUserDto { Id = Guid.NewGuid().ToString(), Name = "Test User", Email = "test@example.com" },
                Action = "Login",
                Resource = "User",
                Timestamp = DateTime.UtcNow,
                IpAddress = "192.168.1.1",
                Severity = "info",
                Details = "User login successful"
            },
            new()
            {
                Id = Guid.NewGuid().ToString(),
                User = new AuditLogUserDto { Id = Guid.NewGuid().ToString(), Name = "Test User 2", Email = "test2@example.com" },
                Action = "Create",
                Resource = "DisasterReport",
                Timestamp = DateTime.UtcNow,
                IpAddress = "192.168.1.2",
                Severity = "info",
                Details = "Disaster report created successfully"
            }
        };

        var paginatedResult = new PaginatedAuditLogsDto
        {
            Logs = auditLogs,
            TotalCount = auditLogs.Count,
            Page = 1,
            PageSize = 10,
            HasMore = false
        };

        _mockAuditService.Setup(x => x.GetLogsAsync(It.IsAny<AuditLogFiltersDto>()))
                       .ReturnsAsync(paginatedResult);

        // Act
        var result = await _controller.GetAuditLogs();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value;
        Assert.NotNull(response);

        // Verify the response structure matches the controller's anonymous object
        var responseType = response.GetType();
        var logsProperty = responseType.GetProperty("logs");
        Assert.NotNull(logsProperty);
        
        var logs = logsProperty.GetValue(response) as IEnumerable<object>;
        Assert.NotNull(logs);
        Assert.Equal(2, logs.Count());
    }

    [Fact]
    public async Task GetAuditLogs_WithPagination_ReturnsOkWithPaginatedResults()
    {
        // Arrange
        var page = 1;
        var pageSize = 10;
        var auditLogs = new List<AuditLogDto>
        {
            new()
            {
                Id = Guid.NewGuid().ToString(),
                User = new AuditLogUserDto { Id = Guid.NewGuid().ToString(), Name = "Admin User", Email = "admin@example.com" },
                Action = "Login",
                Resource = "User",
                Timestamp = DateTime.UtcNow,
                IpAddress = "192.168.1.1",
                Severity = "info",
                Details = "User login successful"
            }
        };

        var paginatedResult = new PaginatedAuditLogsDto
        {
            Logs = auditLogs,
            TotalCount = auditLogs.Count,
            Page = page,
            PageSize = pageSize,
            HasMore = false
        };

        _mockAuditService.Setup(x => x.GetLogsAsync(It.IsAny<AuditLogFiltersDto>()))
                       .ReturnsAsync(paginatedResult);

        // Act
        var result = await _controller.GetAuditLogs(page, pageSize);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value;
        Assert.NotNull(response);

        // Verify the response structure
        var responseType = response.GetType();
        var logsProperty = responseType.GetProperty("logs");
        Assert.NotNull(logsProperty);
        
        var logs = logsProperty.GetValue(response) as IEnumerable<object>;
        Assert.NotNull(logs);
        Assert.Single(logs);
    }

    [Fact]
    public async Task GetAuditLogs_WithFilters_ReturnsOkWithFilteredResults()
    {
        // Arrange
        var action = "Login";
        var startDate = DateTime.UtcNow.AddDays(-7);
        var endDate = DateTime.UtcNow;
        var auditLogs = new List<AuditLogDto>
        {
            new()
            {
                Id = Guid.NewGuid().ToString(),
                User = new AuditLogUserDto { Id = Guid.NewGuid().ToString(), Name = "Filter User", Email = "filter@example.com" },
                Action = "Login",
                Resource = "User",
                Timestamp = DateTime.UtcNow,
                IpAddress = "192.168.1.1",
                Severity = "info",
                Details = "User login successful"
            }
        };

        var paginatedResult = new PaginatedAuditLogsDto
        {
            Logs = auditLogs,
            TotalCount = auditLogs.Count,
            Page = 1,
            PageSize = 10,
            HasMore = false
        };

        _mockAuditService.Setup(x => x.GetLogsAsync(It.IsAny<AuditLogFiltersDto>()))
                       .ReturnsAsync(paginatedResult);

        // Act
        var result = await _controller.GetAuditLogs(action: action, startDate: startDate, endDate: endDate);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value;
        Assert.NotNull(response);

        // Verify the response structure
        var responseType = response.GetType();
        var logsProperty = responseType.GetProperty("logs");
        Assert.NotNull(logsProperty);
        
        var logs = logsProperty.GetValue(response) as IEnumerable<object>;
        Assert.NotNull(logs);
        Assert.Single(logs);
    }

    [Fact]
    public async Task GetAuditLogs_NoResults_ReturnsOkWithEmptyList()
    {
        // Arrange
        var auditLogs = new List<AuditLogDto>();

        var paginatedResult = new PaginatedAuditLogsDto
        {
            Logs = auditLogs,
            TotalCount = auditLogs.Count,
            Page = 1,
            PageSize = 10,
            HasMore = false
        };

        _mockAuditService.Setup(x => x.GetLogsAsync(It.IsAny<AuditLogFiltersDto>()))
                       .ReturnsAsync(paginatedResult);

        // Act
        var result = await _controller.GetAuditLogs();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value;
        Assert.NotNull(response);

        // Verify the response structure
        var responseType = response.GetType();
        var logsProperty = responseType.GetProperty("logs");
        Assert.NotNull(logsProperty);
        
        var logs = logsProperty.GetValue(response) as IEnumerable<object>;
        Assert.NotNull(logs);
        Assert.Empty(logs);
    }

    // Note: GetAuditLogById method does not exist in the actual controller
    // Removed these tests as they don't correspond to actual controller methods

    [Fact]
    public async Task ExportAuditLogs_ReturnsFileResult()
    {
        // Arrange
        var csvData = "Id,UserId,Action,Resource,Timestamp,IpAddress\n" +
                     "123e4567-e89b-12d3-a456-426614174000,987fcdeb-51a2-43d1-9f12-123456789abc,Login,User,2023-01-01T10:00:00Z,192.168.1.1";
        var csvBytes = System.Text.Encoding.UTF8.GetBytes(csvData);

        _mockAuditService.Setup(x => x.ExportLogsAsync(It.IsAny<string>(), It.IsAny<AuditLogFiltersDto>()))
            .ReturnsAsync(csvBytes);

        // Act
        var result = await _controller.ExportAuditLogs();

        // Assert
        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal("text/csv", fileResult.ContentType);
        Assert.StartsWith("audit-logs-", fileResult.FileDownloadName);
        Assert.EndsWith(".csv", fileResult.FileDownloadName);
        Assert.Equal(csvBytes, fileResult.FileContents);
    }

    [Fact]
    public async Task ExportAuditLogs_WithFilters_ReturnsFilteredFileResult()
    {
        // Arrange
        var action = "Login";
        var startDate = DateTime.UtcNow.AddDays(-7);
        var endDate = DateTime.UtcNow;
        var csvData = "Id,UserId,Action,Resource,Timestamp,IpAddress\n" +
                     "123e4567-e89b-12d3-a456-426614174000,987fcdeb-51a2-43d1-9f12-123456789abc,Login,User,2023-01-01T10:00:00Z,192.168.1.1";
        var csvBytes = System.Text.Encoding.UTF8.GetBytes(csvData);

        _mockAuditService.Setup(x => x.ExportLogsAsync(It.IsAny<string>(), It.IsAny<AuditLogFiltersDto>()))
            .ReturnsAsync(csvBytes);

        // Act
        var result = await _controller.ExportAuditLogs(action: action, startDate: startDate, endDate: endDate);

        // Assert
        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal("text/csv", fileResult.ContentType);
        Assert.StartsWith("audit-logs-", fileResult.FileDownloadName);
        Assert.EndsWith(".csv", fileResult.FileDownloadName);
        Assert.Equal(csvBytes, fileResult.FileContents);
    }

    [Fact]
    public async Task ExportAuditLogs_EmptyData_ReturnsEmptyFileResult()
    {
        // Arrange
        var csvData = "Id,UserId,Action,Resource,Timestamp,IpAddress\n";
        var csvBytes = System.Text.Encoding.UTF8.GetBytes(csvData);

        _mockAuditService.Setup(x => x.ExportLogsAsync(It.IsAny<string>(), It.IsAny<AuditLogFiltersDto>()))
            .ReturnsAsync(csvBytes);

        // Act
        var result = await _controller.ExportAuditLogs();

        // Assert
        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal("text/csv", fileResult.ContentType);
        Assert.StartsWith("audit-logs-", fileResult.FileDownloadName);
        Assert.EndsWith(".csv", fileResult.FileDownloadName);
        Assert.Equal(csvBytes, fileResult.FileContents);
    }

    // Note: GetUserAuditLogs method does not exist in the actual controller
    // Removed these tests as they don't correspond to actual controller methods

    [Fact]
    public async Task GetAuditLogStatistics_ReturnsOkWithStatistics()
    {
        // Arrange
        var statistics = new AuditLogStatsDto
        {
            TotalLogs = 1000,
            CriticalAlerts = 50,
            SecurityEvents = 300,
            SystemErrors = 800,
            RecentActivity = 150
        };

        _mockAuditService.Setup(x => x.GetStatisticsAsync())
            .ReturnsAsync(statistics);

        // Act
        var result = await _controller.GetAuditLogStatistics();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value;
        Assert.NotNull(response);
        
        // Verify the response structure matches the controller's anonymous object
        var responseType = response.GetType();
        var totalLogsProperty = responseType.GetProperty("totalLogs");
        var todayLogsProperty = responseType.GetProperty("todayLogs");
        var criticalAlertsProperty = responseType.GetProperty("criticalAlerts");
        Assert.NotNull(totalLogsProperty);
        Assert.NotNull(todayLogsProperty);
        Assert.NotNull(criticalAlertsProperty);
        
        Assert.Equal(1000, totalLogsProperty.GetValue(response));
        Assert.Equal(150, todayLogsProperty.GetValue(response)); // RecentActivity maps to todayLogs
        Assert.Equal(50, criticalAlertsProperty.GetValue(response));
    }

    [Fact]
    public async Task GetAuditLogs_ServiceThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        _mockAuditService.Setup(x => x.GetLogsAsync(It.IsAny<AuditLogFiltersDto>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _controller.GetAuditLogs();

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task ExportAuditLogs_ServiceThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        _mockAuditService.Setup(x => x.ExportLogsAsync(It.IsAny<string>(), It.IsAny<AuditLogFiltersDto>()))
            .ThrowsAsync(new Exception("Export service unavailable"));

        // Act
        var result = await _controller.ExportAuditLogs();

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task GetAuditLogs_InvalidPageSize_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.GetAuditLogs(page: 1, limit: 0);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GetAuditLogs_InvalidPage_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.GetAuditLogs(page: 0, limit: 10);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    // Note: GetUserAuditLogs method does not exist in the actual controller
    // Removed these tests as they don't correspond to actual controller methods
}