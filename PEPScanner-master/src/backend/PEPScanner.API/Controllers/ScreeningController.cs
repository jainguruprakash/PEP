using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PEPScanner.Infrastructure.Data;
using PEPScanner.Domain.Entities;

namespace PEPScanner.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScreeningController : ControllerBase
    {
        private readonly PepScannerDbContext _context;
        private readonly ILogger<ScreeningController> _logger;

        public ScreeningController(PepScannerDbContext context, ILogger<ScreeningController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost("customer")]
        public async Task<IActionResult> ScreenCustomer([FromBody] CustomerScreeningRequest request)
        {
            try
            {
                _logger.LogInformation("Screening customer: {CustomerName}", request.FullName);

                // Perform actual screening against watchlist entries
                var matches = new List<object>();
                var searchName = request.FullName?.ToUpper() ?? "";

                if (!string.IsNullOrEmpty(searchName))
                {
                    var watchlistMatches = await _context.WatchlistEntries
                        .Where(w => w.IsActive &&
                                   (w.PrimaryName.ToUpper().Contains(searchName) ||
                                    (w.AlternateNames != null && w.AlternateNames.ToUpper().Contains(searchName))))
                        .ToListAsync();

                    foreach (var match in watchlistMatches)
                    {
                        var matchScore = CalculateMatchScore(searchName, match.PrimaryName, match.AlternateNames);
                        if (matchScore > 0.7) // Only include high-confidence matches
                        {
                            matches.Add(new
                            {
                                id = match.Id,
                                matchedName = match.PrimaryName,
                                alternateNames = match.AlternateNames,
                                source = match.Source,
                                listType = match.ListType,
                                country = match.Country,
                                positionOrRole = match.PositionOrRole,
                                riskCategory = match.RiskCategory,
                                sanctionType = match.SanctionType,
                                sanctionReason = match.SanctionReason,
                                pepCategory = match.PepCategory,
                                pepPosition = match.PepPosition,
                                matchScore = Math.Round(matchScore, 2),
                                dateAdded = match.DateAddedUtc,
                                lastUpdated = match.DateLastUpdatedUtc
                            });
                        }
                    }
                }

                // Calculate overall risk score
                var riskScore = CalculateRiskScore(matches);
                var status = DetermineStatus(riskScore, matches.Count);

                // Auto-create alerts for matches above threshold
                var alertIds = new List<Guid>();
                if (matches.Count > 0 && riskScore >= 0.5) // Create alerts for medium+ risk
                {
                    alertIds = await CreateAlertsForMatches(request, matches, riskScore);
                }

                var result = new
                {
                    customerId = request.CustomerId ?? "",
                    customerName = request.FullName ?? "",
                    fullName = request.FullName,
                    dateOfBirth = request.DateOfBirth,
                    nationality = request.Nationality,
                    country = request.Country,
                    identificationNumber = request.IdentificationNumber,
                    identificationType = request.IdentificationType,
                    riskScore = Math.Round(riskScore, 2),
                    matches = matches,
                    matchCount = matches.Count,
                    status = status,
                    alertsCreated = alertIds,
                    screenedAt = DateTime.UtcNow,
                    screeningDetails = new
                    {
                        searchCriteria = searchName,
                        totalWatchlistEntries = await _context.WatchlistEntries.CountAsync(w => w.IsActive),
                        sourcesSearched = await _context.WatchlistEntries
                            .Where(w => w.IsActive)
                            .Select(w => w.Source)
                            .Distinct()
                            .ToListAsync()
                    }
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error screening customer");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpPost("transaction")]
        public async Task<IActionResult> ScreenTransaction([FromBody] TransactionScreeningRequest request)
        {
            try
            {
                _logger.LogInformation("Screening transaction: {TransactionId}", request.TransactionId);
                
                var result = new
                {
                    transactionId = request.TransactionId,
                    amount = request.Amount,
                    riskScore = 0.1,
                    matches = new List<object>(),
                    status = "Clear",
                    screenedAt = DateTime.UtcNow
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error screening transaction");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpPost("search")]
        public async Task<IActionResult> SearchName([FromBody] NameSearchRequest request)
        {
            try
            {
                _logger.LogInformation("Searching name: {Name}", request.Name);
                
                var results = new List<object>
                {
                    new
                    {
                        name = request.Name,
                        source = "OFAC",
                        matchScore = 0.95,
                        listType = "Sanctions"
                    }
                };

                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching name");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                var stats = new
                {
                    totalScreenings = 1250,
                    highRiskMatches = 15,
                    mediumRiskMatches = 45,
                    lowRiskMatches = 120,
                    clearScreenings = 1070,
                    period = new { startDate, endDate }
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting statistics");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        private double CalculateMatchScore(string searchName, string primaryName, string? alternateNames)
        {
            var scores = new List<double>();

            // Calculate score for primary name
            scores.Add(CalculateStringSimilarity(searchName, primaryName.ToUpper()));

            // Calculate scores for alternate names
            if (!string.IsNullOrEmpty(alternateNames))
            {
                var alternates = alternateNames.Split(';', ',')
                    .Select(name => name.Trim().ToUpper())
                    .Where(name => !string.IsNullOrEmpty(name));

                foreach (var alternateName in alternates)
                {
                    scores.Add(CalculateStringSimilarity(searchName, alternateName));
                }
            }

            // Return the highest score
            return scores.Any() ? scores.Max() : 0.0;
        }

        private double CalculateStringSimilarity(string str1, string str2)
        {
            if (string.IsNullOrEmpty(str1) || string.IsNullOrEmpty(str2))
                return 0.0;

            // Exact match
            if (str1 == str2)
                return 1.0;

            // Contains match
            if (str1.Contains(str2) || str2.Contains(str1))
                return 0.9;

            // Simple Levenshtein distance-based similarity
            var distance = LevenshteinDistance(str1, str2);
            var maxLength = Math.Max(str1.Length, str2.Length);
            return 1.0 - (double)distance / maxLength;
        }

        private async Task<List<Guid>> CreateAlertsForMatches(CustomerScreeningRequest request, List<object> matches, double riskScore)
        {
            var alertIds = new List<Guid>();
            
            foreach (dynamic match in matches)
            {
                var alert = new Alert
                {
                    Id = Guid.NewGuid(),
                    Context = "CustomerScreening",
                    AlertType = DetermineAlertType(match.listType?.ToString()),
                    SimilarityScore = match.matchScore,
                    Status = "Open",
                    Priority = DeterminePriority(riskScore, match.matchScore),
                    RiskLevel = DetermineRiskLevel(riskScore),
                    SourceList = match.source?.ToString(),
                    SourceCategory = match.listType?.ToString(),
                    MatchingDetails = $"Matched: {match.matchedName} (Score: {match.matchScore})",
                    WorkflowStatus = "PendingReview",
                    CreatedAtUtc = DateTime.UtcNow,
                    UpdatedAtUtc = DateTime.UtcNow,
                    DueDate = DateTime.UtcNow.AddHours(GetSlaHours(riskScore)),
                    SlaHours = GetSlaHours(riskScore)
                };
                
                _context.Alerts.Add(alert);
                alertIds.Add(alert.Id);
            }
            
            await _context.SaveChangesAsync();
            return alertIds;
        }
        
        private string DetermineAlertType(string? listType) => listType switch
        {
            "PEP" => "PEP Match",
            "Sanctions" => "Sanctions Match",
            "Adverse Media" => "Adverse Media Match",
            _ => "Name Match"
        };
        
        private string DeterminePriority(double riskScore, double matchScore) => (riskScore, matchScore) switch
        {
            ( >= 0.8, _) => "Critical",
            ( >= 0.6, >= 0.9) => "High",
            ( >= 0.5, _) => "Medium",
            _ => "Low"
        };
        
        private string DetermineRiskLevel(double riskScore) => riskScore switch
        {
            >= 0.8 => "Critical",
            >= 0.6 => "High",
            >= 0.4 => "Medium",
            _ => "Low"
        };
        
        private int GetSlaHours(double riskScore) => riskScore switch
        {
            >= 0.8 => 4,  // Critical: 4 hours
            >= 0.6 => 24, // High: 24 hours
            >= 0.4 => 72, // Medium: 72 hours
            _ => 168       // Low: 1 week
        };

        private int LevenshteinDistance(string str1, string str2)
        {
            var matrix = new int[str1.Length + 1, str2.Length + 1];

            for (int i = 0; i <= str1.Length; i++)
                matrix[i, 0] = i;

            for (int j = 0; j <= str2.Length; j++)
                matrix[0, j] = j;

            for (int i = 1; i <= str1.Length; i++)
            {
                for (int j = 1; j <= str2.Length; j++)
                {
                    var cost = str1[i - 1] == str2[j - 1] ? 0 : 1;
                    matrix[i, j] = Math.Min(
                        Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                        matrix[i - 1, j - 1] + cost);
                }
            }

            return matrix[str1.Length, str2.Length];
        }

        private double CalculateRiskScore(List<object> matches)
        {
            if (!matches.Any())
                return 0.1; // Low risk for no matches

            // Base risk calculation
            var baseRisk = Math.Min(0.3 + (matches.Count * 0.2), 1.0);

            // Additional risk factors could be added here based on:
            // - Source criticality (OFAC, UN = higher risk)
            // - List type (Sanctions = higher risk than PEP)
            // - Match confidence scores

            return Math.Min(baseRisk, 1.0);
        }

        private string DetermineStatus(double riskScore, int matchCount)
        {
            if (matchCount == 0)
                return "Clear";

            if (riskScore >= 0.8)
                return "High Risk";

            if (riskScore >= 0.5)
                return "Medium Risk";

            return "Low Risk";
        }
    }

    public class CustomerScreeningRequest
    {
        public string? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? FullName { get; set; }
        public string? DateOfBirth { get; set; }
        public string? Nationality { get; set; }
        public string? Country { get; set; }
        public string? IdentificationNumber { get; set; }
        public string? IdentificationType { get; set; }
        public string EntityType { get; set; } = "Individual";
        public bool AutoCreateAlerts { get; set; } = true;
    }

    public class TransactionScreeningRequest
    {
        public string TransactionId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string TransactionType { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public string BeneficiaryName { get; set; } = string.Empty;
    }

    public class NameSearchRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public double Threshold { get; set; } = 0.7;
        public int MaxResults { get; set; } = 50;
    }
}
