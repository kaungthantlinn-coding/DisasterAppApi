using DisasterApp.Application.DTOs;
using DisasterApp.Application.Services.Interfaces;
using DisasterApp.Domain.Entities;
using DisasterApp.Domain.Enums;
using DisasterApp.Infrastructure;
using DisasterApp.Infrastructure.Repositories;
using DisasterApp.Infrastructure.Repositories.Interfaces;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.InkML;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace DisasterApp.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notiRepository;
        private readonly IUserRepository _userRepository;
        private readonly IDisasterReportRepository _disasterReportRepository;
        private readonly INotificationHubService _notificationHubService;
        private readonly ILogger<NotificationService> _logger;
        private readonly IEmailService _emailService;
        public NotificationService(
            INotificationRepository notificationRepository,
            IUserRepository userRepository,
            IDisasterReportRepository disasterReportRepository,
            INotificationHubService notificationHubService,
            ILogger<NotificationService> logger,
            IEmailService emailService)
        {
            _notiRepository = notificationRepository;
            _userRepository = userRepository;
            _disasterReportRepository = disasterReportRepository;
            _notificationHubService = notificationHubService;
            _logger = logger;
            _emailService = emailService;
        }

        public async Task<Notification> CreateNotificationAsync(Guid userId, Guid disasterReportId, string title, string message, NotificationType type)
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                DisasterReportId = disasterReportId,
                Title = title,
                Message = message,
                Type = type,

                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };
            await _notiRepository.CreateNotificationAsync(notification);
            var user = await _userRepository.GetByIdAsync(userId);
            var report = await _disasterReportRepository.GetByIdAsync(disasterReportId);

            var dto = new NotificationDto
            {
                Id = notification.Id,
                Title = notification.Title,
                Message = notification.Message,
                Type = notification.Type,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt ?? DateTime.UtcNow,
                ReadAt = notification.ReadAt,
                UserId = notification.UserId,
                UserName = user?.Name ?? "Unknown",
                DisasterReportTitle = report?.Title

            };
            await _notificationHubService.SendNotificationToUser(userId, dto);
            return notification;
        }

        public async Task<IEnumerable<NotificationDto>> GetAdminNotificationAsync(Guid adminUserId)
        {
            var notifications = await _notiRepository.GetAdminNotificationsAsync();
            return notifications.Select(n => new NotificationDto
            {
                Id = n.Id,
                Title = n.Title,
                Message = n.Message,
                Type = n.Type,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt ?? DateTime.UtcNow,
                ReadAt = n.ReadAt,
                UserId = n.UserId,
                UserName = n.User?.Name ?? "Unknown",
                DisasterReportTitle = n.DisasterReport?.Title
            });
        }
        public async Task<IEnumerable<NotificationDto>> GetUserNotificationAsync(Guid userId)
        {
            var notifications = await _notiRepository.GetUserNotificationsAsync(userId);
            return notifications.Select(n => new NotificationDto
            {
                Id = n.Id,
                Title = n.Title,
                Message = n.Message,
                Type = n.Type,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt ?? DateTime.UtcNow,
                ReadAt = n.ReadAt,
                UserId = n.UserId,
                UserName = n.User?.Name ?? "Unknown",
                DisasterReportTitle = n.DisasterReport?.Title
            });
        }
        public async Task<bool> MarkAsReadAsync(Guid notificationId)
        {
            try
            {
                await _notiRepository.MarkAsReadAsync(notificationId);
                return true;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error occurred while marking notification as read. NotificationId: {NotificationId}", notificationId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while marking notification as read. NotificationId: {NotificationId}", notificationId);
                return false;
            }
        }


        public async Task SendReportSubmittedNotificationAsync(Guid disasterReportId, Guid reporterUserId)
        {
            var title = "Report Submitted";
            var message = "Disaster report has been submitted and is pending review.";
            await CreateNotificationAsync(
                reporterUserId,
                disasterReportId,
                title,
                message,
                NotificationType.ReportSubmitted);
        }

        public async Task SendReportDecisionNotificationAsync(Guid disasterReportId, ReportStatus decision, string? reason = null)
        {
            var report = await _disasterReportRepository.GetByIdAsync(disasterReportId);
            if (report == null)
            {
                _logger.LogWarning("Report not found for notification. ReportId: {ReportId}", disasterReportId);
                return;
            }

            var title = decision == ReportStatus.Verified ? "Report Approved" : "Report Rejected";
            var message = decision == ReportStatus.Verified ? $"Your disaster report `{report.Title}` has been approved."
            : $"Your disaster report `{report.Title}` has been rejected.{(reason != null ? $"Reason : {reason}" : "")}";

            var type = decision == ReportStatus.Verified
                ? NotificationType.ReportApproved
                : NotificationType.ReportRejected;

            await CreateNotificationAsync(
               report.UserId,
               disasterReportId,
               title,
               message,
               type);

        }

        private static readonly HashSet<Guid> _notifiedEvents = new HashSet<Guid>();

        public async Task SendEmailAcceptedNotificationAsync(DisasterReport report)
        {

            // DisasterEvent ??????? ??????
            if (report.DisasterEvent == null) return;

            // ???? DisasterEventId ??? email ?????????????? ??????
            if (_notifiedEvents.Contains(report.DisasterEvent.Id))
            {
                Console.WriteLine($"?? Skipping email: DisasterEvent '{report.DisasterEvent.Name}' already notified.");
                return;
            }
            var users = await _userRepository.GetAllUsersAsync();
            foreach (var user in users)
            {
                var subject = $"?? Disaster Confirmed: {report.Title}";
                var body = $@"
                    <h2>Disaster Alert: {report.Title}</h2>
<p><b>Disaster :</b> {report.DisasterEvent.Name}</p>
<p><b>Description :</b> {report.Description}</p>
<p><b>Severity :</b> {report.Severity}</p>
<p><b>Location :</b> {report.Location?.Address ?? "N/A"}</p>
<p><b>DateTime :</b> {report.Timestamp.ToString("f")}</p>
            <p><a href='https://localhost:5173/reports/{report.Id}'>View Details</a></p>";

                await _emailService.SendEmailAsync(user.Email, subject, body);
            }
        }
    }
}








