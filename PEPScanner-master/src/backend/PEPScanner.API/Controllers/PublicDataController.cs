using Microsoft.AspNetCore.Mvc;
using PEPScanner.API.Services;

namespace PEPScanner.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PublicDataController : ControllerBase
{
    private readonly IPublicDataScrapingService _scrapingService;
    private readonly ILogger<PublicDataController> _logger;

    public PublicDataController(IPublicDataScrapingService scrapingService, ILogger<PublicDataController> logger)
    {
        _scrapingService = scrapingService;
        _logger = logger;
    }

    [HttpPost("scrape/rbi")]
    public async Task<IActionResult> ScrapeRbi()
    {
        try
        {
            var count = await _scrapingService.ScrapeRbiDataAsync();
            return Ok(new { success = true, message = $"Scraped {count} RBI entries", count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scraping RBI data");
            return StatusCode(500, new { error = "Failed to scrape RBI data" });
        }
    }

    [HttpPost("scrape/sebi")]
    public async Task<IActionResult> ScrapeSebi()
    {
        try
        {
            var count = await _scrapingService.ScrapeSebiDataAsync();
            return Ok(new { success = true, message = $"Scraped {count} SEBI entries", count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scraping SEBI data");
            return StatusCode(500, new { error = "Failed to scrape SEBI data" });
        }
    }

    [HttpPost("scrape/parliament")]
    public async Task<IActionResult> ScrapeParliament()
    {
        try
        {
            var count = await _scrapingService.ScrapeParliamentDataAsync();
            return Ok(new { success = true, message = $"Scraped {count} Parliament entries", count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scraping Parliament data");
            return StatusCode(500, new { error = "Failed to scrape Parliament data" });
        }
    }

    [HttpPost("scrape/wikipedia")]
    public async Task<IActionResult> ScrapeWikipedia()
    {
        try
        {
            var count = await _scrapingService.ScrapeWikipediaPepsAsync();
            return Ok(new { success = true, message = $"Scraped {count} Wikipedia entries", count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scraping Wikipedia data");
            return StatusCode(500, new { error = "Failed to scrape Wikipedia data" });
        }
    }

    [HttpPost("scrape/opensanctions")]
    public async Task<IActionResult> ScrapeOpenSanctions()
    {
        try
        {
            var count = await _scrapingService.ScrapeOpenSanctionsAsync();
            return Ok(new { success = true, message = $"Scraped {count} OpenSanctions entries", count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scraping OpenSanctions data");
            return StatusCode(500, new { error = "Failed to scrape OpenSanctions data" });
        }
    }

    [HttpPost("scrape/all")]
    public async Task<IActionResult> ScrapeAll()
    {
        try
        {
            var results = new Dictionary<string, int>
            {
                ["RBI"] = await _scrapingService.ScrapeRbiDataAsync(),
                ["SEBI"] = await _scrapingService.ScrapeSebiDataAsync(),
                ["Parliament"] = await _scrapingService.ScrapeParliamentDataAsync(),
                ["Wikipedia"] = await _scrapingService.ScrapeWikipediaPepsAsync(),
                ["OpenSanctions"] = await _scrapingService.ScrapeOpenSanctionsAsync()
            };

            var total = results.Values.Sum();
            return Ok(new 
            { 
                success = true, 
                message = $"Scraped {total} total entries from all sources", 
                results,
                total 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scraping all data");
            return StatusCode(500, new { error = "Failed to scrape data" });
        }
    }
}