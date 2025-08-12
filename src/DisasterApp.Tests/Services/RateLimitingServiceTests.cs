using DisasterApp.Application.Services.Implementations;
using DisasterApp.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DisasterApp.Tests.Services;

public class RateLimitingServiceTests
{
    private static IConfiguration BuildConfig(Dictionary<string, string?>? overrideValues = null)
    {
        var defaults = new Dictionary<string, string?>
        {
            ["TwoFactor:MaxOtpSendPerHour"] = "3",
            ["TwoFactor:MaxOtpVerifyPerHour"] = "10",
            ["TwoFactor:MaxFailedAttemptsForLockout"] = "5",
            ["TwoFactor:LockoutDurationMinutes"] = "60",
            ["TwoFactor:MaxIpAttemptsPerHour"] = "20",
        };
        if (overrideValues != null)
        {
            foreach (var kv in overrideValues)
            {
                defaults[kv.Key] = kv.Value;
            }
        }
        return new ConfigurationBuilder().AddInMemoryCollection(defaults!).Build();
    }

    [Fact]
    public async Task CanSendOtpAsync_UserAndIpWithinLimits_ReturnsTrue()
    {
        var repo = new Mock<IOtpAttemptRepository>();
        repo.Setup(r => r.CountUserAttemptsAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), "send_otp", null))
            .ReturnsAsync(0);
        repo.Setup(r => r.CountIpAttemptsAsync(It.IsAny<string>(), It.IsAny<DateTime>(), "send_otp", null))
            .ReturnsAsync(0);

        var service = new RateLimitingService(repo.Object, BuildConfig(), Mock.Of<ILogger<RateLimitingService>>());
        var allowed = await service.CanSendOtpAsync(Guid.NewGuid(), "127.0.0.1");
        Assert.True(allowed);
    }

    [Fact]
    public async Task CanSendOtpAsync_UserExceededLimit_ReturnsFalse()
    {
        var userId = Guid.NewGuid();
        var repo = new Mock<IOtpAttemptRepository>();
        repo.Setup(r => r.CountUserAttemptsAsync(userId, It.IsAny<DateTime>(), "send_otp", null))
            .ReturnsAsync(3); // >= default max 3
        repo.Setup(r => r.CountIpAttemptsAsync(It.IsAny<string>(), It.IsAny<DateTime>(), "send_otp", null))
            .ReturnsAsync(0);

        var service = new RateLimitingService(repo.Object, BuildConfig(), Mock.Of<ILogger<RateLimitingService>>());
        var allowed = await service.CanSendOtpAsync(userId, "127.0.0.1");
        Assert.False(allowed);
    }

    [Fact]
    public async Task CanSendOtpAsync_IpExceededLimit_ReturnsFalse()
    {
        var userId = Guid.NewGuid();
        var repo = new Mock<IOtpAttemptRepository>();
        repo.Setup(r => r.CountUserAttemptsAsync(userId, It.IsAny<DateTime>(), "send_otp", null))
            .ReturnsAsync(0);
        repo.Setup(r => r.CountIpAttemptsAsync(It.IsAny<string>(), It.IsAny<DateTime>(), "send_otp", null))
            .ReturnsAsync(20); // >= default max 20

        var service = new RateLimitingService(repo.Object, BuildConfig(), Mock.Of<ILogger<RateLimitingService>>());
        var allowed = await service.CanSendOtpAsync(userId, "127.0.0.1");
        Assert.False(allowed);
    }

    [Fact]
    public async Task CanSendOtpAsync_Exception_ReturnsFalse()
    {
        var userId = Guid.NewGuid();
        var repo = new Mock<IOtpAttemptRepository>();
        repo.Setup(r => r.CountUserAttemptsAsync(userId, It.IsAny<DateTime>(), "send_otp", null))
            .ThrowsAsync(new Exception("db error"));

        var service = new RateLimitingService(repo.Object, BuildConfig(), Mock.Of<ILogger<RateLimitingService>>());
        var allowed = await service.CanSendOtpAsync(userId, "127.0.0.1");
        Assert.False(allowed);
    }

    [Fact]
    public async Task CanVerifyOtpAsync_ExceedsUserLimit_ReturnsFalse()
    {
        var repo = new Mock<IOtpAttemptRepository>();
        repo.Setup(r => r.CountUserAttemptsAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), "verify_otp", null))
            .ReturnsAsync(10);
        repo.Setup(r => r.CountIpAttemptsAsync(It.IsAny<string>(), It.IsAny<DateTime>(), "verify_otp", null))
            .ReturnsAsync(0);

        var service = new RateLimitingService(repo.Object, BuildConfig(), Mock.Of<ILogger<RateLimitingService>>());
        var allowed = await service.CanVerifyOtpAsync(Guid.NewGuid(), "127.0.0.1");
        Assert.False(allowed);
    }

    [Fact]
    public async Task CanVerifyOtpAsync_IpExceededLimit_ReturnsFalse()
    {
        var repo = new Mock<IOtpAttemptRepository>();
        repo.Setup(r => r.CountUserAttemptsAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), "verify_otp", null))
            .ReturnsAsync(0);
        repo.Setup(r => r.CountIpAttemptsAsync(It.IsAny<string>(), It.IsAny<DateTime>(), "verify_otp", null))
            .ReturnsAsync(20);

        var service = new RateLimitingService(repo.Object, BuildConfig(), Mock.Of<ILogger<RateLimitingService>>());
        var allowed = await service.CanVerifyOtpAsync(Guid.NewGuid(), "127.0.0.1");
        Assert.False(allowed);
    }

    [Fact]
    public async Task CanVerifyOtpAsync_AccountLocked_ReturnsFalse()
    {
        var userId = Guid.NewGuid();
        var repo = new Mock<IOtpAttemptRepository>();
        repo.Setup(r => r.CountUserAttemptsAsync(userId, It.IsAny<DateTime>(), "verify_otp", null))
            .ReturnsAsync(0);
        repo.Setup(r => r.CountIpAttemptsAsync(It.IsAny<string>(), It.IsAny<DateTime>(), "verify_otp", null))
            .ReturnsAsync(0);
        // Called by IsAccountLockedAsync: attemptType null, successOnly false
        repo.Setup(r => r.CountUserAttemptsAsync(userId, It.IsAny<DateTime>(), null, false))
            .ReturnsAsync(5);

        var service = new RateLimitingService(repo.Object, BuildConfig(), Mock.Of<ILogger<RateLimitingService>>());
        var allowed = await service.CanVerifyOtpAsync(userId, "127.0.0.1");
        Assert.False(allowed);
    }

    [Fact]
    public async Task CanSendOtpEmailAsync_WithinLimits_ReturnsTrue()
    {
        var repo = new Mock<IOtpAttemptRepository>();
        repo.Setup(r => r.CountEmailAttemptsAsync(It.IsAny<string>(), It.IsAny<DateTime>(), "send_otp", null))
            .ReturnsAsync(0);
        repo.Setup(r => r.CountIpAttemptsAsync(It.IsAny<string>(), It.IsAny<DateTime>(), "send_otp", null))
            .ReturnsAsync(0);

        var service = new RateLimitingService(repo.Object, BuildConfig(), Mock.Of<ILogger<RateLimitingService>>());
        var allowed = await service.CanSendOtpAsync("user@example.com", "127.0.0.1");
        Assert.True(allowed);
    }

    [Fact]
    public async Task CanSendOtpEmailAsync_EmailExceededLimit_ReturnsFalse()
    {
        var repo = new Mock<IOtpAttemptRepository>();
        repo.Setup(r => r.CountEmailAttemptsAsync(It.IsAny<string>(), It.IsAny<DateTime>(), "send_otp", null))
            .ReturnsAsync(3);
        repo.Setup(r => r.CountIpAttemptsAsync(It.IsAny<string>(), It.IsAny<DateTime>(), "send_otp", null))
            .ReturnsAsync(0);

        var service = new RateLimitingService(repo.Object, BuildConfig(), Mock.Of<ILogger<RateLimitingService>>());
        var allowed = await service.CanSendOtpAsync("user@example.com", "127.0.0.1");
        Assert.False(allowed);
    }

    [Fact]
    public async Task CanSendOtpEmailAsync_IpExceededLimit_ReturnsFalse()
    {
        var repo = new Mock<IOtpAttemptRepository>();
        repo.Setup(r => r.CountEmailAttemptsAsync(It.IsAny<string>(), It.IsAny<DateTime>(), "send_otp", null))
            .ReturnsAsync(0);
        repo.Setup(r => r.CountIpAttemptsAsync(It.IsAny<string>(), It.IsAny<DateTime>(), "send_otp", null))
            .ReturnsAsync(20);

        var service = new RateLimitingService(repo.Object, BuildConfig(), Mock.Of<ILogger<RateLimitingService>>());
        var allowed = await service.CanSendOtpAsync("user@example.com", "127.0.0.1");
        Assert.False(allowed);
    }

    [Fact]
    public async Task CanSendOtpEmailAsync_Exception_ReturnsFalse()
    {
        var repo = new Mock<IOtpAttemptRepository>();
        repo.Setup(r => r.CountEmailAttemptsAsync(It.IsAny<string>(), It.IsAny<DateTime>(), "send_otp", null))
            .ThrowsAsync(new Exception("db error"));

        var service = new RateLimitingService(repo.Object, BuildConfig(), Mock.Of<ILogger<RateLimitingService>>());
        var allowed = await service.CanSendOtpAsync("user@example.com", "127.0.0.1");
        Assert.False(allowed);
    }

    [Fact]
    public async Task RecordAttemptAsync_Success_CreatesAttempt()
    {
        var repo = new Mock<IOtpAttemptRepository>();
        repo.Setup(r => r.CreateAsync(It.IsAny<DisasterApp.Domain.Entities.OtpAttempt>()))
            .ReturnsAsync((DisasterApp.Domain.Entities.OtpAttempt a) => a);

        var service = new RateLimitingService(repo.Object, BuildConfig(), Mock.Of<ILogger<RateLimitingService>>());
        await service.RecordAttemptAsync(Guid.NewGuid(), "user@example.com", "127.0.0.1", "send_otp", true);

        repo.Verify(r => r.CreateAsync(It.Is<DisasterApp.Domain.Entities.OtpAttempt>(a => a.Success && a.AttemptType == "send_otp")), Times.Once);
    }

    [Fact]
    public async Task RecordAttemptAsync_Exception_IsSwallowed()
    {
        var repo = new Mock<IOtpAttemptRepository>();
        repo.Setup(r => r.CreateAsync(It.IsAny<DisasterApp.Domain.Entities.OtpAttempt>()))
            .ThrowsAsync(new Exception("db error"));

        var service = new RateLimitingService(repo.Object, BuildConfig(), Mock.Of<ILogger<RateLimitingService>>());
        await service.RecordAttemptAsync(Guid.NewGuid(), "user@example.com", "127.0.0.1", "send_otp", false);
        // No exception thrown
    }

    [Fact]
    public async Task IsAccountLockedAsync_ReturnsTrue_WhenFailedAttemptsExceedThreshold()
    {
        var userId = Guid.NewGuid();
        var repo = new Mock<IOtpAttemptRepository>();
        repo.Setup(r => r.CountUserAttemptsAsync(userId, It.IsAny<DateTime>(), null, false))
            .ReturnsAsync(5);

        var service = new RateLimitingService(repo.Object, BuildConfig(), Mock.Of<ILogger<RateLimitingService>>());
        var locked = await service.IsAccountLockedAsync(userId);
        Assert.True(locked);
    }

    [Fact]
    public async Task IsAccountLockedAsync_Exception_ReturnsFalse()
    {
        var userId = Guid.NewGuid();
        var repo = new Mock<IOtpAttemptRepository>();
        repo.Setup(r => r.CountUserAttemptsAsync(userId, It.IsAny<DateTime>(), null, false))
            .ThrowsAsync(new Exception("db error"));

        var service = new RateLimitingService(repo.Object, BuildConfig(), Mock.Of<ILogger<RateLimitingService>>());
        var locked = await service.IsAccountLockedAsync(userId);
        Assert.False(locked);
    }

    [Fact]
    public async Task IsIpBlockedAsync_True_WhenAttemptsHigh()
    {
        var repo = new Mock<IOtpAttemptRepository>();
        repo.Setup(r => r.CountIpAttemptsAsync(It.IsAny<string>(), It.IsAny<DateTime>(), null, false))
            .ReturnsAsync(40); // >= 2x max (20)

        var service = new RateLimitingService(repo.Object, BuildConfig(), Mock.Of<ILogger<RateLimitingService>>());
        var blocked = await service.IsIpBlockedAsync("127.0.0.1");
        Assert.True(blocked);
    }

    [Fact]
    public async Task IsIpBlockedAsync_False_WhenAttemptsLow()
    {
        var repo = new Mock<IOtpAttemptRepository>();
        repo.Setup(r => r.CountIpAttemptsAsync(It.IsAny<string>(), It.IsAny<DateTime>(), null, false))
            .ReturnsAsync(10);

        var service = new RateLimitingService(repo.Object, BuildConfig(), Mock.Of<ILogger<RateLimitingService>>());
        var blocked = await service.IsIpBlockedAsync("127.0.0.1");
        Assert.False(blocked);
    }

    [Fact]
    public async Task IsIpBlockedAsync_Exception_ReturnsFalse()
    {
        var repo = new Mock<IOtpAttemptRepository>();
        repo.Setup(r => r.CountIpAttemptsAsync(It.IsAny<string>(), It.IsAny<DateTime>(), null, false))
            .ThrowsAsync(new Exception("db error"));

        var service = new RateLimitingService(repo.Object, BuildConfig(), Mock.Of<ILogger<RateLimitingService>>());
        var blocked = await service.IsIpBlockedAsync("127.0.0.1");
        Assert.False(blocked);
    }

    [Fact]
    public async Task GetOtpSendCooldownAsync_NoMax_ReturnsNull()
    {
        var userId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var repo = new Mock<IOtpAttemptRepository>();
        repo.Setup(r => r.GetUserAttemptsAsync(userId, It.IsAny<DateTime>(), "send_otp"))
            .ReturnsAsync(new List<DisasterApp.Domain.Entities.OtpAttempt>
            {
                new() { AttemptedAt = now.AddMinutes(-59) },
                new() { AttemptedAt = now.AddMinutes(-1) },
            });

        var service = new RateLimitingService(repo.Object, BuildConfig(), Mock.Of<ILogger<RateLimitingService>>());
        var remaining = await service.GetOtpSendCooldownAsync(userId);
        Assert.Null(remaining);
    }

    [Fact]
    public async Task GetOtpSendCooldownAsync_Exception_ReturnsNull()
    {
        var userId = Guid.NewGuid();
        var repo = new Mock<IOtpAttemptRepository>();
        repo.Setup(r => r.GetUserAttemptsAsync(userId, It.IsAny<DateTime>(), "send_otp"))
            .ThrowsAsync(new Exception("db error"));

        var service = new RateLimitingService(repo.Object, BuildConfig(), Mock.Of<ILogger<RateLimitingService>>());
        var remaining = await service.GetOtpSendCooldownAsync(userId);
        Assert.Null(remaining);
    }

    [Fact]
    public async Task GetAccountLockoutTimeAsync_NotLocked_ReturnsNull()
    {
        var userId = Guid.NewGuid();
        var repo = new Mock<IOtpAttemptRepository>();
        // IsAccountLockedAsync -> false
        repo.Setup(r => r.CountUserAttemptsAsync(userId, It.IsAny<DateTime>(), null, false))
            .ReturnsAsync(0);

        var service = new RateLimitingService(repo.Object, BuildConfig(), Mock.Of<ILogger<RateLimitingService>>());
        var t = await service.GetAccountLockoutTimeAsync(userId);
        Assert.Null(t);
    }

    [Fact]
    public async Task GetAccountLockoutTimeAsync_Locked_ReturnsRemaining()
    {
        var userId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var repo = new Mock<IOtpAttemptRepository>();
        // IsAccountLockedAsync -> true
        repo.Setup(r => r.CountUserAttemptsAsync(userId, It.IsAny<DateTime>(), null, false))
            .ReturnsAsync(5);
        // Latest failed attempt 30 minutes ago; lockout is 60 minutes
        repo.Setup(r => r.GetFailedAttemptsAsync(userId, It.IsAny<DateTime>(), null))
            .ReturnsAsync(new List<DisasterApp.Domain.Entities.OtpAttempt>
            {
                new() { AttemptedAt = now.AddMinutes(-45) },
                new() { AttemptedAt = now.AddMinutes(-30) },
            });

        var service = new RateLimitingService(repo.Object, BuildConfig(), Mock.Of<ILogger<RateLimitingService>>());
        var t = await service.GetAccountLockoutTimeAsync(userId);
        Assert.NotNull(t);
        Assert.True(t!.Value.TotalMinutes > 0);
    }

    [Fact]
    public async Task GetAccountLockoutTimeAsync_Exception_ReturnsNull()
    {
        var userId = Guid.NewGuid();
        var repo = new Mock<IOtpAttemptRepository>();
        // IsAccountLockedAsync -> true
        repo.Setup(r => r.CountUserAttemptsAsync(userId, It.IsAny<DateTime>(), null, false))
            .ReturnsAsync(5);
        // Throw when fetching failed attempts
        repo.Setup(r => r.GetFailedAttemptsAsync(userId, It.IsAny<DateTime>(), null))
            .ThrowsAsync(new Exception("db error"));

        var service = new RateLimitingService(repo.Object, BuildConfig(), Mock.Of<ILogger<RateLimitingService>>());
        var t = await service.GetAccountLockoutTimeAsync(userId);
        Assert.Null(t);
    }
    [Fact]
    public async Task GetOtpSendCooldownAsync_WhenMaxReached_ReturnsRemaining()
    {
        var now = DateTime.UtcNow;
        var userId = Guid.NewGuid();
        var attempts = new List<DisasterApp.Domain.Entities.OtpAttempt>
        {
            new() { AttemptedAt = now.AddMinutes(-59) },
            new() { AttemptedAt = now.AddMinutes(-30) },
            new() { AttemptedAt = now.AddMinutes(-1) },
        };

        var repo = new Mock<IOtpAttemptRepository>();
        repo.Setup(r => r.GetUserAttemptsAsync(userId, It.IsAny<DateTime>(), "send_otp"))
            .ReturnsAsync(attempts);

        var config = BuildConfig(new() { ["TwoFactor:MaxOtpSendPerHour"] = "3" });
        var service = new RateLimitingService(repo.Object, config, Mock.Of<ILogger<RateLimitingService>>());

        var remaining = await service.GetOtpSendCooldownAsync(userId);
        Assert.NotNull(remaining);
        Assert.True(remaining!.Value.TotalSeconds > 0);
    }

    [Fact]
    public async Task CleanupOldAttemptsAsync_ReturnsDeletedCount()
    {
        var repo = new Mock<IOtpAttemptRepository>();
        repo.Setup(r => r.DeleteOldAttemptsAsync(It.IsAny<DateTime>())).ReturnsAsync(7);

        var service = new RateLimitingService(repo.Object, BuildConfig(), Mock.Of<ILogger<RateLimitingService>>());
        var count = await service.CleanupOldAttemptsAsync(TimeSpan.FromHours(12));
        Assert.Equal(7, count);
    }
}


