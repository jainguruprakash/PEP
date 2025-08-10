using System.ComponentModel.DataAnnotations;

namespace PEPScanner.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string Role { get; set; } = string.Empty; // Analyst, ComplianceOfficer, Manager, Admin
        
        [Required]
        [MaxLength(100)]
        public string Department { get; set; } = string.Empty; // Compliance, Risk, Operations
        
        [MaxLength(100)]
        public string? ManagerEmail { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        [MaxLength(100)]
        public string? Phone { get; set; }
        
        [MaxLength(200)]
        public string? JobTitle { get; set; }
        
        // Authentication fields (simplified for demo)
        [MaxLength(500)]
        public string? PasswordHash { get; set; }
        
        public DateTime? LastLoginUtc { get; set; }
        
        // Permissions
        public bool CanCreateAlerts { get; set; } = true;
        public bool CanReviewAlerts { get; set; } = false;
        public bool CanApproveAlerts { get; set; } = false;
        public bool CanEscalateAlerts { get; set; } = false;
        public bool CanCloseAlerts { get; set; } = false;
        public bool CanViewAllAlerts { get; set; } = false;
        public bool CanAssignAlerts { get; set; } = false;
        public bool CanGenerateReports { get; set; } = false;
        
        // Audit fields
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
        
        [MaxLength(100)]
        public string CreatedBy { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string UpdatedBy { get; set; } = string.Empty;
        
        // Navigation properties
        public virtual ICollection<Alert> AssignedAlerts { get; set; } = new List<Alert>();
        public virtual ICollection<Alert> ReviewedAlerts { get; set; } = new List<Alert>();
        
        // Helper properties
        public string FullName => $"{FirstName} {LastName}";
        
        public bool IsComplianceOfficer => Role == "ComplianceOfficer";
        public bool IsManager => Role == "Manager";
        public bool IsAnalyst => Role == "Analyst";
        public bool IsAdmin => Role == "Admin";
    }
}
