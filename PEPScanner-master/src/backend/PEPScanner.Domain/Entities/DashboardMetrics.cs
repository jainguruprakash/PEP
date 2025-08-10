using System.ComponentModel.DataAnnotations;

namespace PEPScanner.Domain.Entities;

public class DashboardMetrics
{
    public Guid Id { get; set; }
    
    [Required]
    public Guid OrganizationId { get; set; }
    
    [Required]
    public DateTime MetricDate { get; set; }
    
    [Required]
    public string MetricType { get; set; } = string.Empty; // Daily, Weekly, Monthly
    
    // Screening Metrics
    public int TotalCustomersScreened { get; set; }
    public int TotalTransactionsScreened { get; set; }
    public int HighRiskMatches { get; set; }
    public int MediumRiskMatches { get; set; }
    public int LowRiskMatches { get; set; }
    public int FalsePositives { get; set; }
    public int TruePositives { get; set; }
    
    // Alert Metrics
    public int TotalAlertsGenerated { get; set; }
    public int AlertsResolved { get; set; }
    public int AlertsPending { get; set; }
    public int AlertsEscalated { get; set; }
    public decimal AverageResolutionTimeHours { get; set; }
    
    // Report Metrics
    public int SarReportsCreated { get; set; }
    public int SarReportsSubmitted { get; set; }
    public int StrReportsCreated { get; set; }
    public int StrReportsSubmitted { get; set; }
    
    // Compliance Metrics
    public decimal ComplianceScore { get; set; }
    public int RegulatoryDeadlinesMissed { get; set; }
    public int RegulatoryDeadlinesMet { get; set; }
    
    // Performance Metrics
    public decimal SystemUptime { get; set; }
    public decimal AverageScreeningTimeMs { get; set; }
    public int ApiCallsCount { get; set; }
    public int ErrorsCount { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual Organization Organization { get; set; } = null!;
}

public class AlertMetrics
{
    public Guid Id { get; set; }
    
    [Required]
    public Guid OrganizationId { get; set; }
    
    [Required]
    public DateTime Date { get; set; }
    
    public int TotalAlerts { get; set; }
    public int HighPriorityAlerts { get; set; }
    public int MediumPriorityAlerts { get; set; }
    public int LowPriorityAlerts { get; set; }
    
    public int ResolvedAlerts { get; set; }
    public int PendingAlerts { get; set; }
    public int EscalatedAlerts { get; set; }
    
    public decimal AverageResolutionTime { get; set; }
    public decimal ComplianceScore { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual Organization Organization { get; set; } = null!;
}

public class ScreeningMetrics
{
    public Guid Id { get; set; }
    
    [Required]
    public Guid OrganizationId { get; set; }
    
    [Required]
    public DateTime Date { get; set; }
    
    public int CustomersScreened { get; set; }
    public int TransactionsScreened { get; set; }
    
    public int PepMatches { get; set; }
    public int SanctionMatches { get; set; }
    public int WatchlistMatches { get; set; }
    
    public int TruePositives { get; set; }
    public int FalsePositives { get; set; }
    
    public decimal AccuracyRate { get; set; }
    public decimal AverageScreeningTime { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual Organization Organization { get; set; } = null!;
}

public class ComplianceReport
{
    public Guid Id { get; set; }
    
    [Required]
    public Guid OrganizationId { get; set; }
    
    [Required]
    public string ReportType { get; set; } = string.Empty; // Monthly, Quarterly, Annual
    
    [Required]
    public DateTime ReportPeriodStart { get; set; }
    
    [Required]
    public DateTime ReportPeriodEnd { get; set; }
    
    [Required]
    public string ReportData { get; set; } = string.Empty; // JSON data
    
    [Required]
    public ComplianceReportStatus Status { get; set; } = ComplianceReportStatus.Draft;
    
    public Guid? GeneratedById { get; set; }
    
    public DateTime? GeneratedAt { get; set; }
    
    public string? FilePath { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual Organization Organization { get; set; } = null!;
    public virtual OrganizationUser? GeneratedBy { get; set; }
}

public enum ComplianceReportStatus
{
    Draft = 0,
    Generated = 1,
    Reviewed = 2,
    Approved = 3,
    Submitted = 4
}
