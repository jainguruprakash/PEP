using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PEPScanner.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace PEPScanner.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReportsController : ControllerBase
    {
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(ILogger<ReportsController> logger)
        {
            _logger = logger;
        }

        [HttpGet("sars")]
        public async Task<ActionResult<PagedResult<SuspiciousActivityReport>>> GetSars(
            [FromQuery] ReportSearchParams searchParams)
        {
            try
            {
                var reports = GenerateMockSarReports();
                
                // Apply filters
                if (searchParams.Status.HasValue)
                    reports = reports.Where(r => r.Status == searchParams.Status.Value).ToList();
                
                if (searchParams.Priority.HasValue)
                    reports = reports.Where(r => r.Priority == searchParams.Priority.Value).ToList();
                
                if (!string.IsNullOrEmpty(searchParams.SearchTerm))
                    reports = reports.Where(r => 
                        r.SubjectName.Contains(searchParams.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                        r.ReportNumber.Contains(searchParams.SearchTerm, StringComparison.OrdinalIgnoreCase)).ToList();

                var totalCount = reports.Count;
                var pageSize = searchParams.PageSize ?? 10;
                var page = searchParams.Page ?? 1;
                var pagedReports = reports.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                return Ok(new PagedResult<SuspiciousActivityReport>
                {
                    Items = pagedReports,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving SAR reports");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("sars/{id}")]
        public async Task<ActionResult<SuspiciousActivityReport>> GetSar(string id)
        {
            try
            {
                var report = GenerateMockSarReports().FirstOrDefault(r => r.Id == id);
                if (report == null)
                    return NotFound();

                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving SAR report {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("sars")]
        public async Task<ActionResult<SuspiciousActivityReport>> CreateSar([FromBody] CreateSarRequest request)
        {
            try
            {
                var report = new SuspiciousActivityReport
                {
                    Id = Guid.NewGuid(),
                    ReportNumber = GenerateReportNumber("SAR"),
                    OrganizationId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    ReportedById = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    CustomerId = request.CustomerId.HasValue ? Guid.Parse(request.CustomerId) : null,
                    SubjectName = request.SubjectName,
                    SubjectAddress = request.SubjectAddress,
                    SubjectIdentification = request.SubjectIdentification,
                    SubjectDateOfBirth = request.SubjectDateOfBirth,
                    SuspiciousActivity = request.SuspiciousActivity,
                    ActivityDescription = request.ActivityDescription,
                    TransactionAmount = request.TransactionAmount,
                    TransactionCurrency = request.TransactionCurrency,
                    TransactionDate = request.TransactionDate,
                    TransactionLocation = request.TransactionLocation,
                    Status = SarStatus.Draft,
                    Priority = request.Priority,
                    IncidentDate = request.IncidentDate,
                    DiscoveryDate = request.DiscoveryDate ?? DateTime.UtcNow,
                    RegulatoryReferences = request.RegulatoryReferences,
                    InternalNotes = request.InternalNotes,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // In real implementation, save to database
                _logger.LogInformation("Created SAR report {ReportNumber} for subject {SubjectName}", 
                    report.ReportNumber, report.SubjectName);

                return CreatedAtAction(nameof(GetSar), new { id = report.Id }, report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating SAR report");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("sars/{id}")]
        public async Task<ActionResult<SuspiciousActivityReport>> UpdateSar(string id, [FromBody] UpdateSarRequest request)
        {
            try
            {
                var report = GenerateMockSarReports().FirstOrDefault(r => r.Id == id);
                if (report == null)
                    return NotFound();

                // Update fields
                report.SubjectName = request.SubjectName ?? report.SubjectName;
                report.SubjectAddress = request.SubjectAddress ?? report.SubjectAddress;
                report.SuspiciousActivity = request.SuspiciousActivity ?? report.SuspiciousActivity;
                report.ActivityDescription = request.ActivityDescription ?? report.ActivityDescription;
                report.Priority = request.Priority ?? report.Priority;
                report.InternalNotes = request.InternalNotes ?? report.InternalNotes;
                report.UpdatedAt = DateTime.UtcNow;

                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating SAR report {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPatch("sars/{id}/status")]
        public async Task<ActionResult<SuspiciousActivityReport>> UpdateSarStatus(string id, [FromBody] UpdateReportStatusRequest request)
        {
            try
            {
                var report = GenerateMockSarReports().FirstOrDefault(r => r.Id == id);
                if (report == null)
                    return NotFound();

                var oldStatus = report.Status;
                report.Status = request.Status;
                report.ComplianceComments = request.ComplianceComments;
                report.UpdatedAt = DateTime.UtcNow;

                if (request.Status == ReportStatus.Submitted)
                {
                    report.SubmissionDate = DateTime.UtcNow;
                    report.RegulatoryReferenceNumber = GenerateRegulatoryReference();
                }

                _logger.LogInformation("Updated SAR report {ReportNumber} status from {OldStatus} to {NewStatus}", 
                    report.ReportNumber, oldStatus, request.Status);

                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating SAR report status {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        private List<SuspiciousActivityReport> GenerateMockSarReports()
        {
            return new List<SuspiciousActivityReport>
            {
                new SuspiciousActivityReport
                {
                    Id = "sar-1",
                    ReportNumber = "SAR-2024-001",
                    OrganizationId = "org-1",
                    ReportedById = "user-1",
                    CustomerId = "cust-1",
                    SubjectName = "Amit Shah",
                    SubjectAddress = "New Delhi, India",
                    SubjectIdentification = "PAN: ABCDE1234F",
                    SuspiciousActivity = "Politically Exposed Person (PEP) Match",
                    ActivityDescription = "Customer matched against PEP database with high confidence score",
                    Priority = ReportPriority.High,
                    Status = ReportStatus.Draft,
                    IncidentDate = DateTime.UtcNow.AddDays(-5),
                    DiscoveryDate = DateTime.UtcNow.AddDays(-3),
                    RegulatoryReferences = "RBI Master Direction on KYC - Section 51; PMLA Rules 2005 - Rule 9",
                    InternalNotes = "Auto-generated from Alert ID: alert-1",
                    CreatedAt = DateTime.UtcNow.AddDays(-3),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1)
                }
            };
        }

        private string GenerateReportNumber(string prefix)
        {
            return $"{prefix}-{DateTime.UtcNow.Year}-{DateTime.UtcNow.Ticks.ToString().Substring(10)}";
        }

        private string GenerateRegulatoryReference()
        {
            return $"REG-{DateTime.UtcNow.Year}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
        }
    }

    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    public class ReportSearchParams
    {
        public SarStatus? Status { get; set; }
        public SarPriority? Priority { get; set; }
        public string? ReportedById { get; set; }
        public string? ReviewedById { get; set; }
        public string? CustomerId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? SearchTerm { get; set; }
        public int? Page { get; set; } = 1;
        public int? PageSize { get; set; } = 10;
    }

    public class CreateSarRequest
    {
        public string? CustomerId { get; set; }
        [Required] public string SubjectName { get; set; } = string.Empty;
        public string? SubjectAddress { get; set; }
        public string? SubjectIdentification { get; set; }
        public DateTime? SubjectDateOfBirth { get; set; }
        [Required] public string SuspiciousActivity { get; set; } = string.Empty;
        [Required] public string ActivityDescription { get; set; } = string.Empty;
        public decimal? TransactionAmount { get; set; }
        public string? TransactionCurrency { get; set; }
        public DateTime? TransactionDate { get; set; }
        public string? TransactionLocation { get; set; }
        public SarPriority Priority { get; set; } = SarPriority.Medium;
        public DateTime? IncidentDate { get; set; }
        public DateTime? DiscoveryDate { get; set; }
        public string? RegulatoryReferences { get; set; }
        public string? InternalNotes { get; set; }
    }

    public class UpdateSarRequest
    {
        public string? SubjectName { get; set; }
        public string? SubjectAddress { get; set; }
        public string? SubjectIdentification { get; set; }
        public DateTime? SubjectDateOfBirth { get; set; }
        public string? SuspiciousActivity { get; set; }
        public string? ActivityDescription { get; set; }
        public decimal? TransactionAmount { get; set; }
        public string? TransactionCurrency { get; set; }
        public DateTime? TransactionDate { get; set; }
        public string? TransactionLocation { get; set; }
        public ReportPriority? Priority { get; set; }
        public DateTime? IncidentDate { get; set; }
        public DateTime? DiscoveryDate { get; set; }
        public string? RegulatoryReferences { get; set; }
        public string? InternalNotes { get; set; }
    }

    public class UpdateReportStatusRequest
    {
        [Required] public SarStatus Status { get; set; }
        public string? Reason { get; set; }
        public string? ComplianceComments { get; set; }
    }
}