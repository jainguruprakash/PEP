using PEPScanner.Domain.Entities;

namespace PEPScanner.Application.Abstractions
{
    public interface IBaseWatchlistService
    {
        string SourceName { get; }
        string DisplayName { get; }
        string WatchlistType { get; }
        string Country { get; }
        Task<List<WatchlistEntry>> FetchWatchlistDataAsync();
        Task<WatchlistUpdateResult> UpdateWatchlistAsync();
        Task<List<WatchlistEntry>> SearchByNameAsync(string name);
        Task<WatchlistUpdateResult> ProcessFileAsync(IFormFile file);
        Task<DateTime?> GetLastUpdateTimestampAsync();
    }
}
