using Microsoft.AspNetCore.Mvc;
using PEPScanner.API.Services;

namespace PEPScanner.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WatchlistImportController : ControllerBase
{
    private readonly IWatchlistUpdateService _watchlistService;
    private readonly ILogger<WatchlistImportController> _logger;

    public WatchlistImportController(IWatchlistUpdateService watchlistService, ILogger<WatchlistImportController> logger)
    {
        _watchlistService = watchlistService;
        _logger = logger;
    }

    [HttpPost("import-all")]
    public async Task<IActionResult> ImportAllWatchlists()
    {
        try
        {
            await _watchlistService.UpdateAllWatchlistsAsync();
            return Ok(new { message = "All watchlists imported successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing watchlists");
            return StatusCode(500, new { error = "Failed to import watchlists" });
        }
    }

    [HttpPost("import/{source}")]
    public async Task<IActionResult> ImportSpecificWatchlist(string source)
    {
        try
        {
            await _watchlistService.UpdateSpecificWatchlistAsync(source);
            return Ok(new { message = $"{source} watchlist imported successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing {Source} watchlist", source);
            return StatusCode(500, new { error = $"Failed to import {source} watchlist" });
        }
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetImportStatus()
    {
        try
        {
            var status = await _watchlistService.GetUpdateStatusAsync();
            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting import status");
            return StatusCode(500, new { error = "Failed to get import status" });
        }
    }
}