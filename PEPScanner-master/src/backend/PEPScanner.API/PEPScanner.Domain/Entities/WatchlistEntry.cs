using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PEPScanner.Domain.Entities
{
    public class WatchlistEntry
    {
        public Guid Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Source { get; set; } = string.Empty; // OFAC, UN, EU, RBI, FIU-IND, SEBI, etc.
        
        [Required]
        [MaxLength(100)]
        public string ListType { get; set; } = string.Empty; // PEP, Sanctions, Adverse Media, Local Lists
        
        [Required]
        [MaxLength(300)]
        public string PrimaryName { get; set; } = string.Empty;
        
        [MaxLength(1000)]
        public string? AlternateNames { get; set; }
        
        [MaxLength(100)]
        public string? Country { get; set; }
        
        [MaxLength(50)]
        public string? Gender { get; set; }
        
        public DateTime? DateOfBirth { get; set; }
        public DateTime? DateOfBirthFrom { get; set; }
        public DateTime? DateOfBirthTo { get; set; }
        
        [MaxLength(200)]
        public string? PositionOrRole { get; set; }
        
        [MaxLength(100)]
        public string? RiskCategory { get; set; } // High, Medium, Low
        
        [MaxLength(100)]
        public string? RiskLevel { get; set; } // Critical, High, Medium, Low
        
        [MaxLength(100)]
        public string? EntityType { get; set; } // Individual, Organization, Vessel, Aircraft
        
        [MaxLength(100)]
        public string? Nationality { get; set; }
        
        [MaxLength(100)]
        public string? Citizenship { get; set; }
        
        [MaxLength(500)]
        public string? Address { get; set; }
        
        [MaxLength(100)]
        public string? City { get; set; }
        
        [MaxLength(100)]
        public string? State { get; set; }
        
        [MaxLength(100)]
        public string? PostalCode { get; set; }
        
        [MaxLength(100)]
        public string? CountryOfResidence { get; set; }
        
        [MaxLength(100)]
        public string? PlaceOfBirth { get; set; }
        
        [MaxLength(100)]
        public string? PassportNumber { get; set; }
        
        [MaxLength(100)]
        public string? NationalIdNumber { get; set; }
        
        [MaxLength(100)]
        public string? TaxIdNumber { get; set; }
        
        [MaxLength(100)]
        public string? RegistrationNumber { get; set; }
        
        [MaxLength(100)]
        public string? LicenseNumber { get; set; }
        
        [MaxLength(100)]
        public string? VesselName { get; set; }
        
        [MaxLength(100)]
        public string? VesselType { get; set; }
        
        [MaxLength(100)]
        public string? VesselFlag { get; set; }
        
        [MaxLength(100)]
        public string? VesselCallSign { get; set; }
        
        [MaxLength(100)]
        public string? VesselImoNumber { get; set; }
        
        [MaxLength(100)]
        public string? AircraftType { get; set; }
        
        [MaxLength(100)]
        public string? AircraftRegistration { get; set; }
        
        [MaxLength(100)]
        public string? AircraftOperator { get; set; }
        
        // PEP Specific Fields
        [MaxLength(200)]
        public string? PepPosition { get; set; }
        
        [MaxLength(100)]
        public string? PepCountry { get; set; }
        
        public DateTime? PepStartDate { get; set; }
        public DateTime? PepEndDate { get; set; }
        
        [MaxLength(100)]
        public string? PepCategory { get; set; } // Domestic, Foreign, International Organization
        
        [MaxLength(1000)]
        public string? PepDescription { get; set; }
        
        // Sanctions Specific Fields
        [MaxLength(100)]
        public string? SanctionType { get; set; } // Asset Freeze, Travel Ban, Arms Embargo, etc.
        
        [MaxLength(100)]
        public string? SanctionAuthority { get; set; }
        
        [MaxLength(100)]
        public string? SanctionReference { get; set; }
        
        public DateTime? SanctionStartDate { get; set; }
        public DateTime? SanctionEndDate { get; set; }
        
        [MaxLength(1000)]
        public string? SanctionReason { get; set; }
        
        // Adverse Media Fields
        [MaxLength(100)]
        public string? MediaSource { get; set; }
        
        public DateTime? MediaDate { get; set; }
        
        [MaxLength(1000)]
        public string? MediaSummary { get; set; }
        
        [MaxLength(500)]
        public string? MediaUrl { get; set; }
        
        [MaxLength(100)]
        public string? MediaCategory { get; set; } // Corruption, Money Laundering, Terrorism, etc.
        
        // Additional Metadata
        [MaxLength(100)]
        public string? ExternalId { get; set; } // ID from source system
        
        [MaxLength(100)]
        public string? ExternalReference { get; set; }
        
        [MaxLength(1000)]
        public string? Comments { get; set; }
        
        [MaxLength(100)]
        public string? DataQuality { get; set; } // High, Medium, Low
        
        public bool IsActive { get; set; } = true;
        public bool IsWhitelisted { get; set; } = false; // For false positive management
        
        public DateTime DateAddedUtc { get; set; } = DateTime.UtcNow;
        public DateTime? DateRemovedUtc { get; set; }
        public DateTime? DateLastUpdatedUtc { get; set; }
        
        [MaxLength(100)]
        public string? AddedBy { get; set; }
        
        [MaxLength(100)]
        public string? RemovedBy { get; set; }
        
        [MaxLength(100)]
        public string? UpdatedBy { get; set; }
        
        // Navigation Properties
        public ICollection<Alert> Alerts { get; set; } = new List<Alert>();
    }
}
