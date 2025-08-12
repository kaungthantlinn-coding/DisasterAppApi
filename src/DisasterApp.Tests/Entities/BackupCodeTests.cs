using Xunit;
using DisasterApp.Domain.Entities;

namespace DisasterApp.Tests.Entities;

public class BackupCodeTests
{
    [Fact]
    public void BackupCode_DefaultConstructor_SetsDefaultValues()
    {
        // Act
        var backupCode = new BackupCode();

        // Assert
        Assert.NotEqual(Guid.Empty, backupCode.Id); // Should be auto-generated
        Assert.Equal(Guid.Empty, backupCode.UserId);
        Assert.Null(backupCode.CodeHash);
        Assert.Null(backupCode.UsedAt);
        Assert.True(backupCode.CreatedAt > DateTime.MinValue); // Should be set to UtcNow
        Assert.Null(backupCode.User);
    }

    [Fact]
    public void BackupCode_SetProperties_SetsCorrectValues()
    {
        // Arrange
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var codeHash = "hashed-backup-code-123";
        var usedAt = DateTime.UtcNow;
        var createdAt = DateTime.UtcNow.AddMinutes(-5);
        var user = new User { UserId = userId, Email = "test@example.com" };

        // Act
        var backupCode = new BackupCode
        {
            Id = id,
            UserId = userId,
            CodeHash = codeHash,
            UsedAt = usedAt,
            CreatedAt = createdAt,
            User = user
        };

        // Assert
        Assert.Equal(id, backupCode.Id);
        Assert.Equal(userId, backupCode.UserId);
        Assert.Equal(codeHash, backupCode.CodeHash);
        Assert.Equal(usedAt, backupCode.UsedAt);
        Assert.Equal(createdAt, backupCode.CreatedAt);
        Assert.Equal(user, backupCode.User);
    }

    [Theory]
    [InlineData("simple-hash")]
    [InlineData("$2b$10$N9qo8uLOickgx2ZMRZoMye")]
    [InlineData("sha256:abcdef1234567890")]
    [InlineData("bcrypt:$2a$12$R9h/cIPz0gi.URNNX3kh2OPST9/PgBkqquzi.Ss7KIUgO2t0jWMUW")]
    [InlineData("very-long-hash-string-that-represents-a-securely-hashed-backup-code-with-salt-and-pepper")]
    public void BackupCode_SetCodeHash_AcceptsVariousHashFormats(string codeHash)
    {
        // Arrange
        var backupCode = new BackupCode();

        // Act
        backupCode.CodeHash = codeHash;

        // Assert
        Assert.Equal(codeHash, backupCode.CodeHash);
    }

