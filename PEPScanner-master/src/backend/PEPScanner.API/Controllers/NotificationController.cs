using Microsoft.AspNetCore.Mvc;
using PEPScanner.API.Services;

namespace PEPScanner.API.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<NotificationController> _logger;

        public NotificationController(INotificationService notificationService, ILogger<NotificationController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications([FromQuery] string userEmail, [FromQuery] int limit = 50)
        {
            try
            {
                if (string.IsNullOrEmpty(userEmail))
                {
                    return BadRequest(new { error = "User email is required" });
                }

                var notifications = await _notificationService.GetUserNotificationsAsync(userEmail, limit);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching notifications");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpPost("{notificationId}/mark-read")]
        public async Task<IActionResult> MarkAsRead(Guid notificationId, [FromBody] MarkReadRequest request)
        {
            try
            {
                await _notificationService.MarkNotificationAsReadAsync(notificationId, request.UserEmail);
                return Ok(new { message = "Notification marked as read" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification as read");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount([FromQuery] string userEmail)
        {
            try
            {
                if (string.IsNullOrEmpty(userEmail))
                {
                    return BadRequest(new { error = "User email is required" });
                }

                var count = await _notificationService.GetUnreadCountAsync(userEmail);
                return Ok(new { unreadCount = count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching unread count");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }
    }

    public class MarkReadRequest
    {
        public string UserEmail { get; set; } = string.Empty;
    }
}