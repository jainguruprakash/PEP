using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PEPScanner.Domain.Entities;
using PEPScanner.Infrastructure.Services;
using System.Security.Claims;

namespace PEPScanner.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    private Guid GetOrganizationId()
    {
        var orgIdClaim = User.FindFirst("OrganizationId")?.Value;
        return Guid.TryParse(orgIdClaim, out var orgId) ? orgId : Guid.Empty;
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    // SAR Endpoints
    [HttpGet("sar")]
    public async Task<ActionResult<IEnumerable<SuspiciousActivityReport>>> GetSars(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 20)
    {
        var organizationId = GetOrganizationId();
        if (organizationId == Guid.Empty)
            return BadRequest("Invalid organization");

        var sars = await _reportService.GetSarsAsync(organizationId, page, pageSize);
        return Ok(sars);
    }

    [HttpGet("sar/{id}")]
    public async Task<ActionResult<SuspiciousActivityReport>> GetSar(Guid id)
    {
        var sar = await _reportService.GetSarByIdAsync(id);
        if (sar == null)
            return NotFound();

        // Verify organization access
        if (sar.OrganizationId != GetOrganizationId())
            return Forbid();

        return Ok(sar);
    }

    [HttpPost("sar")]
    public async Task<ActionResult<SuspiciousActivityReport>> CreateSar(CreateSarRequest request)
    {
        var organizationId = GetOrganizationId();
        var userId = GetUserId();

        if (organizationId == Guid.Empty || userId == Guid.Empty)
            return BadRequest("Invalid user or organization");

        var sar = new SuspiciousActivityReport
        {
            OrganizationId = organizationId,
            ReportedById = userId,
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
            Priority = request.Priority,
            IncidentDate = request.IncidentDate,
            DiscoveryDate = request.DiscoveryDate,
            CustomerId = request.CustomerId,
            InternalNotes = request.InternalNotes
        };

        var createdSar = await _reportService.CreateSarAsync(sar);
        return CreatedAtAction(nameof(GetSar), new { id = createdSar.Id }, createdSar);
    }

    [HttpPut("sar/{id}")]
    public async Task<ActionResult<SuspiciousActivityReport>> UpdateSar(Guid id, UpdateSarRequest request)
    {
        var existingSar = await _reportService.GetSarByIdAsync(id);
        if (existingSar == null)
            return NotFound();

        if (existingSar.OrganizationId != GetOrganizationId())
            return Forbid();

        // Update fields
        existingSar.SubjectName = request.SubjectName;
        existingSar.SubjectAddress = request.SubjectAddress;
        existingSar.SubjectIdentification = request.SubjectIdentification;
        existingSar.SubjectDateOfBirth = request.SubjectDateOfBirth;
        existingSar.SuspiciousActivity = request.SuspiciousActivity;
        existingSar.ActivityDescription = request.ActivityDescription;
        existingSar.TransactionAmount = request.TransactionAmount;
        existingSar.TransactionCurrency = request.TransactionCurrency;
        existingSar.TransactionDate = request.TransactionDate;
        existingSar.TransactionLocation = request.TransactionLocation;
        existingSar.Priority = request.Priority;
        existingSar.IncidentDate = request.IncidentDate;
        existingSar.DiscoveryDate = request.DiscoveryDate;
        existingSar.InternalNotes = request.InternalNotes;
        existingSar.ComplianceComments = request.ComplianceComments;

        var updatedSar = await _reportService.UpdateSarAsync(existingSar);
        return Ok(updatedSar);
    }

    [HttpPut("sar/{id}/status")]
    public async Task<ActionResult> UpdateSarStatus(Guid id, UpdateStatusRequest request)
    {
        var userId = GetUserId();
        var success = await _reportService.UpdateSarStatusAsync(id, request.Status, userId, request.Reason);
        
        if (!success)
            return NotFound();

        return Ok();
    }

    [HttpDelete("sar/{id}")]
    public async Task<ActionResult> DeleteSar(Guid id)
    {
        var sar = await _reportService.GetSarByIdAsync(id);
        if (sar == null)
            return NotFound();

        if (sar.OrganizationId != GetOrganizationId())
            return Forbid();

        var success = await _reportService.DeleteSarAsync(id);
        if (!success)
            return NotFound();

        return NoContent();
    }

    [HttpPost("sar/{id}/comments")]
    public async Task<ActionResult<SarComment>> AddSarComment(Guid id, AddCommentRequest request)
    {
        var userId = GetUserId();
        var comment = await _reportService.AddSarCommentAsync(id, userId, request.Comment);
        return Ok(comment);
    }

    // STR Endpoints
    [HttpGet("str")]
    public async Task<ActionResult<IEnumerable<SuspiciousTransactionReport>>> GetStrs(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 20)
    {
        var organizationId = GetOrganizationId();
        if (organizationId == Guid.Empty)
            return BadRequest("Invalid organization");

        var strs = await _reportService.GetStrsAsync(organizationId, page, pageSize);
        return Ok(strs);
    }

    [HttpGet("str/{id}")]
    public async Task<ActionResult<SuspiciousTransactionReport>> GetStr(Guid id)
    {
        var str = await _reportService.GetStrByIdAsync(id);
        if (str == null)
            return NotFound();

        if (str.OrganizationId != GetOrganizationId())
            return Forbid();

        return Ok(str);
    }

    [HttpPost("str")]
    public async Task<ActionResult<SuspiciousTransactionReport>> CreateStr(CreateStrRequest request)
    {
        var organizationId = GetOrganizationId();
        var userId = GetUserId();

        if (organizationId == Guid.Empty || userId == Guid.Empty)
            return BadRequest("Invalid user or organization");

        var str = new SuspiciousTransactionReport
        {
            OrganizationId = organizationId,
            ReportedById = userId,
            TransactionReference = request.TransactionReference,
            TransactionAmount = request.TransactionAmount,
            TransactionCurrency = request.TransactionCurrency,
            TransactionDate = request.TransactionDate,
            TransactionType = request.TransactionType,
            OriginatorName = request.OriginatorName,
            OriginatorAccount = request.OriginatorAccount,
            OriginatorBank = request.OriginatorBank,
            BeneficiaryName = request.BeneficiaryName,
            BeneficiaryAccount = request.BeneficiaryAccount,
            BeneficiaryBank = request.BeneficiaryBank,
            SuspicionReason = request.SuspicionReason,
            DetailedDescription = request.DetailedDescription,
            CountryOfOrigin = request.CountryOfOrigin,
            CountryOfDestination = request.CountryOfDestination,
            Priority = request.Priority,
            CustomerId = request.CustomerId,
            InternalNotes = request.InternalNotes
        };

        var createdStr = await _reportService.CreateStrAsync(str);
        return CreatedAtAction(nameof(GetStr), new { id = createdStr.Id }, createdStr);
    }

    [HttpPut("str/{id}/status")]
    public async Task<ActionResult> UpdateStrStatus(Guid id, UpdateStrStatusRequest request)
    {
        var userId = GetUserId();
        var success = await _reportService.UpdateStrStatusAsync(id, request.Status, userId, request.Reason);
        
        if (!success)
            return NotFound();

        return Ok();
    }

    [HttpPost("str/{id}/comments")]
    public async Task<ActionResult<StrComment>> AddStrComment(Guid id, AddCommentRequest request)
    {
        var userId = GetUserId();
        var comment = await _reportService.AddStrCommentAsync(id, userId, request.Comment);
        return Ok(comment);
    }

    // Statistics
    [HttpGet("statistics")]
    public async Task<ActionResult<ReportStatistics>> GetStatistics(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        var organizationId = GetOrganizationId();
        if (organizationId == Guid.Empty)
            return BadRequest("Invalid organization");

        var statistics = await _reportService.GetReportStatisticsAsync(organizationId, fromDate, toDate);
        return Ok(statistics);
    }
}

