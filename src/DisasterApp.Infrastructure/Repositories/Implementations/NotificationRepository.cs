using DisasterApp.Domain.Entities;
using DisasterApp.Domain.Enums;
using DisasterApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DisasterApp.Infrastructure.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly DisasterDbContext _context;
        public NotificationRepository(DisasterDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(Guid userId)
        {
            return await _context.Notifications.Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt).ToListAsync();
        }

        public async Task<IEnumerable<Notification>> GetUnreadNotificationsAsync(Guid userId)
        {
            return await _context.Notifications.Where(n => n.UserId == userId && !n.IsRead).OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
        }
        public async Task<Notification> CreateNotificationAsync(Notification notification)
        {
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            return notification;
        }

        public async Task MarkAsReadAsync(Guid notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> GetUnreadCountAsync(Guid userId)
        {
            return await _context.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        public async Task<IEnumerable<Notification>> GetNotificationsByReportAsync(Guid reportId)
        {
            return await _context.Notifications.Where(n => n.DisasterReportId == reportId).OrderByDescending(n => n.CreatedAt).ToListAsync();
        }

        public async Task<IEnumerable<Notification>> GetAdminNotificationsAsync()
        {
            return await _context.Notifications.Where
                (n => n.Type == NotificationType.ReportApproved  
                || n.Type == NotificationType.ReportRejected
                || n.Type == NotificationType.ReportSubmitted)
                .OrderByDescending(n => n.CreatedAt).ToListAsync();
        }
       

        public async Task<List<User>> GetAdminAsync()
        {
            return await _context.Users
                .Include(u => u.Roles)
                .Where(u => u.Roles.Any(r => r.Name.ToLower() == "admin"))
                .ToListAsync();
        }

        public async Task<List<User>> GetUsersByRolesAsync(string roleName)
        {
            return await _context.Users
                 .Include(u => u.Roles)
                 .Where(u => u.Roles.Any(r => r.Name.ToLower() == roleName.ToLower()))
                 .ToListAsync();
        }
    }
}