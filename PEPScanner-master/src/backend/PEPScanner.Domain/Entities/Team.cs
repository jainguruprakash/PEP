using System.ComponentModel.DataAnnotations;

namespace PEPScanner.Domain.Entities
{
    public class Team
    {
        public Guid Id { get; set; }
        
        public Guid OrganizationId { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string Department { get; set; } = string.Empty; // Compliance, Risk, Operations
        
        public Guid? TeamLeadId { get; set; }
        
        [MaxLength(100)]
        public string? Territory { get; set; } // North, South, Corporate, Retail
        
        [MaxLength(100)]
        public string? ProductType { get; set; } // Corporate, Retail, Trade, Private
        
        public bool IsActive { get; set; } = true;
        
        public int MaxWorkload { get; set; } = 50; // Max alerts per member
        
        // Audit fields
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
        
        [MaxLength(100)]
        public string CreatedBy { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string UpdatedBy { get; set; } = string.Empty;
        
        // Navigation properties
        public virtual Organization? Organization { get; set; }
        public virtual OrganizationUser? TeamLead { get; set; }
        public virtual ICollection<OrganizationUser> Members { get; set; } = new List<OrganizationUser>();
        public virtual ICollection<Alert> Alerts { get; set; } = new List<Alert>();
    }
}