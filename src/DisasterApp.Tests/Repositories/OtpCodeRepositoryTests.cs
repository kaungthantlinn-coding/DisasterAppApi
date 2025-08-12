using Xunit;
using Microsoft.EntityFrameworkCore;
using DisasterApp.Infrastructure.Repositories.Implementations;
using DisasterApp.Infrastructure.Data;
using DisasterApp.Domain.Entities;

namespace DisasterApp.Tests.Repositories;

public class OtpCodeRepositoryTests : IDisposable
{
    private readonly DisasterDbContext _context;
    private readonly OtpCodeRepository _repository;

    public OtpCodeRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<DisasterDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new DisasterDbContext(options);
        _repository = new OtpCodeRepository(_context);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingOtpCode_ReturnsOtpCode()
    {
        // Arrange
        var otpCodeId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        
        // Create a User entity first
        var user = new User
        {
            UserId = userId,
            Email = "otpcode@example.com",
            Name = "OTP Code User",
            AuthProvider = "Email",
            AuthId = "otpcode-auth-id",
            CreatedAt = DateTime.UtcNow
        };
        _context.Users.Add(user);
        
        var otpCode = new OtpCode
        {
            Id = otpCodeId,
            UserId = userId,
            Code = "123456",
            Type = "Login",
            UsedAt = null,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            CreatedAt = DateTime.UtcNow
        };

        _context.OtpCodes.Add(otpCode);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(otpCodeId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(otpCodeId, result.Id);
        Assert.Equal(userId, result.UserId);
        Assert.Equal("123456", result.Code);
        Assert.Equal("Login", result.Type);
        Assert.False(result.IsUsed);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingOtpCode_ReturnsNull()
    {
        // Arrange
        var otpCodeId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(otpCodeId);

        // Assert
        Assert.Null(result);
    }





    [Fact]
    public async Task UpdateAsync_ExistingOtpCode_UpdatesOtpCode()
    {
        // Arrange
        var otpCode = new OtpCode
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Code = "UPDATE123",
            Type = "Login",
            UsedAt = null,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            CreatedAt = DateTime.UtcNow
        };

        _context.OtpCodes.Add(otpCode);
        await _context.SaveChangesAsync();

        // Act
        otpCode.UsedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(otpCode);

        // Assert
        var updatedOtpCode = await _context.OtpCodes
            .FirstOrDefaultAsync(otp => otp.Id == otpCode.Id);
        
        Assert.NotNull(updatedOtpCode);
        Assert.NotNull(updatedOtpCode.UsedAt);
        Assert.NotNull(updatedOtpCode.UsedAt);
    }

    [Fact]
    public async Task DeleteAsync_ExistingOtpCode_DeletesOtpCode()
    {
        // Arrange
        var otpCode = new OtpCode
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Code = "DELETE123",
            Type = "Login",
            UsedAt = null,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            CreatedAt = DateTime.UtcNow
        };

        _context.OtpCodes.Add(otpCode);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(otpCode.Id);

        // Assert
        var deletedOtpCode = await _context.OtpCodes
            .FirstOrDefaultAsync(otp => otp.Id == otpCode.Id);
        
        Assert.Null(deletedOtpCode);
    }



    [Fact]
    public async Task DeleteExpiredAsync_ExpiredCodes_DeletesExpiredCodes()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var otpCodes = new List<OtpCode>
        {
            new OtpCode
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Code = "EXPIRED123",
                Type = "Login",
                UsedAt = null,
                ExpiresAt = now.AddMinutes(-10), // Expired
                CreatedAt = now.AddMinutes(-15)
            },
            new OtpCode
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Code = "VALID123",
                Type = "Login",
                UsedAt = null,
                ExpiresAt = now.AddMinutes(5), // Valid
                CreatedAt = now.AddMinutes(-2)
            },
            new OtpCode
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Code = "EXPIRED456",
                Type = "PasswordReset",
                UsedAt = null,
                ExpiresAt = now.AddMinutes(-5), // Expired
                CreatedAt = now.AddMinutes(-20)
            }
        };

        _context.OtpCodes.AddRange(otpCodes);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteExpiredAsync();

        // Assert
        var remainingCodes = await _context.OtpCodes.ToListAsync();
        Assert.Single(remainingCodes);
        Assert.Equal("VALID123", remainingCodes.First().Code);
        Assert.True(remainingCodes.First().ExpiresAt > now);
    }



    [Fact]
    public async Task GetActiveCountByUserIdAsync_ActiveCodes_ReturnsCorrectCount()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var otpCodes = new List<OtpCode>
        {
            new OtpCode
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Code = "ACTIVE123",
                Type = "Login",
                UsedAt = null,
                ExpiresAt = now.AddMinutes(5), // Active
                CreatedAt = now.AddMinutes(-2)
            },
            new OtpCode
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Code = "EXPIRED123",
                Type = "Login",
                UsedAt = null,
                ExpiresAt = now.AddMinutes(-1), // Expired
                CreatedAt = now.AddMinutes(-10)
            },
            new OtpCode
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Code = "USED123",
                Type = "Login",
                UsedAt = now.AddMinutes(-1), // Used
                ExpiresAt = now.AddMinutes(5),
                CreatedAt = now.AddMinutes(-1)
            },
            new OtpCode
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Code = "ACTIVE456",
                Type = "PasswordReset",
                UsedAt = null,
                ExpiresAt = now.AddMinutes(10), // Active
                CreatedAt = now.AddMinutes(-1)
            }
        };

        _context.OtpCodes.AddRange(otpCodes);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetActiveCountAsync(userId);

        // Assert
        Assert.Equal(2, result);
    }



    public void Dispose()
    {
        _context.Dispose();
    }
}