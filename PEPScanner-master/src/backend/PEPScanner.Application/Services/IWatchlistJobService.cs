using PEPScanner.Application.Contracts;

namespace PEPScanner.Application.Services
{
    public interface IWatchlistJobService
    {
        /// <summary>
        /// Schedule a one-time job to fetch data from a specific source
        /// </summary>
        Task<string> ScheduleFetchJobAsync(string source);

        /// <summary>
        /// Schedule a one-time job to fetch data from all sources
        /// </summary>
        Task<string> ScheduleFetchAllJobAsync();

        /// <summary>
        /// Schedule recurring jobs for all watchlist sources
        /// </summary>
        Task ScheduleRecurringJobsAsync();

        /// <summary>
        /// Cancel a scheduled job
        /// </summary>
        Task<bool> CancelJobAsync(string jobId);

        /// <summary>
        /// Get job status
        /// </summary>
        Task<object> GetJobStatusAsync(string jobId);

        /// <summary>
        /// Get all scheduled jobs
        /// </summary>
        Task<List<object>> GetAllJobsAsync();

        /// <summary>
        /// Manual trigger for OFAC data fetch
        /// </summary>
        Task<WatchlistUpdateResult> TriggerOfacFetchAsync();

        /// <summary>
        /// Manual trigger for UN data fetch
        /// </summary>
        Task<WatchlistUpdateResult> TriggerUnFetchAsync();

        /// <summary>
        /// Manual trigger for RBI data fetch
        /// </summary>
        Task<WatchlistUpdateResult> TriggerRbiFetchAsync();

        /// <summary>
        /// Manual trigger for SEBI data fetch
        /// </summary>
        Task<WatchlistUpdateResult> TriggerSebiFetchAsync();

        /// <summary>
        /// Manual trigger for EU data fetch
        /// </summary>
        Task<WatchlistUpdateResult> TriggerEuFetchAsync();

        /// <summary>
        /// Manual trigger for UK data fetch
        /// </summary>
        Task<WatchlistUpdateResult> TriggerUkFetchAsync();

        /// <summary>
        /// Manual trigger for Parliament data fetch
        /// </summary>
        Task<WatchlistUpdateResult> TriggerParliamentFetchAsync();

        /// <summary>
        /// Manual trigger for all sources
        /// </summary>
        Task<List<WatchlistUpdateResult>> TriggerAllFetchAsync();
    }
}
