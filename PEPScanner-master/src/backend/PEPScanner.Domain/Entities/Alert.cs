using System;
using System.ComponentModel.DataAnnotations;

namespace PEPScanner.Domain.Entities
{
    public class Alert
    {
        public Guid Id { get; set; }
        
        public Guid? CustomerId { get; set; }
        public Guid? WatchlistEntryId { get; set; }
        
        public Guid? TeamId { get; set; } // Team responsible for this alert
        
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

        // Workflow Management
        [MaxLength(100)]
        public string? ApprovedBy { get; set; }

        public DateTime? ApprovedAtUtc { get; set; }

        [MaxLength(100)]
        public string? RejectedBy { get; set; }

        public DateTime? RejectedAtUtc { get; set; }

        [MaxLength(1000)]
        public string? RejectionReason { get; set; }

        [MaxLength(1000)]
        public string? ApprovalComments { get; set; }

        [MaxLength(50)]
        public string WorkflowStatus { get; set; } = "PendingReview"; // PendingReview, UnderReview, PendingApproval, Approved, Rejected, Escalated, Closed

        public int EscalationLevel { get; set; } = 0; // 0=Initial, 1=Level1, 2=Level2, etc.

        [MaxLength(100)]
        public string? CurrentReviewer { get; set; } // Current person responsible for action

        public DateTime? LastActionDateUtc { get; set; }

        [MaxLength(50)]
        public string? LastActionType { get; set; } // Created, Assigned, Reviewed, Approved, Rejected, etc.

        // Audit Fields
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
        
        [MaxLength(100)]
        public string? CreatedBy { get; set; }
        
        [MaxLength(100)]
        public string? UpdatedBy { get; set; }

        // OpenSanctions Integration
        [MaxLength(100)]
        public string? OpenSanctionsEntityId { get; set; }

        public double? OpenSanctionsScore { get; set; }

        [MaxLength(2000)]
        public string? OpenSanctionsDatasets { get; set; } // JSON array of dataset names

        [MaxLength(2000)]
        public string? OpenSanctionsMatchFeatures { get; set; } // JSON object of match feature scores

        public DateTime? OpenSanctionsLastChecked { get; set; }

        [MaxLength(50)]
        public string? OpenSanctionsEntityType { get; set; } // Person, Organization, etc.

        [MaxLength(2000)]
        public string? OpenSanctionsAliases { get; set; } // JSON array of aliases

        [MaxLength(2000)]
        public string? OpenSanctionsSanctions { get; set; } // JSON array of sanctions details

        [MaxLength(2000)]
        public string? OpenSanctionsCountries { get; set; } // JSON array of countries

        [MaxLength(2000)]
        public string? OpenSanctionsAddresses { get; set; } // JSON array of addresses

        public DateTime? OpenSanctionsFirstSeen { get; set; }
        public DateTime? OpenSanctionsLastSeen { get; set; }
        public DateTime? OpenSanctionsLastChange { get; set; }

        // Navigation Properties
        public Customer? Customer { get; set; }
        public WatchlistEntry? WatchlistEntry { get; set; }
        public virtual Team? Team { get; set; }
        public virtual ICollection<AlertAction> Actions { get; set; } = new List<AlertAction>();
    }
}
