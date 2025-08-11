using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PEPScanner.Domain.Entities;
using PEPScanner.Infrastructure.Data;

namespace PEPScanner.Infrastructure.Services
{
    public interface IEscalationService
    {
        Task<List<Alert>> GetAlertsForEscalationAsync();
        Task EscalateAlertAsync(Alert alert);
        Task<OrganizationUser?> GetEscalationTargetAsync(Alert alert, int currentLevel);
        Task ProcessSLABreachesAsync();
    }

    public class EscalationService : IEscalationService
    {
        private readonly PepScannerDbContext _context;
        private readonly ISmartAssignmentService _assignmentService;
        private readonly ILogger<EscalationService> _logger;

        public EscalationService(
            PepScannerDbContext context, 
            ISmartAssignmentService assignmentService,
            ILogger<EscalationService> logger)
        {
            _context = context;
            _assignmentService = assignmentService;
            _logger = logger;
        }

        public async Task<List<Alert>> GetAlertsForEscalationAsync()
        {
            var now = DateTime.UtcNow;
            
            return await _context.Alerts
                .Include(a => a.Team)
                .Where(a => a.Status != "Closed" 
                         && a.Status != "FalsePositive"
                         && a.DueDate.HasValue 
                         && a.DueDate.Value < now
                         && a.EscalationLevel < 3) // Max 3 levels
                .ToListAsync();
        }

        public async Task EscalateAlertAsync(Alert alert)
        {
            try
            {
                var escalationTarget = await GetEscalationTargetAsync(alert, alert.EscalationLevel);
                
                if (escalationTarget == null)
                {
                    _logger.LogWarning("No escalation target found for alert {AlertId} at level {Level}", 
                        alert.Id, alert.EscalationLevel);
                    return;
                }

                // Update alert
                alert.EscalationLevel++;
                alert.EscalatedTo = escalationTarget.Email;
                alert.EscalatedAtUtc = DateTime.UtcNow;
                alert.AssignedTo = escalationTarget.Email;
                alert.CurrentReviewer = escalationTarget.Email;
                alert.WorkflowStatus = GetWorkflowStatusForLevel(alert.EscalationLevel);
                alert.UpdatedAtUtc = DateTime.UtcNow;

                // Set new due date based on escalation level
                alert.DueDate = GetEscalationDueDate(alert.RiskLevel, alert.EscalationLevel);

                // Create alert action
                var action = new AlertAction
                {
                    Id = Guid.NewGuid(),
                    AlertId = alert.Id,
                    ActionType = "Escalated",
                    PerformedBy = "System",
                    PreviousStatus = alert.WorkflowStatus,
                    NewStatus = GetWorkflowStatusForLevel(alert.EscalationLevel),
                    Comments = $"Auto-escalated to {escalationTarget.FullName} due to SLA breach",
                    ActionDateUtc = DateTime.UtcNow
                };

                _context.AlertActions.Add(action);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Alert {AlertId} escalated to level {Level}, assigned to {Assignee}", 
                    alert.Id, alert.EscalationLevel, escalationTarget.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error escalating alert {AlertId}", alert.Id);
            }
        }

        public async Task<OrganizationUser?> GetEscalationTargetAsync(Alert alert, int currentLevel)
        {
            try
            {
                // Get current assignee to find their hierarchy
                var currentAssignee = await _context.OrganizationUsers
                    .Include(u => u.Manager)
                    .Include(u => u.Team)
                    .ThenInclude(t => t.TeamLead)
                    .FirstOrDefaultAsync(u => u.Email == alert.AssignedTo);

                if (currentAssignee == null)
                {
                    _logger.LogWarning("Current assignee not found for alert {AlertId}", alert.Id);
                    return null;
                }

                return currentLevel switch
                {
                    0 => currentAssignee.Team?.TeamLead ?? currentAssignee.Manager, // Analyst -> Team Lead
                    1 => currentAssignee.Manager, // Team Lead -> Manager
                    2 => await GetRiskTeamHeadAsync(currentAssignee.OrganizationId), // Manager -> Risk Head
                    _ => null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting escalation target for alert {AlertId} at level {Level}", 
                    alert.Id, currentLevel);
                return null;
            }
        }

        public async Task ProcessSLABreachesAsync()
        {
            try
            {
                var alertsForEscalation = await GetAlertsForEscalationAsync();
                
                foreach (var alert in alertsForEscalation)
                {
                    await EscalateAlertAsync(alert);
                }

                _logger.LogInformation("Processed {Count} alerts for SLA escalation", alertsForEscalation.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing SLA breaches");
            }
        }

        private async Task<OrganizationUser?> GetRiskTeamHeadAsync(Guid organizationId)
        {
            return await _context.OrganizationUsers
                .FirstOrDefaultAsync(u => u.OrganizationId == organizationId 
                                       && u.IsActive 
                                       && u.Department == "Risk" 
                                       && u.EscalationLevel >= 2);
        }

        private string GetWorkflowStatusForLevel(int level)
        {
            return level switch
            {
                0 => "PendingReview",
                1 => "PendingApproval",
                2 => "PendingManagerReview",
                3 => "PendingRiskReview",
                _ => "Escalated"
            };
        }

        private DateTime GetEscalationDueDate(string? riskLevel, int escalationLevel)
        {
            var baseHours = riskLevel switch
            {
                "Critical" => 2,
                "High" => 6,
                "Medium" => 12,
                "Low" => 24,
                _ => 24
            };

            // Reduce time for higher escalation levels
            var escalationMultiplier = escalationLevel switch
            {
                1 => 0.5, // 50% of original time
                2 => 0.25, // 25% of original time
                3 => 0.125, // 12.5% of original time
                _ => 1.0
            };

            var hours = (int)(baseHours * escalationMultiplier);
            return DateTime.UtcNow.AddHours(Math.Max(1, hours)); // Minimum 1 hour
        }
    }
}