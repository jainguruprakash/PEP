using Microsoft.EntityFrameworkCore;
using PEPScanner.Domain.Entities;
using PEPScanner.Infrastructure.Data;

namespace PEPScanner.Infrastructure.Services;

public interface IDashboardService
{
    Task<DashboardOverview> GetDashboardOverviewAsync(Guid organizationId);
    Task<IEnumerable<ChartData>> GetAlertTrendsAsync(Guid organizationId, int days = 30);
    Task<IEnumerable<ChartData>> GetScreeningMetricsAsync(Guid organizationId, int days = 30);
    Task<IEnumerable<ChartData>> GetReportStatusDistributionAsync(Guid organizationId);
    Task<IEnumerable<ChartData>> GetComplianceScoreHistoryAsync(Guid organizationId, int months = 6);
    Task<IEnumerable<RecentActivity>> GetRecentActivitiesAsync(Guid organizationId, int count = 10);
    Task<PerformanceMetrics> GetPerformanceMetricsAsync(Guid organizationId);
    Task UpdateDashboardMetricsAsync(Guid organizationId);
}

public class DashboardService : IDashboardService
{
    private readonly PepScannerDbContext _context;

    public DashboardService(PepScannerDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardOverview> GetDashboardOverviewAsync(Guid organizationId)
    {
        var today = DateTime.UtcNow.Date;
        var thirtyDaysAgo = today.AddDays(-30);

        // Get current counts
        var totalCustomers = await _context.Customers
            .CountAsync(c => c.OrganizationId == organizationId);

        var totalAlerts = await _context.Alerts
            .CountAsync(a => a.OrganizationId == organizationId);

        var pendingAlerts = await _context.Alerts
            .CountAsync(a => a.OrganizationId == organizationId && a.Status == AlertStatus.Open);

        var highRiskAlerts = await _context.Alerts
            .CountAsync(a => a.OrganizationId == organizationId && 
                           a.RiskLevel == RiskLevel.High && 
                           a.Status == AlertStatus.Open);

        var totalSars = await _context.SuspiciousActivityReports
            .CountAsync(sar => sar.OrganizationId == organizationId);

        var totalStrs = await _context.SuspiciousTransactionReports
            .CountAsync(str => str.OrganizationId == organizationId);

        var pendingSars = await _context.SuspiciousActivityReports
            .CountAsync(sar => sar.OrganizationId == organizationId && 
                              sar.Status == SarStatus.UnderReview);

        var pendingStrs = await _context.SuspiciousTransactionReports
            .CountAsync(str => str.OrganizationId == organizationId && 
                              str.Status == StrStatus.UnderReview);

        // Get trends (compared to previous 30 days)
        var alertsTrend = await CalculateTrendAsync(organizationId, "alerts", thirtyDaysAgo, today);
        var sarsTrend = await CalculateTrendAsync(organizationId, "sars", thirtyDaysAgo, today);
        var strsTrend = await CalculateTrendAsync(organizationId, "strs", thirtyDaysAgo, today);

        // Get latest compliance score
        var latestMetrics = await _context.DashboardMetrics
            .Where(dm => dm.OrganizationId == organizationId)
            .OrderByDescending(dm => dm.MetricDate)
            .FirstOrDefaultAsync();

        return new DashboardOverview
        {
            TotalCustomers = totalCustomers,
            TotalAlerts = totalAlerts,
            PendingAlerts = pendingAlerts,
            HighRiskAlerts = highRiskAlerts,
            TotalSars = totalSars,
            TotalStrs = totalStrs,
            PendingSars = pendingSars,
            PendingStrs = pendingStrs,
            AlertsTrend = alertsTrend,
            SarsTrend = sarsTrend,
            StrsTrend = strsTrend,
            ComplianceScore = latestMetrics?.ComplianceScore ?? 0,
            LastUpdated = DateTime.UtcNow
        };
    }

    public async Task<IEnumerable<ChartData>> GetAlertTrendsAsync(Guid organizationId, int days = 30)
    {
        var startDate = DateTime.UtcNow.Date.AddDays(-days);
        
        var alertData = await _context.Alerts
            .Where(a => a.OrganizationId == organizationId && a.CreatedAt >= startDate)
            .GroupBy(a => a.CreatedAt.Date)
            .Select(g => new ChartData
            {
                Label = g.Key.ToString("MMM dd"),
                Value = g.Count(),
                Date = g.Key
            })
            .OrderBy(cd => cd.Date)
            .ToListAsync();

        // Fill in missing dates with zero values
        var result = new List<ChartData>();
        for (var date = startDate; date <= DateTime.UtcNow.Date; date = date.AddDays(1))
        {
            var existing = alertData.FirstOrDefault(ad => ad.Date == date);
            result.Add(new ChartData
            {
                Label = date.ToString("MMM dd"),
                Value = existing?.Value ?? 0,
                Date = date
            });
        }

        return result;
    }

    public async Task<IEnumerable<ChartData>> GetScreeningMetricsAsync(Guid organizationId, int days = 30)
    {
        var startDate = DateTime.UtcNow.Date.AddDays(-days);

        var screeningData = await _context.ScreeningMetrics
            .Where(sm => sm.OrganizationId == organizationId && sm.Date >= startDate)
            .Select(sm => new ChartData
            {
                Label = sm.Date.ToString("MMM dd"),
                Value = sm.CustomersScreened + sm.TransactionsScreened,
                Date = sm.Date,
                AdditionalData = new Dictionary<string, object>
                {
                    ["CustomersScreened"] = sm.CustomersScreened,
                    ["TransactionsScreened"] = sm.TransactionsScreened,
                    ["TruePositives"] = sm.TruePositives,
                    ["FalsePositives"] = sm.FalsePositives
                }
            })
            .OrderBy(cd => cd.Date)
            .ToListAsync();

        return screeningData;
    }

    public async Task<IEnumerable<ChartData>> GetReportStatusDistributionAsync(Guid organizationId)
    {
        var sarStatusData = await _context.SuspiciousActivityReports
            .Where(sar => sar.OrganizationId == organizationId)
            .GroupBy(sar => sar.Status)
            .Select(g => new ChartData
            {
                Label = g.Key.ToString(),
                Value = g.Count(),
                Category = "SAR"
            })
            .ToListAsync();

        var strStatusData = await _context.SuspiciousTransactionReports
            .Where(str => str.OrganizationId == organizationId)
            .GroupBy(str => str.Status)
            .Select(g => new ChartData
            {
                Label = g.Key.ToString(),
                Value = g.Count(),
                Category = "STR"
            })
            .ToListAsync();

        return sarStatusData.Concat(strStatusData);
    }

    public async Task<IEnumerable<ChartData>> GetComplianceScoreHistoryAsync(Guid organizationId, int months = 6)
    {
        var startDate = DateTime.UtcNow.Date.AddMonths(-months);

        var complianceData = await _context.DashboardMetrics
            .Where(dm => dm.OrganizationId == organizationId && 
                        dm.MetricDate >= startDate && 
                        dm.MetricType == "Monthly")
            .Select(dm => new ChartData
            {
                Label = dm.MetricDate.ToString("MMM yyyy"),
                Value = (double)dm.ComplianceScore,
                Date = dm.MetricDate
            })
            .OrderBy(cd => cd.Date)
            .ToListAsync();

        return complianceData;
    }

    public async Task<IEnumerable<RecentActivity>> GetRecentActivitiesAsync(Guid organizationId, int count = 10)
    {
        var activities = new List<RecentActivity>();

        // Recent alerts
        var recentAlerts = await _context.Alerts
            .Where(a => a.OrganizationId == organizationId)
            .OrderByDescending(a => a.CreatedAt)
            .Take(count / 2)
            .Select(a => new RecentActivity
            {
                Id = a.Id,
                Type = "Alert",
                Title = $"Alert: {a.CustomerName}",
                Description = $"Risk Level: {a.RiskLevel}, Status: {a.Status}",
                Timestamp = a.CreatedAt,
                Priority = a.RiskLevel.ToString(),
                Url = $"/alerts/{a.Id}"
            })
            .ToListAsync();

        activities.AddRange(recentAlerts);

        // Recent reports
        var recentSars = await _context.SuspiciousActivityReports
            .Where(sar => sar.OrganizationId == organizationId)
            .OrderByDescending(sar => sar.CreatedAt)
            .Take(count / 4)
            .Select(sar => new RecentActivity
            {
                Id = sar.Id,
                Type = "SAR",
                Title = $"SAR: {sar.ReportNumber}",
                Description = $"Subject: {sar.SubjectName}, Status: {sar.Status}",
                Timestamp = sar.CreatedAt,
                Priority = sar.Priority.ToString(),
                Url = $"/reports/sar/{sar.Id}"
            })
            .ToListAsync();

        activities.AddRange(recentSars);

        var recentStrs = await _context.SuspiciousTransactionReports
            .Where(str => str.OrganizationId == organizationId)
            .OrderByDescending(str => str.CreatedAt)
            .Take(count / 4)
            .Select(str => new RecentActivity
            {
                Id = str.Id,
                Type = "STR",
                Title = $"STR: {str.ReportNumber}",
                Description = $"Amount: {str.TransactionCurrency} {str.TransactionAmount:N2}, Status: {str.Status}",
                Timestamp = str.CreatedAt,
                Priority = str.Priority.ToString(),
                Url = $"/reports/str/{str.Id}"
            })
            .ToListAsync();

        activities.AddRange(recentStrs);

        return activities.OrderByDescending(a => a.Timestamp).Take(count);
    }

    public async Task<PerformanceMetrics> GetPerformanceMetricsAsync(Guid organizationId)
    {
        var today = DateTime.UtcNow.Date;
        var thirtyDaysAgo = today.AddDays(-30);

        var avgResolutionTime = await _context.Alerts
            .Where(a => a.OrganizationId == organizationId && 
                       a.Status == AlertStatus.Resolved && 
                       a.ResolvedAt.HasValue &&
                       a.CreatedAt >= thirtyDaysAgo)
            .Select(a => EF.Functions.DateDiffHour(a.CreatedAt, a.ResolvedAt!.Value))
            .DefaultIfEmpty(0)
            .AverageAsync();

        var totalScreenings = await _context.ScreeningMetrics
            .Where(sm => sm.OrganizationId == organizationId && sm.Date >= thirtyDaysAgo)
            .SumAsync(sm => sm.CustomersScreened + sm.TransactionsScreened);

        var totalMatches = await _context.ScreeningMetrics
            .Where(sm => sm.OrganizationId == organizationId && sm.Date >= thirtyDaysAgo)
            .SumAsync(sm => sm.PepMatches + sm.SanctionMatches + sm.WatchlistMatches);

        var accuracyRate = totalScreenings > 0 ? 
            await _context.ScreeningMetrics
                .Where(sm => sm.OrganizationId == organizationId && sm.Date >= thirtyDaysAgo)
                .AverageAsync(sm => sm.AccuracyRate) : 0;

        return new PerformanceMetrics
        {
            AverageResolutionTimeHours = avgResolutionTime,
            TotalScreenings = totalScreenings,
            TotalMatches = totalMatches,
            AccuracyRate = accuracyRate,
            MatchRate = totalScreenings > 0 ? (double)totalMatches / totalScreenings * 100 : 0
        };
    }

    public async Task UpdateDashboardMetricsAsync(Guid organizationId)
    {
        var today = DateTime.UtcNow.Date;
        
        // Check if metrics for today already exist
        var existingMetrics = await _context.DashboardMetrics
            .FirstOrDefaultAsync(dm => dm.OrganizationId == organizationId && 
                                      dm.MetricDate == today && 
                                      dm.MetricType == "Daily");

        if (existingMetrics != null)
        {
            // Update existing metrics
            await UpdateExistingMetricsAsync(existingMetrics);
        }
        else
        {
            // Create new metrics
            await CreateNewMetricsAsync(organizationId, today);
        }

        await _context.SaveChangesAsync();
    }

    private async Task<double> CalculateTrendAsync(Guid organizationId, string type, DateTime startDate, DateTime endDate)
    {
        var currentPeriodCount = 0;
        var previousPeriodCount = 0;
        var periodDays = (endDate - startDate).Days;
        var previousStartDate = startDate.AddDays(-periodDays);

        switch (type.ToLower())
        {
            case "alerts":
                currentPeriodCount = await _context.Alerts
                    .CountAsync(a => a.OrganizationId == organizationId && 
                               a.CreatedAt >= startDate && a.CreatedAt < endDate);
                previousPeriodCount = await _context.Alerts
                    .CountAsync(a => a.OrganizationId == organizationId && 
                               a.CreatedAt >= previousStartDate && a.CreatedAt < startDate);
                break;
            case "sars":
                currentPeriodCount = await _context.SuspiciousActivityReports
                    .CountAsync(sar => sar.OrganizationId == organizationId && 
                                     sar.CreatedAt >= startDate && sar.CreatedAt < endDate);
                previousPeriodCount = await _context.SuspiciousActivityReports
                    .CountAsync(sar => sar.OrganizationId == organizationId && 
                                     sar.CreatedAt >= previousStartDate && sar.CreatedAt < startDate);
                break;
            case "strs":
                currentPeriodCount = await _context.SuspiciousTransactionReports
                    .CountAsync(str => str.OrganizationId == organizationId && 
                                     str.CreatedAt >= startDate && str.CreatedAt < endDate);
                previousPeriodCount = await _context.SuspiciousTransactionReports
                    .CountAsync(str => str.OrganizationId == organizationId && 
                                     str.CreatedAt >= previousStartDate && str.CreatedAt < startDate);
                break;
        }

        if (previousPeriodCount == 0) return currentPeriodCount > 0 ? 100 : 0;
        return ((double)(currentPeriodCount - previousPeriodCount) / previousPeriodCount) * 100;
    }

    private async Task UpdateExistingMetricsAsync(DashboardMetrics metrics)
    {
        // Update with current day's data
        var today = DateTime.UtcNow.Date;
        var organizationId = metrics.OrganizationId;

        metrics.TotalAlertsGenerated = await _context.Alerts
            .CountAsync(a => a.OrganizationId == organizationId && a.CreatedAt.Date == today);

        metrics.SarReportsCreated = await _context.SuspiciousActivityReports
            .CountAsync(sar => sar.OrganizationId == organizationId && sar.CreatedAt.Date == today);

        metrics.StrReportsCreated = await _context.SuspiciousTransactionReports
            .CountAsync(str => str.OrganizationId == organizationId && str.CreatedAt.Date == today);

        _context.DashboardMetrics.Update(metrics);
    }

    private async Task CreateNewMetricsAsync(Guid organizationId, DateTime date)
    {
        var metrics = new DashboardMetrics
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            MetricDate = date,
            MetricType = "Daily",
            CreatedAt = DateTime.UtcNow
        };

        // Populate with current data
        metrics.TotalAlertsGenerated = await _context.Alerts
            .CountAsync(a => a.OrganizationId == organizationId && a.CreatedAt.Date == date);

        metrics.SarReportsCreated = await _context.SuspiciousActivityReports
            .CountAsync(sar => sar.OrganizationId == organizationId && sar.CreatedAt.Date == date);

        metrics.StrReportsCreated = await _context.SuspiciousTransactionReports
            .CountAsync(str => str.OrganizationId == organizationId && str.CreatedAt.Date == date);

        // Calculate compliance score (simplified)
        var totalReports = metrics.SarReportsCreated + metrics.StrReportsCreated;
        var submittedReports = await _context.SuspiciousActivityReports
            .CountAsync(sar => sar.OrganizationId == organizationId && 
                              sar.Status == SarStatus.Submitted && 
                              sar.CreatedAt.Date == date) +
            await _context.SuspiciousTransactionReports
            .CountAsync(str => str.OrganizationId == organizationId && 
                              str.Status == StrStatus.Submitted && 
                              str.CreatedAt.Date == date);

        metrics.ComplianceScore = totalReports > 0 ? (decimal)submittedReports / totalReports * 100 : 100;

        _context.DashboardMetrics.Add(metrics);
    }
}

// DTOs for dashboard data
public class DashboardOverview
{
    public int TotalCustomers { get; set; }
    public int TotalAlerts { get; set; }
    public int PendingAlerts { get; set; }
    public int HighRiskAlerts { get; set; }
    public int TotalSars { get; set; }
    public int TotalStrs { get; set; }
    public int PendingSars { get; set; }
    public int PendingStrs { get; set; }
    public double AlertsTrend { get; set; }
    public double SarsTrend { get; set; }
    public double StrsTrend { get; set; }
    public decimal ComplianceScore { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class ChartData
{
    public string Label { get; set; } = string.Empty;
    public double Value { get; set; }
    public DateTime Date { get; set; }
    public string? Category { get; set; }
    public Dictionary<string, object>? AdditionalData { get; set; }
}

public class RecentActivity
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Priority { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}

public class PerformanceMetrics
{
    public double AverageResolutionTimeHours { get; set; }
    public int TotalScreenings { get; set; }
    public int TotalMatches { get; set; }
    public double AccuracyRate { get; set; }
    public double MatchRate { get; set; }
}
