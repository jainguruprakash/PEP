using System.ComponentModel.DataAnnotations;

namespace PEPScanner.API.Models
{
    public class OrganizationConfiguration
    {
        public Guid Id { get; set; }
        
        public Guid OrganizationId { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Category { get; set; } = string.Empty; // Screening, Alerting, Reporting, etc.
        
        [Required]
        [MaxLength(100)]
        public string Key { get; set; } = string.Empty;
        
        [MaxLength(1000)]
        public string Value { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string DataType { get; set; } = "String"; // String, Number, Boolean, JSON
        
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;
        
        public bool IsRequired { get; set; } = false;
        
        public bool IsEncrypted { get; set; } = false;
        
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAtUtc { get; set; }
        
        [MaxLength(100)]
        public string CreatedBy { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string UpdatedBy { get; set; } = string.Empty;
        
        // Navigation properties
        public virtual Organization Organization { get; set; } = null!;
    }
}
