using DisasterApp.Application.DTOs;
using DisasterApp.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DisasterApp.Infrastructure.Data;

namespace DisasterApp.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly DisasterDbContext _context;
    private readonly IAuthService _authService;
    private readonly IEmailService _emailService;
    private readonly ITwoFactorService _twoFactorService;
    private readonly IEmailOtpService _emailOtpService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(DisasterDbContext context, IAuthService authService, IEmailService emailService, ITwoFactorService twoFactorService, IEmailOtpService emailOtpService, ILogger<AuthController> logger)
    {
        _context = context;
        _authService = authService;
        _emailService = emailService;
        _twoFactorService = twoFactorService;
        _emailOtpService = emailOtpService;
        _logger = logger;
    }


    [HttpPost("login")]
    public async Task<ActionResult<CookieAuthResponseDto>> Login([FromBody] LoginRequestDto request)
    {
        try
        {
            var response = await _authService.LoginAsync(request);

            SetRefreshTokenCookie(response.RefreshToken);

            var cookieResponse = new CookieAuthResponseDto
            {
                AccessToken = response.AccessToken,
                ExpiresAt = response.ExpiresAt,
                User = response.User
            };

            _logger.LogInformation("Login response for {Email}: AccessToken={HasToken}, User.UserId={UserId}, User.Name={Name}, User.Email={Email}, User.Roles={Roles}",
                request.Email,
                !string.IsNullOrEmpty(response.AccessToken),
                response.User?.UserId,
                response.User?.Name,
                response.User?.Email,
                response.User?.Roles != null ? string.Join(",", response.User.Roles) : "null");

            return Ok(cookieResponse);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Login failed for email: {Email}. Reason: {Reason}", request.Email, ex.Message);
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for email: {Email}", request.Email);
            return StatusCode(500, new { message = "An error occurred during login" });
        }
    }



    [HttpPost("signup")]
    public async Task<ActionResult<CookieAuthResponseDto>> Signup([FromBody] SignupRequestDto request)
    {
        try
        {
            var response = await _authService.SignupAsync(request);

            SetRefreshTokenCookie(response.RefreshToken);

            var cookieResponse = new CookieAuthResponseDto
            {
                AccessToken = response.AccessToken,
                ExpiresAt = response.ExpiresAt,
                User = response.User
            };

            return Ok(cookieResponse);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Signup failed for email: {Email}. Reason: {Reason}", request.Email, ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during signup for email: {Email}", request.Email);
            return StatusCode(500, new { message = "An error occurred during signup" });
        }
    }

    [HttpPost("google-login")]
    public async Task<ActionResult<CookieAuthResponseDto>> GoogleLogin([FromBody] GoogleLoginRequestDto request)
    {
        _logger.LogInformation("🚀 AuthController - GoogleLogin endpoint called. IdToken length: {TokenLength}, DeviceInfo: {DeviceInfo}",
            request?.IdToken?.Length ?? 0, request?.DeviceInfo ?? "N/A");

        try
        {
            var response = await _authService.GoogleLoginAsync(request);

            SetRefreshTokenCookie(response.RefreshToken);

            var cookieResponse = new CookieAuthResponseDto
            {
                AccessToken = response.AccessToken,
                ExpiresAt = response.ExpiresAt,
                User = response.User
            };

            _logger.LogInformation("✅ AuthController - GoogleLogin successful. Returning user: {UserId}, Name: {Name}, Email: {Email}, Roles: {Roles}",
                response?.User?.UserId, response?.User?.Name, response?.User?.Email,
                response?.User?.Roles != null ? string.Join(", ", response.User.Roles) : "N/A");

            return Ok(cookieResponse);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("❌ AuthController - Google login failed. Reason: {Reason}", ex.Message);
            return Unauthorized(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("⚙️ AuthController - Google login configuration error. Reason: {Reason}", ex.Message);
            return StatusCode(500, new { message = "Google authentication is not properly configured" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 AuthController - Error during Google login: {ErrorMessage}", ex.Message);
            return StatusCode(500, new { message = "An error occurred during Google login" });
        }
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<CookieAuthResponseDto>> RefreshToken()
    {
        try
        {
            var refreshToken = GetRefreshTokenFromCookie();

            _logger.LogInformation("🔍 RefreshToken - Cookie value: {HasCookie}, Length: {Length}",
                !string.IsNullOrEmpty(refreshToken), refreshToken?.Length ?? 0);

            if (string.IsNullOrEmpty(refreshToken))
            {
                _logger.LogWarning("❌ RefreshToken - No refresh token found in cookies");
                return Unauthorized(new { message = "Refresh token not found in cookies" });
            }

            var request = new RefreshTokenRequestDto { RefreshToken = refreshToken };
            var response = await _authService.RefreshTokenAsync(request);

            SetRefreshTokenCookie(response.RefreshToken);

            var cookieResponse = new CookieAuthResponseDto
            {
                AccessToken = response.AccessToken,
                ExpiresAt = response.ExpiresAt,
                User = response.User
            };

            return Ok(cookieResponse);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Token refresh failed. Reason: {Reason}", ex.Message);
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return StatusCode(500, new { message = "An error occurred during token refresh" });
        }
    }


    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult> Logout()
    {
        try
        {
            var refreshToken = GetRefreshTokenFromCookie();
            if (string.IsNullOrEmpty(refreshToken))
            {
                ClearRefreshTokenCookie();
                return Ok(new { message = "Logged out successfully" });
            }

            var success = await _authService.LogoutAsync(refreshToken);

            ClearRefreshTokenCookie();

            if (success)
            {
                return Ok(new { message = "Logged out successfully" });
            }
            return Ok(new { message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            ClearRefreshTokenCookie();
            return StatusCode(500, new { message = "An error occurred during logout" });
        }
    }



    [HttpGet("validate")]
    [Authorize]
    public async Task<ActionResult> ValidateToken()
    {
        try
        {
            var token = Request.Headers.Authorization.ToString().Replace("Bearer ", "");
            var isValid = await _authService.ValidateTokenAsync(token);

            if (isValid)
            {
                return Ok(new { message = "Token is valid", user = User.Identity?.Name });
            }
            return Unauthorized(new { message = "Token is invalid" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token validation");
            return StatusCode(500, new { message = "An error occurred during token validation" });
        }
    }



    [HttpGet("me")]
    [Authorize]
    public ActionResult GetCurrentUser()
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userName = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
            var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            var userRoles = User.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).ToList();

            return Ok(new
            {
                userId,
                name = userName,
                email = userEmail,
                roles = userRoles
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user");
            return StatusCode(500, new { message = "An error occurred while getting user information" });
        }
    }



    [HttpPost("forgot-password")]
    public async Task<ActionResult<ForgotPasswordResponseDto>> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
    {
        try
        {
            var response = await _authService.ForgotPasswordAsync(request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during forgot password for email: {Email}", request.Email);
            return StatusCode(500, new ForgotPasswordResponseDto
            {
                Success = false,
                Message = "An error occurred while processing your request"
            });
        }
    }


    [HttpPost("reset-password")]
    public async Task<ActionResult<ForgotPasswordResponseDto>> ResetPassword([FromBody] ResetPasswordRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .SelectMany(x => x.Value.Errors)
                    .Select(x => x.ErrorMessage)
                    .ToList();

                _logger.LogWarning("Password reset validation failed: {Errors}", string.Join("; ", errors));
                return BadRequest(new ForgotPasswordResponseDto
                {
                    Success = false,
                    Message = string.Join("; ", errors)
                });
            }

            var response = await _authService.ResetPasswordAsync(request);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password reset");
            return StatusCode(500, new ForgotPasswordResponseDto
            {
                Success = false,
                Message = "An error occurred while resetting your password"
            });
        }
    }


    [HttpPost("verify-reset-token")]
    public async Task<ActionResult<VerifyResetTokenResponseDto>> VerifyResetToken([FromBody] VerifyResetTokenRequestDto request)
    {
        try
        {
            var response = await _authService.VerifyResetTokenAsync(request);
            if (!response.IsValid)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token verification");
            return StatusCode(500, new VerifyResetTokenResponseDto
            {
                IsValid = false,
                Message = "An error occurred while verifying the token"
            });
        }
    }


    [HttpPost("validate-password")]
    public ActionResult ValidatePassword([FromBody] ValidatePasswordRequestDto request)
    {
        try
        {
            var isValid = !string.IsNullOrWhiteSpace(request.Password) &&
                         request.Password.Length >= 8 &&
                         System.Text.RegularExpressions.Regex.IsMatch(request.Password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]");

            return Ok(new
            {
                isValid,
                message = isValid ? "Password meets requirements" : "Password does not meet strength requirements"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password validation");
            return StatusCode(500, new { message = "An error occurred while validating the password" });
        }
    }


    [HttpPost("test-email")]
    public async Task<IActionResult> TestEmail([FromBody] ForgotPasswordRequestDto request)
    {
        try
        {
            _logger.LogInformation("=== EMAIL TEST ENDPOINT CALLED ===");
            _logger.LogInformation("Testing email functionality for: {Email}", request.Email);

            var result = await _emailService.SendEmailAsync(
                request.Email,
                "Test Email from Disaster Management System",
                "<h1>Test Email</h1><p>This is a test email to verify email functionality is working.</p><p>If you receive this, email sending is working correctly!</p>"
            );

            _logger.LogInformation("Email test result: {Result}", result);

            return Ok(new
            {
                message = "Test email sent",
                success = result,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending test email");
            return StatusCode(500, new { message = "An error occurred while sending test email" });
        }
    }


    [HttpPost("login-otp")]
    public async Task<ActionResult<EnhancedAuthResponseDto>> LoginWithTwoFactor([FromBody] LoginRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .SelectMany(x => x.Value!.Errors)
                    .Select(x => x.ErrorMessage)
                    .ToList();

                return BadRequest(new { message = "Validation failed", errors });
            }

            var response = await _authService.LoginWithTwoFactorAsync(request);

            if (response.RequiresOTP)
            {
                return Ok(response);
            }
            else if (response.AuthResponse != null)
            {
                return Ok(response);
            }
            else
            {
                return BadRequest(new { message = response.Message });
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Login failed for email: {Email}. Reason: {Reason}", request.Email, ex.Message);
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during enhanced login for email: {Email}", request.Email);
            return StatusCode(500, new { message = "An error occurred during login" });
        }
    }



    [HttpPost("otp/send")]
    public async Task<ActionResult<SendOtpResponseDto>> SendOtp([FromBody] SendOtpRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .SelectMany(x => x.Value!.Errors)
                    .Select(x => x.ErrorMessage)
                    .ToList();

                return BadRequest(new { message = "Validation failed", errors });
            }

            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var response = await _authService.SendOtpAsync(request, ipAddress);

            if (response.Success)
            {
                return Ok(response);
            }
            else
            {
                return BadRequest(response);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending OTP");
            return StatusCode(500, new { message = "An error occurred while sending OTP" });
        }
    }

    [HttpPost("otp/verify")]
    public async Task<ActionResult<AuthResponseDto>> VerifyOtp([FromBody] VerifyOtpRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .SelectMany(x => x.Value!.Errors)
                    .Select(x => x.ErrorMessage)
                    .ToList();

                return BadRequest(new { message = "Validation failed", errors });
            }

            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var response = await _authService.VerifyOtpAsync(request, ipAddress);

            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("OTP verification failed. Reason: {Reason}", ex.Message);
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying OTP");
            return StatusCode(500, new { message = "An error occurred while verifying OTP" });
        }
    }

    [HttpPost("otp/verify-backup")]
    public async Task<ActionResult<AuthResponseDto>> VerifyBackupCode([FromBody] VerifyBackupCodeRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .SelectMany(x => x.Value!.Errors)
                    .Select(x => x.ErrorMessage)
                    .ToList();

                return BadRequest(new { message = "Validation failed", errors });
            }

            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var response = await _authService.VerifyBackupCodeAsync(request, ipAddress);

            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Backup code verification failed. Reason: {Reason}", ex.Message);
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying backup code");
            return StatusCode(500, new { message = "An error occurred while verifying backup code" });
        }
    }

    [HttpGet("2fa/status")]
    [Authorize]
    public async Task<ActionResult<TwoFactorStatusDto>> GetTwoFactorStatus()
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out Guid userGuid))
            {
                return Unauthorized(new { message = "Invalid user token" });
            }

            var status = await _twoFactorService.GetTwoFactorStatusAsync(userGuid);
            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting 2FA status");
            return StatusCode(500, new { message = "An error occurred while getting 2FA status" });
        }
    }

    [HttpPost("2fa/setup")]
    [Authorize]
    public async Task<ActionResult<SetupTwoFactorResponseDto>> SetupTwoFactor([FromBody] SetupTwoFactorRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .SelectMany(x => x.Value!.Errors)
                    .Select(x => x.ErrorMessage)
                    .ToList();

                return BadRequest(new { message = "Validation failed", errors });
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out Guid userGuid))
            {
                return Unauthorized(new { message = "Invalid user token" });
            }

            var response = await _twoFactorService.SetupTwoFactorAsync(userGuid, request.CurrentPassword);

            if (response.Success)
            {
                return Ok(response);
            }
            else
            {
                return BadRequest(response);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting up 2FA");
            return StatusCode(500, new { message = "An error occurred while setting up 2FA" });
        }
    }

    /// <summary>
    /// Complete 2FA setup
    /// </summary>
    [HttpPost("2fa/verify-setup")]
    [Authorize]
    public async Task<ActionResult<VerifySetupResponseDto>> VerifySetup([FromBody] VerifySetupRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .SelectMany(x => x.Value!.Errors)
                    .Select(x => x.ErrorMessage)
                    .ToList();

                return BadRequest(new { message = "Validation failed", errors });
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out Guid userGuid))
            {
                return Unauthorized(new { message = "Invalid user token" });
            }

            var response = await _twoFactorService.VerifySetupAsync(userGuid, request.Code);

            if (response.Success)
            {
                return Ok(response);
            }
            else
            {
                return BadRequest(response);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying 2FA setup");
            return StatusCode(500, new { message = "An error occurred while verifying 2FA setup" });
        }
    }

    [HttpPost("2fa/disable")]
    [Authorize]
    public async Task<ActionResult> DisableTwoFactor([FromBody] DisableTwoFactorRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .SelectMany(x => x.Value!.Errors)
                    .Select(x => x.ErrorMessage)
                    .ToList();

                return BadRequest(new { message = "Validation failed", errors });
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out Guid userGuid))
            {
                return Unauthorized(new { message = "Invalid user token" });
            }

            var success = await _twoFactorService.DisableTwoFactorAsync(userGuid, request.CurrentPassword, request.OtpCode);

            if (success)
            {
                return Ok(new { message = "Two-factor authentication has been disabled successfully" });
            }
            else
            {
                return BadRequest(new { message = "Failed to disable two-factor authentication. Please check your password and try again." });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disabling 2FA");
            return StatusCode(500, new { message = "An error occurred while disabling 2FA" });
        }
    }

    [HttpPost("2fa/backup-codes/generate")]
    [Authorize]
    public async Task<ActionResult<GenerateBackupCodesResponseDto>> GenerateBackupCodes([FromBody] SetupTwoFactorRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .SelectMany(x => x.Value!.Errors)
                    .Select(x => x.ErrorMessage)
                    .ToList();

                return BadRequest(new { message = "Validation failed", errors });
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out Guid userGuid))
            {
                return Unauthorized(new { message = "Invalid user token" });
            }

            var response = await _twoFactorService.GenerateBackupCodesAsync(userGuid, request.CurrentPassword);

            if (response.Success)
            {
                return Ok(response);
            }
            else
            {
                return BadRequest(response);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating backup codes");
            return StatusCode(500, new { message = "An error occurred while generating backup codes" });
        }
    }


    [HttpPost("send-otp")]
    public async Task<ActionResult<SendEmailOtpResponseDto>> SendEmailOtp([FromBody] SendEmailOtpRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .SelectMany(x => x.Value!.Errors)
                    .Select(x => x.ErrorMessage)
                    .ToList();

                return BadRequest(new { message = "Invalid email format", errors });
            }

            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var response = await _emailOtpService.SendOtpAsync(request, ipAddress);

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("Too many requests"))
            {
                return StatusCode(429, new { message = ex.Message, retryAfter = 60 });
            }
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email OTP to {Email}", request.email);
            return StatusCode(500, new { message = "Server error" });
        }
    }

    [HttpPost("verify-otp")]
    public async Task<ActionResult<VerifyEmailOtpResponseDto>> VerifyEmailOtp([FromBody] VerifyEmailOtpRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .SelectMany(x => x.Value!.Errors)
                    .Select(x => x.ErrorMessage)
                    .ToList();

                return BadRequest(new { message = "Invalid/expired OTP", errors });
            }

            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var response = await _emailOtpService.VerifyOtpAsync(request, ipAddress);

            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            if (ex.Message.Contains("not found"))
            {
                return NotFound(new { message = "OTP not found" });
            }
            if (ex.Message.Contains("Too many"))
            {
                return StatusCode(429, new { message = ex.Message });
            }
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying email OTP for {Email}", request.email);
            return StatusCode(500, new { message = "Server error" });
        }
    }

    /// <summary>
    /// Helper method to set refresh token as HTTP-only secure cookie
    /// </summary>
    private void SetRefreshTokenCookie(string refreshToken)
    {
        var environment = HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();
        var isHttps = Request.IsHttps;

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = isHttps,
            SameSite = isHttps ? SameSiteMode.None : SameSiteMode.Lax,
            Expires = DateTime.UtcNow.AddDays(int.Parse(HttpContext.RequestServices.GetRequiredService<IConfiguration>()["Jwt:RefreshTokenExpirationDays"] ?? "30")),
            Path = "/"
        };

        _logger.LogInformation("🍪 SetRefreshTokenCookie - Setting cookie: Length={Length}, IsHttps={IsHttps}, Secure={Secure}, SameSite={SameSite}",
            refreshToken?.Length ?? 0, isHttps, cookieOptions.Secure, cookieOptions.SameSite);

        Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
    }

    /// <summary>
    /// Helper method to get refresh token from HTTP-only cookie
    /// </summary>
    private string? GetRefreshTokenFromCookie()
    {
        return Request.Cookies["refreshToken"];
    }

    /// <summary>
    /// Helper method to clear refresh token cookie
    /// </summary>
    private void ClearRefreshTokenCookie()
    {
        var isHttps = Request.IsHttps;

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = isHttps,
            SameSite = isHttps ? SameSiteMode.None : SameSiteMode.Lax,
            Expires = DateTime.UtcNow.AddDays(-1),
            Path = "/"
        };

        Response.Cookies.Append("refreshToken", "", cookieOptions);
    }
}