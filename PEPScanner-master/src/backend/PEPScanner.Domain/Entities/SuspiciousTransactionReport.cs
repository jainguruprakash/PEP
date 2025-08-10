using System.ComponentModel.DataAnnotations;

namespace PEPScanner.Domain.Entities;

public class SuspiciousTransactionReport
{
    public Guid Id { get; set; }
    
    [Required]
    public string ReportNumber { get; set; } = string.Empty;
    
    [Required]
    public Guid OrganizationId { get; set; }
    
    [Required]
    public Guid ReportedById { get; set; }
    
    public Guid? ReviewedById { get; set; }
    
    public Guid? CustomerId { get; set; }
    
    [Required]
    public string TransactionReference { get; set; } = string.Empty;
    
    [Required]
    public decimal TransactionAmount { get; set; }
    
    [Required]
    public string TransactionCurrency { get; set; } = string.Empty;
    
    [Required]
    public DateTime TransactionDate { get; set; }
    
    [Required]
    public string TransactionType { get; set; } = string.Empty; // Wire, Cash, Check, etc.
    
    public string? OriginatorName { get; set; }
    
    public string? OriginatorAccount { get; set; }
    
    public string? OriginatorBank { get; set; }
    
    public string? BeneficiaryName { get; set; }
    
    public string? BeneficiaryAccount { get; set; }
    
    public string? BeneficiaryBank { get; set; }
    
    [Required]
    public string SuspicionReason { get; set; } = string.Empty;
    
    [Required]
    public string DetailedDescription { get; set; } = string.Empty;
    
    public string? CountryOfOrigin { get; set; }
    
    public string? CountryOfDestination { get; set; }
    
    [Required]
    public StrStatus Status { get; set; } = StrStatus.Draft;
    
    [Required]
    public StrPriority Priority { get; set; } = StrPriority.Medium;
    
    public string? RegulatoryReferences { get; set; }
    
    public string? AttachedDocuments { get; set; }
    
    public string? InternalNotes { get; set; }
    
    public string? ComplianceComments { get; set; }
    
    public DateTime? SubmissionDate { get; set; }
    
    public DateTime? RegulatoryFilingDate { get; set; }
    
    public string? RegulatoryReferenceNumber { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual Organization Organization { get; set; } = null!;
    public virtual OrganizationUser ReportedBy { get; set; } = null!;
    public virtual OrganizationUser? ReviewedBy { get; set; }
    public virtual Customer? Customer { get; set; }
    public virtual ICollection<StrComment> Comments { get; set; } = new List<StrComment>();
    public virtual ICollection<StrStatusHistory> StatusHistory { get; set; } = new List<StrStatusHistory>();
}

public enum StrStatus
{
    Draft = 0,
    UnderReview = 1,
    RequiresMoreInfo = 2,
    Approved = 3,
    Submitted = 4,
    Filed = 5,
    Rejected = 6,
    Closed = 7
}

public enum StrPriority
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}

public class StrComment
{
    public Guid Id { get; set; }
    
    [Required]
    public Guid StrId { get; set; }
    
    [Required]
    public Guid UserId { get; set; }
    
    [Required]
    public string Comment { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual SuspiciousTransactionReport Str { get; set; } = null!;
    public virtual OrganizationUser User { get; set; } = null!;
}

public class StrStatusHistory
{
    public Guid Id { get; set; }
    
    [Required]
    public Guid StrId { get; set; }
    
    [Required]
    public StrStatus FromStatus { get; set; }
    
    [Required]
    public StrStatus ToStatus { get; set; }
    
    [Required]
    public Guid ChangedById { get; set; }
    
    public string? Reason { get; set; }
    
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual SuspiciousTransactionReport Str { get; set; } = null!;
    public virtual OrganizationUser ChangedBy { get; set; } = null!;
}
