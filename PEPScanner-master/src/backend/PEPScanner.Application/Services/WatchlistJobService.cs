using Hangfire;
using Microsoft.Extensions.Logging;
using PEPScanner.Application.Contracts;

namespace PEPScanner.Application.Services
{
    public class WatchlistJobService : IWatchlistJobService
    {
        private readonly IWatchlistDataFetchService _fetchService;
        private readonly ILogger<WatchlistJobService> _logger;

        public WatchlistJobService(
            IWatchlistDataFetchService fetchService,
            ILogger<WatchlistJobService> logger)
        {
            _fetchService = fetchService;
            _logger = logger;
        }

        public async Task<string> ScheduleFetchJobAsync(string source)
        {
            _logger.LogInformation("Scheduling fetch job for source: {Source}", source);

            var jobId = source.ToUpper() switch
            {
                "OFAC" => BackgroundJob.Enqueue(() => _fetchService.FetchOfacDataAsync(CancellationToken.None)),
                "UN" => BackgroundJob.Enqueue(() => _fetchService.FetchUnSanctionsDataAsync(CancellationToken.None)),
                "RBI" => BackgroundJob.Enqueue(() => _fetchService.FetchRbiDataAsync(CancellationToken.None)),
                "SEBI" => BackgroundJob.Enqueue(() => _fetchService.FetchSebiDataAsync(CancellationToken.None)),
                "EU" => BackgroundJob.Enqueue(() => _fetchService.FetchEuSanctionsDataAsync(CancellationToken.None)),
                "UK" => BackgroundJob.Enqueue(() => _fetchService.FetchUkSanctionsDataAsync(CancellationToken.None)),
                "PARLIAMENT" => BackgroundJob.Enqueue(() => _fetchService.FetchIndianParliamentDataAsync(CancellationToken.None)),
                _ => throw new ArgumentException($"Unknown source: {source}")
            };

            _logger.LogInformation("Scheduled job {JobId} for source {Source}", jobId, source);
            return jobId;
        }

        public async Task<string> ScheduleFetchAllJobAsync()
        {
            _logger.LogInformation("Scheduling fetch job for all sources");

            var jobId = BackgroundJob.Enqueue(() => _fetchService.FetchAllWatchlistDataAsync(CancellationToken.None));

            _logger.LogInformation("Scheduled batch job {JobId} for all sources", jobId);
            return jobId;
        }

        public async Task ScheduleRecurringJobsAsync()
        {
            _logger.LogInformation("Setting up recurring jobs for all watchlist sources");

            // OFAC - Every 12 hours
            RecurringJob.AddOrUpdate(
                "ofac-fetch",
                () => _fetchService.FetchOfacDataAsync(CancellationToken.None),
                "0 */12 * * *", // Every 12 hours
                TimeZoneInfo.Utc);

            // UN Sanctions - Every 12 hours
            RecurringJob.AddOrUpdate(
                "un-fetch",
                () => _fetchService.FetchUnSanctionsDataAsync(CancellationToken.None),
                "30 */12 * * *", // Every 12 hours, offset by 30 minutes
                TimeZoneInfo.Utc);

            // RBI - Weekly on Mondays at 2 AM
            RecurringJob.AddOrUpdate(
                "rbi-fetch",
                () => _fetchService.FetchRbiDataAsync(CancellationToken.None),
                "0 2 * * 1", // Monday at 2 AM
                TimeZoneInfo.Utc);

            // SEBI - Weekly on Tuesdays at 2 AM
            RecurringJob.AddOrUpdate(
                "sebi-fetch",
                () => _fetchService.FetchSebiDataAsync(CancellationToken.None),
                "0 2 * * 2", // Tuesday at 2 AM
                TimeZoneInfo.Utc);

            // EU Sanctions - Daily at 3 AM
            RecurringJob.AddOrUpdate(
                "eu-fetch",
                () => _fetchService.FetchEuSanctionsDataAsync(CancellationToken.None),
                "0 3 * * *", // Daily at 3 AM
                TimeZoneInfo.Utc);

            // UK Sanctions - Daily at 4 AM
            RecurringJob.AddOrUpdate(
                "uk-fetch",
                () => _fetchService.FetchUkSanctionsDataAsync(CancellationToken.None),
                "0 4 * * *", // Daily at 4 AM
                TimeZoneInfo.Utc);

            // Indian Parliament - Monthly on 1st at 1 AM
            RecurringJob.AddOrUpdate(
                "parliament-fetch",
                () => _fetchService.FetchIndianParliamentDataAsync(CancellationToken.None),
                "0 1 1 * *", // 1st of every month at 1 AM
                TimeZoneInfo.Utc);

            _logger.LogInformation("Successfully set up recurring jobs for all watchlist sources");
        }

