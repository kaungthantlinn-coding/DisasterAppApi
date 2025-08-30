using DisasterApp.Application.DTOs;
using DisasterApp.Domain.Entities;
using DisasterApp.Domain.Enums;

namespace DisasterApp.Application.Services
{
    public interface INotificationHubService
    {
        Task SendNotificationToUser(Guid userId, NotificationDto notification);
        Task SendNotificationToGroup(string groupName, NotificationDto notification);
    }

}