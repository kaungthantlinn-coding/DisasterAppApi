using DisasterApp.Application.DTOs;
using DisasterApp.Application.Services.Interfaces;
using DisasterApp.WebApi.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using Xunit;

namespace DisasterApp.Tests.Middleware
{
    public class AuditLogMiddlewareTests
    {
        private readonly Mock<RequestDelegate> _mockNext;
        private readonly Mock<ILogger<AuditLogMiddleware>> _mockLogger;
        private readonly Mock<IServiceScopeFactory> _mockServiceScopeFactory;
        private readonly Mock<IServiceScope> _mockServiceScope;
        private readonly Mock<IServiceProvider> _mockServiceProvider;
        private readonly Mock<IAuditService> _mockAuditService;
        private readonly AuditLogMiddleware _middleware;

        public AuditLogMiddlewareTests()
        {
            _mockNext = new Mock<RequestDelegate>();
            _mockLogger = new Mock<ILogger<AuditLogMiddleware>>();
            _mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
            _mockServiceScope = new Mock<IServiceScope>();
            _mockServiceProvider = new Mock<IServiceProvider>();
            _mockAuditService = new Mock<IAuditService>();
            
            _mockServiceScopeFactory.Setup(x => x.CreateScope()).Returns(_mockServiceScope.Object);
            _mockServiceScope.Setup(x => x.ServiceProvider).Returns(_mockServiceProvider.Object);
            _mockServiceProvider.Setup(x => x.GetService(typeof(IAuditService))).Returns(_mockAuditService.Object);
            _mockServiceProvider.Setup(x => x.GetService(It.Is<Type>(t => t == typeof(IAuditService)))).Returns(_mockAuditService.Object);
            
            _middleware = new AuditLogMiddleware(_mockNext.Object, _mockLogger.Object, _mockServiceScopeFactory.Object);
        }

        #region GetActionName Tests

        [Theory]
        [InlineData("/api/auth/login", "POST", "USER_LOGIN")]
        [InlineData("/api/auth/logout", "POST", "USER_LOGOUT")]
        [InlineData("/api/auth/register", "POST", "USER_REGISTER")]
        [InlineData("/api/users", "POST", "USER_CREATE")]
        [InlineData("/api/users/123", "PUT", "USER_UPDATE")]
        [InlineData("/api/users/456", "DELETE", "USER_DELETE")]
        [InlineData("/api/roles", "GET", "ROLE_GET")]
        [InlineData("/api/roles", "POST", "ROLE_POST")]
        [InlineData("/api/roles/123", "PUT", "ROLE_PUT")]
        [InlineData("/api/roles/456", "DELETE", "ROLE_DELETE")]
        [InlineData("/api/reports", "GET", "REPORT_GET")]
        [InlineData("/api/reports/generate", "POST", "REPORT_POST")]
        [InlineData("/api/admin/settings", "PUT", "SYSTEM_SETTINGS_UPDATE")]
        [InlineData("/api/admin/users", "GET", "ADMIN_GET")]
        [InlineData("/api/admin/dashboard", "POST", "ADMIN_POST")]
        public void GetActionName_WithSpecificPaths_ReturnsExpectedActionName(string path, string method, string expectedAction)
        {
            // Arrange
            var context = CreateHttpContext(path, method);

            // Act
            var result = InvokeGetActionName(context);

            // Assert
            Assert.Equal(expectedAction, result);
        }

        [Theory]
        [InlineData("/api/disasters", "GET", "GET_DISASTERS")]
        [InlineData("/api/notifications", "POST", "POST_NOTIFICATIONS")]
        [InlineData("/api/settings/profile", "PUT", "PUT_PROFILE")]
        [InlineData("/api/unknown/endpoint", "PATCH", "PATCH_ENDPOINT")]
        [InlineData("/api/test", "DELETE", "DELETE_TEST")]
        public void GetActionName_WithGenericPaths_ReturnsMethodAndLastSegment(string path, string method, string expectedAction)
        {
            // Arrange
            var context = CreateHttpContext(path, method);

            // Act
            var result = InvokeGetActionName(context);

            // Assert
            Assert.Equal(expectedAction, result);
        }

