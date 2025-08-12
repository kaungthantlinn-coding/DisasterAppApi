using Xunit;
using Microsoft.EntityFrameworkCore;
using DisasterApp.Infrastructure.Repositories.Implementations;
using DisasterApp.Infrastructure.Data;
using DisasterApp.Domain.Entities;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace DisasterApp.Tests.Repositories
{
    public class BackupCodeRepositoryTests : IDisposable
    {
        private readonly DisasterDbContext _context;
        private readonly BackupCodeRepository _repository;

        public BackupCodeRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<DisasterDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new DisasterDbContext(options);
            _repository = new BackupCodeRepository(_context);
        }

        [Fact]
    public async Task GetByIdAsync_ExistingBackupCode_ReturnsBackupCode()
    {
        // Arrange
        var userId = Guid.NewGuid();
        
        // Create a User entity first
        var user = new User
        {
            UserId = userId,
            Email = "backupcode@example.com",
            Name = "Backup Code User",
            AuthProvider = "Email",
            AuthId = "backupcode-auth-id",
            CreatedAt = DateTime.UtcNow
        };
        _context.Users.Add(user);
        
        var backupCode = new BackupCode
        {
            UserId = userId,
            CodeHash = "BACKUP123456_HASH",
            CreatedAt = DateTime.UtcNow
        };
        _context.BackupCodes.Add(backupCode);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(backupCode.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(backupCode.Id, result.Id);
        }

        [Fact]
    public async Task GetUnusedCodesAsync_ReturnsUnusedBackupCodes()
    {
        // Arrange
        var userId = Guid.NewGuid();
        
        // Create a User entity first
        var user = new User
        {
            UserId = userId,
            Email = "backupunused@example.com",
            Name = "Backup Unused User",
            AuthProvider = "Email",
            AuthId = "backupunused-auth-id",
            CreatedAt = DateTime.UtcNow
        };
        _context.Users.Add(user);
        
        var backupCodes = new List<BackupCode>
        {
            new() { UserId = userId, CodeHash = "UNUSED123456", CreatedAt = DateTime.UtcNow },
            new() { UserId = userId, CodeHash = "USED789012", UsedAt = DateTime.UtcNow, CreatedAt = DateTime.UtcNow },
            new() { UserId = userId, CodeHash = "UNUSED345678", CreatedAt = DateTime.UtcNow }
        };

        _context.BackupCodes.AddRange(backupCodes);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetUnusedCodesAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.All(result, bc => Assert.True(bc.IsValid));
            Assert.Contains(result, bc => bc.CodeHash == "UNUSED123456");
            Assert.Contains(result, bc => bc.CodeHash == "UNUSED345678");
        }

    [Fact]
    public async Task MarkAsUsedAsync_ExistingBackupCode_MarksAsUsed()
    {
        // Arrange
        var backupCode = new BackupCode
        {
            UserId = Guid.NewGuid(),
            CodeHash = "MARKUSED123",
            CreatedAt = DateTime.UtcNow
        };

        _context.BackupCodes.Add(backupCode);
        await _context.SaveChangesAsync();

        // Act
        await _repository.MarkAsUsedAsync(backupCode.Id);

        // Assert
        var updatedBackupCode = await _context.BackupCodes
            .FirstOrDefaultAsync(bc => bc.Id == backupCode.Id);
        
        Assert.NotNull(updatedBackupCode);
        Assert.True(updatedBackupCode.IsUsed);
        Assert.NotNull(updatedBackupCode.UsedAt);
    }

    [Fact]
    public async Task DeleteByUserAsync_ExistingUserBackupCodes_DeletesAllUserCodes()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var backupCodes = new List<BackupCode>
        {
            new()
            {
                UserId = userId,
                CodeHash = "DELETE123456",
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                UserId = userId,
                CodeHash = "DELETE789012",
                UsedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                UserId = otherUserId,
                CodeHash = "KEEP345678",
                CreatedAt = DateTime.UtcNow
            }
        };

        _context.BackupCodes.AddRange(backupCodes);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteByUserAsync(userId);

        // Assert
        var remainingCodes = await _context.BackupCodes.ToListAsync();
        Assert.Single(remainingCodes);
        Assert.Equal(otherUserId, remainingCodes.First().UserId);
        Assert.Equal("KEEP345678", remainingCodes.First().CodeHash);
    }

    [Fact]
    public async Task GetUnusedCountAsync_ExistingUserBackupCodes_ReturnsCorrectCount()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var backupCodes = new List<BackupCode>
        {
            new()
            {
                UserId = userId,
                CodeHash = "COUNT123456",
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                UserId = userId,
                CodeHash = "COUNT789012",
                UsedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                UserId = userId,
                CodeHash = "COUNT345678",
                CreatedAt = DateTime.UtcNow
            }
        };

        _context.BackupCodes.AddRange(backupCodes);
        await _context.SaveChangesAsync();

        // Act
        var unusedCount = await _repository.GetUnusedCountAsync(userId);

        // Assert
        Assert.Equal(2, unusedCount);
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
}