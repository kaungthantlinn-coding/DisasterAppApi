namespace DisasterApp.Application.Services.Interfaces;

public interface IEmailService
{
    Task<bool> SendPasswordResetEmailAsync(string email, string resetToken, string resetUrl);
    Task<bool> SendEmailAsync(string to, string subject, string body);
    Task<bool> SendAuthProviderNotificationEmailAsync(string email, string authProvider);

    // Two-Factor Authentication email methods
    Task<bool> SendOtpEmailAsync(string email, string otpCode);
    Task<bool> SendTwoFactorEnabledEmailAsync(string email);
    Task<bool> SendTwoFactorDisabledEmailAsync(string email);
    Task<bool> SendBackupCodeUsedEmailAsync(string email, int remainingCodes);
}
