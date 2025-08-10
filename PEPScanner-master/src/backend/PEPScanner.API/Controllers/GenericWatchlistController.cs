using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PEPScanner.Infrastructure.Data;
using PEPScanner.Domain.Entities;

namespace PEPScanner.API.Controllers
{
    [ApiController]
    [Route("api/genericwatchlist")]
    public class GenericWatchlistController : ControllerBase
    {
        private readonly PepScannerDbContext _context;
        private readonly ILogger<GenericWatchlistController> _logger;

        public GenericWatchlistController(PepScannerDbContext context, ILogger<GenericWatchlistController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("sources")]
        public async Task<IActionResult> GetSources()
        {
            try
            {
                var sources = await _context.WatchlistEntries
                    .Select(w => w.Source)
                    .Distinct()
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToListAsync();

                var sourceInfo = sources.Select(source => new
                {
                    Name = source,
                    DisplayName = GetDisplayName(source),
                    Type = GetSourceType(source),
                    Country = GetSourceCountry(source),
                    IsActive = true
                }).ToList();

                return Ok(sourceInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting watchlist sources");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchAcross([FromQuery] string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return BadRequest(new { error = "Name parameter is required" });
                }

                var results = await _context.WatchlistEntries
                    .Where(w => w.PrimaryName.Contains(name) ||
                               (w.AlternateNames != null && w.AlternateNames.Contains(name)))
                    .Select(w => new
                    {
                        w.Id,
                        FullName = w.PrimaryName,
                        w.Source,
                        w.ListType,
                        w.Country,
                        w.DateOfBirth,
                        AliasNames = w.AlternateNames,
                        Designation = w.PositionOrRole,
                        Reason = w.RiskCategory,
                        SimilarityScore = CalculateSimilarityScore(name, w.PrimaryName)
                    })
                    .OrderByDescending(w => w.SimilarityScore)
                    .Take(50)
                    .ToListAsync();

                return Ok(new
                {
                    Query = name,
                    TotalResults = results.Count,
                    Results = results
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching across watchlists for name {Name}", name);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("search/{source}")]
        public async Task<IActionResult> SearchSpecific(string source, [FromQuery] string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return BadRequest(new { error = "Name parameter is required" });
                }

                var results = await _context.WatchlistEntries
                    .Where(w => w.Source == source &&
                               (w.PrimaryName.Contains(name) ||
                                (w.AlternateNames != null && w.AlternateNames.Contains(name))))
                    .Select(w => new
                    {
                        w.Id,
                        FullName = w.PrimaryName,
                        w.Source,
                        w.ListType,
                        w.Country,
                        w.DateOfBirth,
                        AliasNames = w.AlternateNames,
                        Designation = w.PositionOrRole,
                        Reason = w.RiskCategory,
                        SimilarityScore = CalculateSimilarityScore(name, w.PrimaryName)
                    })
                    .OrderByDescending(w => w.SimilarityScore)
                    .Take(50)
                    .ToListAsync();

                return Ok(new
                {
                    Source = source,
                    Query = name,
                    TotalResults = results.Count,
                    Results = results
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching {Source} watchlist for name {Name}", source, name);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpPost("update/{source}")]
        public async Task<IActionResult> UpdateSource(string source)
        {
            try
            {
                // This would typically trigger an update from the external source
                // For now, we'll return a mock response
                var result = new
                {
                    Success = true,
                    Source = source,
                    Message = $"Update initiated for {source} watchlist",
                    Timestamp = DateTime.UtcNow,
                    EstimatedCompletionTime = DateTime.UtcNow.AddMinutes(5)
                };

                _logger.LogInformation("Update initiated for {Source} watchlist", source);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating {Source} watchlist", source);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpPost("update-all")]
        public async Task<IActionResult> UpdateAll()
        {
            try
            {
                var sources = await _context.WatchlistEntries
                    .Select(w => w.Source)
                    .Distinct()
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToListAsync();

                var result = new
                {
                    Success = true,
                    Message = "Update initiated for all watchlists",
                    Sources = sources,
                    Timestamp = DateTime.UtcNow,
                    EstimatedCompletionTime = DateTime.UtcNow.AddMinutes(15)
                };

                _logger.LogInformation("Update initiated for all watchlists");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating all watchlists");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("last-updates")]
        public async Task<IActionResult> GetLastUpdates()
        {
            try
            {
                var sources = await _context.WatchlistEntries
                    .GroupBy(w => w.Source)
                    .Select(g => new
                    {
                        Source = g.Key,
                        LastUpdate = g.Max(w => w.DateAddedUtc),
                        TotalEntries = g.Count(),
                        DisplayName = GetDisplayName(g.Key ?? "Unknown")
                    })
                    .ToListAsync();

                return Ok(sources);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting last update timestamps");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        private static string GetDisplayName(string source)
        {
            return source?.ToUpper() switch
            {
                "OFAC" => "OFAC (US Treasury)",
                "UN" => "UN Sanctions",
                "RBI" => "RBI (Reserve Bank of India)",
                "SEBI" => "SEBI (Securities Exchange Board of India)",
                "EU" => "EU Sanctions",
                "UK" => "UK Sanctions",
                "PARLIAMENT" => "Indian Parliament Members",
                _ => source ?? "Unknown"
            };
        }

        private static string GetSourceType(string source)
        {
            return source?.ToUpper() switch
            {
                "OFAC" or "UN" or "EU" or "UK" => "Sanctions",
                "RBI" or "SEBI" => "Regulatory",
                "PARLIAMENT" => "PEP",
                _ => "Other"
            };
        }

        private static string GetSourceCountry(string source)
        {
            return source?.ToUpper() switch
            {
                "OFAC" => "US",
                "UN" => "International",
                "RBI" or "SEBI" or "PARLIAMENT" => "India",
                "EU" => "European Union",
                "UK" => "United Kingdom",
                _ => "Unknown"
            };
        }

        private static double CalculateSimilarityScore(string query, string target)
        {
            if (string.IsNullOrEmpty(query) || string.IsNullOrEmpty(target))
                return 0.0;

            // Simple similarity calculation - in production, use more sophisticated algorithms
            var queryLower = query.ToLower();
            var targetLower = target.ToLower();

            if (targetLower.Contains(queryLower))
                return 0.9;

            if (queryLower.Contains(targetLower))
                return 0.8;

            // Calculate Levenshtein distance-based similarity
            var distance = LevenshteinDistance(queryLower, targetLower);
            var maxLength = Math.Max(queryLower.Length, targetLower.Length);
            
            return maxLength == 0 ? 0.0 : 1.0 - (double)distance / maxLength;
        }

        private static int LevenshteinDistance(string s1, string s2)
        {
            if (string.IsNullOrEmpty(s1)) return s2?.Length ?? 0;
            if (string.IsNullOrEmpty(s2)) return s1.Length;

            var matrix = new int[s1.Length + 1, s2.Length + 1];

            for (int i = 0; i <= s1.Length; i++)
                matrix[i, 0] = i;

            for (int j = 0; j <= s2.Length; j++)
                matrix[0, j] = j;

            for (int i = 1; i <= s1.Length; i++)
            {
                for (int j = 1; j <= s2.Length; j++)
                {
                    var cost = s1[i - 1] == s2[j - 1] ? 0 : 1;
                    matrix[i, j] = Math.Min(
                        Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                        matrix[i - 1, j - 1] + cost);
                }
            }

            return matrix[s1.Length, s2.Length];
        }
    }
}
