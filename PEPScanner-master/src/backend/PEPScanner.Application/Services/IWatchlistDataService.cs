using PEPScanner.Domain.Entities;

namespace PEPScanner.Application.Services
{
    public interface IWatchlistDataService
    {
        /// <summary>
        /// Updates all watchlists from external sources
        /// </summary>
        Task<WatchlistUpdateResult> UpdateAllWatchlistsAsync();

        /// <summary>
        /// Updates specific watchlist by source
        /// </summary>
        Task<WatchlistUpdateResult> UpdateWatchlistAsync(string source);

        /// <summary>
        /// Gets the last update timestamp for all sources
        /// </summary>
        Task<Dictionary<string, DateTime?>> GetLastUpdateTimestampsAsync();

        /// <summary>
        /// Processes bank's in-house watchlist file
        /// </summary>
        Task<WatchlistUpdateResult> ProcessInHouseFileAsync(IFormFile file, string sourceName);

        /// <summary>
        /// Gets available watchlist sources
        /// </summary>
        List<string> GetAvailableSources();
    }

    public class WatchlistUpdateResult
    {
        public bool Success { get; set; }
        public string Source { get; set; } = string.Empty;
        public int TotalRecords { get; set; }
        public int NewRecords { get; set; }
        public int UpdatedRecords { get; set; }
        public int DeletedRecords { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime ProcessingDate { get; set; } = DateTime.UtcNow;
        public TimeSpan ProcessingTime { get; set; }
    }

    public class WatchlistSource
    {
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // Global, Local, InHouse
        public string Country { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public string UpdateFrequency { get; set; } = "Daily";
        public string DataFormat { get; set; } = "CSV";
        public string? ApiEndpoint { get; set; }
        public string? FileUrl { get; set; }
        public string? WebScrapingUrl { get; set; }
    }
}
