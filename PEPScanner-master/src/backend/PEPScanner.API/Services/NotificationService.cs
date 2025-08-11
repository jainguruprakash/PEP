using Microsoft.EntityFrameworkCore;
using PEPScanner.Infrastructure.Data;
using PEPScanner.Domain.Entities;

namespace PEPScanner.API.Services
{
    public interface INotificationService
    {
        Task CreateAlertNotificationAsync(Alert alert);
        Task CreateCustomerNotificationAsync(Customer customer, string action);
        Task<List<NotificationDto>> GetUserNotificationsAsync(string userEmail, int limit = 50);
        Task MarkNotificationAsReadAsync(Guid notificationId, string userEmail);
        Task<int> GetUnreadCountAsync(string userEmail);
    }

    public class NotificationService : INotificationService
    {
        private readonly PepScannerDbContext _context;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(PepScannerDbContext context, ILogger<NotificationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task CreateAlertNotificationAsync(Alert alert)
        {
            try
            {
                var customer = await _context.Customers.FindAsync(alert.CustomerId);
                var customerName = customer != null ? $"{customer.FirstName} {customer.LastName}" : "Unknown Customer";

                // Find appropriate user to assign based on priority and hierarchy
                var targetUser = await GetTargetUserForAlert(alert);
                
                var notification = new Notification
                {
                    Id = Guid.NewGuid(),
                    Type = "Alert",
                    Title = $"New {alert.AlertType} Alert",
                    Message = $"Alert created for {customerName} with {alert.Priority} priority",
                    Data = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        alertId = alert.Id,
                        customerId = alert.CustomerId,
                        alertType = alert.AlertType,
                        priority = alert.Priority,
                        customerName
                    }),
                    TargetUserRole = targetUser?.Role ?? GetTargetRole(alert),
                    TargetUserEmail = targetUser?.Email,
                    IsRead = false,
                    CreatedAtUtc = DateTime.UtcNow
                };

                // Auto-assign to the target user if found
                if (targetUser != null)
                {
                    alert.AssignedTo = targetUser.Id.ToString();
                    alert.CurrentReviewer = targetUser.Id.ToString();
                    alert.WorkflowStatus = "PendingReview";
                }

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Notification created for alert: {AlertId}, assigned to: {UserId}", alert.Id, targetUser?.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating alert notification");
            }
        }

        public async Task CreateCustomerNotificationAsync(Customer customer, string action)
        {
            try
            {
                var notification = new Notification
                {
                    Id = Guid.NewGuid(),
                    Type = "Customer",
                    Title = $"Customer {action}",
                    Message = $"{customer.FirstName} {customer.LastName} has been {action.ToLower()}",
                    Data = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        customerId = customer.Id,
                        customerName = $"{customer.FirstName} {customer.LastName}",
                        action = action
                    }),
                    TargetUserRole = "All",
                    IsRead = false,
                    CreatedAtUtc = DateTime.UtcNow
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Notification created for customer action: {CustomerId} - {Action}", customer.Id, action);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating customer notification");
            }
        }

        public async Task<List<NotificationDto>> GetUserNotificationsAsync(string userEmail, int limit = 50)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
                if (user == null) return new List<NotificationDto>();

                var notifications = await _context.Notifications
                    .Where(n => n.TargetUserRole == "All" || n.TargetUserRole == user.Role || n.TargetUserEmail == userEmail)
                    .OrderByDescending(n => n.CreatedAtUtc)
                    .Take(limit)
                    .Select(n => new NotificationDto
                    {
                        Id = n.Id,
                        Type = n.Type,
                        Title = n.Title,
                        Message = n.Message,
                        Data = n.Data,
                        IsRead = n.IsRead,
                        CreatedAtUtc = n.CreatedAtUtc
                    })
                    .ToListAsync();

                return notifications;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user notifications");
                return new List<NotificationDto>();
            }
        }

        public async Task MarkNotificationAsReadAsync(Guid notificationId, string userEmail)
        {
            try
            {
                var notification = await _context.Notifications.FindAsync(notificationId);
                if (notification != null)
                {
                    notification.IsRead = true;
                    notification.ReadAtUtc = DateTime.UtcNow;
                    notification.ReadByUserEmail = userEmail;
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification as read");
            }
        }

        public async Task<int> GetUnreadCountAsync(string userEmail)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
                if (user == null) return 0;

                return await _context.Notifications
                    .CountAsync(n => !n.IsRead && 
                        (n.TargetUserRole == "All" || n.TargetUserRole == user.Role || n.TargetUserEmail == userEmail));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread count");
                return 0;
            }
        }

        private async Task<OrganizationUser?> GetTargetUserForAlert(Alert alert)
        {
            try
            {
                var targetRole = GetTargetRole(alert);
                
                // Find an active user with the target role
                var targetUser = await _context.OrganizationUsers
                    .Where(u => u.Role == targetRole && u.IsActive)
                    .OrderBy(u => u.CreatedAtUtc) // Round-robin or first available
                    .FirstOrDefaultAsync();

                return targetUser;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding target user for alert");
                return null;
            }
        }

        private string GetTargetRole(Alert alert)
        {
            return alert.Priority switch
            {
                "Critical" => "Manager",
                "High" => "ComplianceOfficer",
                _ => "Analyst"
            };
        }
    }

    public class NotificationDto
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Data { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}