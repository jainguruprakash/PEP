using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PEPScanner.Infrastructure.Data;
using PEPScanner.Domain.Entities;

namespace PEPScanner.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AlertsController : ControllerBase
    {
        private readonly PepScannerDbContext _context;
        private readonly ILogger<AlertsController> _logger;

        public AlertsController(PepScannerDbContext context, ILogger<AlertsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? status = null,
            [FromQuery] string? workflowStatus = null,
            [FromQuery] string? assignedTo = null,
            [FromQuery] string? priority = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            [FromQuery] string? organizationId = null)
        {
            try
            {
                var alertsQuery = _context.Alerts.Include(a => a.Customer).AsQueryable();
                
                // Filter by organization if provided
                if (!string.IsNullOrEmpty(organizationId))
                {
                    alertsQuery = alertsQuery.Where(a => a.Customer != null && a.Customer.OrganizationId.ToString() == organizationId);
                }

                // Apply filters
                if (!string.IsNullOrEmpty(status))
                    alertsQuery = alertsQuery.Where(a => a.Status == status);

                if (!string.IsNullOrEmpty(workflowStatus))
                    alertsQuery = alertsQuery.Where(a => a.WorkflowStatus == workflowStatus);

                if (!string.IsNullOrEmpty(assignedTo))
                    alertsQuery = alertsQuery.Where(a => a.AssignedTo == assignedTo || a.CurrentReviewer == assignedTo);

                if (!string.IsNullOrEmpty(priority))
                    alertsQuery = alertsQuery.Where(a => a.Priority == priority);

                // Get total count for pagination
                var totalCount = await alertsQuery.CountAsync();

                // Apply pagination
                var alerts = await alertsQuery
                    .Select(a => new
                    {
                        a.Id,
                        a.AlertType,
                        a.Status,
                        a.WorkflowStatus,
                        a.Priority,
                        a.RiskLevel,
                        a.AssignedTo,
                        a.CurrentReviewer,
                        a.CreatedAtUtc,
                        a.UpdatedAtUtc,
                        a.DueDate,
                        a.SlaStatus,
                        a.EscalationLevel,
                        a.LastActionType,
                        a.LastActionDateUtc,
                        CustomerName = a.Customer != null ? a.Customer.FullName : "Unknown",
                        SourceList = a.SourceList,
                        SimilarityScore = a.SimilarityScore
                    })
                    .OrderByDescending(a => a.CreatedAtUtc)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return Ok(alerts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting alerts");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var alert = await _context.Alerts
                    .Include(a => a.Customer)
                    .Include(a => a.WatchlistEntry)
                    .Include(a => a.Actions.OrderByDescending(ac => ac.ActionDateUtc))
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (alert == null)
                {
                    return NotFound(new { error = "Alert not found" });
                }

                var result = new
                {
                    alert.Id,
                    alert.AlertType,
                    alert.Status,
                    alert.WorkflowStatus,
                    alert.Priority,
                    alert.RiskLevel,
                    alert.AssignedTo,
                    alert.CurrentReviewer,
                    alert.ReviewedBy,
                    alert.ReviewedAtUtc,
                    alert.ApprovedBy,
                    alert.ApprovedAtUtc,
                    alert.RejectedBy,
                    alert.RejectedAtUtc,
                    alert.RejectionReason,
                    alert.ApprovalComments,
                    alert.EscalatedTo,
                    alert.EscalatedAtUtc,
                    alert.OutcomeNotes,
                    alert.Outcome,
                    alert.SimilarityScore,
                    alert.MatchAlgorithm,
                    alert.MatchingDetails,
                    alert.SourceList,
                    alert.SourceCategory,
                    alert.CreatedAtUtc,
                    alert.UpdatedAtUtc,
                    alert.DueDate,
                    alert.EscalationDate,
                    alert.SlaStatus,
                    alert.EscalationLevel,
                    alert.LastActionType,
                    alert.LastActionDateUtc,
                    Customer = alert.Customer != null ? new
                    {
                        alert.Customer.Id,
                        alert.Customer.FullName,
                        alert.Customer.DateOfBirth,
                        alert.Customer.Nationality,
                        alert.Customer.Country
                    } : null,
                    WatchlistEntry = alert.WatchlistEntry != null ? new
                    {
                        alert.WatchlistEntry.Id,
                        alert.WatchlistEntry.PrimaryName,
                        alert.WatchlistEntry.AlternateNames,
                        alert.WatchlistEntry.Source,
                        alert.WatchlistEntry.ListType,
                        alert.WatchlistEntry.Country,
                        alert.WatchlistEntry.RiskCategory
                    } : null,
                    Actions = alert.Actions.Select(a => new
                    {
                        a.Id,
                        a.ActionType,
                        a.PerformedBy,
                        a.PreviousStatus,
                        a.NewStatus,
                        a.Comments,
                        a.Reason,
                        a.ActionDateUtc
                    }).ToList()
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting alert {Id}", id);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAlertRequest request)
        {
            try
            {
                var alert = await _context.Alerts.FindAsync(id);
                if (alert == null)
                {
                    return NotFound(new { error = "Alert not found" });
                }

                alert.Status = request.Status;
                alert.UpdatedAtUtc = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating alert {Id}", id);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetByStatus(string status, [FromQuery] string? organizationId = null)
        {
            try
            {
                var alertsQuery = _context.Alerts
                    .Where(a => a.Status == status);
                    
                if (!string.IsNullOrEmpty(organizationId))
                {
                    alertsQuery = alertsQuery.Where(a => a.Customer != null && a.Customer.OrganizationId.ToString() == organizationId);
                }
                
                var alerts = await alertsQuery
                    .Select(a => new
                    {
                        a.Id,
                        a.AlertType,
                        a.Status,
                        a.WorkflowStatus,
                        a.Priority,
                        a.RiskLevel,
                        a.AssignedTo,
                        a.CurrentReviewer,
                        a.MatchingDetails,
                        a.CreatedAtUtc,
                        a.DueDate,
                        CustomerName = a.Customer != null ? a.Customer.FullName : null
                    })
                    .OrderByDescending(a => a.CreatedAtUtc)
                    .ToListAsync();

                return Ok(alerts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting alerts by status {Status}", status);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        // Workflow Management Endpoints

        [HttpGet("pending-review")]
        public async Task<IActionResult> GetPendingReview([FromQuery] string? reviewerEmail = null, [FromQuery] string? organizationId = null)
        {
            try
            {
                var query = _context.Alerts
                    .Where(a => a.WorkflowStatus == "PendingReview" || a.WorkflowStatus == "UnderReview");
                    
                if (!string.IsNullOrEmpty(organizationId))
                {
                    query = query.Where(a => a.Customer != null && a.Customer.OrganizationId.ToString() == organizationId);
                }

                if (!string.IsNullOrEmpty(reviewerEmail))
                {
                    query = query.Where(a => a.CurrentReviewer == reviewerEmail);
                }

                var alerts = await query
                    .Include(a => a.Customer)
                    .Select(a => new
                    {
                        a.Id,
                        a.AlertType,
                        a.Status,
                        a.WorkflowStatus,
                        a.Priority,
                        a.RiskLevel,
                        a.CurrentReviewer,
                        a.CreatedAtUtc,
                        a.DueDate,
                        a.SlaStatus,
                        a.SimilarityScore,
                        a.SourceList,
                        CustomerName = a.Customer != null ? a.Customer.FullName : null,
                        DaysOld = (DateTime.UtcNow - a.CreatedAtUtc).Days
                    })
                    .OrderBy(a => a.DueDate)
                    .ToListAsync();

                return Ok(alerts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending review alerts");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("pending-approval")]
        public async Task<IActionResult> GetPendingApproval([FromQuery] string? approverEmail = null, [FromQuery] string? organizationId = null)
        {
            try
            {
                var query = _context.Alerts
                    .Where(a => a.WorkflowStatus == "PendingApproval");
                    
                if (!string.IsNullOrEmpty(organizationId))
                {
                    query = query.Where(a => a.Customer != null && a.Customer.OrganizationId.ToString() == organizationId);
                }

                if (!string.IsNullOrEmpty(approverEmail))
                {
                    query = query.Where(a => a.CurrentReviewer == approverEmail);
                }

                var alerts = await query
                    .Include(a => a.Customer)
                    .Select(a => new
                    {
                        a.Id,
                        a.AlertType,
                        a.Status,
                        a.WorkflowStatus,
                        a.Priority,
                        a.RiskLevel,
                        a.CurrentReviewer,
                        a.ReviewedBy,
                        a.ReviewedAtUtc,
                        a.CreatedAtUtc,
                        a.DueDate,
                        a.SlaStatus,
                        a.SimilarityScore,
                        a.SourceList,
                        CustomerName = a.Customer != null ? a.Customer.FullName : null,
                        DaysOld = (DateTime.UtcNow - a.CreatedAtUtc).Days
                    })
                    .OrderBy(a => a.DueDate)
                    .ToListAsync();

                return Ok(alerts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending approval alerts");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpPost("{id}/assign")]
        public async Task<IActionResult> AssignAlert(Guid id, [FromBody] AssignAlertRequest request)
        {
            try
            {
                var alert = await _context.Alerts.FindAsync(id);
                if (alert == null)
                {
                    return NotFound(new { error = "Alert not found" });
                }

                // Validate that the assignee exists in the system
                var assignee = await _context.OrganizationUsers
                    .FirstOrDefaultAsync(u => u.Id.ToString() == request.AssignedTo && u.IsActive);
                
                if (assignee == null)
                {
                    return BadRequest(new { error = "Invalid assignee. User not found or inactive." });
                }

                var previousAssignee = alert.CurrentReviewer;
                alert.CurrentReviewer = request.AssignedTo;
                alert.AssignedTo = request.AssignedTo;
                alert.WorkflowStatus = "UnderReview";
                alert.UpdatedAtUtc = DateTime.UtcNow;
                alert.UpdatedBy = request.AssignedBy;
                alert.LastActionType = "Assigned";
                alert.LastActionDateUtc = DateTime.UtcNow;

                // Create audit trail
                var action = new AlertAction
                {
                    Id = Guid.NewGuid(),
                    AlertId = id,
                    ActionType = "Assigned",
                    PerformedBy = request.AssignedBy,
                    PreviousAssignee = previousAssignee,
                    NewAssignee = request.AssignedTo,
                    Comments = request.Comments,
                    ActionDateUtc = DateTime.UtcNow
                };

                // Create notification for assignee
                var notification = new Notification
                {
                    Id = Guid.NewGuid(),
                    Type = "AlertAssignment",
                    Title = "Alert Assigned to You",
                    Message = $"Alert {alert.AlertType} has been assigned to you for review",
                    TargetUserEmail = assignee.Email,
                    Data = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        alertId = alert.Id,
                        alertType = alert.AlertType,
                        priority = alert.Priority
                    }),
                    IsRead = false,
                    CreatedAtUtc = DateTime.UtcNow
                };

                _context.AlertActions.Add(action);
                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Alert assigned successfully", assigneeName = assignee.FullName });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning alert {Id}", id);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpPost("{id}/approve")]
        public async Task<IActionResult> ApproveAlert(Guid id, [FromBody] ApproveAlertRequest request)
        {
            try
            {
                var alert = await _context.Alerts.FindAsync(id);
                if (alert == null)
                {
                    return NotFound(new { error = "Alert not found" });
                }

                // Validate that the user can approve this alert
                if (alert.WorkflowStatus != "PendingApproval" && alert.WorkflowStatus != "UnderReview")
                {
                    return BadRequest(new { error = "Alert is not in a state that can be approved" });
                }

                var previousStatus = alert.WorkflowStatus;
                alert.WorkflowStatus = "Approved";
                alert.Status = "Closed";
                alert.ApprovedBy = request.ApprovedBy;
                alert.ApprovedAtUtc = DateTime.UtcNow;
                alert.ApprovalComments = request.Comments;
                alert.Outcome = request.Outcome ?? "Approved";
                alert.OutcomeNotes = request.Comments;
                alert.UpdatedAtUtc = DateTime.UtcNow;
                alert.UpdatedBy = request.ApprovedBy;
                alert.LastActionType = "Approved";
                alert.LastActionDateUtc = DateTime.UtcNow;

                // Create audit trail
                var action = new AlertAction
                {
                    Id = Guid.NewGuid(),
                    AlertId = id,
                    ActionType = "Approved",
                    PerformedBy = request.ApprovedBy,
                    PreviousStatus = previousStatus,
                    NewStatus = "Approved",
                    Comments = request.Comments,
                    ActionDateUtc = DateTime.UtcNow
                };

                _context.AlertActions.Add(action);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Alert approved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving alert {Id}", id);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpPost("{id}/reject")]
        public async Task<IActionResult> RejectAlert(Guid id, [FromBody] RejectAlertRequest request)
        {
            try
            {
                var alert = await _context.Alerts.FindAsync(id);
                if (alert == null)
                {
                    return NotFound(new { error = "Alert not found" });
                }

                // Validate that the user can reject this alert
                if (alert.WorkflowStatus != "PendingApproval" && alert.WorkflowStatus != "UnderReview")
                {
                    return BadRequest(new { error = "Alert is not in a state that can be rejected" });
                }

                var previousStatus = alert.WorkflowStatus;
                alert.WorkflowStatus = "Rejected";
                alert.Status = "FalsePositive";
                alert.RejectedBy = request.RejectedBy;
                alert.RejectedAtUtc = DateTime.UtcNow;
                alert.RejectionReason = request.Reason;
                alert.Outcome = "FalsePositive";
                alert.OutcomeNotes = request.Reason;
                alert.UpdatedAtUtc = DateTime.UtcNow;
                alert.UpdatedBy = request.RejectedBy;
                alert.LastActionType = "Rejected";
                alert.LastActionDateUtc = DateTime.UtcNow;

                // Create audit trail
                var action = new AlertAction
                {
                    Id = Guid.NewGuid(),
                    AlertId = id,
                    ActionType = "Rejected",
                    PerformedBy = request.RejectedBy,
                    PreviousStatus = previousStatus,
                    NewStatus = "Rejected",
                    Comments = request.Reason,
                    Reason = request.Reason,
                    ActionDateUtc = DateTime.UtcNow
                };

                _context.AlertActions.Add(action);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Alert rejected successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting alert {Id}", id);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpPost("create-from-screening")]
        public async Task<IActionResult> CreateFromScreening([FromBody] CreateAlertFromScreeningRequest request)
        {
            try
            {
                var alert = new Alert
                {
                    Id = Guid.NewGuid(),
                    Context = "CustomerScreening",
                    AlertType = request.AlertType,
                    SimilarityScore = request.SimilarityScore,
                    Status = "Open",
                    Priority = request.Priority,
                    RiskLevel = request.RiskLevel,
                    SourceList = request.SourceList,
                    SourceCategory = request.SourceCategory,
                    MatchingDetails = request.MatchingDetails,
                    WorkflowStatus = "PendingReview",
                    CreatedAtUtc = DateTime.UtcNow,
                    UpdatedAtUtc = DateTime.UtcNow,
                    CreatedBy = request.CreatedBy,
                    DueDate = DateTime.UtcNow.AddHours(request.SlaHours ?? 24),
                    SlaHours = request.SlaHours ?? 24
                };

                _context.Alerts.Add(alert);
                await _context.SaveChangesAsync();

                return Ok(new { alertId = alert.Id, message = "Alert created successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating alert from screening");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpPost("{id}/escalate")]
        public async Task<IActionResult> EscalateAlert(Guid id, [FromBody] EscalateAlertRequest request)
        {
            try
            {
                var alert = await _context.Alerts.FindAsync(id);
                if (alert == null)
                {
                    return NotFound(new { error = "Alert not found" });
                }

                var user = await _context.OrganizationUsers
                    .FirstOrDefaultAsync(u => u.Id.ToString() == request.EscalatedBy);
                
                if (user == null)
                {
                    return BadRequest(new { error = "User not found" });
                }

                var alertAssignmentService = HttpContext.RequestServices.GetRequiredService<PEPScanner.API.Services.IAlertAssignmentService>();
                var success = await alertAssignmentService.AssignAlertToSeniorAsync(id, request.EscalatedBy);

                if (!success)
                {
                    return BadRequest(new { error = "No senior user found for escalation or escalation failed" });
                }

                alert.EscalationLevel = alert.EscalationLevel + 1;
                alert.EscalatedAtUtc = DateTime.UtcNow;
                alert.EscalatedTo = alert.AssignedTo;
                alert.LastActionType = "Escalated";
                alert.LastActionDateUtc = DateTime.UtcNow;

                var action = new AlertAction
                {
                    Id = Guid.NewGuid(),
                    AlertId = id,
                    ActionType = "Escalated",
                    PerformedBy = request.EscalatedBy,
                    Comments = request.Reason,
                    Reason = request.Reason,
                    ActionDateUtc = DateTime.UtcNow
                };

                _context.AlertActions.Add(action);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Alert escalated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error escalating alert {Id}", id);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }
    }

    // Request DTOs
    public class UpdateAlertRequest
    {
        public string Status { get; set; } = string.Empty;
    }

    public class AssignAlertRequest
    {
        public string AssignedTo { get; set; } = string.Empty;
        public string AssignedBy { get; set; } = string.Empty;
        public string? Comments { get; set; }
    }

    public class ApproveAlertRequest
    {
        public string ApprovedBy { get; set; } = string.Empty;
        public string? Comments { get; set; }
        public string? Outcome { get; set; }
    }

    public class EscalateAlertRequest
    {
        public string EscalatedBy { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }
}

public class RejectAlertRequest
{
    public string RejectedBy { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}

public class CreateAlertFromScreeningRequest
{
    public string AlertType { get; set; } = string.Empty;
    public double SimilarityScore { get; set; }
    public string Priority { get; set; } = "Medium";
    public string RiskLevel { get; set; } = "Medium";
    public string? SourceList { get; set; }
    public string? SourceCategory { get; set; }
    public string? MatchingDetails { get; set; }
    public string? CreatedBy { get; set; }
    public int? SlaHours { get; set; }
}
