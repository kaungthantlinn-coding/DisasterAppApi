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

            // Add detailed logging for debugging
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

            // Validate configuration
            if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(senderEmail) ||
                string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                _logger.LogWarning("Email configuration is incomplete. Logging email instead of sending.");
                _logger.LogInformation("Email would be sent to: {Email}", to);
                _logger.LogInformation("Subject: {Subject}", subject);
                _logger.LogInformation("Body: {Body}", body);
                return true; // Return true for development
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

            // Fallback: Log the email content for development
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
}
