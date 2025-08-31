using DisasterApp.Application.DTOs;
using DisasterApp.Domain.Entities;
using DisasterApp.Domain.Enums;

namespace DisasterApp.Application.Services
{
    public interface INotificationService
    {
        Task<Notification> CreateNotificationAsync(Guid userId, Guid disasterReporId, string title, string message, NotificationType type);
        Task<IEnumerable<NotificationDto>> GetAdminNotificationAsync(Guid adminUserId);
        Task<IEnumerable<NotificationDto>> GetUserNotificationAsync(Guid userId);
        Task<bool> MarkAsReadAsync(Guid notificationId);
        Task SendReportSubmittedNotificationAsync(Guid disasterReportId, Guid reporterUserId);
        Task SendReportDecisionNotificationAsync(Guid disasterReportId, ReportStatus decision, string? reason = null);

        Task SendEmailAcceptedNotificationAsync(DisasterReport report);

    }
}