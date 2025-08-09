using PEPScanner.Domain.Entities;

namespace PEPScanner.Application.Services
{
    /// <summary>
    /// Base interface for all watchlist services to ensure consistent implementation
    /// </summary>
    public interface IBaseWatchlistService
    {
        /// <summary>
        /// Gets the source name for this watchlist service
        /// </summary>
        string SourceName { get; }

        /// <summary>
        /// Gets the display name for this watchlist service
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Gets the type of watchlist (Global, Local, InHouse)
        /// </summary>
        string WatchlistType { get; }

        /// <summary>
        /// Gets the country this watchlist is associated with
        /// </summary>
        string Country { get; }

        /// <summary>
        /// Fetches watchlist data from the source
        /// </summary>
        /// <returns>List of watchlist entries</returns>
        Task<List<WatchlistEntry>> FetchWatchlistDataAsync();

        /// <summary>
        /// Updates the local watchlist database with data from the source
        /// </summary>
        /// <returns>Update result</returns>
        Task<WatchlistUpdateResult> UpdateWatchlistAsync();

        /// <summary>
        /// Searches for entries by name
        /// </summary>
        /// <param name="name">Name to search for</param>
        /// <returns>Matching entries</returns>
        Task<List<WatchlistEntry>> SearchByNameAsync(string name);

        /// <summary>
        /// Processes a file upload for this watchlist source
        /// </summary>
        /// <param name="file">Uploaded file</param>
        /// <returns>Processing result</returns>
        Task<WatchlistUpdateResult> ProcessFileAsync(IFormFile file);

        /// <summary>
        /// Gets the last update timestamp for this watchlist
        /// </summary>
        /// <returns>Last update timestamp</returns>
        Task<DateTime?> GetLastUpdateTimestampAsync();

        /// <summary>
        /// Gets configuration information for this watchlist source
        /// </summary>
        /// <returns>Watchlist source configuration</returns>
        WatchlistSource GetSourceConfiguration();
    }

    /// <summary>
    /// Configuration for a watchlist source
    /// </summary>
    public class WatchlistSourceConfig
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
        public Dictionary<string, string> AdditionalConfig { get; set; } = new();
    }
}
