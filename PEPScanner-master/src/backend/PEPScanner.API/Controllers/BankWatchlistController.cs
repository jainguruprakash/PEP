using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PEPScanner.Infrastructure.Data;
using PEPScanner.Domain.Entities;
using System.Text.Json;

namespace PEPScanner.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BankWatchlistController : ControllerBase
{
    private readonly PepScannerDbContext _context;
    private readonly ILogger<BankWatchlistController> _logger;

    public BankWatchlistController(PepScannerDbContext context, ILogger<BankWatchlistController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadBankWatchlist(IFormFile file, [FromForm] string bankName, [FromForm] string listType = "Custom")
    {
        try
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { error = "No file uploaded" });

            if (string.IsNullOrEmpty(bankName))
                return BadRequest(new { error = "Bank name is required" });

            var entries = new List<WatchlistEntry>();
            using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream);

            var header = await reader.ReadLineAsync();
            if (string.IsNullOrEmpty(header))
                return BadRequest(new { error = "File is empty" });

            var headerFields = header.Split(',').Select(h => h.Trim().ToLower()).ToArray();
            var nameIndex = Array.FindIndex(headerFields, h => h.Contains("name"));
            var countryIndex = Array.FindIndex(headerFields, h => h.Contains("country"));
            var reasonIndex = Array.FindIndex(headerFields, h => h.Contains("reason") || h.Contains("description"));

            if (nameIndex == -1)
                return BadRequest(new { error = "Name column not found in CSV" });

            string line;
            var lineNumber = 1;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                lineNumber++;
                var fields = line.Split(',').Select(f => f.Trim()).ToArray();
                
                if (fields.Length <= nameIndex || string.IsNullOrEmpty(fields[nameIndex]))
                    continue;

                var entry = new WatchlistEntry
                {
                    Id = Guid.NewGuid(),
                    ExternalId = $"BANK_{bankName}_{lineNumber}",
                    Source = $"BANK_{bankName}",
                    ListType = listType,
                    PrimaryName = fields[nameIndex],
                    Country = countryIndex >= 0 && countryIndex < fields.Length ? fields[countryIndex] : null,
                    SanctionReason = reasonIndex >= 0 && reasonIndex < fields.Length ? fields[reasonIndex] : "Bank internal list",
                    RiskCategory = "Medium",
                    IsActive = true,
                    DateAddedUtc = DateTime.UtcNow,
                    DateLastUpdatedUtc = DateTime.UtcNow
                };

                entries.Add(entry);
            }

            // Clear existing entries for this bank
            var existingEntries = await _context.WatchlistEntries
                .Where(w => w.Source == $"BANK_{bankName}")
                .ToListAsync();
            
            _context.WatchlistEntries.RemoveRange(existingEntries);
            
            // Add new entries
            await _context.WatchlistEntries.AddRangeAsync(entries);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = $"Successfully uploaded {entries.Count} entries for {bankName}",
                bankName = bankName,
                entriesCount = entries.Count,
                uploadedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading bank watchlist");
            return StatusCode(500, new { error = "Failed to upload watchlist" });
        }
    }

    [HttpGet("banks")]
    public async Task<IActionResult> GetBankWatchlists()
    {
        try
        {
            var bankLists = await _context.WatchlistEntries
                .Where(w => w.Source.StartsWith("BANK_"))
                .GroupBy(w => w.Source)
                .Select(g => new
                {
                    BankName = g.Key.Replace("BANK_", ""),
                    Source = g.Key,
                    Count = g.Count(),
                    LastUpdated = g.Max(w => w.DateLastUpdatedUtc),
                    ListTypes = g.Select(w => w.ListType).Distinct().ToList()
                })
                .ToListAsync();

            return Ok(new
            {
                success = true,
                banks = bankLists,
                totalBanks = bankLists.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting bank watchlists");
            return StatusCode(500, new { error = "Failed to get bank watchlists" });
        }
    }

    [HttpDelete("banks/{bankName}")]
    public async Task<IActionResult> DeleteBankWatchlist(string bankName)
    {
        try
        {
            var entries = await _context.WatchlistEntries
                .Where(w => w.Source == $"BANK_{bankName}")
                .ToListAsync();

            if (!entries.Any())
                return NotFound(new { error = "Bank watchlist not found" });

            _context.WatchlistEntries.RemoveRange(entries);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = $"Deleted {entries.Count} entries for {bankName}",
                deletedCount = entries.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting bank watchlist");
            return StatusCode(500, new { error = "Failed to delete bank watchlist" });
        }
    }
}