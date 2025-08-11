using Microsoft.AspNetCore.Mvc;
using PEPScanner.API.Services;
using Hangfire;

namespace PEPScanner.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly IPublicDataScrapingService _scrapingService;
    private readonly IWatchlistUpdateService _watchlistService;
    private readonly ILogger<TestController> _logger;

    public TestController(
        IPublicDataScrapingService scrapingService,
        IWatchlistUpdateService watchlistService,
        ILogger<TestController> logger)
    {
        _scrapingService = scrapingService;
        _watchlistService = watchlistService;
        _logger = logger;
    }

    [HttpGet("scrapers")]
    public async Task<IActionResult> TestAllScrapers()
    {
        try
        {
            var results = new Dictionary<string, object>();
            
            // Test each scraper
            results["RBI"] = await _scrapingService.ScrapeRbiDataAsync();
            results["SEBI"] = await _scrapingService.ScrapeSebiDataAsync();
            results["Parliament"] = await _scrapingService.ScrapeParliamentDataAsync();
            results["Wikipedia"] = await _scrapingService.ScrapeWikipediaPepsAsync();
            results["OpenSanctions"] = await _scrapingService.ScrapeOpenSanctionsAsync();
            
            var total = results.Values.Cast<int>().Sum();
            
            return Ok(new
            {
                success = true,
                message = $"All scrapers tested successfully. Total entries: {total}",
                results,
                total
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing scrapers");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("jobs")]
    public IActionResult TestJobs()
    {
        try
        {
            // Trigger immediate job execution for testing
            BackgroundJob.Enqueue(() => _watchlistService.UpdateAllWatchlistsAsync());
            BackgroundJob.Enqueue(() => _watchlistService.UpdateSpecificWatchlistAsync("OFAC"));
            BackgroundJob.Enqueue(() => _watchlistService.UpdateSpecificWatchlistAsync("UN"));
            
            return Ok(new
            {
                success = true,
                message = "Test jobs queued successfully",
                jobs = new[]
                {
                    "UpdateAllWatchlistsAsync",
                    "UpdateSpecificWatchlistAsync(OFAC)",
                    "UpdateSpecificWatchlistAsync(UN)"
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing jobs");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetTestStatus()
    {
        try
        {
            var status = await _watchlistService.GetUpdateStatusAsync();
            
            return Ok(new
            {
                success = true,
                message = "System status retrieved successfully",
                status,
                hangfireDashboard = "/hangfire",
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting test status");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}