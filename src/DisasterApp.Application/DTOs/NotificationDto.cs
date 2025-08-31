using DisasterApp.Domain.Enums;
using DocumentFormat.OpenXml.Bibliography;

namespace DisasterApp.Application.DTOs
{
    public class NotificationDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string Message { get; set; } = null!;
        public NotificationType Type { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReadAt { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string? DisasterReportTitle { get; set; }
    }

    public class CreateNotificationDto
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public NotificationType Type { get; set; }
        public Guid UserId { get; set; }
        public Guid DisasterReportId { get; set; }
    }
    public class UpdateNotificationDto
    {
        public string? Title { get; set; }
        public string? Message { get; set; }
        public bool? IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public NotificationType? Type { get; set; }
    }
}