        [Theory]
        [InlineData("/", "GET", "GET_")]
        [InlineData("/api", "POST", "POST_API")]
        [InlineData("/api/", "PUT", "PUT_")]
        [InlineData("", "DELETE", "DELETE_")]
        public void GetActionName_WithEdgeCasePaths_ReturnsExpectedActionName(string path, string method, string expectedAction)
        {
            // Arrange
            var context = CreateHttpContext(path, method);

            // Act
            var result = InvokeGetActionName(context);

            // Assert
            Assert.Equal(expectedAction, result);
        }

        [Fact]
        public void GetActionName_WithNullPath_ReturnsMethodUnknown()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Method = "GET";
            context.Request.Path = new PathString(); // This will be null

            // Act
            var result = InvokeGetActionName(context);

            // Assert
            Assert.Equal("GET_", result);
        }

        [Theory]
        [InlineData("/API/AUTH/LOGIN", "POST", "POST_LOGIN")] // Case sensitive - won't match /auth/login
        [InlineData("/api/Auth/Login", "post", "post_LOGIN")] // Case sensitive - won't match /auth/login
        [InlineData("/Api/Users/123", "Put", "Put_123")] // Case sensitive - won't match /users
        public void GetActionName_WithDifferentCasing_ReturnsExpectedActionName(string path, string method, string expectedAction)
        {
            // Arrange
            var context = CreateHttpContext(path, method);

            // Act
            var result = InvokeGetActionName(context);

            // Assert
            Assert.Equal(expectedAction, result);
        }

        [Theory]
        [InlineData("/api/auth/login/google", "POST", "USER_LOGIN")]
        [InlineData("/api/users/123/profile", "PUT", "USER_UPDATE")]
        [InlineData("/api/roles/admin/permissions", "GET", "ROLE_GET")]
        [InlineData("/api/admin/settings/security", "PUT", "SYSTEM_SETTINGS_UPDATE")]
        public void GetActionName_WithNestedPaths_ReturnsExpectedActionName(string path, string method, string expectedAction)
        {
            // Arrange
            var context = CreateHttpContext(path, method);

            // Act
            var result = InvokeGetActionName(context);

            // Assert
            Assert.Equal(expectedAction, result);
        }

        [Theory]
        [InlineData("/api/auth/login", "OPTIONS", "USER_LOGIN")] // Should still match auth patterns
        [InlineData("/api/users", "HEAD", "HEAD_USERS")] // Should fall back to generic pattern
        [InlineData("/api/roles", "TRACE", "ROLE_TRACE")] // Should match roles pattern
        public void GetActionName_WithUncommonHttpMethods_ReturnsExpectedActionName(string path, string method, string expectedAction)
        {
            // Arrange
            var context = CreateHttpContext(path, method);

            // Act
            var result = InvokeGetActionName(context);

            // Assert
            Assert.Equal(expectedAction, result);
        }

        #endregion

        #region Helper Methods

        private HttpContext CreateHttpContext(string path, string method)
        {
            var context = new DefaultHttpContext();
            context.Request.Path = new PathString(path);
            context.Request.Method = method;
            return context;
        }

        private string InvokeGetActionName(HttpContext context)
        {
            // Use reflection to call the private GetActionName method
            var methodInfo = typeof(AuditLogMiddleware).GetMethod("GetActionName", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(methodInfo);
            
            var result = methodInfo.Invoke(_middleware, new object[] { context });
            return result as string ?? string.Empty;
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidParameters_CreatesInstance()
        {
            // Act & Assert
            Assert.NotNull(_middleware);
        }

        [Fact]
        public void Constructor_WithNullNext_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                new AuditLogMiddleware(null!, _mockLogger.Object, _mockServiceScopeFactory.Object));
        }

