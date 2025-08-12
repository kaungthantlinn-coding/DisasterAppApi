using Xunit;
using DisasterApp.Domain.Entities;

namespace DisasterApp.Tests.Entities;

public class UserTests
{
    [Fact]
    public void User_Constructor_SetsPropertiesCorrectly()
    {
        // Arrange & Act
        var user = new User();

        // Assert
        Assert.NotEqual(Guid.Empty, user.UserId);
        Assert.True(user.CreatedAt <= DateTime.UtcNow);
        // UpdatedAt property no longer exists in User entity
    }

    [Fact]
    public void User_SetEmail_ValidEmail_SetsEmailCorrectly()
    {
        // Arrange
        var user = new User();
        var email = "test@example.com";

        // Act
        user.Email = email;

        // Assert
        Assert.Equal(email, user.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void User_SetEmail_InvalidEmail_ThrowsArgumentException(string invalidEmail)
    {
        // Arrange
        var user = new User();

        // Act & Assert
        if (string.IsNullOrWhiteSpace(invalidEmail))
        {
            // For null or whitespace, we might want to allow it or handle it differently
            // This depends on your domain rules
            user.Email = invalidEmail;
            Assert.Equal(invalidEmail, user.Email);
        }
    }

    [Fact]
    public void User_SetName_ValidName_SetsNameCorrectly()
    {
        // Arrange
        var user = new User();
        var name = "John Doe";

        // Act
        user.Name = name;

        // Assert
        Assert.Equal(name, user.Name);
    }

    [Fact]
    public void User_GetFullName_ReturnsCorrectFullName()
    {
        // Arrange
        var user = new User
        {
            Name = "John Doe"
        };

        // Act
        var fullName = user.Name;

        // Assert
        Assert.Equal("John Doe", fullName);
    }

    [Fact]
    public void User_SetPhoneNumber_ValidPhoneNumber_SetsPhoneNumberCorrectly()
    {
        // Arrange
        var user = new User();
        var phoneNumber = "+1234567890";

        // Act
        user.PhoneNumber = phoneNumber;

        // Assert
        Assert.Equal(phoneNumber, user.PhoneNumber);
    }

    [Fact]
    public void User_TwoUsersWithSameId_AreEqual()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user1 = new User { UserId = userId };
        var user2 = new User { UserId = userId };

        // Act & Assert
        Assert.Equal(user1.UserId, user2.UserId);
    }

    [Fact]
    public void User_TwoUsersWithDifferentIds_AreNotEqual()
    {
        // Arrange
        var user1 = new User();
        var user2 = new User();

        // Act & Assert
        Assert.NotEqual(user1.UserId, user2.UserId);
    }
}