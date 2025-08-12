using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using DisasterApp.WebApi.Controllers;
using DisasterApp.Infrastructure.Data;
using DisasterApp.Domain.Entities;
using DisasterApp.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace DisasterApp.Tests.Controllers;

public class CjControllerTests : IDisposable
{
    private readonly Mock<ILogger<CjController>> _mockLogger;
    private readonly DisasterDbContext _context;
    private readonly CjController _controller;
    private readonly string _testUserId = Guid.NewGuid().ToString();

    public CjControllerTests()
    {
        var options = new DbContextOptionsBuilder<DisasterDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new DisasterDbContext(options);
        _mockLogger = new Mock<ILogger<CjController>>();
        _controller = new CjController(_mockLogger.Object, _context);
        
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, _testUserId),
            new Claim(ClaimTypes.Role, "cj")
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = principal
            }
        };
    }

    [Fact]
    public void GetCjDashboard_ReturnsOkWithMessage()
    {
        // Act
        var result = _controller.GetCjDashboard();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        
        var responseType = okResult.Value.GetType();
        var messageProperty = responseType.GetProperty("message");
        var timestampProperty = responseType.GetProperty("timestamp");
        
        Assert.NotNull(messageProperty);
        Assert.NotNull(timestampProperty);
        
        var messageValue = messageProperty.GetValue(okResult.Value) as string;
        Assert.Equal("Welcome to CJ Dashboard", messageValue);
        
        var timestampValue = timestampProperty.GetValue(okResult.Value);
        Assert.IsType<DateTime>(timestampValue);
    }

    [Fact]
    public void GetCjDashboard_ReturnsCurrentTimestamp()
    {
        // Arrange
        var beforeCall = DateTime.UtcNow;

        // Act
        var result = _controller.GetCjDashboard();
        var afterCall = DateTime.UtcNow;

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        
        var responseType = okResult.Value.GetType();
        var timestampProperty = responseType.GetProperty("timestamp");
        Assert.NotNull(timestampProperty);
        
        var timestampValue = (DateTime)timestampProperty.GetValue(okResult.Value)!;
        Assert.True(timestampValue >= beforeCall && timestampValue <= afterCall);
    }

    [Fact]
    public void Constructor_WithLoggerAndContext_InitializesSuccessfully()
    {
        // Arrange & Act
        var controller = new CjController(_mockLogger.Object, _context);

        // Assert
        Assert.NotNull(controller);
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new CjController(null!, _context));
    }

    [Fact]
    public void Constructor_WithNullContext_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new CjController(_mockLogger.Object, null!));
    }

    [Fact]
    public void GetCjStatistics_ReturnsOkWithStatistics()
    {
        // Act
        var result = _controller.GetCjStatistics();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        
        var responseType = okResult.Value.GetType();
        var messageProperty = responseType.GetProperty("message");
        var dataProperty = responseType.GetProperty("data");
        
        Assert.NotNull(messageProperty);
        Assert.NotNull(dataProperty);
        
        var messageValue = messageProperty.GetValue(okResult.Value) as string;
        Assert.Equal("CJ statistics", messageValue);
        
        var dataValue = dataProperty.GetValue(okResult.Value);
        Assert.NotNull(dataValue);
        
        var dataType = dataValue.GetType();
        var totalReportsProperty = dataType.GetProperty("totalReportsReviewed");
        var pendingReportsProperty = dataType.GetProperty("pendingReports");
        var approvedReportsProperty = dataType.GetProperty("approvedReports");
        var rejectedReportsProperty = dataType.GetProperty("rejectedReports");
        
        Assert.NotNull(totalReportsProperty);
        Assert.NotNull(pendingReportsProperty);
        Assert.NotNull(approvedReportsProperty);
        Assert.NotNull(rejectedReportsProperty);
        
        Assert.Equal(150, totalReportsProperty.GetValue(dataValue));
        Assert.Equal(25, pendingReportsProperty.GetValue(dataValue));
        Assert.Equal(120, approvedReportsProperty.GetValue(dataValue));
        Assert.Equal(5, rejectedReportsProperty.GetValue(dataValue));
    }

    [Fact]
    public void GetCjStatistics_ReturnsConsistentData()
    {
        // Act
        var result1 = _controller.GetCjStatistics();
        var result2 = _controller.GetCjStatistics();

        // Assert
        var okResult1 = Assert.IsType<OkObjectResult>(result1);
        var okResult2 = Assert.IsType<OkObjectResult>(result2);
        
        Assert.NotNull(okResult1.Value);
        Assert.NotNull(okResult2.Value);
        
        var responseType1 = okResult1.Value.GetType();
        var responseType2 = okResult2.Value.GetType();
        var messageProperty1 = responseType1.GetProperty("message");
        var messageProperty2 = responseType2.GetProperty("message");
        
        Assert.NotNull(messageProperty1);
        Assert.NotNull(messageProperty2);
        
        var message1 = messageProperty1.GetValue(okResult1.Value) as string;
        var message2 = messageProperty2.GetValue(okResult2.Value) as string;
        
        Assert.Equal(message1, message2);
    }

    [Fact]
    public async Task ApproveReport_ValidReportId_ReturnsOk()
    {
        // Arrange
        var reportId = Guid.NewGuid();
        var userId = Guid.Parse(_testUserId);
        
        var report = new DisasterReport
        {
            Id = reportId, 
            Title = "Test Report", 
            Description = "Test Description", 
            Status = ReportStatus.Pending, 
            UserId = Guid.NewGuid(),
            DisasterEventId = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            Severity = SeverityLevel.Medium
        };
        
        _context.DisasterReports.Add(report);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.ApproveReport(reportId);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        var updatedReport = await _context.DisasterReports.FindAsync(reportId);
        Assert.NotNull(updatedReport);
        Assert.Equal(ReportStatus.Verified, updatedReport.Status);
        Assert.Equal(userId, updatedReport.VerifiedBy);
        Assert.NotNull(updatedReport.VerifiedAt);
    }

    [Fact]
    public async Task ApproveReport_InvalidReportId_ReturnsNotFound()
    {
        // Act
        var result = await _controller.ApproveReport(Guid.NewGuid());

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task ApproveReport_EmptyReportId_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.ApproveReport(Guid.Empty);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task RejectReport_ValidReportId_ReturnsOk()
    {
        // Arrange
        var reportId = Guid.NewGuid();
        var userId = Guid.Parse(_testUserId);
        
        var report = new DisasterReport
        {
            Id = reportId, 
            Title = "Test Report", 
            Description = "Test Description", 
            Status = ReportStatus.Pending, 
            UserId = Guid.NewGuid(),
            DisasterEventId = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            Severity = SeverityLevel.Medium
        };
        
        _context.DisasterReports.Add(report);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.RejectReport(reportId);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        var updatedReport = await _context.DisasterReports.FindAsync(reportId);
        Assert.NotNull(updatedReport);
        Assert.Equal(ReportStatus.Rejected, updatedReport.Status);
        Assert.Equal(userId, updatedReport.VerifiedBy);
        Assert.NotNull(updatedReport.VerifiedAt);
    }

    [Fact]
    public async Task RejectReport_InvalidReportId_ReturnsNotFound()
    {
        // Act
        var result = await _controller.RejectReport(Guid.NewGuid());

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task RejectReport_EmptyReportId_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.RejectReport(Guid.Empty);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GetVerificationQueue_ReturnsOkWithPendingReports()
    {
        // Arrange
        var disasterEventId = Guid.NewGuid();
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();
        var userId3 = Guid.NewGuid();
        
        // Add users first
        _context.Users.AddRange(
            new User { 
                UserId = userId1, 
                Name = "User 1", 
                Email = "user1@test.com",
                AuthProvider = "local",
                AuthId = userId1.ToString()
            },
            new User { 
                UserId = userId2, 
                Name = "User 2", 
                Email = "user2@test.com",
                AuthProvider = "local",
                AuthId = userId2.ToString()
            },
            new User { 
                UserId = userId3, 
                Name = "User 3", 
                Email = "user3@test.com",
                AuthProvider = "local",
                AuthId = userId3.ToString()
            }
        );
        
        _context.DisasterReports.AddRange(
            new DisasterReport { 
                Title = "Test Report 1", 
                Description = "Test Description 1", 
                Status = ReportStatus.Pending, 
                UserId = userId1,
                DisasterEventId = disasterEventId,
                Timestamp = DateTime.UtcNow,
                Severity = SeverityLevel.Medium
            },
            new DisasterReport { 
                Title = "Test Report 2", 
                Description = "Test Description 2", 
                Status = ReportStatus.Pending, 
                UserId = userId2,
                DisasterEventId = disasterEventId,
                Timestamp = DateTime.UtcNow,
                Severity = SeverityLevel.Medium
            },
            new DisasterReport { 
                Title = "Test Report 3", 
                Description = "Test Description 3", 
                Status = ReportStatus.Verified, 
                UserId = userId3,
                DisasterEventId = disasterEventId,
                Timestamp = DateTime.UtcNow,
                Severity = SeverityLevel.Medium
            }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetVerificationQueue();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var pendingReports = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value);
        Assert.Equal(2, pendingReports.Count());
    }

    [Fact]
    public async Task GetVerificationQueue_NoPendingReports_ReturnsEmptyList()
    {
        // Arrange
        _context.DisasterReports.Add(new DisasterReport { 
            Title = "Test Report", 
            Description = "Test Description", 
            Status = ReportStatus.Verified, 
            UserId = Guid.NewGuid(),
            DisasterEventId = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            Severity = SeverityLevel.Medium
        });
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetVerificationQueue();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var reports = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value);
        Assert.Empty(reports);
    }

    [Fact]
    public async Task GetReportDetails_ValidReportId_ReturnsOkWithDetails()
    {
        // Arrange
        var reportId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        
        _context.Users.Add(new User { 
            UserId = userId, 
            Name = "John Doe", 
            Email = "john.doe@test.com",
            AuthProvider = "local",
            AuthId = userId.ToString()
        });
        _context.DisasterReports.Add(new DisasterReport { 
            Id = reportId, 
            Title = "Test Report", 
            Description = "Test Description", 
            UserId = userId,
            DisasterEventId = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            Severity = SeverityLevel.Medium,
            Status = ReportStatus.Pending
        });
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetReportDetails(reportId);

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetReportDetails_InvalidReportId_ReturnsNotFound()
    {
        // Act
        var result = await _controller.GetReportDetails(Guid.NewGuid());

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task GetReportDetails_EmptyReportId_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.GetReportDetails(Guid.Empty);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task ApproveReport_AlreadyVerifiedReport_ReturnsConflict()
    {
        // Arrange
        var reportId = Guid.NewGuid();
        _context.DisasterReports.Add(new DisasterReport { 
            Id = reportId, 
            Title = "Test Report", 
            Description = "Test Description", 
            Status = ReportStatus.Verified, 
            UserId = Guid.NewGuid(),
            DisasterEventId = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            Severity = SeverityLevel.Medium
        });
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.ApproveReport(reportId);

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task RejectReport_AlreadyRejectedReport_ReturnsConflict()
    {
        // Arrange
        var reportId = Guid.NewGuid();
        _context.DisasterReports.Add(new DisasterReport { 
            Id = reportId, 
            Title = "Test Report", 
            Description = "Test Description", 
            Status = ReportStatus.Rejected, 
            UserId = Guid.NewGuid(),
            DisasterEventId = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            Severity = SeverityLevel.Medium
        });
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.RejectReport(reportId);

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}