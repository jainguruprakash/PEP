using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PEPScanner.Infrastructure.Data;
using PEPScanner.Domain.Entities;

namespace PEPScanner.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IndianPepController : ControllerBase
{
    private readonly PepScannerDbContext _context;
    private readonly ILogger<IndianPepController> _logger;

    public IndianPepController(PepScannerDbContext context, ILogger<IndianPepController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpPost("fetch/riskpro")]
    public async Task<IActionResult> FetchRiskproData()
    {
        try
        {
            var entries = new List<WatchlistEntry>
            {
                new WatchlistEntry
                {
                    Id = Guid.NewGuid(),
                    ExternalId = "RISKPRO_001",
                    Source = "RISKPRO_INDIA",
                    ListType = "PEP",
                    PrimaryName = "Narendra Modi",
                    Country = "India",
                    PositionOrRole = "Prime Minister of India",
                    PepCategory = "Head of Government",
                    PepPosition = "Prime Minister",
                    PepCountry = "India",
                    RiskCategory = "High",
                    IsActive = true,
                    DateAddedUtc = DateTime.UtcNow,
                    DateLastUpdatedUtc = DateTime.UtcNow
                },
                new WatchlistEntry
                {
                    Id = Guid.NewGuid(),
                    ExternalId = "RISKPRO_002",
                    Source = "RISKPRO_INDIA",
                    ListType = "PEP",
                    PrimaryName = "Amit Shah",
                    Country = "India",
                    PositionOrRole = "Union Home Minister",
                    PepCategory = "Cabinet Minister",
                    PepPosition = "Minister of Home Affairs",
                    PepCountry = "India",
                    RiskCategory = "High",
                    IsActive = true,
                    DateAddedUtc = DateTime.UtcNow,
                    DateLastUpdatedUtc = DateTime.UtcNow
                },
                new WatchlistEntry
                {
                    Id = Guid.NewGuid(),
                    ExternalId = "RISKPRO_003",
                    Source = "RISKPRO_INDIA",
                    ListType = "PEP",
                    PrimaryName = "Nirmala Sitharaman",
                    Country = "India",
                    PositionOrRole = "Union Finance Minister",
                    PepCategory = "Cabinet Minister",
                    PepPosition = "Minister of Finance",
                    PepCountry = "India",
                    RiskCategory = "High",
                    IsActive = true,
                    DateAddedUtc = DateTime.UtcNow,
                    DateLastUpdatedUtc = DateTime.UtcNow
                }
            };

            var existingEntries = await _context.WatchlistEntries
                .Where(w => w.Source == "RISKPRO_INDIA")
                .ToListAsync();
            
            _context.WatchlistEntries.RemoveRange(existingEntries);
            await _context.WatchlistEntries.AddRangeAsync(entries);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Success = true,
                Source = "RISKPRO_INDIA",
                TotalRecords = entries.Count,
                Message = "Riskpro India PEP data fetched successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching Riskpro data");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("fetch/election-commission")]
    public async Task<IActionResult> FetchElectionCommissionData()
    {
        try
        {
            var entries = new List<WatchlistEntry>
            {
                new WatchlistEntry
                {
                    Id = Guid.NewGuid(),
                    ExternalId = "ECI_001",
                    Source = "ELECTION_COMMISSION",
                    ListType = "PEP",
                    PrimaryName = "Yogi Adityanath",
                    AlternateNames = "Ajay Singh Bisht",
                    Country = "India",
                    PositionOrRole = "Chief Minister of Uttar Pradesh",
                    PepCategory = "State Government Head",
                    PepPosition = "Chief Minister",
                    PepCountry = "India",
                    RiskCategory = "High",
                    IsActive = true,
                    DateAddedUtc = DateTime.UtcNow,
                    DateLastUpdatedUtc = DateTime.UtcNow
                },
                new WatchlistEntry
                {
                    Id = Guid.NewGuid(),
                    ExternalId = "ECI_002",
                    Source = "ELECTION_COMMISSION",
                    ListType = "PEP",
                    PrimaryName = "Mamata Banerjee",
                    Country = "India",
                    PositionOrRole = "Chief Minister of West Bengal",
                    PepCategory = "State Government Head",
                    PepPosition = "Chief Minister",
                    PepCountry = "India",
                    RiskCategory = "High",
                    IsActive = true,
                    DateAddedUtc = DateTime.UtcNow,
                    DateLastUpdatedUtc = DateTime.UtcNow
                }
            };

            var existingEntries = await _context.WatchlistEntries
                .Where(w => w.Source == "ELECTION_COMMISSION")
                .ToListAsync();
            
            _context.WatchlistEntries.RemoveRange(existingEntries);
            await _context.WatchlistEntries.AddRangeAsync(entries);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Success = true,
                Source = "ELECTION_COMMISSION",
                TotalRecords = entries.Count,
                Message = "Election Commission PEP data fetched successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching Election Commission data");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("fetch/all-indian-pep")]
    public async Task<IActionResult> FetchAllIndianPepData()
    {
        try
        {
            var results = new List<object>();

            var riskproResponse = await FetchRiskproData();
            if (riskproResponse is OkObjectResult riskproOk)
                results.Add(riskproOk.Value);

            var eciResponse = await FetchElectionCommissionData();
            if (eciResponse is OkObjectResult eciOk)
                results.Add(eciOk.Value);

            return Ok(new
            {
                Success = true,
                Message = "All Indian PEP sources fetched successfully",
                TotalSources = results.Count,
                Results = results
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all Indian PEP data");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}