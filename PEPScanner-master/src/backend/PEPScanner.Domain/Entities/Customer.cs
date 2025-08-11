using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PEPScanner.Domain.Entities
{
    public class Customer
    {
        public Guid Id { get; set; }
        
        public Guid OrganizationId { get; set; } // Multi-tenant support
        
        [Required]
        [MaxLength(300)]
        public string FullName { get; set; } = string.Empty;
        
        [MaxLength(1000)]
        public string? AliasNames { get; set; }
        
        public DateTime? DateOfBirth { get; set; }
        
        [MaxLength(100)]
        public string? Nationality { get; set; }
        
        [MaxLength(50)]
        public string? IdentificationNumber { get; set; }
        
        [MaxLength(50)]
        public string? IdentificationType { get; set; } // Aadhaar, PAN, Passport, etc.
        
        [MaxLength(500)]
        public string? Address { get; set; }
        
        [MaxLength(100)]
        public string? City { get; set; }
        
        [MaxLength(100)]
        public string? State { get; set; }
        
        [MaxLength(10)]
        public string? PostalCode { get; set; }
        
        [MaxLength(100)]
        public string? Country { get; set; }
        
        [MaxLength(100)]
        public string? Territory { get; set; } // Geographic or business territory
        
        [MaxLength(200)]
        public string? Occupation { get; set; }
        
        [MaxLength(200)]
        public string? Employer { get; set; }
        
        [MaxLength(100)]
        public string? PhoneNumber { get; set; }
        
        [MaxLength(100)]
        public string? EmailAddress { get; set; }
        
        // Additional properties for compatibility
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? CustomerType { get; set; }
        public string? Status { get; set; }
        public DateTime? OnboardingDate { get; set; }
        
        // PEP and Risk Assessment
        public bool IsPep { get; set; }
        public int RiskScore { get; set; } = 0;
        
        [MaxLength(50)]
        public string RiskLevel { get; set; } = "Low"; // Low, Medium, High, Critical
        
        [MaxLength(200)]
        public string? PepPosition { get; set; }
        
        [MaxLength(100)]
        public string? PepCountry { get; set; }
        
        public DateTime? PepStartDate { get; set; }
        public DateTime? PepEndDate { get; set; }
        
        // Biometric and Image Data
        [MaxLength(500)]
        public string? PhotoUrl { get; set; }
        
        [MaxLength(500)]
        public string? FingerprintData { get; set; }
        
        [MaxLength(500)]
        public string? FaceBiometricData { get; set; }
        
        // Compliance and Monitoring
        public DateTime? LastScreeningDate { get; set; }
        public DateTime? NextScreeningDate { get; set; }
        
        [MaxLength(50)]
        public string ScreeningFrequency { get; set; } = "Monthly"; // Daily, Weekly, Monthly, Quarterly
        
        public bool IsActive { get; set; } = true;
        public bool RequiresEdd { get; set; } = false; // Enhanced Due Diligence
        
        [MaxLength(1000)]
        public string? EddNotes { get; set; }
        
        // Account and Transaction Data
        [MaxLength(50)]
        public string? AccountNumber { get; set; }
        
        [MaxLength(100)]
        public string? AccountType { get; set; }
        
        public decimal? MonthlyTransactionVolume { get; set; }
        public decimal? AverageTransactionAmount { get; set; }

        // Financial Intelligence Properties
        [MaxLength(20)]
        public string? PanNumber { get; set; }

        [MaxLength(20)]
        public string? GstNumber { get; set; }

        [MaxLength(30)]
        public string? CinNumber { get; set; }

        public DateTime? LastMediaScanDate { get; set; }

        // Relationships and Associations
        public ICollection<CustomerRelationship> Relationships { get; set; } = new List<CustomerRelationship>();
        public ICollection<Alert> Alerts { get; set; } = new List<Alert>();
        public ICollection<CustomerDocument> Documents { get; set; } = new List<CustomerDocument>();
        
        // Navigation properties
        public virtual Organization Organization { get; set; } = null!;
        
        // Audit Fields
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
        
        [MaxLength(100)]
        public string? CreatedBy { get; set; }
        
        [MaxLength(100)]
        public string? UpdatedBy { get; set; }
        
        // Soft Delete
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAtUtc { get; set; }
    }
}