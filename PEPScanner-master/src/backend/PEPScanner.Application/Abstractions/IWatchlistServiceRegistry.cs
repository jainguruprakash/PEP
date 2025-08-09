using PEPScanner.Domain.Entities;

namespace PEPScanner.Application.Abstractions
{
    public interface IWatchlistServiceRegistry
    {
        void RegisterService(IBaseWatchlistService service);
        IBaseWatchlistService? GetService(string sourceName);
        List<IBaseWatchlistService> GetAllServices();
        List<IBaseWatchlistService> GetServicesByType(string watchlistType);
        List<IBaseWatchlistService> GetServicesByCountry(string country);
        Task<WatchlistUpdateResult> UpdateAllWatchlistsAsync();
        Task<List<WatchlistEntry>> SearchAllWatchlistsAsync(string name);
    }
}
