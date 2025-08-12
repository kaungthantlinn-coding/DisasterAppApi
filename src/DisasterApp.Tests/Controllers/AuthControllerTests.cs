using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using DisasterApp.Application.DTOs;
using DisasterApp.Application.Services.Interfaces;
using DisasterApp.WebApi.Controllers;
using DisasterApp.Infrastructure.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DisasterApp.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<DisasterDbContext> _mockDbContext;
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly Mock<ILogger<AuthController>> _mockLogger;
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly Mock<ITwoFactorService> _mockTwoFactorService;
        private readonly Mock<IEmailOtpService> _mockEmailOtpService;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            var options = new DbContextOptionsBuilder<DisasterDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _mockDbContext = new Mock<DisasterDbContext>(options);
            _mockAuthService = new Mock<IAuthService>();
            _mockLogger = new Mock<ILogger<AuthController>>();
            _mockEmailService = new Mock<IEmailService>();
            _mockTwoFactorService = new Mock<ITwoFactorService>();
            _mockEmailOtpService = new Mock<IEmailOtpService>();
            _controller = new AuthController(
                _mockDbContext.Object,
                _mockAuthService.Object,
                _mockEmailService.Object,
                _mockTwoFactorService.Object,
                _mockEmailOtpService.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsOkWithToken()
        {
            // Arrange
            var loginDto = new LoginRequestDto
            {
                Email = "test@example.com",
                Password = "password123"
            };
            var authResponse = new AuthResponseDto
            {
                AccessToken = "test-token",
                User = new UserDto
                {
                    UserId = Guid.NewGuid(),
                    Email = "test@example.com",
                    Name = "Test User"
                }
            };

            _mockAuthService.Setup(x => x.LoginAsync(loginDto))
                           .ReturnsAsync(authResponse);

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<AuthResponseDto>(okResult.Value);
            Assert.Equal(authResponse.AccessToken, returnValue.AccessToken);
        }

        #region GoogleLogin Tests

        [Fact]
        public async Task GoogleLogin_ValidToken_ReturnsOkWithAuthResponse()
        {
            // Arrange
            var googleLoginDto = new GoogleLoginRequestDto
            {
                IdToken = "valid-google-token",
                DeviceInfo = "Test Device"
            };
            var authResponse = new AuthResponseDto
            {
                AccessToken = "test-access-token",
                RefreshToken = "test-refresh-token",
                ExpiresAt = DateTime.UtcNow.AddMinutes(60),
                User = new UserDto
                {
                    UserId = Guid.NewGuid(),
                    Email = "test@gmail.com",
                    Name = "Test User",
                    PhotoUrl = "https://example.com/photo.jpg",
                    Roles = ["User"]
                }
            };

            _mockAuthService.Setup(x => x.GoogleLoginAsync(googleLoginDto))
                           .ReturnsAsync(authResponse);

            // Act
            var result = await _controller.GoogleLogin(googleLoginDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<AuthResponseDto>(okResult.Value);
            Assert.Equal(authResponse.AccessToken, returnValue.AccessToken);
            Assert.Equal(authResponse.RefreshToken, returnValue.RefreshToken);
            Assert.Equal(authResponse.User.Email, returnValue.User.Email);
        }

        [Fact]
        public async Task GoogleLogin_UnauthorizedAccessException_ReturnsUnauthorized()
        {
            // Arrange
            var googleLoginDto = new GoogleLoginRequestDto
            {
                IdToken = "invalid-google-token"
            };
            var exceptionMessage = "Invalid Google token";

            _mockAuthService.Setup(x => x.GoogleLoginAsync(googleLoginDto))
                           .ThrowsAsync(new UnauthorizedAccessException(exceptionMessage));

            // Act
            var result = await _controller.GoogleLogin(googleLoginDto);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
            var response = unauthorizedResult.Value;
            var messageProperty = response.GetType().GetProperty("message");
            Assert.NotNull(messageProperty);
            var messageValue = messageProperty.GetValue(response) as string;
            Assert.Equal(exceptionMessage, messageValue);
        }

        [Fact]
        public async Task GoogleLogin_InvalidOperationException_ReturnsInternalServerError()
        {
            // Arrange
            var googleLoginDto = new GoogleLoginRequestDto
            {
                IdToken = "valid-token"
            };
            var exceptionMessage = "Google Client ID not configured";

            _mockAuthService.Setup(x => x.GoogleLoginAsync(googleLoginDto))
                           .ThrowsAsync(new InvalidOperationException(exceptionMessage));

            // Act
            var result = await _controller.GoogleLogin(googleLoginDto);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            var response = statusCodeResult.Value;
            var messageProperty = response.GetType().GetProperty("message");
            Assert.NotNull(messageProperty);
            var messageValue = messageProperty.GetValue(response) as string;
            Assert.Equal("Google authentication is not properly configured", messageValue);
        }

        [Fact]
        public async Task GoogleLogin_GeneralException_ReturnsInternalServerError()
        {
            // Arrange
            var googleLoginDto = new GoogleLoginRequestDto
            {
                IdToken = "valid-token"
            };
            var exceptionMessage = "Database connection failed";

            _mockAuthService.Setup(x => x.GoogleLoginAsync(googleLoginDto))
                           .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _controller.GoogleLogin(googleLoginDto);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            var response = statusCodeResult.Value;
            var messageProperty = response.GetType().GetProperty("message");
            Assert.NotNull(messageProperty);
            var messageValue = messageProperty.GetValue(response) as string;
            Assert.Equal("An error occurred during Google login", messageValue);
        }

        [Fact]
        public async Task GoogleLogin_NullRequest_ReturnsInternalServerError()
        {
            // Arrange
            GoogleLoginRequestDto nullRequest = null;

            _mockAuthService.Setup(x => x.GoogleLoginAsync(nullRequest))
                           .ThrowsAsync(new ArgumentNullException(nameof(nullRequest)));

            // Act
            var result = await _controller.GoogleLogin(nullRequest);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task GoogleLogin_EmptyIdToken_CallsAuthServiceWithEmptyToken()
        {
            // Arrange
            var googleLoginDto = new GoogleLoginRequestDto
            {
                IdToken = "",
                DeviceInfo = "Test Device"
            };

            _mockAuthService.Setup(x => x.GoogleLoginAsync(googleLoginDto))
                           .ThrowsAsync(new UnauthorizedAccessException("Invalid Google token"));

            // Act
            var result = await _controller.GoogleLogin(googleLoginDto);

            // Assert
            _mockAuthService.Verify(x => x.GoogleLoginAsync(googleLoginDto), Times.Once);
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
            Assert.NotNull(unauthorizedResult.Value);
        }

        #endregion

        #region RefreshToken Tests

        [Fact]
        public async Task RefreshToken_ValidToken_ReturnsOkWithNewTokens()
        {
            // Arrange
            var refreshTokenDto = new RefreshTokenRequestDto
            {
                RefreshToken = "valid-refresh-token"
            };
            var authResponse = new AuthResponseDto
            {
                AccessToken = "new-access-token",
                RefreshToken = "new-refresh-token",
                ExpiresAt = DateTime.UtcNow.AddMinutes(60),
                User = new UserDto
                {
                    UserId = Guid.NewGuid(),
                    Email = "test@example.com",
                    Name = "Test User",
                    Roles = ["User"]
                }
            };

            _mockAuthService.Setup(x => x.RefreshTokenAsync(refreshTokenDto))
                           .ReturnsAsync(authResponse);

            // Act
            var result = await _controller.RefreshToken(refreshTokenDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<AuthResponseDto>(okResult.Value);
            Assert.Equal(authResponse.AccessToken, returnValue.AccessToken);
            Assert.Equal(authResponse.RefreshToken, returnValue.RefreshToken);
            Assert.Equal(authResponse.User.Email, returnValue.User.Email);
        }

        [Fact]
        public async Task RefreshToken_InvalidToken_ReturnsUnauthorized()
        {
            // Arrange
            var refreshTokenDto = new RefreshTokenRequestDto
            {
                RefreshToken = "invalid-refresh-token"
            };
            var exceptionMessage = "Invalid or expired refresh token";

            _mockAuthService.Setup(x => x.RefreshTokenAsync(refreshTokenDto))
                           .ThrowsAsync(new UnauthorizedAccessException(exceptionMessage));

            // Act
            var result = await _controller.RefreshToken(refreshTokenDto);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
            var response = unauthorizedResult.Value;
            var messageProperty = response.GetType().GetProperty("message");
            Assert.NotNull(messageProperty);
            var messageValue = messageProperty.GetValue(response) as string;
            Assert.Equal(exceptionMessage, messageValue);
        }

        [Fact]
        public async Task RefreshToken_GeneralException_ReturnsInternalServerError()
        {
            // Arrange
            var refreshTokenDto = new RefreshTokenRequestDto
            {
                RefreshToken = "valid-refresh-token"
            };
            var exceptionMessage = "Database connection failed";

            _mockAuthService.Setup(x => x.RefreshTokenAsync(refreshTokenDto))
                           .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _controller.RefreshToken(refreshTokenDto);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            var response = statusCodeResult.Value;
            var messageProperty = response.GetType().GetProperty("message");
            Assert.NotNull(messageProperty);
            var messageValue = messageProperty.GetValue(response) as string;
            Assert.Equal("An error occurred during token refresh", messageValue);
        }

        [Fact]
        public async Task RefreshToken_NullRequest_ReturnsInternalServerError()
        {
            // Arrange
            RefreshTokenRequestDto nullRequest = null;

            _mockAuthService.Setup(x => x.RefreshTokenAsync(nullRequest))
                           .ThrowsAsync(new ArgumentNullException(nameof(nullRequest)));

            // Act
            var result = await _controller.RefreshToken(nullRequest);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        #endregion

        #region LoginWithTwoFactor Tests

        [Fact]
        public async Task LoginWithTwoFactor_ValidCredentialsRequiresOTP_ReturnsOkWithOTPRequired()
        {
            // Arrange
            var loginDto = new LoginRequestDto
            {
                Email = "test@example.com",
                Password = "password123"
            };
            var enhancedResponse = new EnhancedAuthResponseDto
            {
                RequiresOTP = true,
                Message = "OTP sent to your email",
                LoginToken = "session-123"
            };

            _mockAuthService.Setup(x => x.LoginWithTwoFactorAsync(loginDto))
                           .ReturnsAsync(enhancedResponse);

            // Act
            var result = await _controller.LoginWithTwoFactor(loginDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<EnhancedAuthResponseDto>(okResult.Value);
            Assert.True(returnValue.RequiresOTP);
            Assert.Equal(enhancedResponse.Message, returnValue.Message);
            Assert.Equal(enhancedResponse.LoginToken, returnValue.LoginToken);
        }

        [Fact]
        public async Task LoginWithTwoFactor_ValidCredentialsNoOTP_ReturnsOkWithAuthResponse()
        {
            // Arrange
            var loginDto = new LoginRequestDto
            {
                Email = "test@example.com",
                Password = "password123"
            };
            var enhancedResponse = new EnhancedAuthResponseDto
            {
                RequiresOTP = false,
                AuthResponse = new AuthResponseDto
                {
                    AccessToken = "access-token",
                    RefreshToken = "refresh-token",
                    User = new UserDto
                    {
                        UserId = Guid.NewGuid(),
                        Email = "test@example.com",
                        Name = "Test User",
                        Roles = ["User"]
                    }
                }
            };

            _mockAuthService.Setup(x => x.LoginWithTwoFactorAsync(loginDto))
                           .ReturnsAsync(enhancedResponse);

            // Act
            var result = await _controller.LoginWithTwoFactor(loginDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<EnhancedAuthResponseDto>(okResult.Value);
            Assert.False(returnValue.RequiresOTP);
            Assert.NotNull(returnValue.AuthResponse);
            Assert.Equal(enhancedResponse.AuthResponse.AccessToken, returnValue.AuthResponse.AccessToken);
        }

        [Fact]
        public async Task LoginWithTwoFactor_InvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var loginDto = new LoginRequestDto
            {
                Email = "test@example.com",
                Password = "wrongpassword"
            };
            var exceptionMessage = "Invalid email or password";

            _mockAuthService.Setup(x => x.LoginWithTwoFactorAsync(loginDto))
                           .ThrowsAsync(new UnauthorizedAccessException(exceptionMessage));

            // Act
            var result = await _controller.LoginWithTwoFactor(loginDto);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
            var response = unauthorizedResult.Value;
            var messageProperty = response.GetType().GetProperty("message");
            Assert.NotNull(messageProperty);
            var messageValue = messageProperty.GetValue(response) as string;
            Assert.Equal(exceptionMessage, messageValue);
        }

        [Fact]
        public async Task LoginWithTwoFactor_GeneralException_ReturnsInternalServerError()
        {
            // Arrange
            var loginDto = new LoginRequestDto
            {
                Email = "test@example.com",
                Password = "password123"
            };
            var exceptionMessage = "Database connection failed";

            _mockAuthService.Setup(x => x.LoginWithTwoFactorAsync(loginDto))
                           .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _controller.LoginWithTwoFactor(loginDto);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            var response = statusCodeResult.Value;
            var messageProperty = response.GetType().GetProperty("message");
            Assert.NotNull(messageProperty);
            var messageValue = messageProperty.GetValue(response) as string;
            Assert.Equal("An error occurred during login", messageValue);
        }

        [Fact]
        public async Task LoginWithTwoFactor_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            var loginDto = new LoginRequestDto
            {
                Email = "", // Invalid email
                Password = "password123"
            };

            _controller.ModelState.AddModelError("Email", "Email is required");

            // Act
            var result = await _controller.LoginWithTwoFactor(loginDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = badRequestResult.Value;
            var messageProperty = response.GetType().GetProperty("message");
            Assert.NotNull(messageProperty);
            var messageValue = messageProperty.GetValue(response) as string;
            Assert.Equal("Validation failed", messageValue);
        }

        [Fact]
        public async Task LoginWithTwoFactor_FailedResponse_ReturnsBadRequest()
        {
            // Arrange
            var loginDto = new LoginRequestDto
            {
                Email = "test@example.com",
                Password = "password123"
            };
            var enhancedResponse = new EnhancedAuthResponseDto
            {
                RequiresOTP = false,
                AuthResponse = null,
                Message = "Login failed"
            };

            _mockAuthService.Setup(x => x.LoginWithTwoFactorAsync(loginDto))
                           .ReturnsAsync(enhancedResponse);

            // Act
            var result = await _controller.LoginWithTwoFactor(loginDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = badRequestResult.Value;
            var messageProperty = response.GetType().GetProperty("message");
            Assert.NotNull(messageProperty);
            var messageValue = messageProperty.GetValue(response) as string;
            Assert.Equal("Login failed", messageValue);
        }

        #endregion
    }
}