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

    /// User login endpoint

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequestDto request)
    {
        try
        {
            var response = await _authService.LoginAsync(request);
            return Ok(response);
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


    /// User signup endpoint

    [HttpPost("signup")]
    public async Task<ActionResult<AuthResponseDto>> Signup([FromBody] SignupRequestDto request)
    {
        try
        {
            var response = await _authService.SignupAsync(request);
            return Ok(response);
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


    /// Google OAuth login endpoint

    [HttpPost("google-login")]
    public async Task<ActionResult<AuthResponseDto>> GoogleLogin([FromBody] GoogleLoginRequestDto request)
    {
        try
        {
            var response = await _authService.GoogleLoginAsync(request);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Google login failed. Reason: {Reason}", ex.Message);
            return Unauthorized(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Google login configuration error. Reason: {Reason}", ex.Message);
            return StatusCode(500, new { message = "Google authentication is not properly configured" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Google login");
            return StatusCode(500, new { message = "An error occurred during Google login" });
        }
    }

    /// Refresh access token using refresh token
    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponseDto>> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        try
        {
            var response = await _authService.RefreshTokenAsync(request);
            return Ok(response);
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

    /// User logout endpoint

    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult> Logout([FromBody] RefreshTokenRequestDto request)
    {
        try
        {
            var success = await _authService.LogoutAsync(request.RefreshToken);
            if (success)
            {
                return Ok(new { message = "Logged out successfully" });
            }
            return BadRequest(new { message = "Invalid refresh token" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return StatusCode(500, new { message = "An error occurred during logout" });
        }
    }


    /// Validate access token

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


    /// Get current user information

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


    /// Initiate password reset process

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

    
    // reset password using reset token
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

    // verify reset token validity

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

    // validate password strength
    
    [HttpPost("validate-password")]
    public ActionResult ValidatePassword([FromBody] ValidatePasswordRequestDto request)
    {
        try
        {
            // basic validation that matches the DTO validation
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

    // test email functionality
    
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

            return Ok(new {
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

    // =====================================================
    // TWO-FACTOR AUTHENTICATION ENDPOINTS
    // =====================================================

    /// <summary>
    /// Enhanced login with 2FA support
    /// </summary>
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



    /// <summary>
    /// Send OTP code via email
    /// </summary>
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

    /// <summary>
    /// Verify OTP code
    /// </summary>
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

    /// <summary>
    /// Verify backup code
    /// </summary>
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

    /// <summary>
    /// Get user's 2FA status
    /// </summary>
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

    /// <summary>
    /// Initialize 2FA setup
    /// </summary>
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

    /// <summary>
    /// Disable 2FA
    /// </summary>
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

    /// <summary>
    /// Generate new backup codes
    /// </summary>
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

    // =====================================================
    // EMAIL OTP AUTHENTICATION ENDPOINTS
    // =====================================================

    /// <summary>
    /// Send OTP code via email for authentication
    /// </summary>
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

    /// <summary>
    /// Verify OTP code and authenticate user
    /// </summary>
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
}