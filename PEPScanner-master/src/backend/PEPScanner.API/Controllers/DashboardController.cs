using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PEPScanner.Infrastructure.Data;

namespace PEPScanner.API.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    public class DashboardController : ControllerBase
    {
        private readonly PepScannerDbContext _context;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(PepScannerDbContext context, ILogger<DashboardController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("overview")]
        public async Task<IActionResult> GetDashboardOverview()
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var thisWeek = today.AddDays(-7);
                var thisMonth = today.AddDays(-30);

                // Alert Statistics
                var totalAlerts = await _context.Alerts.CountAsync();
                var pendingAlerts = await _context.Alerts.CountAsync(a => a.WorkflowStatus == "PendingReview");
                var criticalAlerts = await _context.Alerts.CountAsync(a => a.Priority == "Critical" && a.WorkflowStatus != "Closed");
                var todayAlerts = await _context.Alerts.CountAsync(a => a.CreatedAtUtc.Date == today);

                // Customer Statistics
                var totalCustomers = await _context.Customers.CountAsync();
                var activeCustomers = await _context.Customers.CountAsync(c => c.Status == "Active");
                var newCustomersToday = await _context.Customers.CountAsync(c => c.OnboardingDate.HasValue && c.OnboardingDate.Value.Date == today);
                var highRiskCustomers = await _context.Customers.CountAsync(c => c.RiskLevel == "High");

                // Screening Statistics
                var totalScreenings = await _context.Alerts.Select(a => a.CustomerId).Distinct().CountAsync();
                var screeningsToday = await _context.Alerts.Where(a => a.CreatedAtUtc.Date == today).Select(a => a.CustomerId).Distinct().CountAsync();

                // Watchlist Statistics
                var totalWatchlistEntries = await _context.WatchlistEntries.CountAsync();
                var pepEntries = await _context.WatchlistEntries.CountAsync(w => w.Category == "PEP");
                var sanctionsEntries = await _context.WatchlistEntries.CountAsync(w => w.Category == "Sanctions");

                // Recent Activity
                var recentAlerts = await _context.Alerts
                    .Include(a => a.Customer)
                    .OrderByDescending(a => a.CreatedAtUtc)
                    .Take(10)
                    .Select(a => new
                    {
                        a.Id,
                        a.AlertType,
                        a.Priority,
                        a.WorkflowStatus,
                        a.CreatedAtUtc,
                        CustomerName = a.Customer != null ? $"{a.Customer.FirstName} {a.Customer.LastName}" : "Unknown"
                    })
                    .ToListAsync();

                // Alert Trends (Last 7 days)
                var alertTrends = await _context.Alerts
                    .Where(a => a.CreatedAtUtc >= thisWeek)
                    .GroupBy(a => a.CreatedAtUtc.Date)
                    .Select(g => new
                    {
                        Date = g.Key,
                        Count = g.Count()
                    })
                    .OrderBy(x => x.Date)
                    .ToListAsync();

                // Alert Distribution by Type
                var alertsByType = await _context.Alerts
                    .GroupBy(a => a.AlertType)
                    .Select(g => new
                    {
                        Type = g.Key,
                        Count = g.Count()
                    })
                    .ToListAsync();

                // Alert Distribution by Priority
                var alertsByPriority = await _context.Alerts
                    .GroupBy(a => a.Priority)
                    .Select(g => new
                    {
                        Priority = g.Key,
                        Count = g.Count()
                    })
                    .ToListAsync();

                var dashboard = new
                {
                    summary = new
                    {
                        totalAlerts,
                        pendingAlerts,
                        criticalAlerts,
                        todayAlerts,
                        totalCustomers,
                        activeCustomers,
                        newCustomersToday,
                        highRiskCustomers,
                        totalScreenings,
                        screeningsToday,
                        totalWatchlistEntries,
                        pepEntries,
                        sanctionsEntries
                    },
                    recentActivity = recentAlerts,
                    trends = new
                    {
                        alertTrends,
                        alertsByType,
                        alertsByPriority
                    },
                    lastUpdated = DateTime.UtcNow
                };

                return Ok(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching dashboard overview");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("alerts-summary")]
        public async Task<IActionResult> GetAlertsSummary()
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var thisWeek = today.AddDays(-7);

                var summary = new
                {
                    total = await _context.Alerts.CountAsync(),
                    pending = await _context.Alerts.CountAsync(a => a.WorkflowStatus == "PendingReview"),
                    underReview = await _context.Alerts.CountAsync(a => a.WorkflowStatus == "UnderReview"),
                    approved = await _context.Alerts.CountAsync(a => a.WorkflowStatus == "Approved"),
                    rejected = await _context.Alerts.CountAsync(a => a.WorkflowStatus == "Rejected"),
                    critical = await _context.Alerts.CountAsync(a => a.Priority == "Critical"),
                    high = await _context.Alerts.CountAsync(a => a.Priority == "High"),
                    medium = await _context.Alerts.CountAsync(a => a.Priority == "Medium"),
                    low = await _context.Alerts.CountAsync(a => a.Priority == "Low"),
                    todayCount = await _context.Alerts.CountAsync(a => a.CreatedAtUtc.Date == today),
                    weekCount = await _context.Alerts.CountAsync(a => a.CreatedAtUtc >= thisWeek)
                };

                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching alerts summary");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("customer-metrics")]
        public async Task<IActionResult> GetCustomerMetrics()
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var thisMonth = today.AddDays(-30);

                var metrics = new
                {
                    total = await _context.Customers.CountAsync(),
                    active = await _context.Customers.CountAsync(c => c.Status == "Active"),
                    inactive = await _context.Customers.CountAsync(c => c.Status == "Inactive"),
                    pending = await _context.Customers.CountAsync(c => c.Status == "Pending"),
                    highRisk = await _context.Customers.CountAsync(c => c.RiskLevel == "High"),
                    mediumRisk = await _context.Customers.CountAsync(c => c.RiskLevel == "Medium"),
                    lowRisk = await _context.Customers.CountAsync(c => c.RiskLevel == "Low"),
                    newThisMonth = await _context.Customers.CountAsync(c => c.OnboardingDate.HasValue && c.OnboardingDate.Value >= thisMonth),
                    withAlerts = await _context.Customers.CountAsync(c => c.Alerts.Any())
                };

                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching customer metrics");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("screening-stats")]
        public async Task<IActionResult> GetScreeningStats()
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var thisWeek = today.AddDays(-7);

                var stats = new
                {
                    totalScreenings = await _context.Alerts.Select(a => a.CustomerId).Distinct().CountAsync(),
                    screeningsToday = await _context.Alerts.Where(a => a.CreatedAtUtc.Date == today).Select(a => a.CustomerId).Distinct().CountAsync(),
                    screeningsThisWeek = await _context.Alerts.Where(a => a.CreatedAtUtc >= thisWeek).Select(a => a.CustomerId).Distinct().CountAsync(),
                    adverseMediaAlerts = await _context.Alerts.CountAsync(a => a.AlertType == "Adverse Media"),
                    pepAlerts = await _context.Alerts.CountAsync(a => a.AlertType == "PEP"),
                    sanctionsAlerts = await _context.Alerts.CountAsync(a => a.AlertType == "Sanctions"),
                    averageProcessingTime = await CalculateAverageProcessingTime()
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching screening stats");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("recent-activity")]
        public async Task<IActionResult> GetRecentActivity([FromQuery] int limit = 20)
        {
            try
            {
                var activities = await _context.Alerts
                    .Include(a => a.Customer)
                    .OrderByDescending(a => a.UpdatedAtUtc)
                    .Take(limit)
                    .Select(a => new
                    {
                        a.Id,
                        a.AlertType,
                        a.Priority,
                        a.WorkflowStatus,
                        a.CreatedAtUtc,
                        a.UpdatedAtUtc,
                        a.ReviewedBy,
                        a.ApprovedBy,
                        a.RejectedBy,
                        CustomerName = a.Customer != null ? $"{a.Customer.FirstName} {a.Customer.LastName}" : "Unknown",
                        CustomerId = a.CustomerId
                    })
                    .ToListAsync();

                return Ok(activities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching recent activity");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        private async Task<double> CalculateAverageProcessingTime()
        {
            var processedAlerts = await _context.Alerts
                .Where(a => a.WorkflowStatus == "Approved" || a.WorkflowStatus == "Rejected")
                .Where(a => a.ApprovedAtUtc.HasValue || a.RejectedAtUtc.HasValue)
                .Select(a => new
                {
                    CreatedAt = a.CreatedAtUtc,
                    ProcessedAt = a.ApprovedAtUtc ?? a.RejectedAtUtc
                })
                .ToListAsync();

            if (!processedAlerts.Any())
                return 0;

            var totalHours = processedAlerts
                .Where(a => a.ProcessedAt.HasValue)
                .Sum(a => (a.ProcessedAt.Value - a.CreatedAt).TotalHours);

            return totalHours / processedAlerts.Count;
        }
    }
}