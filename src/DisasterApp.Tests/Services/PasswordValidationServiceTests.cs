using DisasterApp.Application.Services.Implementations;
using Xunit;

namespace DisasterApp.Tests.Services;

public class PasswordValidationServiceTests
{
    private readonly PasswordValidationService _service = new();

    [Fact]
    public void ValidatePassword_ReturnsInvalid_WhenNullOrWhitespace()
    {
        var result1 = _service.ValidatePassword(null!);
        var result2 = _service.ValidatePassword("");
        var result3 = _service.ValidatePassword("   ");

        Assert.False(result1.IsValid);
        Assert.Contains("Password is required", result1.Errors);
        Assert.False(result2.IsValid);
        Assert.Contains("Password is required", result2.Errors);
        Assert.False(result3.IsValid);
        Assert.Contains("Password is required", result3.Errors);
    }

    [Fact]
    public void ValidatePassword_FailsMinLength_AndSetsFeedback()
    {
        var result = _service.ValidatePassword("Ab1!");

        Assert.False(result.IsValid);
        Assert.Contains("Password must be at least 8 characters long", result.Errors);
        Assert.False(result.Strength.HasMinLength);
        // Even though it fails min length, the score reflects present character classes
        Assert.Equal(4, result.Strength.Score);
        Assert.Contains("Good password", result.Strength.Feedback);
    }

    [Theory]
    [InlineData("alllowercase1!")]
    [InlineData("ALLUPPERCASE1!")]
    [InlineData("NoNumbers!!")]
    [InlineData("NoSpecial11")]
    public void ValidatePassword_FlagsMissingCharacterClasses(string password)
    {
        var result = _service.ValidatePassword(password);

        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
        Assert.InRange(result.Strength.Score, 0, 5);
    }

    [Fact]
    public void ValidatePassword_FailsWhenTooLong()
    {
        var overlyLong = new string('A', 101) + "a1!";
        var result = _service.ValidatePassword(overlyLong);

        Assert.False(result.IsValid);
        Assert.Contains("Password must not exceed 100 characters", result.Errors);
    }

    [Fact]
    public void IsPasswordStrong_ReturnsTrue_ForStrongPassword()
    {
        var strong = "Str0ng@Passw0rd!"; // has all classes and length
        var isStrong = _service.IsPasswordStrong(strong);
        var details = _service.ValidatePassword(strong);

        Assert.True(isStrong);
        Assert.True(details.IsValid);
        Assert.True(details.Strength.Score >= 4);
    }
}


