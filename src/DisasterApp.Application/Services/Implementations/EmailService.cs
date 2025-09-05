using DisasterApp.Application.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;

namespace DisasterApp.Application.Services.Implementations;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> SendPasswordResetEmailAsync(string email, string resetToken, string resetUrl)
    {
        var subject = "Password Reset Request - DisasterWatch";
        var body = $@"
            <html>
            <body>
                <h2>Password Reset Request</h2>
                <p>You have requested to reset your password for your DisasterWatch account.</p>
                <p>Click the link below to reset your password:</p>
                <p><a href='{resetUrl}?token={resetToken}'>Reset Password</a></p>
                <p>This link will expire in 1 hour.</p>
                <p>If you did not request this password reset, please ignore this email.</p>
                <br>
                <p>Best regards,<br>DisasterWatch Team</p>
            </body>
            </html>";

        return await SendEmailAsync(email, subject, body);
    }

    public async Task<bool> SendEmailAsync(string to, string subject, string body)
    {
        try
        {
            var smtpServer = _configuration["Email:SmtpServer"];
            var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
            var senderEmail = _configuration["Email:SenderEmail"];
            var senderName = _configuration["Email:SenderName"];
            var username = _configuration["Email:Username"];
            var password = _configuration["Email:Password"];
            var enableSsl = bool.Parse(_configuration["Email:EnableSsl"] ?? "true");

            _logger.LogInformation("=== EMAIL SERVICE DEBUG INFO ===");
            _logger.LogInformation("SmtpServer: '{SmtpServer}'", smtpServer ?? "NULL");
            _logger.LogInformation("SmtpPort: {SmtpPort}", smtpPort);
            _logger.LogInformation("SenderEmail: '{SenderEmail}'", senderEmail ?? "NULL");
            _logger.LogInformation("SenderName: '{SenderName}'", senderName ?? "NULL");
            _logger.LogInformation("Username: '{Username}'", username ?? "NULL");
            _logger.LogInformation("Password: '{Password}'", string.IsNullOrEmpty(password) ? "NULL/EMPTY" : "***PROVIDED***");
            _logger.LogInformation("EnableSsl: {EnableSsl}", enableSsl);
            _logger.LogInformation("To: '{To}'", to);
            _logger.LogInformation("Subject: '{Subject}'", subject);

            if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(senderEmail) ||
                string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                _logger.LogWarning("Email configuration is incomplete. Logging email instead of sending.");
                _logger.LogInformation("Email would be sent to: {Email}", to);
                _logger.LogInformation("Subject: {Subject}", subject);
                _logger.LogInformation("Body: {Body}", body);
                return true;
            }

            _logger.LogInformation("Configuration validated successfully. Attempting to send email...");

            using var client = new SmtpClient(smtpServer, smtpPort);
            client.EnableSsl = enableSsl;
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(username, password);

            _logger.LogInformation("SMTP client configured. Creating mail message...");

            using var mailMessage = new MailMessage();
            mailMessage.From = new MailAddress(senderEmail, senderName);
            mailMessage.To.Add(to);
            mailMessage.Subject = subject;
            mailMessage.Body = body;
            mailMessage.IsBodyHtml = true;

            _logger.LogInformation("Sending email via SMTP...");
            await client.SendMailAsync(mailMessage);

            _logger.LogInformation("✅ Email sent successfully to: {Email}", to);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to send email to {Email}. Error: {ErrorMessage}", to, ex.Message);
            _logger.LogError("Exception details: {ExceptionDetails}", ex.ToString());

            _logger.LogInformation("Email fallback - would be sent to: {Email}", to);
            _logger.LogInformation("Subject: {Subject}", subject);
            _logger.LogInformation("Body: {Body}", body);

            return false;
        }
    }

    public async Task<bool> SendAuthProviderNotificationEmailAsync(string email, string authProvider)
    {
        var subject = "Password Reset Request - DisasterWatch";
        var body = $@"
            <html>
            <body>
                <h2>Password Reset Request</h2>
                <p>We received a password reset request for your DisasterWatch account ({email}).</p>
                <p><strong>Your account uses {authProvider} authentication.</strong></p>
                <p>To access your account, please:</p>
                <ul>
                    <li>Go to the DisasterWatch login page</li>
                    <li>Click ""Sign in with {authProvider}""</li>
                    <li>Use your {authProvider} credentials to log in</li>
                </ul>
                <p>If you're having trouble accessing your {authProvider} account, please visit {authProvider}'s help center or contact their support team.</p>
                <p>If you did not request this password reset, you can safely ignore this email.</p>
                <br>
                <p>Best regards,<br>DisasterWatch Team</p>
            </body>
            </html>";

        return await SendEmailAsync(email, subject, body);
    }

    public async Task<bool> SendOtpEmailAsync(string email, string otpCode)
    {
        var subject = "Your Verification Code - DisasterWatch";
        var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                <div style='background-color: #f8f9fa; padding: 20px; border-radius: 8px;'>
                    <h2 style='color: #333; text-align: center;'>Verification Code</h2>
                    <p>You have requested a verification code for your DisasterWatch account.</p>

                    <div style='background-color: #fff; padding: 20px; border-radius: 8px; text-align: center; margin: 20px 0;'>
                        <h1 style='color: #007bff; font-size: 32px; letter-spacing: 8px; margin: 0;'>{otpCode}</h1>
                        <p style='color: #666; margin: 10px 0 0 0;'>Enter this code to continue</p>
                    </div>

                    <div style='background-color: #fff3cd; border: 1px solid #ffeaa7; padding: 15px; border-radius: 8px; margin: 20px 0;'>
                        <p style='margin: 0; color: #856404;'><strong>Important:</strong></p>
                        <ul style='margin: 5px 0 0 0; color: #856404;'>
                            <li>This code expires in <strong>5 minutes</strong></li>
                            <li>Do not share this code with anyone</li>
                            <li>If you didn't request this code, please ignore this email</li>
                        </ul>
                    </div>

                    <p style='color: #666; font-size: 14px; text-align: center; margin-top: 30px;'>
                        Best regards,<br>
                        DisasterWatch Security Team
                    </p>
                </div>
            </body>
            </html>";

        return await SendEmailAsync(email, subject, body);
    }

    public async Task<bool> SendTwoFactorEnabledEmailAsync(string email)
    {
        var subject = "Two-Factor Authentication Enabled - DisasterWatch";
        var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                <div style='background-color: #f8f9fa; padding: 20px; border-radius: 8px;'>
                    <h2 style='color: #28a745; text-align: center;'>🔒 Two-Factor Authentication Enabled</h2>

                    <div style='background-color: #d4edda; border: 1px solid #c3e6cb; padding: 15px; border-radius: 8px; margin: 20px 0;'>
                        <p style='margin: 0; color: #155724;'>
                            <strong>Great news!</strong> Two-factor authentication has been successfully enabled for your DisasterWatch account.
                        </p>
                    </div>

                    <div style='background-color: #fff; padding: 20px; border-radius: 8px; margin: 20px 0;'>
                        <h3 style='color: #333; margin-top: 0;'>What this means:</h3>
                        <ul style='color: #666;'>
                            <li>Your account is now more secure</li>
                            <li>You'll need to enter a verification code when logging in</li>
                            <li>You have backup codes for emergency access</li>
                        </ul>

                        <h3 style='color: #333;'>Important reminders:</h3>
                        <ul style='color: #666;'>
                            <li>Keep your backup codes in a safe place</li>
                            <li>Don't share verification codes with anyone</li>
                            <li>You can disable 2FA anytime from your account settings</li>
                        </ul>
                    </div>

                    <p style='color: #666; font-size: 14px; text-align: center; margin-top: 30px;'>
                        If you didn't enable this feature, please contact our support team immediately.<br><br>
                        Best regards,<br>
                        DisasterWatch Security Team
                    </p>
                </div>
            </body>
            </html>";

        return await SendEmailAsync(email, subject, body);
    }

    public async Task<bool> SendTwoFactorDisabledEmailAsync(string email)
    {
        var subject = "Two-Factor Authentication Disabled - DisasterWatch";
        var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                <div style='background-color: #f8f9fa; padding: 20px; border-radius: 8px;'>
                    <h2 style='color: #dc3545; text-align: center;'>🔓 Two-Factor Authentication Disabled</h2>

                    <div style='background-color: #f8d7da; border: 1px solid #f5c6cb; padding: 15px; border-radius: 8px; margin: 20px 0;'>
                        <p style='margin: 0; color: #721c24;'>
                            <strong>Security Notice:</strong> Two-factor authentication has been disabled for your DisasterWatch account.
                        </p>
                    </div>

                    <div style='background-color: #fff; padding: 20px; border-radius: 8px; margin: 20px 0;'>
                        <h3 style='color: #333; margin-top: 0;'>What this means:</h3>
                        <ul style='color: #666;'>
                            <li>Your account security has been reduced</li>
                            <li>You'll only need your password to log in</li>
                            <li>All backup codes have been invalidated</li>
                        </ul>

                        <h3 style='color: #333;'>We recommend:</h3>
                        <ul style='color: #666;'>
                            <li>Re-enabling two-factor authentication for better security</li>
                            <li>Using a strong, unique password</li>
                            <li>Monitoring your account for suspicious activity</li>
                        </ul>
                    </div>

                    <p style='color: #666; font-size: 14px; text-align: center; margin-top: 30px;'>
                        If you didn't disable this feature, please contact our support team immediately and change your password.<br><br>
                        Best regards,<br>
                        DisasterWatch Security Team
                    </p>
                </div>
            </body>
            </html>";

        return await SendEmailAsync(email, subject, body);
    }

    public async Task<bool> SendBackupCodeUsedEmailAsync(string email, int remainingCodes)
    {
        var subject = "Backup Code Used - DisasterWatch";
        var urgencyColor = remainingCodes <= 2 ? "#dc3545" : remainingCodes <= 4 ? "#ffc107" : "#17a2b8";
        var urgencyBg = remainingCodes <= 2 ? "#f8d7da" : remainingCodes <= 4 ? "#fff3cd" : "#d1ecf1";
        var urgencyBorder = remainingCodes <= 2 ? "#f5c6cb" : remainingCodes <= 4 ? "#ffeaa7" : "#bee5eb";

        var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                <div style='background-color: #f8f9fa; padding: 20px; border-radius: 8px;'>
                    <h2 style='color: {urgencyColor}; text-align: center;'>🔑 Backup Code Used</h2>

                    <div style='background-color: {urgencyBg}; border: 1px solid {urgencyBorder}; padding: 15px; border-radius: 8px; margin: 20px 0;'>
                        <p style='margin: 0; color: {urgencyColor};'>
                            <strong>Security Alert:</strong> A backup code was used to access your DisasterWatch account.
                        </p>
                    </div>

                    <div style='background-color: #fff; padding: 20px; border-radius: 8px; margin: 20px 0;'>
                        <h3 style='color: #333; margin-top: 0;'>Account Status:</h3>
                        <ul style='color: #666;'>
                            <li>Backup codes remaining: <strong>{remainingCodes}</strong></li>
                            <li>Time: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</li>
                        </ul>

                        {(remainingCodes <= 2 ? $@"
                        <div style='background-color: #f8d7da; border: 1px solid #f5c6cb; padding: 15px; border-radius: 8px; margin: 15px 0;'>
                            <h4 style='color: #721c24; margin-top: 0;'>⚠️ Urgent Action Required</h4>
                            <p style='margin: 0; color: #721c24;'>
                                You have only {remainingCodes} backup codes remaining. Please generate new backup codes immediately to avoid being locked out of your account.
                            </p>
                        </div>" : remainingCodes <= 4 ? $@"
                        <div style='background-color: #fff3cd; border: 1px solid #ffeaa7; padding: 15px; border-radius: 8px; margin: 15px 0;'>
                            <h4 style='color: #856404; margin-top: 0;'>⚠️ Low Backup Codes</h4>
                            <p style='margin: 0; color: #856404;'>
                                You have {remainingCodes} backup codes remaining. Consider generating new backup codes soon.
                            </p>
                        </div>" : "")}
                    </div>

                    <p style='color: #666; font-size: 14px; text-align: center; margin-top: 30px;'>
                        If you didn't use this backup code, please contact our support team immediately.<br><br>
                        Best regards,<br>
                        DisasterWatch Security Team
                    </p>
                </div>
            </body>
            </html>";

        return await SendEmailAsync(email, subject, body);
    }
}
