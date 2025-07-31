namespace DisasterApp.Application.Services.Interfaces;

public interface IEmailService
{
    Task<bool> SendPasswordResetEmailAsync(string email, string resetToken, string resetUrl);
    Task<bool> SendEmailAsync(string to, string subject, string body);
    Task<bool> SendAuthProviderNotificationEmailAsync(string email, string authProvider);
}
