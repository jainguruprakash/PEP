using Microsoft.EntityFrameworkCore;
using PEPScanner.Infrastructure.Data;

namespace PEPScanner.API.Data;

public static class ClearDatabase
{
    public static async Task ClearAllDataAsync(PepScannerDbContext context)
    {
        await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"AlertMetrics\" RESTART IDENTITY CASCADE");
        await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Alerts\" RESTART IDENTITY CASCADE");
        await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"ComplianceReports\" RESTART IDENTITY CASCADE");
        await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Customers\" RESTART IDENTITY CASCADE");
        await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"DashboardMetrics\" RESTART IDENTITY CASCADE");
        await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"NotificationRules\" RESTART IDENTITY CASCADE");
        await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"OrganizationUsers\" RESTART IDENTITY CASCADE");
        await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Organizations\" RESTART IDENTITY CASCADE");
        await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"SarComments\" RESTART IDENTITY CASCADE");
        await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"SarStatusHistories\" RESTART IDENTITY CASCADE");
        await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"ScreeningMetrics\" RESTART IDENTITY CASCADE");
        await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"StrComments\" RESTART IDENTITY CASCADE");
        await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"StrStatusHistories\" RESTART IDENTITY CASCADE");
        await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"SuspiciousActivityReports\" RESTART IDENTITY CASCADE");
        await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"SuspiciousTransactionReports\" RESTART IDENTITY CASCADE");
        await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Teams\" RESTART IDENTITY CASCADE");
        await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Transactions\" RESTART IDENTITY CASCADE");
        await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Users\" RESTART IDENTITY CASCADE");
        await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"WatchlistEntries\" RESTART IDENTITY CASCADE");
    }
}