using System.ComponentModel.DataAnnotations;

namespace PEPScanner.Domain.Entities;

public class SuspiciousActivityReport
{
    public Guid Id { get; set; }
    
    [Required]
    public string ReportNumber { get; set; } = string.Empty;
    
    [Required]
    public Guid OrganizationId { get; set; }
    
    [Required]
    public Guid ReportedById { get; set; } // OrganizationUser who created the report
    
    public Guid? ReviewedById { get; set; } // OrganizationUser who reviewed the report
    
    public Guid? CustomerId { get; set; } // Related customer if applicable
    
    [Required]
    public string SubjectName { get; set; } = string.Empty;
    
    public string? SubjectAddress { get; set; }
    
    public string? SubjectIdentification { get; set; }
    
    public DateTime? SubjectDateOfBirth { get; set; }
    
    [Required]
    public string SuspiciousActivity { get; set; } = string.Empty;
    
    [Required]
    public string ActivityDescription { get; set; } = string.Empty;
    
    public decimal? TransactionAmount { get; set; }
    
    public string? TransactionCurrency { get; set; }
    
    public DateTime? TransactionDate { get; set; }
    
    public string? TransactionLocation { get; set; }
    
    [Required]
    public SarStatus Status { get; set; } = SarStatus.Draft;
    
    [Required]
    public SarPriority Priority { get; set; } = SarPriority.Medium;
    
    public DateTime? IncidentDate { get; set; }
    
    public DateTime? DiscoveryDate { get; set; }
    
    public string? RegulatoryReferences { get; set; }
    
    public string? AttachedDocuments { get; set; } // JSON array of document paths
    
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
    public virtual ICollection<SarComment> Comments { get; set; } = new List<SarComment>();
    public virtual ICollection<SarStatusHistory> StatusHistory { get; set; } = new List<SarStatusHistory>();
}

public enum SarStatus
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

public enum SarPriority
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}

public class SarComment
{
    public Guid Id { get; set; }
    
    [Required]
    public Guid SarId { get; set; }
    
    [Required]
    public Guid UserId { get; set; }
    
    [Required]
    public string Comment { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual SuspiciousActivityReport Sar { get; set; } = null!;
    public virtual OrganizationUser User { get; set; } = null!;
}

public class SarStatusHistory
{
    public Guid Id { get; set; }
    
    [Required]
    public Guid SarId { get; set; }
    
    [Required]
    public SarStatus FromStatus { get; set; }
    
    [Required]
    public SarStatus ToStatus { get; set; }
    
    [Required]
    public Guid ChangedById { get; set; }
    
    public string? Reason { get; set; }
    
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual SuspiciousActivityReport Sar { get; set; } = null!;
    public virtual OrganizationUser ChangedBy { get; set; } = null!;
}
