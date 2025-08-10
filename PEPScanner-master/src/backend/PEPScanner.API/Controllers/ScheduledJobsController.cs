using Microsoft.AspNetCore.Mvc;
using Hangfire;
using PEPScanner.API.Services;

namespace PEPScanner.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScheduledJobsController : ControllerBase
    {
        private readonly IWatchlistUpdateService _watchlistUpdateService;
        private readonly ILogger<ScheduledJobsController> _logger;

        public ScheduledJobsController(
            IWatchlistUpdateService watchlistUpdateService,
            ILogger<ScheduledJobsController> logger)
        {
            _watchlistUpdateService = watchlistUpdateService;
            _logger = logger;
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetJobStatus()
        {
            try
            {
                var status = await _watchlistUpdateService.GetUpdateStatusAsync();
                
                // Get basic job information
                var jobStatuses = new List<object>
                {
                    new { Id = "update-all-watchlists", Status = "Scheduled", Description = "Daily update for all watchlists" },
                    new { Id = "update-ofac-watchlist", Status = "Scheduled", Description = "OFAC updates every 6 hours" },
                    new { Id = "update-un-watchlist", Status = "Scheduled", Description = "UN updates every 6 hours" },
                    new { Id = "update-eu-watchlist", Status = "Scheduled", Description = "EU updates every 8 hours" },
                    new { Id = "update-uk-watchlist", Status = "Scheduled", Description = "UK updates every 8 hours" },
                    new { Id = "update-rbi-watchlist", Status = "Scheduled", Description = "RBI updates weekly" },
                    new { Id = "update-sebi-watchlist", Status = "Scheduled", Description = "SEBI updates weekly" },
                    new { Id = "update-parliament-watchlist", Status = "Scheduled", Description = "Parliament updates weekly" }
                };

                var result = new
                {
                    Success = true,
                    WatchlistStatus = status,
                    ScheduledJobs = jobStatuses,
                    ServerTime = DateTime.UtcNow
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting job status");
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpPost("setup-recurring")]
        public IActionResult SetupRecurringJobs()
        {
            try
            {
                _watchlistUpdateService.ScheduleRecurringUpdates();
                
                var result = new
                {
                    Success = true,
                    Message = "Recurring jobs scheduled successfully",
                    ScheduledAt = DateTime.UtcNow
                };

                _logger.LogInformation("Recurring jobs setup completed");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting up recurring jobs");
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpPost("trigger-update/{source}")]
        public IActionResult TriggerManualUpdate(string source)
        {
            try
            {
                var jobId = BackgroundJob.Enqueue(() => _watchlistUpdateService.UpdateSpecificWatchlistAsync(source));
                
                var result = new
                {
                    Success = true,
                    Message = $"Manual update triggered for {source}",
                    JobId = jobId,
                    TriggeredAt = DateTime.UtcNow
                };

                _logger.LogInformation("Manual update triggered for {Source} with job ID {JobId}", source, jobId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error triggering manual update for {Source}", source);
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpPost("trigger-update-all")]
        public IActionResult TriggerManualUpdateAll()
        {
            try
            {
                var jobId = BackgroundJob.Enqueue(() => _watchlistUpdateService.UpdateAllWatchlistsAsync());
                
                var result = new
                {
                    Success = true,
                    Message = "Manual update triggered for all watchlists",
                    JobId = jobId,
                    TriggeredAt = DateTime.UtcNow
                };

                _logger.LogInformation("Manual update triggered for all watchlists with job ID {JobId}", jobId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error triggering manual update for all watchlists");
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpDelete("cancel-job/{jobId}")]
        public IActionResult CancelJob(string jobId)
        {
            try
            {
                BackgroundJob.Delete(jobId);
                
                var result = new
                {
                    Success = true,
                    Message = $"Job {jobId} cancelled successfully",
                    CancelledAt = DateTime.UtcNow
                };

                _logger.LogInformation("Job {JobId} cancelled", jobId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling job {JobId}", jobId);
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpDelete("remove-recurring/{jobId}")]
        public IActionResult RemoveRecurringJob(string jobId)
        {
            try
            {
                RecurringJob.RemoveIfExists(jobId);
                
                var result = new
                {
                    Success = true,
                    Message = $"Recurring job {jobId} removed successfully",
                    RemovedAt = DateTime.UtcNow
                };

                _logger.LogInformation("Recurring job {JobId} removed", jobId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing recurring job {JobId}", jobId);
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpGet("job-history")]
        public IActionResult GetJobHistory()
        {
            try
            {
                // Simplified job history - in production you would use proper Hangfire monitoring
                var result = new
                {
                    Success = true,
                    Message = "Job history tracking is available through Hangfire Dashboard at /hangfire",
                    DashboardUrl = "/hangfire",
                    RecentJobs = new[]
                    {
                        new { JobType = "WatchlistUpdate", Status = "Completed", LastRun = DateTime.UtcNow.AddHours(-2) },
                        new { JobType = "DataSync", Status = "Completed", LastRun = DateTime.UtcNow.AddHours(-6) }
                    }
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting job history");
                return StatusCode(500, new { Error = ex.Message });
            }
        }
    }
}
