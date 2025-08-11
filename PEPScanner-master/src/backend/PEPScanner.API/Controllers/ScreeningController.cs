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
                _logger.LogInformation("Screening customer: {CustomerName} with sources: {Sources}", 
                    request.FullName, string.Join(", ", request.Sources ?? new List<string>()));

                // Perform actual screening against watchlist entries
                var matches = new List<object>();
                var searchName = request.FullName?.ToUpper() ?? "";

                if (!string.IsNullOrEmpty(searchName))
                {
                    var query = _context.WatchlistEntries.Where(w => w.IsActive);
                    
                    // Apply source filtering if sources are specified
                    if (request.Sources != null && request.Sources.Any())
                    {
                        query = query.Where(w => request.Sources.Contains(w.Source));
                    }
                    
                    // Apply name matching with threshold consideration
                    var threshold = (request.Threshold ?? 70) / 100.0;
                    
                    var watchlistMatches = await query
                        .Where(w => w.PrimaryName.ToUpper().Contains(searchName) ||
                                   (w.AlternateNames != null && w.AlternateNames.ToUpper().Contains(searchName)))
                        .ToListAsync();

                    // Add MCA Director screening
                    var mcaDirectors = await SearchMcaDirectorsAsync(searchName);
                    foreach (var director in mcaDirectors)
                    {
                        var directorMatchScore = CalculateStringSimilarity(searchName, director.Name.ToUpper(), request);
                        if (directorMatchScore >= threshold)
                        {
                            matches.Add(new
                            {
                                id = director.Din,
                                matchedName = director.Name,
                                alternateNames = director.CompanyName,
                                source = "MCA",
                                listType = "Director",
                                country = "India",
                                positionOrRole = director.Designation,
                                riskCategory = director.RiskLevel,
                                pepCategory = "Corporate Director",
                                pepPosition = director.Designation,
                                companyName = director.CompanyName,
                                cin = director.Cin,
                                din = director.Din,
                                appointmentDate = director.AppointmentDate,
                                matchScore = Math.Round(directorMatchScore, 2),
                                dateAdded = DateTime.UtcNow,
                                lastUpdated = DateTime.UtcNow
                            });
                        }
                    }

                    // Process regular watchlist matches
                    foreach (var match in watchlistMatches)
                    {
                        var matchScore = CalculateMatchScore(searchName, match.PrimaryName, match.AlternateNames, request);
                        if (matchScore >= threshold) // Use dynamic threshold
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

                // Check for existing alerts
                var existingAlerts = new List<object>();
                if (!string.IsNullOrEmpty(request.FullName))
                {
                    var existingAlertsList = await _context.Alerts
                        .Where(a => a.MatchingDetails != null && a.MatchingDetails.Contains(request.FullName) && a.Status == "Open")
                        .Select(a => new { a.Id, a.AlertType, a.CreatedAtUtc, a.WorkflowStatus })
                        .ToListAsync();
                    existingAlerts.AddRange(existingAlertsList);
                }

                // Auto-create alerts for matches above threshold (only if no existing alerts)
                var alertIds = new List<Guid>();
                if (matches.Count > 0 && riskScore >= 0.5 && !existingAlerts.Any()) // Create alerts for medium+ risk
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
                    existingAlerts = existingAlerts,
                    screenedAt = DateTime.UtcNow,
                    screeningDetails = new
                    {
                        searchCriteria = searchName,
                        threshold = request.Threshold ?? 70,
                        sourcesRequested = request.Sources ?? new List<string>(),
                        includeAliases = request.IncludeAliases ?? true,
                        includeFuzzyMatching = request.IncludeFuzzyMatching ?? true,
                        includePhoneticMatching = request.IncludePhoneticMatching ?? true,
                        totalWatchlistEntries = await GetFilteredWatchlistCount(request.Sources),
                        sourcesSearched = await GetSearchedSources(request.Sources)
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

        [HttpPost("batch-file")]
        public async Task<IActionResult> ScreenBatchFile(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { error = "No file uploaded" });
                }

                var results = new List<object>();
                using var stream = file.OpenReadStream();
                using var reader = new StreamReader(stream);

                // Skip header row
                var header = await reader.ReadLineAsync();
                if (string.IsNullOrEmpty(header))
                {
                    return BadRequest(new { error = "File is empty or invalid" });
                }

                var lineNumber = 1;
                string line;

                while ((line = await reader.ReadLineAsync()) != null)
                {
                    lineNumber++;

                    try
                    {
                        var fields = line.Split(',');
                        if (fields.Length < 2)
                        {
                            continue;
                        }

                        var fullName = fields[0].Trim();
                        var country = fields.Length > 1 ? fields[1].Trim() : "";

                        if (string.IsNullOrEmpty(fullName))
                        {
                            continue;
                        }

                        // Perform screening for each customer
                        var searchName = fullName.ToUpper();
                        var watchlistMatches = await _context.WatchlistEntries
                            .Where(w => w.IsActive &&
                                       (w.PrimaryName.ToUpper().Contains(searchName) ||
                                        (w.AlternateNames != null && w.AlternateNames.ToUpper().Contains(searchName))))
                            .Take(10) // Limit matches for performance
                            .ToListAsync();

                        var matches = new List<object>();
                        foreach (var match in watchlistMatches)
                        {
                            var matchScore = CalculateMatchScore(searchName, match.PrimaryName, match.AlternateNames, new CustomerScreeningRequest());
                            if (matchScore >= 0.7)
                            {
                                matches.Add(new
                                {
                                    matchedName = match.PrimaryName,
                                    source = match.Source,
                                    listType = match.ListType,
                                    matchScore = Math.Round(matchScore, 2)
                                });
                            }
                        }

                        var riskScore = CalculateRiskScore(matches);
                        var status = DetermineStatus(riskScore, matches.Count);

                        results.Add(new
                        {
                            customerName = fullName,
                            country = country,
                            riskScore = Math.Round(riskScore * 100, 0),
                            matchCount = matches.Count,
                            status = status,
                            matches = matches.Take(5).ToList(), // Limit matches in response
                            screenedAt = DateTime.UtcNow
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing line {LineNumber}", lineNumber);
                    }
                }

                _logger.LogInformation("Batch screening completed: {Count} customers processed", results.Count);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing batch file");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpPost("approve")]
        public async Task<IActionResult> ApproveCustomer([FromBody] CustomerActionRequest request)
        {
            try
            {
                // Process approval
                return Ok(new { success = true, message = "Customer approved" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving customer");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpPost("flag")]
        public async Task<IActionResult> FlagForReview([FromBody] CustomerActionRequest request)
        {
            try
            {
                // Process flag
                return Ok(new { success = true, message = "Customer flagged for review" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error flagging customer");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpPost("edd")]
        public async Task<IActionResult> RequestEDD([FromBody] EDDRequest request)
        {
            try
            {
                // Process EDD request
                return Ok(new { success = true, message = "EDD requested" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error requesting EDD");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("history/{customerId}")]
        public async Task<IActionResult> GetScreeningHistory(string customerId)
        {
            try
            {
                var history = new[]
                {
                    new { date = DateTime.Now.AddDays(-1), action = "Screening", user = "John Doe", result = "Medium Risk" },
                    new { date = DateTime.Now.AddDays(-7), action = "Screening", user = "Jane Smith", result = "Clear" }
                };
                return Ok(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting screening history");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("templates")]
        public async Task<IActionResult> GetSearchTemplates()
        {
            try
            {
                var templates = new[]
                {
                    new { id = "high-risk-pep", name = "High Risk PEP Check", config = new { threshold = 90, sources = new[] { "OFAC", "UN" } } },
                    new { id = "sanctions-only", name = "Sanctions Only", config = new { threshold = 80, sources = new[] { "OFAC", "UN", "EU" } } },
                    new { id = "local-lists", name = "Local Lists Only", config = new { threshold = 70, sources = new[] { "RBI", "SEBI" } } }
                };
                return Ok(templates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting templates");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpPost("templates")]
        public async Task<IActionResult> SaveSearchTemplate([FromBody] SearchTemplate template)
        {
            try
            {
                // Save template
                return Ok(new { success = true, templateId = Guid.NewGuid() });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving template");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        private double CalculateMatchScore(string searchName, string primaryName, string? alternateNames, CustomerScreeningRequest request)
        {
            var scores = new List<double>();

            // Calculate score for primary name
            scores.Add(CalculateStringSimilarity(searchName, primaryName.ToUpper(), request));

            // Calculate scores for alternate names if enabled
            if ((request.IncludeAliases ?? true) && !string.IsNullOrEmpty(alternateNames))
            {
                var alternates = alternateNames.Split(';', ',')
                    .Select(name => name.Trim().ToUpper())
                    .Where(name => !string.IsNullOrEmpty(name));

                foreach (var alternateName in alternates)
                {
                    scores.Add(CalculateStringSimilarity(searchName, alternateName, request));
                }
            }

            // Return the highest score
            return scores.Any() ? scores.Max() : 0.0;
        }

        private double CalculateStringSimilarity(string str1, string str2, CustomerScreeningRequest request)
        {
            if (string.IsNullOrEmpty(str1) || string.IsNullOrEmpty(str2))
                return 0.0;

            // Exact match
            if (str1 == str2)
                return 1.0;

            // Contains match
            if (str1.Contains(str2) || str2.Contains(str1))
                return 0.9;

            // Fuzzy matching if enabled
            if (request.IncludeFuzzyMatching ?? true)
            {
                var distance = LevenshteinDistance(str1, str2);
                var maxLength = Math.Max(str1.Length, str2.Length);
                var similarity = 1.0 - (double)distance / maxLength;
                
                // Phonetic matching if enabled
                if ((request.IncludePhoneticMatching ?? true) && similarity < 0.8)
                {
                    var phoneticSimilarity = CalculatePhoneticSimilarity(str1, str2);
                    similarity = Math.Max(similarity, phoneticSimilarity);
                }
                
                return similarity;
            }

            return 0.0;
        }

        private async Task<List<Guid>> CreateAlertsForMatches(CustomerScreeningRequest request, List<object> matches, double riskScore)
        {
            var alertIds = new List<Guid>();
            
            // Find a senior user to assign alerts to (Manager or ComplianceOfficer)
            var seniorUser = await _context.Users
                .Where(u => u.Role == "Manager" || u.Role == "ComplianceOfficer")
                .OrderBy(u => u.CreatedAtUtc)
                .FirstOrDefaultAsync();
            
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
                    MatchingDetails = $"Customer: {request.FullName} matched with {match.matchedName} (Score: {match.matchScore})",
                    WorkflowStatus = "PendingReview",
                    AssignedToUserId = seniorUser?.Id,
                    CreatedAtUtc = DateTime.UtcNow,
                    UpdatedAtUtc = DateTime.UtcNow,
                    DueDate = DateTime.UtcNow.AddHours(GetSlaHours(riskScore)),
                    SlaHours = GetSlaHours(riskScore)
                };
                
                _context.Alerts.Add(alert);
                alertIds.Add(alert.Id);
            }
            
            await _context.SaveChangesAsync();
            _logger.LogInformation("Created {Count} alerts for customer {CustomerName}, assigned to {AssignedUser}", 
                alertIds.Count, request.FullName, seniorUser?.Username ?? "System");
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
        
        private double CalculatePhoneticSimilarity(string str1, string str2)
        {
            // Simple phonetic matching - in production, use a proper phonetic algorithm like Soundex or Metaphone
            var phonetic1 = GetSimplePhonetic(str1);
            var phonetic2 = GetSimplePhonetic(str2);
            
            if (phonetic1 == phonetic2)
                return 0.8;
                
            return 0.0;
        }
        
        private string GetSimplePhonetic(string input)
        {
            // Very basic phonetic conversion - replace with proper algorithm in production
            return input.ToUpper()
                .Replace("PH", "F")
                .Replace("CK", "K")
                .Replace("C", "K")
                .Replace("QU", "KW")
                .Replace("X", "KS")
                .Replace("Z", "S");
        }
        
        private async Task<int> GetFilteredWatchlistCount(List<string>? sources)
        {
            var query = _context.WatchlistEntries.Where(w => w.IsActive);
            
            if (sources != null && sources.Any())
            {
                query = query.Where(w => sources.Contains(w.Source));
            }
            
            return await query.CountAsync();
        }
        
        private async Task<List<string>> GetSearchedSources(List<string>? requestedSources)
        {
            var query = _context.WatchlistEntries.Where(w => w.IsActive);
            
            if (requestedSources != null && requestedSources.Any())
            {
                query = query.Where(w => requestedSources.Contains(w.Source));
            }
            
            var sources = await query.Select(w => w.Source).Distinct().ToListAsync();
            
            // Add MCA as a source if not filtered out
            if (requestedSources == null || requestedSources.Contains("MCA"))
            {
                sources.Add("MCA");
            }
            
            return sources;
        }
        
        private async Task<List<PEPScanner.API.Controllers.McaDirector>> SearchMcaDirectorsAsync(string searchName)
        {
            try
            {
                // Mock MCA director search - replace with actual MCA API call
                var directors = new List<PEPScanner.API.Controllers.McaDirector>();
                
                if (searchName.Contains("AMIT") && searchName.Contains("SHAH"))
                {
                    // Return PEP data for Amit Shah (Home Minister)
                    directors.Add(new PEPScanner.API.Controllers.McaDirector
                    {
                        Din = "PEP001",
                        Name = "Amit Shah",
                        CompanyName = "Government of India",
                        Cin = "GOI-HM-2019",
                        Designation = "Union Home Minister",
                        AppointmentDate = "2019-05-30",
                        Status = "Active",
                        Nationality = "Indian",
                        RiskLevel = "High"
                    });
                }
                
                return directors;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching MCA directors for: {SearchName}", searchName);
                return new List<PEPScanner.API.Controllers.McaDirector>();
            }
        }

        private double CalculateRiskScore(List<object> matches)
        {
            if (!matches.Any())
                return 0.1; // Low risk for no matches

            double totalRisk = 0.0;
            
            foreach (dynamic match in matches)
            {
                double matchRisk = 0.0;
                
                // Source-based risk weighting
                matchRisk += match.source?.ToString() switch
                {
                    "OFAC" => 0.4,
                    "UN" => 0.4,
                    "EU" => 0.3,
                    "RBI" => 0.3,
                    "SEBI" => 0.2,
                    _ => 0.1
                };
                
                // List type risk weighting
                matchRisk += match.listType?.ToString() switch
                {
                    "Sanctions" => 0.4,
                    "PEP" => 0.2,
                    "Adverse Media" => 0.1,
                    _ => 0.1
                };
                
                // Match score weighting
                matchRisk += (double)(match.matchScore ?? 0.0) * 0.2;
                
                totalRisk += matchRisk;
            }
            
            // Normalize and cap at 1.0
            return Math.Min(totalRisk / matches.Count, 1.0);
        }

        private string DetermineStatus(double riskScore, int matchCount)
        {
            if (matchCount == 0)
                return "Clear";

            return riskScore switch
            {
                >= 0.8 => "High Risk",
                >= 0.6 => "Medium Risk", 
                >= 0.4 => "Low Risk",
                _ => "Minimal Risk"
            };
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
        public int? Threshold { get; set; } = 70;
        public List<string>? Sources { get; set; }
        public bool? IncludeAliases { get; set; } = true;
        public bool? IncludeFuzzyMatching { get; set; } = true;
        public bool? IncludePhoneticMatching { get; set; } = true;
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

    public class CustomerActionRequest
    {
        public string CustomerId { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }

    public class EDDRequest
    {
        public string CustomerId { get; set; } = string.Empty;
        public List<string> Requirements { get; set; } = new();
    }

    public class SearchTemplate
    {
        public string Name { get; set; } = string.Empty;
        public object Config { get; set; } = new();
    }

    // McaDirector class moved to McaController to avoid duplication

}
