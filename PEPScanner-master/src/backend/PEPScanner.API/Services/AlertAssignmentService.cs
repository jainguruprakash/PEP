using Microsoft.EntityFrameworkCore;
using PEPScanner.Infrastructure.Data;
using PEPScanner.Domain.Entities;

namespace PEPScanner.API.Services
{
    public interface IAlertAssignmentService
    {
        Task<string?> GetSeniorUserIdAsync(string userId, Guid organizationId);
        Task<bool> AssignAlertToSeniorAsync(Guid alertId, string userId);
        Task<List<string>> GetUserHierarchyAsync(string userId, Guid organizationId);
    }

    public class AlertAssignmentService : IAlertAssignmentService
    {
        private readonly PepScannerDbContext _context;
        private readonly ILogger<AlertAssignmentService> _logger;

        public AlertAssignmentService(PepScannerDbContext context, ILogger<AlertAssignmentService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<string?> GetSeniorUserIdAsync(string userId, Guid organizationId)
        {
            try
            {
                var user = await _context.OrganizationUsers
                    .FirstOrDefaultAsync(u => u.Id.ToString() == userId && u.OrganizationId == organizationId);

                if (user == null) return null;

                // Find senior based on role hierarchy
                var seniorRole = GetSeniorRole(user.Role);
                if (seniorRole == null) return null;

                var senior = await _context.OrganizationUsers
                    .Where(u => u.OrganizationId == organizationId && 
                               u.Role == seniorRole && 
                               u.IsActive &&
                               u.Department == user.Department)
                    .FirstOrDefaultAsync();

                return senior?.Id.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting senior user for {UserId}", userId);
                return null;
            }
        }

        public async Task<bool> AssignAlertToSeniorAsync(Guid alertId, string userId)
        {
            try
            {
                var alert = await _context.Alerts.FindAsync(alertId);
                if (alert == null) return false;

                var user = await _context.OrganizationUsers
                    .FirstOrDefaultAsync(u => u.Id.ToString() == userId);

                if (user == null) return false;

                var seniorUserId = await GetSeniorUserIdAsync(userId, user.OrganizationId);
                if (seniorUserId == null)
                {
                    _logger.LogWarning("No senior user found for user {UserId}", userId);
                    return false;
                }

                var senior = await _context.OrganizationUsers.FindAsync(Guid.Parse(seniorUserId));
                if (senior == null) return false;

                alert.AssignedTo = seniorUserId;
                alert.CurrentReviewer = seniorUserId;
                alert.WorkflowStatus = "PendingReview";
                alert.UpdatedAtUtc = DateTime.UtcNow;
                alert.UpdatedBy = userId;

                // Create notification for senior
                var notification = new Notification
                {
                    Id = Guid.NewGuid(),
                    Type = "AlertAssignment",
                    Title = "Alert Assigned to You",
                    Message = $"Alert {alert.AlertType} has been assigned to you for review",
                    TargetUserEmail = senior.Email,
                    Data = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        alertId = alert.Id,
                        alertType = alert.AlertType,
                        priority = alert.Priority,
                        assignedBy = user.FullName
                    }),
                    IsRead = false,
                    CreatedAtUtc = DateTime.UtcNow
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Alert {AlertId} assigned to senior {SeniorId}", alertId, seniorUserId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning alert {AlertId} to senior", alertId);
                return false;
            }
        }

        public async Task<List<string>> GetUserHierarchyAsync(string userId, Guid organizationId)
        {
            var hierarchy = new List<string>();
            var currentUserId = userId;

            while (currentUserId != null)
            {
                hierarchy.Add(currentUserId);
                currentUserId = await GetSeniorUserIdAsync(currentUserId, organizationId);
                
                if (hierarchy.Count > 10) break; // Prevent infinite loops
            }

            return hierarchy;
        }

        private string? GetSeniorRole(string currentRole)
        {
            return currentRole switch
            {
                "Analyst" => "ComplianceOfficer",
                "ComplianceOfficer" => "Manager",
                "Manager" => "Admin",
                _ => null
            };
        }
    }
}