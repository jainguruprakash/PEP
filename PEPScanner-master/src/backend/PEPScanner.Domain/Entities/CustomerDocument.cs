using System;
using System.ComponentModel.DataAnnotations;

namespace PEPScanner.Domain.Entities
{
    public class CustomerDocument
    {
        public Guid Id { get; set; }
        
        public Guid CustomerId { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string DocumentType { get; set; } = string.Empty; // Aadhaar, PAN, Passport, Driving License, etc.
        
        [Required]
        [MaxLength(100)]
        public string DocumentNumber { get; set; } = string.Empty;
        
        [MaxLength(200)]
        public string? DocumentName { get; set; }
        
        [MaxLength(500)]
        public string? DocumentUrl { get; set; }
        
        [MaxLength(50)]
        public string? DocumentFormat { get; set; } // PDF, JPG, PNG, etc.
        
        public DateTime? IssueDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        
        [MaxLength(100)]
        public string? IssuingAuthority { get; set; }
        
        [MaxLength(100)]
        public string? IssuingCountry { get; set; }
        
        public bool IsVerified { get; set; } = false;
        
        [MaxLength(100)]
        public string? VerifiedBy { get; set; }
        
        public DateTime? VerifiedAtUtc { get; set; }
        
        [MaxLength(1000)]
        public string? VerificationNotes { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        // Audit Fields
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
        
        [MaxLength(100)]
        public string? CreatedBy { get; set; }
        
        [MaxLength(100)]
        public string? UpdatedBy { get; set; }
        
        // Navigation Property
        public Customer Customer { get; set; } = null!;
    }
}
