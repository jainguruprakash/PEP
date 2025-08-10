using Hangfire;
using Hangfire.Dashboard;
using PEPScanner.Infrastructure.Data;
using PEPScanner.Domain.Entities;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Options;

namespace PEPScanner.API
{
    // Hangfire authorization filter
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context) => true; // Allow all for development
    }

    // Seed data method
    public static class SeedDataHelper
    {
        public static async Task SeedDataAsync(PepScannerDbContext context)
        {
            if (!context.WatchlistEntries.Any())
            {
                var watchlistEntries = new[]
                {
                    new WatchlistEntry
                    {
                        Id = Guid.NewGuid(),
                        Source = "OFAC",
                        ListType = "Sanctions",
                        PrimaryName = "John Doe",
                        Country = "United States",
                        RiskLevel = "High",
                        EntityType = "Individual",
                        DateAddedUtc = DateTime.UtcNow,
                        IsActive = true
                    },
                    new WatchlistEntry
                    {
                        Id = Guid.NewGuid(),
                        Source = "UN",
                        ListType = "PEP",
                        PrimaryName = "Jane Smith",
                        Country = "India",
                        RiskLevel = "Medium",
                        EntityType = "Individual",
                        PepPosition = "Member of Parliament",
                        PepCountry = "India",
                        DateAddedUtc = DateTime.UtcNow,
                        IsActive = true
                    },
                    new WatchlistEntry
                    {
                        Id = Guid.NewGuid(),
                        Source = "RBI",
                        ListType = "Local Lists",
                        PrimaryName = "Rajesh Kumar",
                        Country = "India",
                        RiskLevel = "Critical",
                        EntityType = "Individual",
                        DateAddedUtc = DateTime.UtcNow,
                        IsActive = true
                    }
                };

                context.WatchlistEntries.AddRange(watchlistEntries);
                await context.SaveChangesAsync();
            }
        }
    }
}
