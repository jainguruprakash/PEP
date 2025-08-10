using System.ComponentModel.DataAnnotations;

namespace PEPScanner.Domain.Entities
{
    public class AlertAction
    {
        public Guid Id { get; set; }
        
        public Guid AlertId { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string ActionType { get; set; } = string.Empty; // Created, Assigned, Reviewed, Approved, Rejected, Escalated, Closed, Reopened
        
        [Required]
        [MaxLength(100)]
        public string PerformedBy { get; set; } = string.Empty; // User email or system
        
        [MaxLength(50)]
        public string? PreviousStatus { get; set; }
        
        [MaxLength(50)]
        public string? NewStatus { get; set; }
        
        [MaxLength(100)]
        public string? PreviousAssignee { get; set; }
        
        [MaxLength(100)]
        public string? NewAssignee { get; set; }
        
        [MaxLength(1000)]
        public string? Comments { get; set; }
        
        [MaxLength(1000)]
        public string? Reason { get; set; } // Reason for rejection, escalation, etc.
        
        [MaxLength(500)]
        public string? AdditionalData { get; set; } // JSON for any additional context
        
        public DateTime ActionDateUtc { get; set; } = DateTime.UtcNow;
        
        [MaxLength(100)]
        public string? IpAddress { get; set; }
        
        [MaxLength(500)]
        public string? UserAgent { get; set; }
        
        // Navigation properties
        public virtual Alert Alert { get; set; } = null!;
    }
}
