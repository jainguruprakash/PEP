using System.ComponentModel.DataAnnotations;

namespace PEPScanner.Domain.Entities
{
    /// <summary>
    /// Represents a bank's custom PEP or customer list
    /// </summary>
    public class OrganizationCustomList
    {
        [Key]
        public Guid Id { get; set; }
        
        public Guid OrganizationId { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string ListName { get; set; } = string.Empty; // e.g., "Internal PEP List", "Existing Customers", "VIP Clients"
        
        [Required]
        [MaxLength(50)]
        public string ListType { get; set; } = string.Empty; // PEP, Customer, Blacklist, Whitelist, VIP
        
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;
        
        public bool IsActive { get; set; } = true;
        
        public bool IsPrivate { get; set; } = true; // Only accessible to this organization
        
        [MaxLength(50)]
        public string RiskLevel { get; set; } = "Medium"; // Low, Medium, High, Critical
        
        public double MatchThreshold { get; set; } = 0.8; // Minimum similarity score for matches
        
        public bool AutoAlert { get; set; } = true; // Generate alerts on matches
        
        public bool RequireReview { get; set; } = true; // Require manual review
        
        [MaxLength(100)]
        public string ReviewRole { get; set; } = "ComplianceOfficer"; // Role required for review
        
        [MaxLength(50)]
        public string UpdateFrequency { get; set; } = "Manual"; // Manual, Daily, Weekly, Monthly
        
        public DateTime? LastUpdateAtUtc { get; set; }
        
        public DateTime? NextUpdateAtUtc { get; set; }
        
        public int TotalEntries { get; set; } = 0;
        
        public int ActiveEntries { get; set; } = 0;
        
        [MaxLength(100)]
        public string SourceFormat { get; set; } = "Manual"; // Manual, CSV, Excel, JSON, API
        
        [MaxLength(500)]
        public string SourceLocation { get; set; } = string.Empty; // File path, API endpoint, etc.
        
        [MaxLength(1000)]
        public string Configuration { get; set; } = string.Empty; // JSON configuration for custom settings
        
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAtUtc { get; set; }
        
        [MaxLength(100)]
        public string CreatedBy { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string UpdatedBy { get; set; } = string.Empty;
        
        // Navigation properties
        public virtual Organization Organization { get; set; } = null!;
        public virtual ICollection<OrganizationCustomListEntry> Entries { get; set; } = new List<OrganizationCustomListEntry>();
    }
}
