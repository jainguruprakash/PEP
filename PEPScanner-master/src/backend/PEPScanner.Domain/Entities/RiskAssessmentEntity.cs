using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PEPScanner.Domain.Entities
{
    [Table("RiskAssessments")]
    public class RiskAssessmentEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid CustomerId { get; set; }

        [Required]
        [Range(0, 100)]
        public double RiskScore { get; set; }

        [Required]
        [Range(0, 100)]
        public double ConfidenceLevel { get; set; }

        [Required]
        [StringLength(50)]
        public string ModelVersion { get; set; } = string.Empty;

        [Required]
        public DateTime CalculatedAt { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string RiskFactorsJson { get; set; } = string.Empty;

        [Column(TypeName = "nvarchar(max)")]
        public string ComponentScoresJson { get; set; } = string.Empty;

        [Column(TypeName = "nvarchar(max)")]
        public string PredictiveInsightsJson { get; set; } = string.Empty;

        [Column(TypeName = "nvarchar(max)")]
        public string RecommendedActionsJson { get; set; } = string.Empty;

        [StringLength(20)]
        public string RiskTrend { get; set; } = "stable";

        // Navigation properties
        [ForeignKey("CustomerId")]
        public virtual Customer? Customer { get; set; }

        // Indexes for performance
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    [Table("Notifications")]
    public class NotificationEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Type { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Message { get; set; } = string.Empty;

        [Column(TypeName = "nvarchar(max)")]
        public string DataJson { get; set; } = string.Empty;

        [Required]
        public DateTime Timestamp { get; set; }

        [Required]
        [StringLength(20)]
        public string Priority { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Category { get; set; } = string.Empty;

        public bool ActionRequired { get; set; } = false;

        [StringLength(500)]
        public string? ActionUrl { get; set; }

        public bool IsRead { get; set; } = false;

        public Guid? UserId { get; set; }

        public Guid? CustomerId { get; set; }

        // Navigation properties
        [ForeignKey("CustomerId")]
        public virtual Customer? Customer { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    [Table("AIModelMetrics")]
    public class AIModelMetricsEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(50)]
        public string ModelVersion { get; set; } = string.Empty;

        [Required]
        public DateTime MetricDate { get; set; }

        [Range(0, 1)]
        public double Accuracy { get; set; }

        [Range(0, 1)]
        public double Precision { get; set; }

        [Range(0, 1)]
        public double Recall { get; set; }

        [Range(0, 1)]
        public double F1Score { get; set; }

        public int TotalPredictions { get; set; }

        public int TruePositives { get; set; }

        public int TrueNegatives { get; set; }

        public int FalsePositives { get; set; }

        public int FalseNegatives { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string PerformanceMetricsJson { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    [Table("RiskFactorTemplates")]
    public class RiskFactorTemplateEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Category { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0, 1)]
        public double BaseWeight { get; set; }

        [Required]
        [StringLength(20)]
        public string Severity { get; set; } = string.Empty;

        [Column(TypeName = "nvarchar(max)")]
        public string ConditionsJson { get; set; } = string.Empty;

        [Column(TypeName = "nvarchar(max)")]
        public string ActionsJson { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    [Table("CustomerRiskProfiles")]
    public class CustomerRiskProfileEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid CustomerId { get; set; }

        [Range(0, 100)]
        public double CurrentRiskScore { get; set; }

        [Range(0, 100)]
        public double PreviousRiskScore { get; set; }

        [StringLength(20)]
        public string RiskTrend { get; set; } = "stable";

        [StringLength(20)]
        public string RiskLevel { get; set; } = "Low";

        public DateTime LastAssessmentDate { get; set; }

        public DateTime NextAssessmentDue { get; set; }

        [StringLength(20)]
        public string MonitoringFrequency { get; set; } = "Monthly";

        public int AlertCount { get; set; } = 0;

        public int HighRiskAlertCount { get; set; } = 0;

        [Column(TypeName = "nvarchar(max)")]
        public string RiskFactorSummaryJson { get; set; } = string.Empty;

        [Column(TypeName = "nvarchar(max)")]
        public string MonitoringNotesJson { get; set; } = string.Empty;

        // Navigation properties
        [ForeignKey("CustomerId")]
        public virtual Customer? Customer { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
