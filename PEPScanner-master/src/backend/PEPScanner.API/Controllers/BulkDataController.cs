using Microsoft.AspNetCore.Mvc;
using PEPScanner.API.Services;

namespace PEPScanner.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BulkDataController : ControllerBase
{
    private readonly IBulkDataImportService _bulkImportService;
    private readonly ILogger<BulkDataController> _logger;

    public BulkDataController(IBulkDataImportService bulkImportService, ILogger<BulkDataController> logger)
    {
        _bulkImportService = bulkImportService;
        _logger = logger;
    }

    [HttpPost("import/opensanctions")]
    public async Task<IActionResult> ImportOpenSanctions()
    {
        try
        {
            var count = await _bulkImportService.ImportOpenSanctionsBulkDataAsync();
            return Ok(new { success = true, message = $"Imported {count} OpenSanctions records", count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing OpenSanctions data");
            return StatusCode(500, new { error = "Failed to import OpenSanctions data" });
        }
    }

    [HttpPost("import/ofac")]
    public async Task<IActionResult> ImportOfac()
    {
        try
        {
            var count = await _bulkImportService.ImportOfacBulkDataAsync();
            return Ok(new { success = true, message = $"Imported {count} OFAC records", count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing OFAC data");
            return StatusCode(500, new { error = "Failed to import OFAC data" });
        }
    }

    [HttpPost("import/un")]
    public async Task<IActionResult> ImportUn()
    {
        try
        {
            var count = await _bulkImportService.ImportUnBulkDataAsync();
            return Ok(new { success = true, message = $"Imported {count} UN records", count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing UN data");
            return StatusCode(500, new { error = "Failed to import UN data" });
        }
    }

    [HttpPost("import/eu")]
    public async Task<IActionResult> ImportEu()
    {
        try
        {
            var count = await _bulkImportService.ImportEuBulkDataAsync();
            return Ok(new { success = true, message = $"Imported {count} EU records", count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing EU data");
            return StatusCode(500, new { error = "Failed to import EU data" });
        }
    }

    [HttpPost("import/uk")]
    public async Task<IActionResult> ImportUk()
    {
        try
        {
            var count = await _bulkImportService.ImportUkBulkDataAsync();
            return Ok(new { success = true, message = $"Imported {count} UK records", count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing UK data");
            return StatusCode(500, new { error = "Failed to import UK data" });
        }
    }

    [HttpPost("import/worldbank")]
    public async Task<IActionResult> ImportWorldBank()
    {
        try
        {
            var count = await _bulkImportService.ImportWorldBankDataAsync();
            return Ok(new { success = true, message = $"Imported {count} World Bank records", count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing World Bank data");
            return StatusCode(500, new { error = "Failed to import World Bank data" });
        }
    }

    [HttpPost("import/interpol")]
    public async Task<IActionResult> ImportInterpol()
    {
        try
        {
            var count = await _bulkImportService.ImportInterpolDataAsync();
            return Ok(new { success = true, message = $"Imported {count} Interpol records", count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing Interpol data");
            return StatusCode(500, new { error = "Failed to import Interpol data" });
        }
    }

    [HttpPost("import/riskpro")]
    public async Task<IActionResult> ImportRiskPro()
    {
        try
        {
            var count = await _bulkImportService.ImportRiskProDataAsync();
            return Ok(new { success = true, message = $"Imported {count} RiskPro records", count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing RiskPro data");
            return StatusCode(500, new { error = "Failed to import RiskPro data" });
        }
    }

    [HttpPost("import/regtechtimes")]
    public async Task<IActionResult> ImportRegTechTimes()
    {
        try
        {
            var count = await _bulkImportService.ImportRegTechTimesDataAsync();
            return Ok(new { success = true, message = $"Imported {count} RegTech Times records", count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing RegTech Times data");
            return StatusCode(500, new { error = "Failed to import RegTech Times data" });
        }
    }

    [HttpPost("import/globalvendors")]
    public async Task<IActionResult> ImportGlobalVendors()
    {
        try
        {
            var count = await _bulkImportService.ImportGlobalVendorsDataAsync();
            return Ok(new { success = true, message = $"Imported {count} Global Vendors records", count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing Global Vendors data");
            return StatusCode(500, new { error = "Failed to import Global Vendors data" });
        }
    }

    [HttpPost("import/all")]
    public async Task<IActionResult> ImportAll()
    {
        try
        {
            var count = await _bulkImportService.ImportAllBulkDataAsync();
            return Ok(new { success = true, message = $"Imported {count} total records from all sources", count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing all bulk data");
            return StatusCode(500, new { error = "Failed to import bulk data" });
        }
    }
}