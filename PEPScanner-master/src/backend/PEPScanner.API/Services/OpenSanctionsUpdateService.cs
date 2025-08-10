using PEPScanner.Infrastructure.Services;
using Hangfire;

namespace PEPScanner.API.Services
{
    public interface IOpenSanctionsUpdateService
    {
        Task ScheduleRecurringUpdatesAsync();
        Task UpdateOpenSanctionsDataAsync();
    }

    public class OpenSanctionsUpdateService : IOpenSanctionsUpdateService
    {
        private readonly IOpenSanctionsDataService _openSanctionsDataService;
        private readonly ILogger<OpenSanctionsUpdateService> _logger;

        public OpenSanctionsUpdateService(
            IOpenSanctionsDataService openSanctionsDataService,
            ILogger<OpenSanctionsUpdateService> logger)
        {
            _openSanctionsDataService = openSanctionsDataService;
            _logger = logger;
        }

        public Task ScheduleRecurringUpdatesAsync()
        {
            try
            {
                _logger.LogInformation("Setting up recurring OpenSanctions data update jobs");

                // Schedule daily update at 2 AM UTC
                RecurringJob.AddOrUpdate(
                    "opensanctions-daily-update",
                    () => UpdateOpenSanctionsDataAsync(),
                    "0 2 * * *", // Daily at 2 AM UTC
                    TimeZoneInfo.Utc);

                _logger.LogInformation("Recurring OpenSanctions update jobs scheduled successfully");
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting up recurring OpenSanctions update jobs");
                throw;
            }
        }

        [AutomaticRetry(Attempts = 3)]
        public async Task UpdateOpenSanctionsDataAsync()
        {
            try
            {
                _logger.LogInformation("Starting scheduled OpenSanctions data update");

                var success = await _openSanctionsDataService.DownloadAndUpdateDataAsync();

                if (success)
                {
                    var totalEntities = await _openSanctionsDataService.GetTotalEntitiesCountAsync();
                    _logger.LogInformation("Scheduled OpenSanctions data update completed successfully. Total entities: {TotalEntities}", totalEntities);
                }
                else
                {
                    _logger.LogWarning("Scheduled OpenSanctions data update failed");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during scheduled OpenSanctions data update");
                throw;
            }
        }
    }
}
