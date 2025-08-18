using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using DisasterApp.Infrastructure.Repositories.Implementations;
using DisasterApp.Infrastructure.Data;
using DisasterApp.Domain.Entities;

namespace DisasterApp.Tests.Repositories;

public class RefreshTokenRepositoryTests : IDisposable
{
    private readonly DisasterDbContext _context;
    private readonly RefreshTokenRepository _repository;

    public RefreshTokenRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<DisasterDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new DisasterDbContext(options);
        var mockLogger = new Mock<ILogger<RefreshTokenRepository>>();
        _repository = new RefreshTokenRepository(_context, mockLogger.Object);
    }

    [Fact]
    public async Task GetByTokenAsync_ExistingToken_ReturnsToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tokenValue = "test-refresh-token";
        
        // Create a User entity first
        var user = new User
        {
            UserId = userId,
            Email = "test@example.com",
            Name = "Test User",
            AuthProvider = "Email",
            AuthId = "test-auth-id",
            CreatedAt = DateTime.UtcNow
        };
        _context.Users.Add(user);
        
        var refreshToken = new RefreshToken
        {
            RefreshTokenId = Guid.NewGuid(),
            Token = tokenValue,
            UserId = userId,
            ExpiredAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByTokenAsync(tokenValue);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(tokenValue, result.Token);
        Assert.Equal(userId, result.UserId);
    }

    [Fact]
    public async Task GetByTokenAsync_NonExistingToken_ReturnsNull()
    {
        // Arrange
        var tokenValue = "non-existing-token";

        // Act
        var result = await _repository.GetByTokenAsync(tokenValue);

        // Assert
        Assert.Null(result);
    }



    [Fact]
    public async Task CreateAsync_ValidToken_AddsToken()
    {
        // Arrange
        var refreshToken = new RefreshToken
        {
            RefreshTokenId = Guid.NewGuid(),
            Token = "new-refresh-token",
            UserId = Guid.NewGuid(),
            ExpiredAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };

        // Act
        await _repository.CreateAsync(refreshToken);

        // Assert
        var savedToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken.Token);
        
        Assert.NotNull(savedToken);
        Assert.Equal(refreshToken.Token, savedToken.Token);
        Assert.Equal(refreshToken.UserId, savedToken.UserId);
    }



    [Fact]
    public async Task DeleteAsync_ExistingToken_DeletesToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        
        // Create a User entity first
        var user = new User
        {
            UserId = userId,
            Email = "delete@example.com",
            Name = "Delete User",
            AuthProvider = "Email",
            AuthId = "delete-auth-id",
            CreatedAt = DateTime.UtcNow
        };
        _context.Users.Add(user);
        
        var refreshToken = new RefreshToken
        {
            RefreshTokenId = Guid.NewGuid(),
            Token = "delete-test-token",
            UserId = userId,
            ExpiredAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(refreshToken.Token);

        // Assert
        var deletedToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.RefreshTokenId == refreshToken.RefreshTokenId);
        
        Assert.Null(deletedToken);
    }

    [Fact]
    public async Task DeleteAllUserTokensAsync_ExistingTokens_DeletesAllTokens()
    {
        // Arrange
        var userId = Guid.NewGuid();
        
        // Create a User entity first
        var user = new User
        {
            UserId = userId,
            Email = "deleteall@example.com",
            Name = "Delete All User",
            AuthProvider = "Email",
            AuthId = "deleteall-auth-id",
            CreatedAt = DateTime.UtcNow
        };
        _context.Users.Add(user);
        
        var refreshTokens = new List<RefreshToken>
        {
            new RefreshToken
            {
                RefreshTokenId = Guid.NewGuid(),
                Token = "token1",
                UserId = userId,
                ExpiredAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            },
            new RefreshToken
            {
                RefreshTokenId = Guid.NewGuid(),
                Token = "token2",
                UserId = userId,
                ExpiredAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow.AddMinutes(-30)
            }
        };

        _context.RefreshTokens.AddRange(refreshTokens);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteAllUserTokensAsync(userId);

        // Assert
        var userTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId)
            .ToListAsync();
        
        Assert.Empty(userTokens);
    }





    public void Dispose()
    {
        _context.Dispose();
    }
}