        [Fact]
        public void Constructor_WithNullLogger_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                new AuditLogMiddleware(_mockNext.Object, null!, _mockServiceScopeFactory.Object));
        }

        [Fact]
        public void Constructor_WithNullServiceScopeFactory_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                new AuditLogMiddleware(_mockNext.Object, _mockLogger.Object, null!));
        }

        #endregion

        #region InvokeAsync Tests

        [Fact]
        public async Task InvokeAsync_WithLoggableAction_CallsAuditService()
        {
            // Arrange
            var context = CreateHttpContextWithUser("/api/users", "POST");
            var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            _mockNext.Setup(x => x(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(context);

            // Assert
            _mockAuditService.Verify(x => x.CreateLogAsync(It.IsAny<CreateAuditLogDto>()), Times.Once);
            _mockNext.Verify(x => x(context), Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_WithNonLoggableAction_DoesNotCallAuditService()
        {
            // Arrange
            var context = CreateHttpContextWithUser("/api/health", "GET");
            var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            _mockNext.Setup(x => x(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(context);

            // Assert
            _mockAuditService.Verify(x => x.CreateLogAsync(It.IsAny<CreateAuditLogDto>()), Times.Never);
            _mockNext.Verify(x => x(context), Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_WithException_LogsErrorAndRethrows()
        {
            // Arrange
            var context = CreateHttpContextWithUser("/api/users", "POST");
            var responseBody = new MemoryStream();
            context.Response.Body = responseBody;
            var expectedException = new InvalidOperationException("Test exception");

            _mockNext.Setup(x => x(It.IsAny<HttpContext>())).ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _middleware.InvokeAsync(context));
            Assert.Equal("Test exception", exception.Message);
            
            _mockAuditService.Verify(x => x.CreateLogAsync(It.Is<CreateAuditLogDto>(dto => 
                dto.Severity == "error")), Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_WithAuditServiceException_LogsErrorAndContinues()
        {
            // Arrange
            var context = CreateHttpContextWithUser("/api/users", "POST");
            var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            _mockNext.Setup(x => x(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);
            _mockAuditService.Setup(x => x.CreateLogAsync(It.IsAny<CreateAuditLogDto>()))
                .ThrowsAsync(new Exception("Audit service failed"));

            // Act
            await _middleware.InvokeAsync(context);

            // Assert
            _mockNext.Verify(x => x(context), Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_WithRequestBody_CapturesRequestBody()
        {
            // Arrange
            var context = CreateHttpContextWithUser("/api/users", "POST");
            var requestBody = "{\"name\":\"Test User\",\"email\":\"test@example.com\"}";
            var requestBodyBytes = Encoding.UTF8.GetBytes(requestBody);
            context.Request.Body = new MemoryStream(requestBodyBytes);
            context.Request.ContentLength = requestBodyBytes.Length;
            context.Request.ContentType = "application/json";
            
            var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            _mockNext.Setup(x => x(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(context);

            // Assert
            _mockAuditService.Verify(x => x.CreateLogAsync(It.Is<CreateAuditLogDto>(dto => 
                dto.Details.Contains(requestBody))), Times.Once);
        }

        #endregion

        #region ShouldLogAction Tests

        [Theory]
        [InlineData("/api/users", "POST", true)]
        [InlineData("/api/users", "PUT", true)]
        [InlineData("/api/users", "DELETE", true)]
        [InlineData("/api/users", "PATCH", true)]
        [InlineData("/api/users", "GET", false)]
        [InlineData("/api/admin/settings", "GET", true)] // Admin endpoints log all methods
        [InlineData("/api/admin/users", "POST", true)]
        [InlineData("/api/auth/refresh", "POST", false)] // Excluded path
        [InlineData("/api/health", "GET", false)] // Excluded path
        [InlineData("/api/audit-logs", "GET", false)] // Excluded path
        public void ShouldLogAction_WithVariousPaths_ReturnsExpectedResult(string path, string method, bool expected)
        {
            // Arrange
            var context = CreateHttpContext(path, method);

            // Act
            var result = InvokeShouldLogAction(context);

            // Assert
            Assert.Equal(expected, result);
        }

        #endregion

        #region GetRequestBodyAsync Tests

        [Fact]
        public async Task GetRequestBodyAsync_WithJsonContent_ReturnsBody()
        {
            // Arrange
            var requestBody = "{\"test\":\"data\"}";
            var requestBodyBytes = Encoding.UTF8.GetBytes(requestBody);
            var context = new DefaultHttpContext();
            context.Request.Body = new MemoryStream(requestBodyBytes);
            context.Request.ContentLength = requestBodyBytes.Length;
            context.Request.ContentType = "application/json";

            // Act
            var result = await InvokeGetRequestBodyAsync(context.Request);

            // Assert
            Assert.Equal(requestBody, result);
        }

        [Fact]
        public async Task GetRequestBodyAsync_WithFormContent_ReturnsEmpty()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.ContentType = "application/x-www-form-urlencoded";
            context.Request.ContentLength = 10;

            // Act
            var result = await InvokeGetRequestBodyAsync(context.Request);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public async Task GetRequestBodyAsync_WithNoContent_ReturnsEmpty()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.ContentLength = 0;

            // Act
            var result = await InvokeGetRequestBodyAsync(context.Request);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        #endregion

        #region GetActionDetails Tests

        [Fact]
        public void GetActionDetails_WithSuccessfulRequest_ReturnsSuccessDetails()
        {
            // Arrange
            var context = CreateHttpContext("/api/users", "POST");
            context.Response.StatusCode = 200;
            var requestBody = "{\"name\":\"Test\"}";

            // Act
            var result = InvokeGetActionDetails(context, requestBody, null);

            // Assert
            Assert.Contains("POST /api/users completed successfully", result);
            Assert.Contains("Status: 200", result);
            Assert.Contains(requestBody, result);
        }

        [Fact]
        public void GetActionDetails_WithException_ReturnsErrorDetails()
        {
            // Arrange
            var context = CreateHttpContext("/api/users", "POST");
            var exception = new InvalidOperationException("Test error");

            // Act
            var result = InvokeGetActionDetails(context, "", exception);

            // Assert
            Assert.Contains("POST /api/users failed with error: Test error", result);
        }

        [Fact]
        public void GetActionDetails_WithAuthPath_ExcludesRequestBody()
        {
            // Arrange
            var context = CreateHttpContext("/api/auth/login", "POST");
            context.Response.StatusCode = 200;
            var requestBody = "{\"password\":\"secret\"}";

            // Act
            var result = InvokeGetActionDetails(context, requestBody, null);

            // Assert
            Assert.Contains("POST /api/auth/login completed successfully", result);
            Assert.DoesNotContain(requestBody, result);
        }

        [Fact]
        public void GetActionDetails_WithLongRequestBody_TruncatesBody()
        {
            // Arrange
            var context = CreateHttpContext("/api/users", "POST");
            context.Response.StatusCode = 200;
            var longRequestBody = new string('a', 600); // Longer than 500 chars

            // Act
            var result = InvokeGetActionDetails(context, longRequestBody, null);

            // Assert
            Assert.Contains("...", result);
            Assert.DoesNotContain(longRequestBody, result);
        }

        #endregion

        #region GetResourceName Tests

        [Theory]
        [InlineData("/api/auth/login", "authentication")]
        [InlineData("/api/users/123", "user_management")]
        [InlineData("/api/roles/admin", "role_management")]
        [InlineData("/api/reports/generate", "reports")]
        [InlineData("/api/admin/settings", "admin")]
        [InlineData("/api/disasters/list", "disasters")]
        [InlineData("/api/unknown", "api")]
        public void GetResourceName_WithVariousPaths_ReturnsExpectedResource(string path, string expectedResource)
        {
            // Arrange
            var context = CreateHttpContext(path, "GET");

            // Act
            var result = InvokeGetResourceName(context);

            // Assert
            Assert.Equal(expectedResource, result);
        }

        #endregion

        #region GetMetadata Tests

        [Fact]
        public void GetMetadata_WithValidContext_ReturnsMetadata()
        {
            // Arrange
            var context = CreateHttpContext("/api/users", "POST");
            context.Request.ContentType = "application/json";
            context.Request.ContentLength = 100;
            context.Request.Headers.Referer = "https://example.com";
            context.Response.StatusCode = 201;
            var startTime = DateTime.UtcNow.AddSeconds(-1);

            // Act
            var result = InvokeGetMetadata(context, startTime);

            // Assert
            Assert.Equal("POST", result["method"]);
            Assert.Equal("/api/users", result["path"]);
            Assert.Equal(201, result["statusCode"]);
            Assert.Equal("application/json", result["contentType"]);
            Assert.Equal(100L, result["contentLength"]);
            Assert.Equal("https://example.com", result["referer"]);
            Assert.True((double)result["duration"] > 0);
            Assert.NotNull(result["timestamp"]);
        }

        #endregion

        #region GetClientIpAddress Tests

        [Fact]
        public void GetClientIpAddress_WithXForwardedFor_ReturnsForwardedIp()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Headers["X-Forwarded-For"] = "192.168.1.1, 10.0.0.1";
            context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");

            // Act
            var result = InvokeGetClientIpAddress(context);

            // Assert
            Assert.Equal("192.168.1.1", result);
        }

        [Fact]
        public void GetClientIpAddress_WithXRealIp_ReturnsRealIp()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Headers["X-Real-IP"] = "192.168.1.2";
            context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");

            // Act
            var result = InvokeGetClientIpAddress(context);

            // Assert
            Assert.Equal("192.168.1.2", result);
        }

        [Fact]
        public void GetClientIpAddress_WithRemoteIpOnly_ReturnsRemoteIp()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("192.168.1.3");

            // Act
            var result = InvokeGetClientIpAddress(context);

            // Assert
            Assert.Equal("192.168.1.3", result);
        }

        [Fact]
        public void GetClientIpAddress_WithNoIpAddress_ReturnsUnknown()
        {
            // Arrange
            var context = new DefaultHttpContext();

            // Act
            var result = InvokeGetClientIpAddress(context);

            // Assert
            Assert.Equal("unknown", result);
        }

        #endregion

        #region Extension Method Tests

        [Fact]
        public void UseAuditLogging_WithValidBuilder_ReturnsBuilder()
        {
            // Arrange
            var mockBuilder = new Mock<IApplicationBuilder>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockBuilder.Setup(x => x.ApplicationServices).Returns(mockServiceProvider.Object);
            mockBuilder.Setup(x => x.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>())).Returns(mockBuilder.Object);

            // Act
            var result = AuditLogMiddlewareExtensions.UseAuditLogging(mockBuilder.Object);

            // Assert
            Assert.NotNull(result);
            Assert.Same(mockBuilder.Object, result);
        }

        #endregion

        #region Helper Methods

        private HttpContext CreateHttpContextWithUser(string path, string method)
        {
            var context = CreateHttpContext(path, method);
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new(ClaimTypes.Name, "Test User")
            };
            context.User = new ClaimsPrincipal(new ClaimsIdentity(claims));
            return context;
        }

        private bool InvokeShouldLogAction(HttpContext context)
        {
            var methodInfo = typeof(AuditLogMiddleware).GetMethod("ShouldLogAction", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(methodInfo);
            return (bool)methodInfo.Invoke(_middleware, new object[] { context })!;
        }

        private async Task<string> InvokeGetRequestBodyAsync(HttpRequest request)
        {
            var methodInfo = typeof(AuditLogMiddleware).GetMethod("GetRequestBodyAsync", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(methodInfo);
            var task = (Task<string>)methodInfo.Invoke(_middleware, new object[] { request })!;
            return await task;
        }

        private string InvokeGetActionDetails(HttpContext context, string requestBody, Exception? exception)
        {
            var methodInfo = typeof(AuditLogMiddleware).GetMethod("GetActionDetails", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(methodInfo);
            return (string)methodInfo.Invoke(_middleware, new object[] { context, requestBody, exception })!;
        }

        private string InvokeGetResourceName(HttpContext context)
        {
            var methodInfo = typeof(AuditLogMiddleware).GetMethod("GetResourceName", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(methodInfo);
            return (string)methodInfo.Invoke(_middleware, new object[] { context })!;
        }

        private Dictionary<string, object> InvokeGetMetadata(HttpContext context, DateTime startTime)
        {
            var methodInfo = typeof(AuditLogMiddleware).GetMethod("GetMetadata", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(methodInfo);
            return (Dictionary<string, object>)methodInfo.Invoke(_middleware, new object[] { context, startTime })!;
        }

        private string InvokeGetClientIpAddress(HttpContext context)
        {
            var methodInfo = typeof(AuditLogMiddleware).GetMethod("GetClientIpAddress", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(methodInfo);
            return (string)methodInfo.Invoke(_middleware, new object[] { context })!;
        }

        #endregion
    }
}