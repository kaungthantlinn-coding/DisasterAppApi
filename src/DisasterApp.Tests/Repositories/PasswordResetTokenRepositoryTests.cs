using Xunit;
using Microsoft.EntityFrameworkCore;
using DisasterApp.Infrastructure.Repositories.Implementations;
using DisasterApp.Infrastructure.Data;
using DisasterApp.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace DisasterApp.Tests.Repositories;

public class PasswordResetTokenRepositoryTests : IDisposable
{
    private readonly DisasterDbContext _context;
    private readonly PasswordResetTokenRepository _repository;

    public PasswordResetTokenRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<DisasterDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new DisasterDbContext(options);
        var logger = new Mock<ILogger<PasswordResetTokenRepository>>();
        _repository = new PasswordResetTokenRepository(_context, logger.Object);
    }



    [Fact]
    public async Task GetByTokenAsync_ExistingToken_ReturnsToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        
        // Create a User entity first
        var user = new User
        {
            UserId = userId,
            Email = "passwordreset@example.com",
            Name = "Password Reset User",
            AuthProvider = "Email",
            AuthId = "passwordreset-auth-id",
            CreatedAt = DateTime.UtcNow
        };
        _context.Users.Add(user);
        
        var tokenValue = "unique-reset-token-456";
        var token = new PasswordResetToken
        {
            PasswordResetTokenId = Guid.NewGuid(),
            UserId = userId,
            Token = tokenValue,
            IsUsed = false,
            ExpiredAt = DateTime.UtcNow.AddHours(2),
            CreatedAt = DateTime.UtcNow
        };

        _context.PasswordResetTokens.Add(token);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByTokenAsync(tokenValue);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(tokenValue, result.Token);
        Assert.False(result.IsUsed);
    }

    [Fact]
    public async Task GetByTokenAsync_NonExistingToken_ReturnsNull()
    {
        // Arrange
        var tokenValue = "non-existent-token";

        // Act
        var result = await _repository.GetByTokenAsync(tokenValue);

        // Assert
        Assert.Null(result);
    }



    [Fact]
    public async Task CreateAsync_ValidToken_AddsToken()
    {
        // Arrange
        var token = new PasswordResetToken
        {
            PasswordResetTokenId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Token = "new-reset-token",
            IsUsed = false,
            ExpiredAt = DateTime.UtcNow.AddHours(3),
            CreatedAt = DateTime.UtcNow
        };

        // Act
        await _repository.CreateAsync(token);

        // Assert
        var savedToken = await _context.PasswordResetTokens
            .FirstOrDefaultAsync(t => t.Token == token.Token);
        
        Assert.NotNull(savedToken);
        Assert.Equal(token.Token, savedToken.Token);
        Assert.Equal(token.UserId, savedToken.UserId);
        Assert.False(savedToken.IsUsed);
    }



    [Fact]
    public async Task DeleteAsync_ExistingToken_DeletesToken()
    {
        // Arrange
        var token = new PasswordResetToken
        {
            PasswordResetTokenId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Token = "delete-token",
            IsUsed = false,
            ExpiredAt = DateTime.UtcNow.AddHours(1),
            CreatedAt = DateTime.UtcNow
        };

        _context.PasswordResetTokens.Add(token);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(token.Token);

        // Assert
        var deletedToken = await _context.PasswordResetTokens
            .FirstOrDefaultAsync(t => t.Token == token.Token);
        
        Assert.Null(deletedToken);
    }



    [Fact]
    public async Task MarkAsUsedAsync_ValidToken_MarksTokenAsUsed()
    {
        // Arrange
        var tokenValue = "valid-token";
        var token = new PasswordResetToken
        {
            PasswordResetTokenId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Token = tokenValue,
            IsUsed = false,
            ExpiredAt = DateTime.UtcNow.AddHours(1),
            CreatedAt = DateTime.UtcNow
        };

        _context.PasswordResetTokens.Add(token);
        await _context.SaveChangesAsync();

        // Act
        await _repository.MarkAsUsedAsync(tokenValue);

        // Assert
        var updatedToken = await _context.PasswordResetTokens
            .FirstOrDefaultAsync(t => t.Token == tokenValue);
        
        Assert.NotNull(updatedToken);
        Assert.True(updatedToken.IsUsed);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}