using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PEPScanner.Infrastructure.Services;
using System.Security.Claims;

namespace PEPScanner.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    private Guid GetOrganizationId()
    {
        var orgIdClaim = User.FindFirst("OrganizationId")?.Value;
        return Guid.TryParse(orgIdClaim, out var orgId) ? orgId : Guid.Empty;
    }

    [HttpGet("overview")]
    public async Task<ActionResult<DashboardOverview>> GetOverview()
    {
        var organizationId = GetOrganizationId();
        if (organizationId == Guid.Empty)
            return BadRequest("Invalid organization");

        var overview = await _dashboardService.GetDashboardOverviewAsync(organizationId);
        return Ok(overview);
    }

    [HttpGet("charts/alert-trends")]
    public async Task<ActionResult<IEnumerable<ChartData>>> GetAlertTrends([FromQuery] int days = 30)
    {
        var organizationId = GetOrganizationId();
        if (organizationId == Guid.Empty)
            return BadRequest("Invalid organization");

        var data = await _dashboardService.GetAlertTrendsAsync(organizationId, days);
        return Ok(data);
    }

    [HttpGet("charts/screening-metrics")]
    public async Task<ActionResult<IEnumerable<ChartData>>> GetScreeningMetrics([FromQuery] int days = 30)
    {
        var organizationId = GetOrganizationId();
        if (organizationId == Guid.Empty)
            return BadRequest("Invalid organization");

        var data = await _dashboardService.GetScreeningMetricsAsync(organizationId, days);
        return Ok(data);
    }

    [HttpGet("charts/report-status")]
    public async Task<ActionResult<IEnumerable<ChartData>>> GetReportStatusDistribution()
    {
        var organizationId = GetOrganizationId();
        if (organizationId == Guid.Empty)
            return BadRequest("Invalid organization");

        var data = await _dashboardService.GetReportStatusDistributionAsync(organizationId);
        return Ok(data);
    }

    [HttpGet("charts/compliance-score")]
    public async Task<ActionResult<IEnumerable<ChartData>>> GetComplianceScoreHistory([FromQuery] int months = 6)
    {
        var organizationId = GetOrganizationId();
        if (organizationId == Guid.Empty)
            return BadRequest("Invalid organization");

        var data = await _dashboardService.GetComplianceScoreHistoryAsync(organizationId, months);
        return Ok(data);
    }

    [HttpGet("recent-activities")]
    public async Task<ActionResult<IEnumerable<RecentActivity>>> GetRecentActivities([FromQuery] int count = 10)
    {
        var organizationId = GetOrganizationId();
        if (organizationId == Guid.Empty)
            return BadRequest("Invalid organization");

        var activities = await _dashboardService.GetRecentActivitiesAsync(organizationId, count);
        return Ok(activities);
    }

    [HttpGet("performance")]
    public async Task<ActionResult<PerformanceMetrics>> GetPerformanceMetrics()
    {
        var organizationId = GetOrganizationId();
        if (organizationId == Guid.Empty)
            return BadRequest("Invalid organization");

        var metrics = await _dashboardService.GetPerformanceMetricsAsync(organizationId);
        return Ok(metrics);
    }

    [HttpPost("refresh-metrics")]
    public async Task<ActionResult> RefreshMetrics()
    {
        var organizationId = GetOrganizationId();
        if (organizationId == Guid.Empty)
            return BadRequest("Invalid organization");

        await _dashboardService.UpdateDashboardMetricsAsync(organizationId);
        return Ok(new { message = "Metrics updated successfully" });
    }

    [HttpGet("kpis")]
    public async Task<ActionResult<DashboardKpis>> GetKpis()
    {
        var organizationId = GetOrganizationId();
        if (organizationId == Guid.Empty)
            return BadRequest("Invalid organization");

        var overview = await _dashboardService.GetDashboardOverviewAsync(organizationId);
        var performance = await _dashboardService.GetPerformanceMetricsAsync(organizationId);

        var kpis = new DashboardKpis
        {
            TotalAlerts = overview.TotalAlerts,
            PendingAlerts = overview.PendingAlerts,
            HighRiskAlerts = overview.HighRiskAlerts,
            AlertsTrend = overview.AlertsTrend,
            TotalReports = overview.TotalSars + overview.TotalStrs,
            PendingReports = overview.PendingSars + overview.PendingStrs,
            ComplianceScore = overview.ComplianceScore,
            AverageResolutionTime = performance.AverageResolutionTimeHours,
            AccuracyRate = performance.AccuracyRate,
            TotalScreenings = performance.TotalScreenings,
            MatchRate = performance.MatchRate
        };

        return Ok(kpis);
    }

    [HttpGet("widgets")]
    public async Task<ActionResult<DashboardWidgets>> GetWidgets()
    {
        var organizationId = GetOrganizationId();
        if (organizationId == Guid.Empty)
            return BadRequest("Invalid organization");

        var overview = await _dashboardService.GetDashboardOverviewAsync(organizationId);
        var recentActivities = await _dashboardService.GetRecentActivitiesAsync(organizationId, 5);
        var alertTrends = await _dashboardService.GetAlertTrendsAsync(organizationId, 7);
        var reportStatus = await _dashboardService.GetReportStatusDistributionAsync(organizationId);

        var widgets = new DashboardWidgets
        {
            Overview = overview,
            RecentActivities = recentActivities,
            AlertTrends = alertTrends,
            ReportStatusDistribution = reportStatus
        };

        return Ok(widgets);
    }
}

// Response DTOs
public class DashboardKpis
{
    public int TotalAlerts { get; set; }
    public int PendingAlerts { get; set; }
    public int HighRiskAlerts { get; set; }
    public double AlertsTrend { get; set; }
    public int TotalReports { get; set; }
    public int PendingReports { get; set; }
    public decimal ComplianceScore { get; set; }
    public double AverageResolutionTime { get; set; }
    public double AccuracyRate { get; set; }
    public int TotalScreenings { get; set; }
    public double MatchRate { get; set; }
}

public class DashboardWidgets
{
    public DashboardOverview Overview { get; set; } = new();
    public IEnumerable<RecentActivity> RecentActivities { get; set; } = new List<RecentActivity>();
    public IEnumerable<ChartData> AlertTrends { get; set; } = new List<ChartData>();
    public IEnumerable<ChartData> ReportStatusDistribution { get; set; } = new List<ChartData>();
}
