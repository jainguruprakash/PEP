using System.ComponentModel.DataAnnotations;

namespace PEPScanner.Domain.Entities
{
    public class OrganizationUser
    {
        public Guid Id { get; set; }
        
        public Guid OrganizationId { get; set; }

        // Navigation property
        public virtual Organization? Organization { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string Role { get; set; } = "User"; // Admin, Manager, ComplianceOfficer, User
        
        [MaxLength(100)]
        public string Department { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string Position { get; set; } = string.Empty;
        
        [MaxLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;
        
        public bool IsActive { get; set; } = true;
        
        public bool IsEmailVerified { get; set; } = false;
        
        public DateTime? LastLoginAtUtc { get; set; }
        
        public DateTime? PasswordChangedAtUtc { get; set; }
        
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAtUtc { get; set; }
        
        [MaxLength(100)]
        public string CreatedBy { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string UpdatedBy { get; set; } = string.Empty;

        // Authentication fields
        [MaxLength(500)]
        public string PasswordHash { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? RefreshToken { get; set; }

        public DateTime? RefreshTokenExpiryTime { get; set; }
        
        // Permissions and settings
        [MaxLength(1000)]
        public string Permissions { get; set; } = string.Empty; // JSON permissions
        
        [MaxLength(100)]
        public string TimeZone { get; set; } = "Asia/Kolkata";
        
        [MaxLength(10)]
        public string Language { get; set; } = "en";
        
        // Navigation properties
        public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    }
}
