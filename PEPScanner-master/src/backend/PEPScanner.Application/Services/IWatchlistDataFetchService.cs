using PEPScanner.Application.Contracts;

namespace PEPScanner.Application.Services
{
    public interface IWatchlistDataFetchService
    {
        /// <summary>
        /// Fetch data from OFAC (US Treasury) sanctions list
        /// </summary>
        Task<WatchlistUpdateResult> FetchOfacDataAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Fetch data from UN Sanctions list
        /// </summary>
        Task<WatchlistUpdateResult> FetchUnSanctionsDataAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Fetch data from RBI (Reserve Bank of India) sanctions list
        /// </summary>
        Task<WatchlistUpdateResult> FetchRbiDataAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Fetch data from SEBI (Securities Exchange Board of India) list
        /// </summary>
        Task<WatchlistUpdateResult> FetchSebiDataAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Fetch data from EU Sanctions list
        /// </summary>
        Task<WatchlistUpdateResult> FetchEuSanctionsDataAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Fetch data from UK Sanctions list
        /// </summary>
        Task<WatchlistUpdateResult> FetchUkSanctionsDataAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Fetch Indian Parliament members (PEP) data
        /// </summary>
        Task<WatchlistUpdateResult> FetchIndianParliamentDataAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Fetch all watchlist data from all sources
        /// </summary>
        Task<List<WatchlistUpdateResult>> FetchAllWatchlistDataAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Get the last update timestamp for a specific source
        /// </summary>
        Task<DateTime?> GetLastUpdateAsync(string source);

        /// <summary>
        /// Check if a source needs updating based on configured frequency
        /// </summary>
        Task<bool> ShouldUpdateSourceAsync(string source);
    }
}
