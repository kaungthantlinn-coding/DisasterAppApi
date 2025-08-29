using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace DisasterApp.Hubs

{
    [Authorize]
    public class NotificationHub : Hub
    {
        public async Task JoinAdminGroup()
        {
            if (Context.User?.IsInRole("admin") == true)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "AdminNotifications");

            }
        }
        public async Task JoinUserGroup()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
            }

        }
    }
}
