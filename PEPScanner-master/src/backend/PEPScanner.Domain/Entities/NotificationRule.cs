using System.ComponentModel.DataAnnotations;

namespace PEPScanner.Domain.Entities
{
    public class NotificationRule
    {
        public Guid Id { get; set; }
        
        public Guid OrganizationId { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string AlertType { get; set; } = string.Empty; // PEP, Sanctions, etc.
        
        [Required]
        [MaxLength(50)]
        public string RiskLevel { get; set; } = string.Empty; // Low, Medium, High, Critical
        
        public bool NotifyAssignee { get; set; } = true;
        public bool NotifyTeamLead { get; set; } = true;
        public bool NotifyManager { get; set; } = false;
        public bool NotifyRiskTeam { get; set; } = false;
        
        public int Level1EscalationHours { get; set; } = 24; // Analyst to Team Lead
        public int Level2EscalationHours { get; set; } = 48; // Team Lead to Manager
        public int Level3EscalationHours { get; set; } = 72; // Manager to Risk Team
        
        public bool AutoAssignToTeamLead { get; set; } = false;
        public bool AutoEscalateOnSLA { get; set; } = true;
        
        public bool IsActive { get; set; } = true;
        
        // Audit fields
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
        
        [MaxLength(100)]
        public string CreatedBy { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string UpdatedBy { get; set; } = string.Empty;
        
        // Navigation properties
        public virtual Organization? Organization { get; set; }
    }
}