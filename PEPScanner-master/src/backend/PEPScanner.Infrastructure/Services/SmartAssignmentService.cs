using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PEPScanner.Domain.Entities;
using PEPScanner.Infrastructure.Data;

namespace PEPScanner.Infrastructure.Services
{
    public interface ISmartAssignmentService
    {
        Task<OrganizationUser?> GetOptimalAssigneeAsync(Alert alert, Guid organizationId);
        Task<Team?> GetResponsibleTeamAsync(Alert alert, Guid organizationId);
        Task UpdateWorkloadAsync(Guid userId, int increment = 1);
        Task<List<OrganizationUser>> GetNotificationRecipientsAsync(Alert alert, Guid organizationId);
    }

    public class SmartAssignmentService : ISmartAssignmentService
    {
        private readonly PepScannerDbContext _context;
        private readonly ILogger<SmartAssignmentService> _logger;

        public SmartAssignmentService(PepScannerDbContext context, ILogger<SmartAssignmentService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<OrganizationUser?> GetOptimalAssigneeAsync(Alert alert, Guid organizationId)
        {
            try
            {
                // Step 1: Find responsible team
                var team = await GetResponsibleTeamAsync(alert, organizationId);
                if (team == null)
                {
                    _logger.LogWarning("No team found for alert {AlertId} in organization {OrganizationId}", alert.Id, organizationId);
                    return await GetFallbackAssigneeAsync(organizationId);
                }

                // Step 2: Get available team members (analysts)
                var availableAnalysts = await _context.OrganizationUsers
                    .Where(u => u.OrganizationId == organizationId 
                             && u.TeamId == team.Id 
                             && u.IsActive 
                             && u.EscalationLevel == 0 // Analysts
                             && u.CurrentWorkload < u.MaxWorkload)
                    .OrderBy(u => u.CurrentWorkload)
                    .ThenBy(u => u.LastLoginAtUtc ?? DateTime.MinValue)
                    .ToListAsync();

                if (availableAnalysts.Any())
                {
                    return availableAnalysts.First();
                }

                // Step 3: If no analysts available, assign to team lead
                if (team.TeamLeadId.HasValue)
                {
                    var teamLead = await _context.OrganizationUsers
                        .FirstOrDefaultAsync(u => u.Id == team.TeamLeadId.Value && u.IsActive);
                    
                    if (teamLead != null && teamLead.CurrentWorkload < teamLead.MaxWorkload)
                    {
                        return teamLead;
                    }
                }

                // Step 4: Fallback to any available user
                return await GetFallbackAssigneeAsync(organizationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting optimal assignee for alert {AlertId}", alert.Id);
                return await GetFallbackAssigneeAsync(organizationId);
            }
        }

        public async Task<Team?> GetResponsibleTeamAsync(Alert alert, Guid organizationId)
        {
            try
            {
                // Priority 1: Customer territory-based assignment
                if (alert.CustomerId.HasValue)
                {
                    var customer = await _context.Customers
                        .FirstOrDefaultAsync(c => c.Id == alert.CustomerId.Value);
                    
                    if (customer != null && !string.IsNullOrEmpty(customer.Territory))
                    {
                        var territoryTeam = await _context.Teams
                            .FirstOrDefaultAsync(t => t.OrganizationId == organizationId 
                                                   && t.IsActive 
                                                   && t.Territory == customer.Territory);
                        if (territoryTeam != null) return territoryTeam;
                    }
                }

                // Priority 2: Alert type and risk level-based assignment
                var team = await _context.Teams
                    .Where(t => t.OrganizationId == organizationId && t.IsActive)
                    .FirstOrDefaultAsync(t => 
                        (alert.AlertType == "PEP" && t.Department == "Compliance") ||
                        (alert.AlertType == "Sanctions" && t.Department == "Compliance") ||
                        (alert.RiskLevel == "Critical" && t.Name.Contains("Senior")) ||
                        (alert.RiskLevel == "High" && t.Department == "Compliance"));

                if (team != null) return team;

                // Priority 3: Default compliance team
                return await _context.Teams
                    .FirstOrDefaultAsync(t => t.OrganizationId == organizationId 
                                           && t.IsActive 
                                           && t.Department == "Compliance");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting responsible team for alert {AlertId}", alert.Id);
                return null;
            }
        }

        public async Task UpdateWorkloadAsync(Guid userId, int increment = 1)
        {
            try
            {
                var user = await _context.OrganizationUsers
                    .FirstOrDefaultAsync(u => u.Id == userId);
                
                if (user != null)
                {
                    user.CurrentWorkload = Math.Max(0, user.CurrentWorkload + increment);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating workload for user {UserId}", userId);
            }
        }

        public async Task<List<OrganizationUser>> GetNotificationRecipientsAsync(Alert alert, Guid organizationId)
        {
            var recipients = new List<OrganizationUser>();

            try
            {
                // Get notification rules for this alert type and risk level
                var rule = await _context.NotificationRules
                    .FirstOrDefaultAsync(nr => nr.OrganizationId == organizationId 
                                            && nr.AlertType == alert.AlertType 
                                            && nr.RiskLevel == alert.RiskLevel 
                                            && nr.IsActive);

                if (rule == null)
                {
                    // Default rule: notify assignee only
                    var assignee = await GetAssigneeByEmailAsync(alert.AssignedTo, organizationId);
                    if (assignee != null) recipients.Add(assignee);
                    return recipients;
                }

                // Apply notification rules
                if (rule.NotifyAssignee && !string.IsNullOrEmpty(alert.AssignedTo))
                {
                    var assignee = await GetAssigneeByEmailAsync(alert.AssignedTo, organizationId);
                    if (assignee != null) recipients.Add(assignee);
                }

                if (rule.NotifyTeamLead && alert.TeamId.HasValue)
                {
                    var team = await _context.Teams
                        .Include(t => t.TeamLead)
                        .FirstOrDefaultAsync(t => t.Id == alert.TeamId.Value);
                    
                    if (team?.TeamLead != null && !recipients.Any(r => r.Id == team.TeamLead.Id))
                    {
                        recipients.Add(team.TeamLead);
                    }
                }

                if (rule.NotifyManager)
                {
                    var assignee = await GetAssigneeByEmailAsync(alert.AssignedTo, organizationId);
                    if (assignee?.Manager != null && !recipients.Any(r => r.Id == assignee.Manager.Id))
                    {
                        recipients.Add(assignee.Manager);
                    }
                }

                if (rule.NotifyRiskTeam)
                {
                    var riskTeamMembers = await _context.OrganizationUsers
                        .Where(u => u.OrganizationId == organizationId 
                                 && u.IsActive 
                                 && u.Department == "Risk")
                        .ToListAsync();
                    
                    foreach (var member in riskTeamMembers)
                    {
                        if (!recipients.Any(r => r.Id == member.Id))
                        {
                            recipients.Add(member);
                        }
                    }
                }

                return recipients;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification recipients for alert {AlertId}", alert.Id);
                return recipients;
            }
        }

        private async Task<OrganizationUser?> GetFallbackAssigneeAsync(Guid organizationId)
        {
            return await _context.OrganizationUsers
                .Where(u => u.OrganizationId == organizationId 
                         && u.IsActive 
                         && u.CurrentWorkload < u.MaxWorkload)
                .OrderBy(u => u.CurrentWorkload)
                .FirstOrDefaultAsync();
        }

        private async Task<OrganizationUser?> GetAssigneeByEmailAsync(string? email, Guid organizationId)
        {
            if (string.IsNullOrEmpty(email)) return null;
            
            return await _context.OrganizationUsers
                .Include(u => u.Manager)
                .FirstOrDefaultAsync(u => u.OrganizationId == organizationId 
                                       && u.Email == email 
                                       && u.IsActive);
        }
    }
}