using System.ComponentModel.DataAnnotations;

namespace PEPScanner.Domain.Entities
{
    public class OrganizationWatchlist
    {
        public Guid Id { get; set; }
        
        public Guid OrganizationId { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string WatchlistSource { get; set; } = string.Empty; // OFAC, UN, RBI, SEBI, etc.
        
        [MaxLength(100)]
        public string WatchlistType { get; set; } = string.Empty; // PEP, Sanctions, Adverse Media
        
        public bool IsEnabled { get; set; } = true;
        
        public bool IsRequired { get; set; } = false; // Mandatory for compliance
        
        public int Priority { get; set; } = 1; // Priority order for screening
        
        public double MatchThreshold { get; set; } = 0.8; // Minimum similarity score
        
        [MaxLength(100)]
        public string RiskLevel { get; set; } = "Medium"; // Low, Medium, High, Critical
        
        public bool AutoAlert { get; set; } = true; // Auto-generate alerts on matches
        
        public bool RequireReview { get; set; } = true; // Require manual review
        
        [MaxLength(100)]
        public string ReviewRole { get; set; } = "ComplianceOfficer"; // Role required for review
        
        public DateTime? LastUpdateAtUtc { get; set; }
        
        public DateTime? NextUpdateAtUtc { get; set; }
        
        [MaxLength(50)]
        public string UpdateFrequency { get; set; } = "Daily"; // Daily, Weekly, Monthly
        
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAtUtc { get; set; }
        
        [MaxLength(100)]
        public string CreatedBy { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string UpdatedBy { get; set; } = string.Empty;
        
        // Configuration
        [MaxLength(1000)]
        public string Configuration { get; set; } = string.Empty; // JSON configuration
        
        // Navigation properties
        public virtual Organization Organization { get; set; } = null!;
    }
}
