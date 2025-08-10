using System.ComponentModel.DataAnnotations;

namespace PEPScanner.Domain.Entities
{
    /// <summary>
    /// Individual entry in a bank's custom list
    /// </summary>
    public class OrganizationCustomListEntry
    {
        [Key]
        public Guid Id { get; set; }
        
        public Guid CustomListId { get; set; }
        
        public Guid OrganizationId { get; set; }
        
        [MaxLength(50)]
        public string ExternalId { get; set; } = string.Empty; // Bank's internal ID
        
        [Required]
        [MaxLength(200)]
        public string PrimaryName { get; set; } = string.Empty;
        
        [MaxLength(1000)]
        public string AlternateNames { get; set; } = string.Empty; // Comma-separated or JSON
        
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string MiddleName { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;
        
        public DateTime? DateOfBirth { get; set; }
        
        [MaxLength(100)]
        public string PlaceOfBirth { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string Nationality { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string Country { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string Address { get; set; } = string.Empty;
        
        [MaxLength(20)]
        public string PostalCode { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string City { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string State { get; set; } = string.Empty;
        
        [MaxLength(200)]
        public string PositionOrRole { get; set; } = string.Empty;
        
        [MaxLength(200)]
        public string OrganizationName { get; set; } = string.Empty;
        
        [MaxLength(50)]
        public string Gender { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string IdType { get; set; } = string.Empty; // PAN, Aadhaar, Passport, etc.
        
        [MaxLength(100)]
        public string IdNumber { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string PhoneNumber { get; set; } = string.Empty;
        
        [MaxLength(200)]
        public string EmailAddress { get; set; } = string.Empty;
        
        [MaxLength(50)]
        public string RiskCategory { get; set; } = "Medium"; // Low, Medium, High, Critical
        
        [MaxLength(50)]
        public string EntryType { get; set; } = string.Empty; // PEP, Customer, Blacklist, Whitelist, VIP
        
        [MaxLength(100)]
        public string CustomerType { get; set; } = string.Empty; // Individual, Corporate, Trust, etc.
        
        [MaxLength(100)]
        public string RelationshipType { get; set; } = string.Empty; // Existing, Prospect, Former, etc.
        
        public DateTime? RelationshipStartDate { get; set; }
        
        public DateTime? RelationshipEndDate { get; set; }
        
        [MaxLength(100)]
        public string AccountNumbers { get; set; } = string.Empty; // Comma-separated account numbers
        
        public decimal? TotalBalance { get; set; }
        
        [MaxLength(50)]
        public string Currency { get; set; } = "INR";
        
        public bool IsActive { get; set; } = true;
        
        public bool IsVerified { get; set; } = false;
        
        [MaxLength(500)]
        public string Notes { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string Tags { get; set; } = string.Empty; // Comma-separated tags
        
        [MaxLength(1000)]
        public string AdditionalData { get; set; } = string.Empty; // JSON for custom fields
        
        public DateTime DateAddedUtc { get; set; } = DateTime.UtcNow;
        
        public DateTime? LastUpdatedUtc { get; set; }
        
        public DateTime? LastVerifiedUtc { get; set; }
        
        [MaxLength(100)]
        public string AddedBy { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string UpdatedBy { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string VerifiedBy { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string Source { get; set; } = "Manual"; // Manual, Import, API, etc.
        
        [MaxLength(200)]
        public string SourceReference { get; set; } = string.Empty; // File name, API call ID, etc.
        
        // Navigation properties
        public virtual OrganizationCustomList CustomList { get; set; } = null!;
        public virtual Organization Organization { get; set; } = null!;
    }
}
