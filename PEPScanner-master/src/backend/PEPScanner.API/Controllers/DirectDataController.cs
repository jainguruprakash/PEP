using Microsoft.AspNetCore.Mvc;
using PEPScanner.API.Services;

namespace PEPScanner.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DirectDataController : ControllerBase
{
    private readonly IDirectDataFetcher _dataFetcher;
    private readonly ILogger<DirectDataController> _logger;

    public DirectDataController(IDirectDataFetcher dataFetcher, ILogger<DirectDataController> logger)
    {
        _dataFetcher = dataFetcher;
        _logger = logger;
    }

    [HttpPost("fetch/opensanctions")]
    public async Task<IActionResult> FetchOpenSanctions()
    {
        try
        {
            var count = await _dataFetcher.FetchOpenSanctionsDirectAsync();
            return Ok(new { success = true, message = $"Fetched {count} OpenSanctions records", count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching OpenSanctions data");
            return StatusCode(500, new { error = "Failed to fetch OpenSanctions data" });
        }
    }

    [HttpPost("fetch/ofac")]
    public async Task<IActionResult> FetchOfac()
    {
        try
        {
            var count = await _dataFetcher.FetchOfacDirectAsync();
            return Ok(new { success = true, message = $"Fetched {count} OFAC records", count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching OFAC data");
            return StatusCode(500, new { error = "Failed to fetch OFAC data" });
        }
    }

    [HttpPost("fetch/un")]
    public async Task<IActionResult> FetchUn()
    {
        try
        {
            var count = await _dataFetcher.FetchUnDirectAsync();
            return Ok(new { success = true, message = $"Fetched {count} UN records", count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching UN data");
            return StatusCode(500, new { error = "Failed to fetch UN data" });
        }
    }

    [HttpPost("fetch/all")]
    public async Task<IActionResult> FetchAll()
    {
        try
        {
            var count = await _dataFetcher.FetchAllDirectAsync();
            return Ok(new { success = true, message = $"Fetched {count} total records from all sources", count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all data");
            return StatusCode(500, new { error = "Failed to fetch all data" });
        }
    }
}