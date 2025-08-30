using DisasterApp.Domain.Entities;

namespace DisasterApp.Infrastructure.Repositories
{
    public interface INotificationRepository
    {
        Task<IEnumerable<Notification>> GetUserNotificationsAsync(Guid userId);
        Task<IEnumerable<Notification>> GetUnreadNotificationsAsync(Guid userId);
        Task<Notification> CreateNotificationAsync(Notification notification);
        Task MarkAsReadAsync(Guid notificationId);
        Task<int> GetUnreadCountAsync(Guid userId);
        Task<IEnumerable<Notification>> GetNotificationsByReportAsync(Guid reportId);
        Task<IEnumerable<Notification>> GetAdminNotificationsAsync();
        Task<List<User>> GetAdminAsync();
        Task<List<User>> GetUsersByRolesAsync(string roleName);
    }
}