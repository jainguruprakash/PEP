using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PEPScanner.Infrastructure.Data;
using PEPScanner.Domain.Entities;
using PEPScanner.API.Services;

namespace PEPScanner.API.Controllers
{
    [ApiController]
    [Route("api/alerts")]
    public class AlertController : ControllerBase
    {
        private readonly PepScannerDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly ILogger<AlertController> _logger;

        public AlertController(PepScannerDbContext context, INotificationService notificationService, ILogger<AlertController> logger)
        {
            _context = context;
            _notificationService = notificationService;
            _logger = logger;
        }

        [HttpPost("create-adverse-media")]
        public async Task<IActionResult> CreateAdverseMediaAlert([FromBody] CreateAdverseMediaAlertRequest request)
        {
            try
            {
                var alert = new Alert
                {
                    Id = Guid.NewGuid(),
                    CustomerId = request.CustomerId,
                    Context = "AdverseMedia",
                    AlertType = "Adverse Media",
                    SimilarityScore = request.SimilarityScore,
                    Status = "Open",
                    Priority = request.Priority,
                    WorkflowStatus = "PendingReview",
                    MatchingDetails = request.MatchingDetails,
                    SourceList = request.SourceName,
                    CreatedBy = request.CreatedBy,
                    CreatedAtUtc = DateTime.UtcNow,
                    UpdatedAtUtc = DateTime.UtcNow
                };

                _context.Alerts.Add(alert);
                await _context.SaveChangesAsync();

                // Create notification
                await _notificationService.CreateAlertNotificationAsync(alert);

                _logger.LogInformation("Adverse media alert created: {AlertId} for customer: {CustomerId}", alert.Id, request.CustomerId);

                return Ok(new { alertId = alert.Id, message = "Alert created successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating adverse media alert");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpPost("{alertId}/review")]
        public async Task<IActionResult> ReviewAlert(Guid alertId, [FromBody] ReviewAlertRequest request)
        {
            try
            {
                var alert = await _context.Alerts.FindAsync(alertId);
                if (alert == null)
                {
                    return NotFound(new { error = "Alert not found" });
                }

                alert.WorkflowStatus = "UnderReview";
                alert.CurrentReviewer = request.ReviewerEmail;
                alert.ReviewedBy = request.ReviewerEmail;
                alert.ReviewedAtUtc = DateTime.UtcNow;
                alert.OutcomeNotes = request.ReviewNotes;
                alert.UpdatedBy = request.ReviewerEmail;
                alert.UpdatedAtUtc = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Alert {AlertId} reviewed by {Reviewer}", alertId, request.ReviewerEmail);

                return Ok(new { message = "Alert reviewed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reviewing alert");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpPost("{alertId}/approve")]
        public async Task<IActionResult> ApproveAlert(Guid alertId, [FromBody] ApproveRejectAlertRequest request)
        {
            try
            {
                var alert = await _context.Alerts.FindAsync(alertId);
                if (alert == null)
                {
                    return NotFound(new { error = "Alert not found" });
                }

                alert.WorkflowStatus = "Approved";
                alert.ApprovedBy = request.OfficerEmail;
                alert.ApprovedAtUtc = DateTime.UtcNow;
                alert.ApprovalComments = request.Comments;
                alert.Outcome = "Confirmed";
                alert.ComplianceAction = request.ComplianceAction;
                alert.UpdatedBy = request.OfficerEmail;
                alert.UpdatedAtUtc = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Alert {AlertId} approved by {Officer}", alertId, request.OfficerEmail);

                return Ok(new { message = "Alert approved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving alert");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpPost("{alertId}/reject")]
        public async Task<IActionResult> RejectAlert(Guid alertId, [FromBody] ApproveRejectAlertRequest request)
        {
            try
            {
                var alert = await _context.Alerts.FindAsync(alertId);
                if (alert == null)
                {
                    return NotFound(new { error = "Alert not found" });
                }

                alert.WorkflowStatus = "Rejected";
                alert.RejectedBy = request.OfficerEmail;
                alert.RejectedAtUtc = DateTime.UtcNow;
                alert.RejectionReason = request.Comments;
                alert.Outcome = "FalsePositive";
                alert.UpdatedBy = request.OfficerEmail;
                alert.UpdatedAtUtc = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Alert {AlertId} rejected by {Officer}", alertId, request.OfficerEmail);

                return Ok(new { message = "Alert rejected successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting alert");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("pending-review")]
        public async Task<IActionResult> GetPendingReviewAlerts([FromQuery] string? assignedTo = null)
        {
            try
            {
                var query = _context.Alerts
                    .Where(a => a.WorkflowStatus == "PendingReview" || a.WorkflowStatus == "UnderReview")
                    .Include(a => a.Customer);

                if (!string.IsNullOrEmpty(assignedTo))
                {
                    query = query.Where(a => a.CurrentReviewer == assignedTo);
                }

                var alerts = await query
                    .OrderByDescending(a => a.CreatedAtUtc)
                    .Select(a => new
                    {
                        a.Id,
                        a.AlertType,
                        a.Priority,
                        a.WorkflowStatus,
                        a.SimilarityScore,
                        a.CreatedAtUtc,
                        a.CurrentReviewer,
                        Customer = a.Customer != null ? new
                        {
                            a.Customer.Id,
                            a.Customer.FirstName,
                            a.Customer.LastName,
                            a.Customer.Email
                        } : null
                    })
                    .ToListAsync();

                return Ok(alerts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching pending review alerts");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("pending-approval")]
        public async Task<IActionResult> GetPendingApprovalAlerts()
        {
            try
            {
                var alerts = await _context.Alerts
                    .Where(a => a.WorkflowStatus == "UnderReview")
                    .Include(a => a.Customer)
                    .OrderByDescending(a => a.ReviewedAtUtc)
                    .Select(a => new
                    {
                        a.Id,
                        a.AlertType,
                        a.Priority,
                        a.WorkflowStatus,
                        a.SimilarityScore,
                        a.ReviewedBy,
                        a.ReviewedAtUtc,
                        a.OutcomeNotes,
                        Customer = a.Customer != null ? new
                        {
                            a.Customer.Id,
                            a.Customer.FirstName,
                            a.Customer.LastName,
                            a.Customer.Email
                        } : null
                    })
                    .ToListAsync();

                return Ok(alerts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching pending approval alerts");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }
    }

    public class CreateAdverseMediaAlertRequest
    {
        public Guid CustomerId { get; set; }
        public double SimilarityScore { get; set; }
        public string Priority { get; set; } = "Medium";
        public string MatchingDetails { get; set; } = string.Empty;
        public string SourceName { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
    }

    public class ReviewAlertRequest
    {
        public string ReviewerEmail { get; set; } = string.Empty;
        public string ReviewNotes { get; set; } = string.Empty;
    }

    public class ApproveRejectAlertRequest
    {
        public string OfficerEmail { get; set; } = string.Empty;
        public string Comments { get; set; } = string.Empty;
        public string ComplianceAction { get; set; } = "NoAction";
    }
}