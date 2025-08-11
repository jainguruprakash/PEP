using Microsoft.AspNetCore.Mvc;
using PEPScanner.API.Services;

namespace PEPScanner.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OpenSanctionsController : ControllerBase
{
    private readonly IOpenSanctionsDatasetService _datasetService;
    private readonly ILogger<OpenSanctionsController> _logger;

    public OpenSanctionsController(IOpenSanctionsDatasetService datasetService, ILogger<OpenSanctionsController> logger)
    {
        _datasetService = datasetService;
        _logger = logger;
    }

    [HttpPost("import/crime")]
    public async Task<IActionResult> ImportCrimeDataset()
    {
        try
        {
            var count = await _datasetService.ImportCrimeDatasetAsync();
            return Ok(new { success = true, message = $"Imported {count} crime records", count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing crime dataset");
            return StatusCode(500, new { error = "Failed to import crime dataset" });
        }
    }

    [HttpPost("import/sanctions")]
    public async Task<IActionResult> ImportSanctionsDataset()
    {
        try
        {
            var count = await _datasetService.ImportSanctionsDatasetAsync();
            return Ok(new { success = true, message = $"Imported {count} sanctions records", count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing sanctions dataset");
            return StatusCode(500, new { error = "Failed to import sanctions dataset" });
        }
    }

    [HttpPost("import/pep")]
    public async Task<IActionResult> ImportPepDataset()
    {
        try
        {
            var count = await _datasetService.ImportPepDatasetAsync();
            return Ok(new { success = true, message = $"Imported {count} PEP records", count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing PEP dataset");
            return StatusCode(500, new { error = "Failed to import PEP dataset" });
        }
    }

    [HttpPost("import/all")]
    public async Task<IActionResult> ImportAllDatasets()
    {
        try
        {
            var count = await _datasetService.ImportAllDatasetsAsync();
            return Ok(new { success = true, message = $"Imported {count} total records from all OpenSanctions datasets", count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing all datasets");
            return StatusCode(500, new { error = "Failed to import all datasets" });
        }
    }
}