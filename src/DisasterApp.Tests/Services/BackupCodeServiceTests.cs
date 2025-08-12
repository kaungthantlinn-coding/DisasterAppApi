using DisasterApp.Application.Services.Implementations;
using DisasterApp.Domain.Entities;
using DisasterApp.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DisasterApp.Tests.Services;

public class BackupCodeServiceTests
{
    [Fact]
    public async Task GenerateBackupCodesAsync_CreatesAndPersists_AndUpdatesUser()
    {
        var userId = Guid.NewGuid();
        var userRepo = new Mock<IUserRepository>();
        var codeRepo = new Mock<IBackupCodeRepository>();
        var logger = Mock.Of<ILogger<BackupCodeService>>();

        userRepo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(new User { UserId = userId });
        codeRepo.Setup(r => r.DeleteByUserAsync(userId)).ReturnsAsync(0);
        codeRepo.Setup(r => r.CreateManyAsync(It.IsAny<List<BackupCode>>())).ReturnsAsync((List<BackupCode> l) => l);
        userRepo.Setup(r => r.UpdateAsync(It.IsAny<User>())).ReturnsAsync((User u) => u);

        var service = new BackupCodeService(codeRepo.Object, userRepo.Object, logger);
        var codes = await service.GenerateBackupCodesAsync(userId, count: 4);

        Assert.Equal(4, codes.Count);
        Assert.All(codes, c => Assert.Equal(8, c.Length));
        codeRepo.Verify(r => r.CreateManyAsync(It.Is<List<BackupCode>>(l => l.Count == 4)), Times.Once);
        userRepo.Verify(r => r.UpdateAsync(It.Is<User>(u => u.BackupCodesRemaining == 4)), Times.Once);
    }

    [Fact]
    public async Task VerifyAndUseBackupCodeAsync_MatchesAnyUnused_MarksUsedAndUpdatesCount()
    {
        var userId = Guid.NewGuid();
        var userRepo = new Mock<IUserRepository>();
        var codeRepo = new Mock<IBackupCodeRepository>();
        var logger = Mock.Of<ILogger<BackupCodeService>>();

        // Prepare a stored hash that matches a generated code
        var serviceForHash = new BackupCodeService(codeRepo.Object, userRepo.Object, logger);
        var plain = serviceForHash.GenerateBackupCode();
        var hash = serviceForHash.HashBackupCode(plain);

        codeRepo.Setup(r => r.GetUnusedCodesAsync(userId)).ReturnsAsync(new List<BackupCode>
        {
            new() { Id = Guid.NewGuid(), UserId = userId, CodeHash = hash }
        });
        codeRepo.Setup(r => r.MarkAsUsedAsync(It.IsAny<Guid>())).ReturnsAsync(true);
        userRepo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(new User { UserId = userId, BackupCodesRemaining = 2 });
        userRepo.Setup(r => r.UpdateAsync(It.IsAny<User>())).ReturnsAsync((User u) => u);

        var service = new BackupCodeService(codeRepo.Object, userRepo.Object, logger);
        var ok = await service.VerifyAndUseBackupCodeAsync(userId, plain);

        Assert.True(ok);
        codeRepo.Verify(r => r.MarkAsUsedAsync(It.IsAny<Guid>()), Times.Once);
        userRepo.Verify(r => r.UpdateAsync(It.Is<User>(u => u.BackupCodesRemaining == 1)), Times.Once);
    }

    [Fact]
    public async Task GetUnusedBackupCodeCountAsync_ReturnsRepoValue()
    {
        var userId = Guid.NewGuid();
        var userRepo = new Mock<IUserRepository>();
        var codeRepo = new Mock<IBackupCodeRepository>();
        var logger = Mock.Of<ILogger<BackupCodeService>>();

        codeRepo.Setup(r => r.GetUnusedCountAsync(userId)).ReturnsAsync(3);

        var service = new BackupCodeService(codeRepo.Object, userRepo.Object, logger);
        var count = await service.GetUnusedBackupCodeCountAsync(userId);
        Assert.Equal(3, count);
    }

    [Fact]
    public async Task InvalidateAllBackupCodesAsync_DeletesAndResetsUserCount()
    {
        var userId = Guid.NewGuid();
        var userRepo = new Mock<IUserRepository>();
        var codeRepo = new Mock<IBackupCodeRepository>();
        var logger = Mock.Of<ILogger<BackupCodeService>>();

        userRepo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(new User { UserId = userId, BackupCodesRemaining = 5 });
        userRepo.Setup(r => r.UpdateAsync(It.IsAny<User>())).ReturnsAsync((User u) => u);
        codeRepo.Setup(r => r.DeleteByUserAsync(userId)).ReturnsAsync(5);

        var service = new BackupCodeService(codeRepo.Object, userRepo.Object, logger);
        var deleted = await service.InvalidateAllBackupCodesAsync(userId);

        Assert.Equal(5, deleted);
        userRepo.Verify(r => r.UpdateAsync(It.Is<User>(u => u.BackupCodesRemaining == 0)), Times.Once);
    }
}


