using System;
using System.ComponentModel.DataAnnotations;

namespace PEPScanner.Domain.Entities
{
    public class Alert
    {
        public Guid Id { get; set; }
        
        public Guid? CustomerId { get; set; }
        public Guid? WatchlistEntryId { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Context { get; set; } = string.Empty; // Onboarding, Batch, RealTime, Transaction, Periodic
        
        [Required]
        [MaxLength(100)]
        public string AlertType { get; set; } = string.Empty; // PEP, Sanctions, Adverse Media, Name Similarity
        
        public double SimilarityScore { get; set; }
        
        [MaxLength(100)]
        public string MatchAlgorithm { get; set; } = string.Empty; // Levenshtein, JaroWinkler, Soundex, etc.
        
        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Open"; // Open, UnderReview, Escalated, Closed, FalsePositive
        
        [MaxLength(50)]
        public string Priority { get; set; } = "Medium"; // Low, Medium, High, Critical
        
        [MaxLength(100)]
        public string? AssignedTo { get; set; }
        
        [MaxLength(100)]
        public string? ReviewedBy { get; set; }
        
        public DateTime? ReviewedAtUtc { get; set; }
        
        [MaxLength(100)]
        public string? EscalatedTo { get; set; }
        
        public DateTime? EscalatedAtUtc { get; set; }
        
        [MaxLength(1000)]
        public string? OutcomeNotes { get; set; }
        
        [MaxLength(50)]
        public string? Outcome { get; set; } // Confirmed, FalsePositive, RequiresInvestigation, Escalated
        
        [MaxLength(100)]
        public string? RiskLevel { get; set; } // Low, Medium, High, Critical
        
        [MaxLength(100)]
        public string? ComplianceAction { get; set; } // EDD, STR, SAR, Monitoring, NoAction
        
        public bool RequiresEdd { get; set; } = false;
        public bool RequiresStr { get; set; } = false;
        public bool RequiresSar { get; set; } = false;
        
        [MaxLength(100)]
        public string? StrReference { get; set; }
        
        [MaxLength(100)]
        public string? SarReference { get; set; }
        
        public DateTime? StrFiledAtUtc { get; set; }
        public DateTime? SarFiledAtUtc { get; set; }
        
        [MaxLength(100)]
        public string? StrFiledBy { get; set; }
        
        [MaxLength(100)]
        public string? SarFiledBy { get; set; }
        
        // Matching Details
        [MaxLength(1000)]
        public string? MatchedFields { get; set; } // JSON of matched fields
        
        [MaxLength(1000)]
        public string? MatchingDetails { get; set; } // Detailed matching information
        
        [MaxLength(100)]
        public string? SourceList { get; set; } // OFAC, UN, RBI, etc.
        
        [MaxLength(100)]
        public string? SourceCategory { get; set; } // PEP, Sanctions, Adverse Media
        
        // Transaction Context (if applicable)
        [MaxLength(100)]
        public string? TransactionId { get; set; }
        
        public decimal? TransactionAmount { get; set; }
        
        [MaxLength(100)]
        public string? TransactionType { get; set; }
        
        public DateTime? TransactionDate { get; set; }
        
        // Workflow and SLA
        public DateTime? DueDate { get; set; }
        public DateTime? EscalationDate { get; set; }
        
        [MaxLength(50)]
        public string? SlaStatus { get; set; } // OnTime, Overdue, Escalated
        
        public int? SlaHours { get; set; }
        public int? ActualHours { get; set; }
        
        // Tags and Categories
        [MaxLength(500)]
        public string? Tags { get; set; } // Comma-separated tags
        
        [MaxLength(100)]
        public string? Category { get; set; }
        
        [MaxLength(100)]
        public string? SubCategory { get; set; }
        
        // Audit Fields
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
        
        [MaxLength(100)]
        public string? CreatedBy { get; set; }
        
        [MaxLength(100)]
        public string? UpdatedBy { get; set; }
        
        // Navigation Properties
        public Customer? Customer { get; set; }
        public WatchlistEntry? WatchlistEntry { get; set; }
    }
}
