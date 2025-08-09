using PEPScanner.API.Models;

namespace PEPScanner.API.Services
{
    /// <summary>
    /// Registry for managing all watchlist services
    /// </summary>
    public interface IWatchlistServiceRegistry
    {
        /// <summary>
        /// Gets a watchlist service by source name
        /// </summary>
        /// <param name="sourceName">Source name (e.g., "OFAC", "RBI", "SEBI")</param>
        /// <returns>Watchlist service or null if not found</returns>
        IBaseWatchlistService? GetService(string sourceName);

        /// <summary>
        /// Gets all available watchlist services
        /// </summary>
        /// <returns>List of all registered services</returns>
        IEnumerable<IBaseWatchlistService> GetAllServices();

        /// <summary>
        /// Gets all active watchlist services
        /// </summary>
        /// <returns>List of active services</returns>
        IEnumerable<IBaseWatchlistService> GetActiveServices();

        /// <summary>
        /// Gets services by type (Global, Local, InHouse)
        /// </summary>
        /// <param name="type">Service type</param>
        /// <returns>List of services of the specified type</returns>
        IEnumerable<IBaseWatchlistService> GetServicesByType(string type);

        /// <summary>
        /// Gets services by country
        /// </summary>
        /// <param name="country">Country name</param>
        /// <returns>List of services for the specified country</returns>
        IEnumerable<IBaseWatchlistService> GetServicesByCountry(string country);

        /// <summary>
        /// Gets all available source configurations
        /// </summary>
        /// <returns>List of source configurations</returns>
        List<WatchlistSource> GetAllSourceConfigurations();

        /// <summary>
        /// Registers a new watchlist service
        /// </summary>
        /// <param name="service">Service to register</param>
        void RegisterService(IBaseWatchlistService service);

        /// <summary>
        /// Unregisters a watchlist service
        /// </summary>
        /// <param name="sourceName">Source name to unregister</param>
        void UnregisterService(string sourceName);
    }

    /// <summary>
    /// Implementation of watchlist service registry
    /// </summary>
    public class WatchlistServiceRegistry : IWatchlistServiceRegistry
    {
        private readonly Dictionary<string, IBaseWatchlistService> _services = new();
        private readonly ILogger<WatchlistServiceRegistry> _logger;

        public WatchlistServiceRegistry(ILogger<WatchlistServiceRegistry> logger)
        {
            _logger = logger;
        }

        public IBaseWatchlistService? GetService(string sourceName)
        {
            return _services.TryGetValue(sourceName, out var service) ? service : null;
        }

        public IEnumerable<IBaseWatchlistService> GetAllServices()
        {
            return _services.Values;
        }

        public IEnumerable<IBaseWatchlistService> GetActiveServices()
        {
            return _services.Values.Where(s => s.GetSourceConfiguration().IsActive);
        }

        public IEnumerable<IBaseWatchlistService> GetServicesByType(string type)
        {
            return _services.Values.Where(s => s.WatchlistType.Equals(type, StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerable<IBaseWatchlistService> GetServicesByCountry(string country)
        {
            return _services.Values.Where(s => s.Country.Equals(country, StringComparison.OrdinalIgnoreCase));
        }

        public List<WatchlistSource> GetAllSourceConfigurations()
        {
            return _services.Values.Select(s => s.GetSourceConfiguration()).ToList();
        }

        public void RegisterService(IBaseWatchlistService service)
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));

            var sourceName = service.SourceName;
            if (string.IsNullOrEmpty(sourceName))
                throw new ArgumentException("Service source name cannot be null or empty");

            if (_services.ContainsKey(sourceName))
            {
                _logger.LogWarning("Service with source name '{SourceName}' is already registered. Replacing existing service.", sourceName);
            }

            _services[sourceName] = service;
            _logger.LogInformation("Registered watchlist service: {SourceName} ({DisplayName})", sourceName, service.DisplayName);
        }

        public void UnregisterService(string sourceName)
        {
            if (string.IsNullOrEmpty(sourceName))
                throw new ArgumentException("Source name cannot be null or empty");

            if (_services.Remove(sourceName))
            {
                _logger.LogInformation("Unregistered watchlist service: {SourceName}", sourceName);
            }
            else
            {
                _logger.LogWarning("Attempted to unregister non-existent service: {SourceName}", sourceName);
            }
        }
    }
}
