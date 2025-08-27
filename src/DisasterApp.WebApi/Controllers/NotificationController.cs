using DisasterApp.Application.DTOs;
using DisasterApp.Application.Services;
using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DisasterApp.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }
        // Get Admin notifications(only for Admin Role)
        [HttpGet("admin")]
        public async Task<IActionResult> GetAdminNotifications()
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            

            var notifications = await _notificationService.GetAdminNotificationAsync(userId);
            return Ok(notifications);
        }

        //Get Currengt User Notifications
        [HttpGet("user")]
        public async Task<IActionResult> GetUserNotifications()
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var notifications = await _notificationService.GetUserNotificationAsync(userId);
            return Ok(notifications);
        }
        // Mark Notification as Read
        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            var result = await _notificationService.MarkAsReadAsync(id);
            if (!result) return NotFound();
            return Ok();
        }

        // Create notification (admin use case)
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<NotificationDto>> CreateNotification([FromBody] CreateNotificationDto dto)
        {
            var notification = await _notificationService.CreateNotificationAsync(
                dto.UserId,
                dto.DisasterReportId,
                dto.Title,
                dto.Message,
                dto.Type
            );

            var result = new NotificationDto
            {
                Id = notification.Id,
                Title = notification.Title,
                Message = notification.Message,
                Type = notification.Type,
                CreatedAt = notification.CreatedAt??DateTime.UtcNow,
                IsRead = notification.IsRead,
                UserId = notification.UserId
            };

            return CreatedAtAction(nameof(GetUserNotifications), new { id = result.Id }, result);
        }

        // ✅ Mark all notifications as read (for current user)
        [HttpPut("read-all")]
        public async Task<ActionResult> MarkAllAsRead()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            var userId = Guid.Parse(userIdClaim);

            // get user notifications
            var notifications = await _notificationService.GetUserNotificationAsync(userId);
            foreach (var n in notifications)
            {
                await _notificationService.MarkAsReadAsync(n.Id);
            }

            return NoContent();
        }

        //[HttpGet("unread-count")]
        //public async Task<IActionResult> GetUnreadCount()
        //{
        //    var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        //    var count = await _notificationService.GetUnreadCountAsync(userId);
        //    return Ok(new { count });
        //}
    }
}
