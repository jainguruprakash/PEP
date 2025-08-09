using System.ComponentModel.DataAnnotations;

namespace PEPScanner.Domain.Entities
{
    public class NotificationLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public Guid? AlertId { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string NotificationType { get; set; } = string.Empty; // Email, SMS, Webhook, Alert
        
        [MaxLength(1000)]
        public string Payload { get; set; } = string.Empty; // JSON payload
        
        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = string.Empty; // Sent, Failed, Pending
        
        [MaxLength(500)]
        public string? ErrorMessage { get; set; }
        
        [MaxLength(100)]
        public string? Recipient { get; set; } // Email address, phone number, or webhook URL
        
        [MaxLength(200)]
        public string? Subject { get; set; } // For email notifications
        
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        
        public DateTime? SentAtUtc { get; set; }
        
        public int RetryCount { get; set; } = 0;
        
        [MaxLength(100)]
        public string? CreatedBy { get; set; }
    }
}
