using System;
using System.ComponentModel.DataAnnotations;

namespace PEPScanner.API.Models
{
    public class ScreeningJob
    {
        public Guid Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string JobName { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string JobType { get; set; } = string.Empty; // FullScan, Incremental, RealTime, Batch
        
        [MaxLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Running, Completed, Failed, Cancelled
        
        public DateTime? StartedAtUtc { get; set; }
        public DateTime? CompletedAtUtc { get; set; }
        
        public int TotalRecords { get; set; } = 0;
        public int ProcessedRecords { get; set; } = 0;
        public int MatchesFound { get; set; } = 0;
        public int AlertsGenerated { get; set; } = 0;
        
        [MaxLength(1000)]
        public string? ErrorMessage { get; set; }
        
        [MaxLength(500)]
        public string? InputFile { get; set; }
        
        [MaxLength(500)]
        public string? OutputFile { get; set; }
        
        [MaxLength(100)]
        public string? TriggeredBy { get; set; }
        
        [MaxLength(100)]
        public string? AssignedTo { get; set; }
        
        public int Priority { get; set; } = 5; // 1=High, 5=Normal, 10=Low
        
        [MaxLength(1000)]
        public string? Configuration { get; set; } // JSON configuration for the job
        
        // Audit Fields
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
        
        [MaxLength(100)]
        public string? CreatedBy { get; set; }
        
        [MaxLength(100)]
        public string? UpdatedBy { get; set; }
    }
}