    [Fact]
    public void BackupCode_IsValid_ReturnsTrueWhenNotUsed()
    {
        // Arrange
        var backupCode = new BackupCode
        {
            UsedAt = null
        };

        // Act
        var isValid = backupCode.IsValid;

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void BackupCode_IsValid_ReturnsFalseWhenUsed()
    {
        // Arrange
        var backupCode = new BackupCode
        {
            UsedAt = DateTime.UtcNow
        };

        // Act
        var isValid = backupCode.IsValid;

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void BackupCode_IsUsed_ReturnsFalseWhenNotUsed()
    {
        // Arrange
        var backupCode = new BackupCode
        {
            UsedAt = null
        };

        // Act
        var isUsed = backupCode.IsUsed;

        // Assert
        Assert.False(isUsed);
    }

    [Fact]
    public void BackupCode_IsUsed_ReturnsTrueWhenUsed()
    {
        // Arrange
        var backupCode = new BackupCode
        {
            UsedAt = DateTime.UtcNow
        };

        // Act
        var isUsed = backupCode.IsUsed;

        // Assert
        Assert.True(isUsed);
    }

    [Fact]
    public void BackupCode_MarkAsUsed_SetsUsedAtToCurrentTime()
    {
        // Arrange
        var backupCode = new BackupCode
        {
            UsedAt = null
        };
        var beforeMarkingAsUsed = DateTime.UtcNow.AddSeconds(-1);

        // Act
        backupCode.MarkAsUsed();
        var afterMarkingAsUsed = DateTime.UtcNow.AddSeconds(1);

        // Assert
        Assert.NotNull(backupCode.UsedAt);
        Assert.True(backupCode.UsedAt >= beforeMarkingAsUsed);
        Assert.True(backupCode.UsedAt <= afterMarkingAsUsed);
    }

    [Fact]
    public void BackupCode_MarkAsUsed_ChangesIsValidToFalse()
    {
        // Arrange
        var backupCode = new BackupCode
        {
            UsedAt = null
        };
        Assert.True(backupCode.IsValid); // Verify initial state

        // Act
        backupCode.MarkAsUsed();

        // Assert
        Assert.False(backupCode.IsValid);
    }

    [Fact]
    public void BackupCode_MarkAsUsed_ChangesIsUsedToTrue()
    {
        // Arrange
        var backupCode = new BackupCode
        {
            UsedAt = null
        };
        Assert.False(backupCode.IsUsed); // Verify initial state

        // Act
        backupCode.MarkAsUsed();

        // Assert
        Assert.True(backupCode.IsUsed);
    }

    [Fact]
    public void BackupCode_MarkAsUsed_CanBeCalledMultipleTimes()
    {
        // Arrange
        var backupCode = new BackupCode
        {
            UsedAt = null
        };

        // Act
        backupCode.MarkAsUsed();
        var firstUsedAt = backupCode.UsedAt;
        
        // Wait a small amount to ensure different timestamps
        Thread.Sleep(10);
        
        backupCode.MarkAsUsed();
        var secondUsedAt = backupCode.UsedAt;

        // Assert
        Assert.NotNull(firstUsedAt);
        Assert.NotNull(secondUsedAt);
        Assert.True(secondUsedAt >= firstUsedAt);
        Assert.True(backupCode.IsUsed);
        Assert.False(backupCode.IsValid);
    }

    [Fact]
    public void BackupCode_UserNavigation_MaintainsReferenceIntegrity()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            UserId = userId,
            Email = "backup@example.com",
            Name = "John Doe"
        };
        var backupCode = new BackupCode
        {
            UserId = userId,
            User = user
        };

        // Act & Assert
        Assert.Equal(userId, backupCode.UserId);
        Assert.Equal(userId, backupCode.User.UserId);
        Assert.Equal(user.Email, backupCode.User.Email);
        Assert.Equal(user.Name, backupCode.User.Name);
    }

    [Fact]
    public void BackupCode_CreatedAt_IsSetToRecentTime()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow.AddSeconds(-1);
        
        // Act
        var backupCode = new BackupCode();
        var afterCreation = DateTime.UtcNow.AddSeconds(1);

