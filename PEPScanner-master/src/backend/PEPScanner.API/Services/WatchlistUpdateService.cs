using Hangfire;
using Microsoft.EntityFrameworkCore;
using PEPScanner.Infrastructure.Data;
using PEPScanner.API.Controllers;

namespace PEPScanner.API.Services
{
    public interface IWatchlistUpdateService
    {
        Task UpdateAllWatchlistsAsync();
        Task UpdateSpecificWatchlistAsync(string source);
        void ScheduleRecurringUpdates();
        Task<Dictionary<string, object>> GetUpdateStatusAsync();
    }

    public class WatchlistUpdateService : IWatchlistUpdateService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<WatchlistUpdateService> _logger;
        private readonly PepScannerDbContext _context;

        public WatchlistUpdateService(
            IServiceProvider serviceProvider,
            ILogger<WatchlistUpdateService> logger,
            PepScannerDbContext context)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _context = context;
        }

        public async Task UpdateAllWatchlistsAsync()
        {
            _logger.LogInformation("Starting scheduled update for all watchlist sources");

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var watchlistController = scope.ServiceProvider.GetRequiredService<WatchlistDataController>();

                // Call the fetch all method
                var result = await watchlistController.FetchAllData();
                
                _logger.LogInformation("Completed scheduled update for all watchlist sources");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during scheduled watchlist update");
                throw;
            }
        }

        public async Task UpdateSpecificWatchlistAsync(string source)
        {
            _logger.LogInformation("Starting scheduled update for {Source} watchlist", source);

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var watchlistController = scope.ServiceProvider.GetRequiredService<WatchlistDataController>();

                switch (source.ToUpper())
                {
                    case "OFAC":
                        await watchlistController.FetchOfacData();
                        break;
                    case "UN":
                        await watchlistController.FetchUnData();
                        break;
                    case "RBI":
                        await watchlistController.FetchRbiData();
                        break;
                    case "EU":
                        await watchlistController.FetchEuData();
                        break;
                    case "UK":
                        await watchlistController.FetchUkData();
                        break;
                    case "SEBI":
                        await watchlistController.FetchSebiData();
                        break;
                    case "PARLIAMENT":
                        await watchlistController.FetchParliamentData();
                        break;
                    default:
                        _logger.LogWarning("Unknown watchlist source: {Source}", source);
                        break;
                }

                _logger.LogInformation("Completed scheduled update for {Source} watchlist", source);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during scheduled {Source} watchlist update", source);
                throw;
            }
        }

        public void ScheduleRecurringUpdates()
        {
            _logger.LogInformation("Setting up recurring watchlist update jobs");

            // Schedule daily updates for all sources at 2 AM
            RecurringJob.AddOrUpdate(
                "update-all-watchlists",
                () => UpdateAllWatchlistsAsync(),
                "0 2 * * *"); // Daily at 2 AM

            // Schedule more frequent updates for critical sources
            RecurringJob.AddOrUpdate(
                "update-ofac-watchlist",
                () => UpdateSpecificWatchlistAsync("OFAC"),
                "0 */6 * * *"); // Every 6 hours

            RecurringJob.AddOrUpdate(
                "update-un-watchlist",
                () => UpdateSpecificWatchlistAsync("UN"),
                "0 */6 * * *"); // Every 6 hours

            RecurringJob.AddOrUpdate(
                "update-eu-watchlist",
                () => UpdateSpecificWatchlistAsync("EU"),
                "0 */8 * * *"); // Every 8 hours

            RecurringJob.AddOrUpdate(
                "update-uk-watchlist",
                () => UpdateSpecificWatchlistAsync("UK"),
                "0 */8 * * *"); // Every 8 hours

            // Schedule weekly updates for domestic sources
            RecurringJob.AddOrUpdate(
                "update-rbi-watchlist",
                () => UpdateSpecificWatchlistAsync("RBI"),
                "0 3 * * 1"); // Weekly on Monday at 3 AM

            RecurringJob.AddOrUpdate(
                "update-sebi-watchlist",
                () => UpdateSpecificWatchlistAsync("SEBI"),
                "0 4 * * 1"); // Weekly on Monday at 4 AM

            RecurringJob.AddOrUpdate(
                "update-parliament-watchlist",
                () => UpdateSpecificWatchlistAsync("PARLIAMENT"),
                "0 5 * * 1"); // Weekly on Monday at 5 AM

            _logger.LogInformation("Recurring watchlist update jobs scheduled successfully");
        }

        public async Task<Dictionary<string, object>> GetUpdateStatusAsync()
        {
            var sources = await _context.WatchlistEntries
                .Where(w => w.Source != null && w.Source != "")
                .GroupBy(w => w.Source)
                .Select(g => new
                {
                    Source = g.Key,
                    Count = g.Count(),
                    LastUpdated = g.Max(w => w.DateLastUpdatedUtc)
                })
                .ToListAsync();

            var status = new Dictionary<string, object>
            {
                ["TotalSources"] = sources.Count,
                ["TotalRecords"] = sources.Sum(s => s.Count),
                ["LastGlobalUpdate"] = sources.Any() ? sources.Max(s => s.LastUpdated) : (DateTime?)null,
                ["Sources"] = sources.Select(s => new
                {
                    s.Source,
                    s.Count,
                    s.LastUpdated,
                    Status = GetSourceStatus(s.LastUpdated)
                }).ToList()
            };

            return status;
        }

        private string GetSourceStatus(DateTime? lastUpdated)
        {
            if (!lastUpdated.HasValue)
                return "Never Updated";

            var hoursSinceUpdate = (DateTime.UtcNow - lastUpdated.Value).TotalHours;

            return hoursSinceUpdate switch
            {
                < 1 => "Recently Updated",
                < 6 => "Up to Date",
                < 24 => "Needs Update Soon",
                < 72 => "Update Overdue",
                _ => "Critically Outdated"
            };
        }
    }
}
