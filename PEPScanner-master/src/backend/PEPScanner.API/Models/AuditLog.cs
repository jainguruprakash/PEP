using System;
using System.ComponentModel.DataAnnotations;

namespace PEPScanner.API.Models
{
    public class AuditLog
    {
        public Guid Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Action { get; set; } = string.Empty; // Create, Read, Update, Delete, Login, Logout, etc.
        
        [Required]
        [MaxLength(100)]
        public string EntityType { get; set; } = string.Empty; // Customer, Alert, WatchlistEntry, etc.
        
        public Guid? EntityId { get; set; }
        
        [MaxLength(100)]
        public string? UserId { get; set; }
        
        [MaxLength(100)]
        public string? UserName { get; set; }
        
        [MaxLength(100)]
        public string? UserRole { get; set; }
        
        [MaxLength(50)]
        public string? IpAddress { get; set; }
        
        [MaxLength(500)]
        public string? UserAgent { get; set; }
        
        [MaxLength(100)]
        public string? SessionId { get; set; }
        
        [MaxLength(1000)]
        public string? OldValues { get; set; } // JSON of old values
        
        [MaxLength(1000)]
        public string? NewValues { get; set; } // JSON of new values
        
        [MaxLength(1000)]
        public string? AdditionalData { get; set; } // JSON of additional context
        
        [MaxLength(50)]
        public string? Severity { get; set; } = "Info"; // Info, Warning, Error, Critical
        
        public bool IsSuccessful { get; set; } = true;
        
        [MaxLength(1000)]
        public string? ErrorMessage { get; set; }
        
        // Timestamp
        public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
        
        // Indexing fields for performance
        public DateTime Date { get; set; } = DateTime.UtcNow.Date;
        public int Hour { get; set; } = DateTime.UtcNow.Hour;
    }
}
