using DisasterApp.Application.Services.Implementations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DisasterApp.Tests.Services;

public class TokenServiceTests
{
    private static IConfiguration BuildConfig()
    {
        var dict = new Dictionary<string, string?>
        {
            ["Jwt:Key"] = "test_secret_key_1234567890_test_secret_key_1234567890",
            ["Jwt:Issuer"] = "test-issuer",
            ["Jwt:Audience"] = "test-audience"
        }!;

        return new ConfigurationBuilder()
            .AddInMemoryCollection(dict)
            .Build();
    }

    [Fact]
    public async Task GenerateAndValidateLoginToken_Works()
    {
        var config = BuildConfig();
        var logger = new Mock<ILogger<TokenService>>();
        var service = new TokenService(config, logger.Object);

        var userId = Guid.NewGuid();
        var token = service.GenerateLoginToken(userId);
        Assert.False(string.IsNullOrWhiteSpace(token));

        var parsed = await service.ValidateLoginTokenAsync(token);
        Assert.Equal(userId, parsed);
    }

    [Fact]
    public async Task ValidateLoginToken_ReturnsNull_ForInvalidToken()
    {
        var config = BuildConfig();
        var logger = new Mock<ILogger<TokenService>>();
        var service = new TokenService(config, logger.Object);

        var result = await service.ValidateLoginTokenAsync("not-a-jwt");
        Assert.Null(result);
    }
}


