using DisasterApp.Application.DTOs;
using DisasterApp.Application.Services.Interfaces;
using DisasterApp.Domain.Entities;
using DisasterApp.Domain.Enums;
using DisasterApp.Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using DisasterApp.Hubs; 

namespace DisasterApp.Application.Services
{
    public class NotificationHubService : INotificationHubService
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationHubService(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        // Send notification to a specific user
        public async Task SendNotificationToUser(Guid userId, NotificationDto notification)
        {
            await _hubContext.Clients.Group($"User_{userId}")
                                     .SendAsync("ReceiveNotification", notification);
        }

        // Send notification to a group (e.g., admin group)
        public async Task SendNotificationToGroup(string groupName, NotificationDto notification)
        {
            await _hubContext.Clients.Group(groupName)
                                     .SendAsync("ReceiveNotification", notification);
        }
    }
}