        // Assert
        Assert.True(backupCode.CreatedAt >= beforeCreation);
        Assert.True(backupCode.CreatedAt <= afterCreation);
    }

    [Fact]
    public void BackupCode_Id_IsUniqueForEachInstance()
    {
        // Act
        var backupCode1 = new BackupCode();
        var backupCode2 = new BackupCode();

        // Assert
        Assert.NotEqual(backupCode1.Id, backupCode2.Id);
        Assert.NotEqual(Guid.Empty, backupCode1.Id);
        Assert.NotEqual(Guid.Empty, backupCode2.Id);
    }

    [Fact]
    public void BackupCode_PropertiesAreIndependent_CanBeSetSeparately()
    {
        // Arrange
        var backupCode = new BackupCode();
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var codeHash = "independent-hash";
        var createdAt = DateTime.UtcNow.AddMinutes(-10);

        // Act & Assert
        backupCode.Id = id;
        Assert.Equal(id, backupCode.Id);
        Assert.Equal(Guid.Empty, backupCode.UserId);

        backupCode.UserId = userId;
        Assert.Equal(userId, backupCode.UserId);
        Assert.Null(backupCode.CodeHash);

        backupCode.CodeHash = codeHash;
        Assert.Equal(codeHash, backupCode.CodeHash);
        Assert.Null(backupCode.UsedAt);

        backupCode.CreatedAt = createdAt;
        Assert.Equal(createdAt, backupCode.CreatedAt);
        Assert.True(backupCode.IsValid);
        Assert.False(backupCode.IsUsed);
    }

    [Fact]
    public void BackupCode_MultipleInstances_AreIndependent()
    {
        // Arrange & Act
        var backupCode1 = new BackupCode
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            CodeHash = "hash-1",
            UsedAt = null
        };

        var backupCode2 = new BackupCode
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            CodeHash = "hash-2",
            UsedAt = DateTime.UtcNow
        };

        // Assert
        Assert.NotEqual(backupCode1.Id, backupCode2.Id);
        Assert.NotEqual(backupCode1.UserId, backupCode2.UserId);
        Assert.NotEqual(backupCode1.CodeHash, backupCode2.CodeHash);
        Assert.NotEqual(backupCode1.UsedAt, backupCode2.UsedAt);
        Assert.NotEqual(backupCode1.IsValid, backupCode2.IsValid);
        Assert.NotEqual(backupCode1.IsUsed, backupCode2.IsUsed);
    }

    [Fact]
    public void BackupCode_ComplexScenario_ValidUnusedCode()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            UserId = userId,
            Email = "user@example.com"
        };

        var backupCode = new BackupCode
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CodeHash = "$2b$10$validHashedBackupCode123",
            UsedAt = null,
            CreatedAt = DateTime.UtcNow.AddMinutes(-30),
            User = user
        };

        // Act & Assert
        Assert.NotEqual(Guid.Empty, backupCode.Id);
        Assert.Equal(userId, backupCode.UserId);
        Assert.Equal("$2b$10$validHashedBackupCode123", backupCode.CodeHash);
        Assert.Null(backupCode.UsedAt);
        Assert.True(backupCode.CreatedAt < DateTime.UtcNow);
        Assert.True(backupCode.IsValid);
        Assert.False(backupCode.IsUsed);
        Assert.Equal(user.Email, backupCode.User.Email);
    }

    [Fact]
    public void BackupCode_ComplexScenario_UsedCode()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            UserId = userId,
            Email = "used@example.com"
        };

        var backupCode = new BackupCode
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CodeHash = "$2b$10$usedHashedBackupCode456",
            UsedAt = DateTime.UtcNow.AddMinutes(-5),
            CreatedAt = DateTime.UtcNow.AddHours(-1),
            User = user
        };

        // Act & Assert
        Assert.NotEqual(Guid.Empty, backupCode.Id);
        Assert.Equal(userId, backupCode.UserId);
        Assert.Equal("$2b$10$usedHashedBackupCode456", backupCode.CodeHash);
        Assert.NotNull(backupCode.UsedAt);
        Assert.True(backupCode.UsedAt < DateTime.UtcNow);
        Assert.True(backupCode.CreatedAt < DateTime.UtcNow);
        Assert.False(backupCode.IsValid);
        Assert.True(backupCode.IsUsed);
        Assert.Equal(user.Email, backupCode.User.Email);
    }

    [Fact]
    public void BackupCode_LifecycleTest_FromCreationToUsage()
    {
        // Arrange - Create a new backup code
        var userId = Guid.NewGuid();
        var backupCode = new BackupCode
        {
            UserId = userId,
            CodeHash = "lifecycle-test-hash"
        };

        // Assert initial state
        Assert.True(backupCode.IsValid);
        Assert.False(backupCode.IsUsed);
        Assert.Null(backupCode.UsedAt);

        // Act - Mark as used
        backupCode.MarkAsUsed();

        // Assert final state
        Assert.False(backupCode.IsValid);
        Assert.True(backupCode.IsUsed);
        Assert.NotNull(backupCode.UsedAt);
        Assert.True(backupCode.UsedAt <= DateTime.UtcNow);
    }
}