// Request DTOs
public class CreateSarRequest
{
    public string SubjectName { get; set; } = string.Empty;
    public string? SubjectAddress { get; set; }
    public string? SubjectIdentification { get; set; }
    public DateTime? SubjectDateOfBirth { get; set; }
    public string SuspiciousActivity { get; set; } = string.Empty;
    public string ActivityDescription { get; set; } = string.Empty;
    public decimal? TransactionAmount { get; set; }
    public string? TransactionCurrency { get; set; }
    public DateTime? TransactionDate { get; set; }
    public string? TransactionLocation { get; set; }
    public SarPriority Priority { get; set; } = SarPriority.Medium;
    public DateTime? IncidentDate { get; set; }
    public DateTime? DiscoveryDate { get; set; }
    public Guid? CustomerId { get; set; }
    public string? InternalNotes { get; set; }
}

public class UpdateSarRequest : CreateSarRequest
{
    public string? ComplianceComments { get; set; }
}

public class CreateStrRequest
{
    public string TransactionReference { get; set; } = string.Empty;
    public decimal TransactionAmount { get; set; }
    public string TransactionCurrency { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public string? OriginatorName { get; set; }
    public string? OriginatorAccount { get; set; }
    public string? OriginatorBank { get; set; }
    public string? BeneficiaryName { get; set; }
    public string? BeneficiaryAccount { get; set; }
    public string? BeneficiaryBank { get; set; }
    public string SuspicionReason { get; set; } = string.Empty;
    public string DetailedDescription { get; set; } = string.Empty;
    public string? CountryOfOrigin { get; set; }
    public string? CountryOfDestination { get; set; }
    public StrPriority Priority { get; set; } = StrPriority.Medium;
    public Guid? CustomerId { get; set; }
    public string? InternalNotes { get; set; }
}

public class UpdateStatusRequest
{
    public SarStatus Status { get; set; }
    public string? Reason { get; set; }
}

public class UpdateStrStatusRequest
{
    public StrStatus Status { get; set; }
    public string? Reason { get; set; }
}

public class AddCommentRequest
{
    public string Comment { get; set; } = string.Empty;
}
