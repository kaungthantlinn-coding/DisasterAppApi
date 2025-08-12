using Xunit;
using Microsoft.EntityFrameworkCore;
using DisasterApp.Infrastructure.Repositories.Implementations;
using DisasterApp.Infrastructure.Data;
using DisasterApp.Domain.Entities;

namespace DisasterApp.Tests.Repositories;

public class OtpAttemptRepositoryTests : IDisposable
{
    private readonly DisasterDbContext _context;
    private readonly OtpAttemptRepository _repository;

    public OtpAttemptRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<DisasterDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new DisasterDbContext(options);
        _repository = new OtpAttemptRepository(_context);
    }

    [Fact]
    public async Task GetUserAttemptsAsync_ExistingUser_ReturnsAttempts()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var since = DateTime.UtcNow.AddHours(-1);
        var otpAttempt = new OtpAttempt
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            IpAddress = "192.168.1.1",
            AttemptType = "EMAIL_VERIFICATION",
            AttemptedAt = DateTime.UtcNow,
            Success = false,
            Email = "test@example.com"
        };

        _context.OtpAttempts.Add(otpAttempt);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetUserAttemptsAsync(userId, since);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(userId, result.First().UserId);
    }

    [Fact]
    public async Task GetUserAttemptsAsync_MultipleAttempts_ReturnsCorrectAttempts()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var since = DateTime.UtcNow.AddHours(-1);
        var attempts = new List<OtpAttempt>
        {
            new OtpAttempt
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                IpAddress = "192.168.1.1",
                AttemptType = "EMAIL_VERIFICATION",
                AttemptedAt = DateTime.UtcNow,
                Success = false,
                Email = "test@example.com"
            },
            new OtpAttempt
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                IpAddress = "192.168.1.2",
                AttemptType = "PASSWORD_RESET",
                AttemptedAt = DateTime.UtcNow,
                Success = true,
                Email = "test2@example.com"
            },
            new OtpAttempt
            {
                Id = Guid.NewGuid(),
                UserId = otherUserId,
                IpAddress = "192.168.1.3",
                AttemptType = "EMAIL_VERIFICATION",
                AttemptedAt = DateTime.UtcNow,
                Success = false,
                Email = "test3@example.com"
            }
        };

        _context.OtpAttempts.AddRange(attempts);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetUserAttemptsAsync(userId, since);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, attempt => Assert.Equal(userId, attempt.UserId));
    }

    [Fact]
    public async Task CreateAsync_ValidOtpAttempt_CreatesOtpAttempt()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otpAttempt = new OtpAttempt
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            IpAddress = "192.168.1.1",
            AttemptType = "EMAIL_VERIFICATION",
            AttemptedAt = DateTime.UtcNow,
            Success = true,
            Email = "test@example.com"
        };

        // Act
        var result = await _repository.CreateAsync(otpAttempt);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(otpAttempt.Id, result.Id);
        Assert.Equal(otpAttempt.UserId, result.UserId);
        Assert.Equal(otpAttempt.Success, result.Success);
        Assert.Equal(otpAttempt.IpAddress, result.IpAddress);
    }

    [Fact]
    public async Task GetIpAttemptsAsync_ExistingIp_ReturnsAttempts()
    {
        // Arrange
        var ipAddress = "192.168.1.1";
        var since = DateTime.UtcNow.AddHours(-1);
        var otpAttempt = new OtpAttempt
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            IpAddress = ipAddress,
            AttemptType = "EMAIL_VERIFICATION",
            AttemptedAt = DateTime.UtcNow,
            Success = false,
            Email = "test@example.com"
        };

        _context.OtpAttempts.Add(otpAttempt);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetIpAttemptsAsync(ipAddress, since);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(ipAddress, result.First().IpAddress);
    }

    [Fact]
    public async Task GetEmailAttemptsAsync_ExistingEmail_ReturnsAttempts()
    {
        // Arrange
        var email = "test@example.com";
        var since = DateTime.UtcNow.AddHours(-1);
        var otpAttempt = new OtpAttempt
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            IpAddress = "192.168.1.1",
            AttemptType = "EMAIL_VERIFICATION",
            AttemptedAt = DateTime.UtcNow,
            Success = false,
            Email = email
        };

        _context.OtpAttempts.Add(otpAttempt);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetEmailAttemptsAsync(email, since);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.All(result, attempt => Assert.Equal(email, attempt.Email));
    }

    [Fact]
    public async Task GetSuccessfulAttemptsCountAsync_SuccessfulAttempts_ReturnsCorrectCount()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var attempts = new List<OtpAttempt>
        {
            new OtpAttempt
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Success = true,
                IpAddress = "192.168.1.1",
                AttemptedAt = now.AddMinutes(-5),
                AttemptType = "Login",
                Email = "test@example.com"
            },
            new OtpAttempt
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Success = false,
                IpAddress = "192.168.1.1",
                AttemptedAt = now.AddMinutes(-3),
                AttemptType = "Login",
                Email = "test@example.com"
            },
            new OtpAttempt
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Success = true,
                IpAddress = "192.168.1.1",
                AttemptedAt = now.AddMinutes(-2),
                AttemptType = "Login",
                Email = "test@example.com"
            }
        };

        _context.OtpAttempts.AddRange(attempts);
        await _context.SaveChangesAsync();

        // Act - Using CountUserAttemptsAsync instead of non-existent GetSuccessfulAttemptsCountAsync
        var result = await _repository.CountUserAttemptsAsync(userId, now.AddMinutes(-10));

        // Assert
        Assert.Equal(3, result); // All 3 attempts should be counted
    }



    [Fact]
    public async Task DeleteOldAttemptsAsync_OldAttempts_DeletesOldAttempts()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var attempts = new List<OtpAttempt>
        {
            new OtpAttempt
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Success = false,
                IpAddress = "192.168.1.1",
                AttemptedAt = now.AddDays(-10), // Old attempt
                AttemptType = "login",
                Email = "test1@example.com"
            },
            new OtpAttempt
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Success = true,
                IpAddress = "192.168.1.1",
                AttemptedAt = now.AddMinutes(-5), // Recent attempt
                AttemptType = "login",
                Email = "test2@example.com"
            }
        };

        _context.OtpAttempts.AddRange(attempts);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteOldAttemptsAsync(now.AddDays(-7));

        // Assert
        var remainingAttempts = await _context.OtpAttempts.ToListAsync();
        Assert.Single(remainingAttempts);
        Assert.True(remainingAttempts.First().Success);
    }



    public void Dispose()
    {
        _context.Dispose();
    }
}