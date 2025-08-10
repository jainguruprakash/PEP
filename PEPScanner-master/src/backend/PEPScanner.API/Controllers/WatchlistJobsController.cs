using Microsoft.AspNetCore.Mvc;
using PEPScanner.Application.Services;

namespace PEPScanner.API.Controllers
{
    [ApiController]
    [Route("api/watchlist-jobs")]
    public class WatchlistJobsController : ControllerBase
    {
        private readonly IWatchlistJobService _jobService;
        private readonly ILogger<WatchlistJobsController> _logger;

        public WatchlistJobsController(
            IWatchlistJobService jobService,
            ILogger<WatchlistJobsController> logger)
        {
            _jobService = jobService;
            _logger = logger;
        }

        [HttpPost("schedule/{source}")]
        public async Task<IActionResult> ScheduleSourceFetch(string source)
        {
            try
            {
                var jobId = await _jobService.ScheduleFetchJobAsync(source);
                return Ok(new
                {
                    Success = true,
                    JobId = jobId,
                    Source = source,
                    Message = $"Scheduled fetch job for {source}",
                    ScheduledAt = DateTime.UtcNow
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling job for source {Source}", source);
                return StatusCode(500, new { Error = "Internal server error" });
            }
        }

        [HttpPost("schedule-all")]
        public async Task<IActionResult> ScheduleAllFetch()
        {
            try
            {
                var jobId = await _jobService.ScheduleFetchAllJobAsync();
                return Ok(new
                {
                    Success = true,
                    JobId = jobId,
                    Message = "Scheduled fetch job for all sources",
                    ScheduledAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling job for all sources");
                return StatusCode(500, new { Error = "Internal server error" });
            }
        }

        [HttpPost("setup-recurring")]
        public async Task<IActionResult> SetupRecurringJobs()
        {
            try
            {
                await _jobService.ScheduleRecurringJobsAsync();
                return Ok(new
                {
                    Success = true,
                    Message = "Successfully set up recurring jobs for all watchlist sources",
                    SetupAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting up recurring jobs");
                return StatusCode(500, new { Error = "Internal server error" });
            }
        }

        [HttpDelete("{jobId}")]
        public async Task<IActionResult> CancelJob(string jobId)
        {
            try
            {
                var result = await _jobService.CancelJobAsync(jobId);
                if (result)
                {
                    return Ok(new
                    {
                        Success = true,
                        JobId = jobId,
                        Message = "Job cancelled successfully"
                    });
                }
                else
                {
                    return NotFound(new
                    {
                        Success = false,
                        JobId = jobId,
                        Message = "Job not found or could not be cancelled"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling job {JobId}", jobId);
                return StatusCode(500, new { Error = "Internal server error" });
            }
        }

        [HttpGet("{jobId}/status")]
        public async Task<IActionResult> GetJobStatus(string jobId)
        {
            try
            {
                var status = await _jobService.GetJobStatusAsync(jobId);
                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting job status for {JobId}", jobId);
                return StatusCode(500, new { Error = "Internal server error" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllJobs()
        {
            try
            {
                var jobs = await _jobService.GetAllJobsAsync();
                return Ok(new
                {
                    Success = true,
                    Jobs = jobs,
                    Count = jobs.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all jobs");
                return StatusCode(500, new { Error = "Internal server error" });
            }
        }

        // Manual trigger endpoints
        [HttpPost("trigger/ofac")]
        public async Task<IActionResult> TriggerOfacFetch()
        {
            try
            {
                var result = await _jobService.TriggerOfacFetchAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error triggering OFAC fetch");
                return StatusCode(500, new { Error = "Internal server error" });
            }
        }

        [HttpPost("trigger/un")]
        public async Task<IActionResult> TriggerUnFetch()
        {
            try
            {
                var result = await _jobService.TriggerUnFetchAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error triggering UN fetch");
                return StatusCode(500, new { Error = "Internal server error" });
            }
        }

        [HttpPost("trigger/rbi")]
        public async Task<IActionResult> TriggerRbiFetch()
        {
            try
            {
                var result = await _jobService.TriggerRbiFetchAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error triggering RBI fetch");
                return StatusCode(500, new { Error = "Internal server error" });
            }
        }

        [HttpPost("trigger/sebi")]
        public async Task<IActionResult> TriggerSebiFetch()
        {
            try
            {
                var result = await _jobService.TriggerSebiFetchAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error triggering SEBI fetch");
                return StatusCode(500, new { Error = "Internal server error" });
            }
        }

        [HttpPost("trigger/eu")]
        public async Task<IActionResult> TriggerEuFetch()
        {
            try
            {
                var result = await _jobService.TriggerEuFetchAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error triggering EU fetch");
                return StatusCode(500, new { Error = "Internal server error" });
            }
        }

        [HttpPost("trigger/uk")]
        public async Task<IActionResult> TriggerUkFetch()
        {
            try
            {
                var result = await _jobService.TriggerUkFetchAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error triggering UK fetch");
                return StatusCode(500, new { Error = "Internal server error" });
            }
        }

        [HttpPost("trigger/parliament")]
        public async Task<IActionResult> TriggerParliamentFetch()
        {
            try
            {
                var result = await _jobService.TriggerParliamentFetchAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error triggering Parliament fetch");
                return StatusCode(500, new { Error = "Internal server error" });
            }
        }

        [HttpPost("trigger/all")]
        public async Task<IActionResult> TriggerAllFetch()
        {
            try
            {
                var results = await _jobService.TriggerAllFetchAsync();
                return Ok(new
                {
                    Success = true,
                    Results = results,
                    TotalSources = results.Count,
                    SuccessfulSources = results.Count(r => r.Success),
                    FailedSources = results.Count(r => !r.Success)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error triggering all sources fetch");
                return StatusCode(500, new { Error = "Internal server error" });
            }
        }

        [HttpGet("sources")]
        public async Task<IActionResult> GetAvailableSources()
        {
            var sources = new[]
            {
                new { Code = "OFAC", Name = "OFAC (US Treasury)", Type = "Sanctions", Country = "US", UpdateFrequency = "12 hours" },
                new { Code = "UN", Name = "UN Sanctions", Type = "Sanctions", Country = "International", UpdateFrequency = "12 hours" },
                new { Code = "RBI", Name = "RBI (Reserve Bank of India)", Type = "Regulatory", Country = "India", UpdateFrequency = "Weekly" },
                new { Code = "SEBI", Name = "SEBI (Securities Exchange Board)", Type = "Regulatory", Country = "India", UpdateFrequency = "Weekly" },
                new { Code = "EU", Name = "EU Sanctions", Type = "Sanctions", Country = "European Union", UpdateFrequency = "Daily" },
                new { Code = "UK", Name = "UK Sanctions", Type = "Sanctions", Country = "United Kingdom", UpdateFrequency = "Daily" },
                new { Code = "PARLIAMENT", Name = "Indian Parliament Members", Type = "PEP", Country = "India", UpdateFrequency = "Monthly" }
            };

            return Ok(new
            {
                Success = true,
                Sources = sources,
                Count = sources.Length
            });
        }
    }
}