        public async Task<bool> CancelJobAsync(string jobId)
        {
            try
            {
                var result = BackgroundJob.Delete(jobId);
                _logger.LogInformation("Job {JobId} cancellation result: {Result}", jobId, result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling job {JobId}", jobId);
                return false;
            }
        }

        public async Task<object> GetJobStatusAsync(string jobId)
        {
            try
            {
                var connection = JobStorage.Current.GetConnection();
                var jobData = connection.GetJobData(jobId);
                
                if (jobData == null)
                {
                    return new { JobId = jobId, Status = "NotFound" };
                }

                return new
                {
                    JobId = jobId,
                    Status = jobData.State,
                    Job = jobData.Job?.ToString(),
                    CreatedAt = jobData.CreatedAt,
                    StateHistory = connection.GetStateHistory(jobId)
                        .Select(s => new { s.StateName, s.CreatedAt, s.Reason })
                        .ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting job status for {JobId}", jobId);
                return new { JobId = jobId, Status = "Error", Error = ex.Message };
            }
        }

        public async Task<List<object>> GetAllJobsAsync()
        {
            try
            {
                var connection = JobStorage.Current.GetConnection();
                var recurringJobs = connection.GetRecurringJobs();

                return recurringJobs.Select(job => new
                {
                    Id = job.Id,
                    Cron = job.Cron,
                    NextExecution = job.NextExecution,
                    LastExecution = job.LastExecution,
                    Job = job.Job?.ToString(),
                    TimeZone = job.TimeZoneId
                }).Cast<object>().ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all jobs");
                return new List<object>();
            }
        }

        // Manual trigger methods
        public async Task<WatchlistUpdateResult> TriggerOfacFetchAsync()
        {
            _logger.LogInformation("Manual trigger for OFAC data fetch");
            return await _fetchService.FetchOfacDataAsync();
        }

        public async Task<WatchlistUpdateResult> TriggerUnFetchAsync()
        {
            _logger.LogInformation("Manual trigger for UN data fetch");
            return await _fetchService.FetchUnSanctionsDataAsync();
        }

        public async Task<WatchlistUpdateResult> TriggerRbiFetchAsync()
        {
            _logger.LogInformation("Manual trigger for RBI data fetch");
            return await _fetchService.FetchRbiDataAsync();
        }

        public async Task<WatchlistUpdateResult> TriggerSebiFetchAsync()
        {
            _logger.LogInformation("Manual trigger for SEBI data fetch");
            return await _fetchService.FetchSebiDataAsync();
        }

        public async Task<WatchlistUpdateResult> TriggerEuFetchAsync()
        {
            _logger.LogInformation("Manual trigger for EU data fetch");
            return await _fetchService.FetchEuSanctionsDataAsync();
        }

        public async Task<WatchlistUpdateResult> TriggerUkFetchAsync()
        {
            _logger.LogInformation("Manual trigger for UK data fetch");
            return await _fetchService.FetchUkSanctionsDataAsync();
        }

        public async Task<WatchlistUpdateResult> TriggerParliamentFetchAsync()
        {
            _logger.LogInformation("Manual trigger for Parliament data fetch");
            return await _fetchService.FetchIndianParliamentDataAsync();
        }

        public async Task<List<WatchlistUpdateResult>> TriggerAllFetchAsync()
        {
            _logger.LogInformation("Manual trigger for all sources data fetch");
            return await _fetchService.FetchAllWatchlistDataAsync();
        }
    }
}
