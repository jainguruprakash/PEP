using System.ComponentModel.DataAnnotations;

namespace PEPScanner.Domain.Entities
{
    public class Organization
    {
        public Guid Id { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string Code { get; set; } = string.Empty; // Unique organization code
        
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string Type { get; set; } = string.Empty; // Bank, Financial Institution, Corporate, etc.
        
        [MaxLength(100)]
        public string Industry { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string Country { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string State { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string City { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string Address { get; set; } = string.Empty;
        
        [MaxLength(20)]
        public string PostalCode { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string ContactPerson { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string ContactEmail { get; set; } = string.Empty;
        
        [MaxLength(20)]
        public string ContactPhone { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string Website { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string LicenseNumber { get; set; } = string.Empty; // Banking license, etc.
        
        [MaxLength(100)]
        public string RegulatoryBody { get; set; } = string.Empty; // RBI, SEBI, etc.
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAtUtc { get; set; }
        
        [MaxLength(100)]
        public string CreatedBy { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string UpdatedBy { get; set; } = string.Empty;
        
        // Subscription and licensing
        public DateTime? SubscriptionStartDate { get; set; }
        
        public DateTime? SubscriptionEndDate { get; set; }
        
        [MaxLength(50)]
        public string SubscriptionPlan { get; set; } = "Basic"; // Basic, Professional, Enterprise
        
        public int MaxUsers { get; set; } = 10;
        
        public int MaxCustomers { get; set; } = 10000;
        
        public bool IsTrial { get; set; } = false;
        
        // Configuration
        [MaxLength(1000)]
        public string Configuration { get; set; } = string.Empty; // JSON configuration
        
        [MaxLength(100)]
        public string TimeZone { get; set; } = "Asia/Kolkata";
        
        [MaxLength(10)]
        public string Currency { get; set; } = "INR";
        
        [MaxLength(10)]
        public string Language { get; set; } = "en";
        
        // Navigation properties
        public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();
        public virtual ICollection<OrganizationUser> Users { get; set; } = new List<OrganizationUser>();
        public virtual ICollection<OrganizationWatchlist> Watchlists { get; set; } = new List<OrganizationWatchlist>();
        public virtual ICollection<OrganizationConfiguration> Configurations { get; set; } = new List<OrganizationConfiguration>();
    }
}
