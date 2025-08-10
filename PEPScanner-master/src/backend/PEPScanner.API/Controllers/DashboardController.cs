using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace PEPScanner.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    [HttpGet("overview")]
    public ActionResult<object> GetOverview()
    {
        var overview = new
        {
            TotalCustomers = 1250,
            ActiveAlerts = 23,
            PendingReviews = 8,
            ComplianceScore = 94.5,
            LastUpdated = DateTime.UtcNow
        };
        return Ok(overview);
    }

    [HttpGet("charts/alert-trends")]
    public ActionResult<object> GetAlertTrends([FromQuery] int days = 30)
    {
        var data = new[]
        {
            new { Date = DateTime.UtcNow.AddDays(-7), Value = 15 },
            new { Date = DateTime.UtcNow.AddDays(-6), Value = 22 },
            new { Date = DateTime.UtcNow.AddDays(-5), Value = 18 },
            new { Date = DateTime.UtcNow.AddDays(-4), Value = 25 },
            new { Date = DateTime.UtcNow.AddDays(-3), Value = 30 },
            new { Date = DateTime.UtcNow.AddDays(-2), Value = 28 },
            new { Date = DateTime.UtcNow.AddDays(-1), Value = 23 }
        };
        return Ok(data);
    }

    [HttpGet("charts/risk-distribution")]
    public ActionResult<object> GetRiskDistribution()
    {
        var data = new[]
        {
            new { Label = "Low", Value = 65 },
            new { Label = "Medium", Value = 25 },
            new { Label = "High", Value = 10 }
        };
        return Ok(data);
    }

    [HttpGet("recent-activities")]
    public ActionResult<object> GetRecentActivities()
    {
        var activities = new[]
        {
            new { Type = "Alert", Description = "New PEP match detected", Timestamp = DateTime.UtcNow.AddMinutes(-15) },
            new { Type = "Screening", Description = "Customer batch screening completed", Timestamp = DateTime.UtcNow.AddMinutes(-30) },
            new { Type = "Report", Description = "SAR report submitted", Timestamp = DateTime.UtcNow.AddHours(-2) }
        };
        return Ok(activities);
    }

    [HttpGet("performance")]
    public ActionResult<object> GetPerformanceMetrics()
    {
        var metrics = new
        {
            ScreeningSpeed = "2.3s avg",
            Accuracy = "99.2%",
            FalsePositives = "3.1%",
            SystemUptime = "99.9%"
        };
        return Ok(metrics);
    }
}
