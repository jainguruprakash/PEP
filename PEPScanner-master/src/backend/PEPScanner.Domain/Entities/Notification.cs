using System.ComponentModel.DataAnnotations;

namespace PEPScanner.Domain.Entities
{
    public class Notification
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = string.Empty; // Alert, Customer, System, etc.

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(1000)]
        public string Message { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Data { get; set; } // JSON data for additional context

        [MaxLength(100)]
        public string? TargetUserEmail { get; set; } // Specific user email

        [MaxLength(50)]
        public string? TargetUserRole { get; set; } // Role-based targeting

        public bool IsRead { get; set; } = false;

        public DateTime? ReadAtUtc { get; set; }

        [MaxLength(100)]
        public string? ReadByUserEmail { get; set; }

        [MaxLength(50)]
        public string Priority { get; set; } = "Medium"; // Low, Medium, High, Critical

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public DateTime? ExpiresAtUtc { get; set; }

        [MaxLength(100)]
        public string? CreatedBy { get; set; }
    }
}