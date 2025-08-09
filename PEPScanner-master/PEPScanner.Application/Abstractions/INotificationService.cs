using PEPScanner.Domain.Entities;

namespace PEPScanner.Application.Abstractions
{
    public interface INotificationService
    {
        Task SendAlertNotificationAsync(Alert alert);
        Task SendBatchNotificationAsync(List<Alert> alerts);
        Task SendCustomNotificationAsync(string recipient, string subject, string message, NotificationType type);
        Task<List<NotificationTemplate>> GetNotificationTemplatesAsync();
        Task<NotificationResult> ProcessNotificationQueueAsync();
    }

    public enum NotificationType
    {
        Email,
        SMS,
        InApp,
        Webhook,
        Slack,
        Teams
    }

    public class NotificationTemplate
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Template { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }

    public class NotificationResult
    {
        public int TotalProcessed { get; set; }
        public int Successful { get; set; }
        public int Failed { get; set; }
        public List<string> Errors { get; set; } = new();
        public DateTime ProcessedDate { get; set; } = DateTime.UtcNow;
    }
}
