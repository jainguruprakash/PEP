using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PEPScanner.Infrastructure.Data;
using PEPScanner.Domain.Entities;
using System.Text.Json;

namespace PEPScanner.API.Controllers
{
    [ApiController]
    [Route("api/adverse-media")]
    public class AdverseMediaController : ControllerBase
    {
        private readonly PepScannerDbContext _context;
        private readonly ILogger<AdverseMediaController> _logger;

        public AdverseMediaController(PepScannerDbContext context, ILogger<AdverseMediaController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost("screen")]
        public async Task<IActionResult> ScreenForAdverseMedia([FromBody] AdverseMediaScreeningRequest request)
        {
            try
            {
                _logger.LogInformation("Starting adverse media screening for: {Name}", request.FullName);

                var results = new List<AdverseMediaResult>();

                // Simulate adverse media screening with sample data
                var adverseMediaSources = new[]
                {
                    new { Source = "Reuters", Category = "Financial Crime", Severity = "High" },
                    new { Source = "BBC News", Category = "Corruption", Severity = "Medium" },
                    new { Source = "Financial Times", Category = "Money Laundering", Severity = "High" },
                    new { Source = "Bloomberg", Category = "Sanctions Violation", Severity = "Critical" },
                    new { Source = "Wall Street Journal", Category = "Fraud", Severity = "High" }
                };

                // Check for matches based on name similarity
                var nameWords = request.FullName.ToLower().Split(' ');
                var hasMatch = false;

                // Simulate some matches for demonstration
                if (nameWords.Any(w => w.Contains("shah") || w.Contains("modi") || w.Contains("trump") || w.Contains("putin")))
                {
                    hasMatch = true;
                    var matchCount = Random.Shared.Next(1, 4);
                    
                    for (int i = 0; i < matchCount; i++)
                    {
                        var source = adverseMediaSources[Random.Shared.Next(adverseMediaSources.Length)];
                        var similarity = Random.Shared.Next(75, 95);
                        
                        results.Add(new AdverseMediaResult
                        {
                            Id = Guid.NewGuid(),
                            MatchedName = request.FullName,
                            Source = source.Source,
                            Category = source.Category,
                            Severity = source.Severity,
                            SimilarityScore = similarity,
                            PublicationDate = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 365)),
                            Headline = $"{source.Category} allegations involving {request.FullName}",
                            Summary = $"News report from {source.Source} regarding {source.Category.ToLower()} allegations",
                            Url = $"https://{source.Source.ToLower().Replace(" ", "")}.com/news/{Guid.NewGuid()}",
                            Country = request.Country,
                            ScreeningDate = DateTime.UtcNow
                        });
                    }
                }

                var response = new AdverseMediaScreeningResponse
                {
                    ScreeningId = Guid.NewGuid(),
                    FullName = request.FullName,
                    Country = request.Country,
                    ScreeningDate = DateTime.UtcNow,
                    TotalMatches = results.Count,
                    HighRiskMatches = results.Count(r => r.Severity == "High" || r.Severity == "Critical"),
                    Results = results,
                    OverallRiskLevel = DetermineOverallRisk(results),
                    RecommendedAction = DetermineRecommendedAction(results)
                };

                // Create alerts for high-risk matches
                if (results.Any(r => r.Severity == "High" || r.Severity == "Critical"))
                {
                    await CreateAdverseMediaAlert(request, results);
                }

                _logger.LogInformation("Adverse media screening completed for {Name}. Found {Count} matches", 
                    request.FullName, results.Count);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during adverse media screening for {Name}", request.FullName);
                return StatusCode(500, new { error = "Adverse media screening failed", details = ex.Message });
            }
        }

        [HttpGet("results/{screeningId}")]
        public async Task<IActionResult> GetScreeningResults(Guid screeningId)
        {
            try
            {
                // In a real implementation, this would retrieve stored screening results
                // For now, return a sample response
                var response = new AdverseMediaScreeningResponse
                {
                    ScreeningId = screeningId,
                    FullName = "Sample Customer",
                    ScreeningDate = DateTime.UtcNow,
                    TotalMatches = 0,
                    HighRiskMatches = 0,
                    Results = new List<AdverseMediaResult>(),
                    OverallRiskLevel = "Low",
                    RecommendedAction = "Proceed"
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving adverse media results for screening {ScreeningId}", screeningId);
                return StatusCode(500, new { error = "Failed to retrieve screening results" });
            }
        }

        [HttpPost("create-alert")]
        public async Task<IActionResult> CreateAdverseMediaAlert([FromBody] CreateAdverseMediaAlertRequest request)
        {
            try
            {
                // Find customer
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.FullName == request.CustomerName && !c.IsDeleted);

                if (customer == null)
                {
                    return BadRequest(new { error = "Customer not found" });
                }

                // Check if alert already exists for this customer and source
                var existingAlert = await _context.Alerts
                    .FirstOrDefaultAsync(a => a.CustomerId == customer.Id && 
                                           a.AlertType == "Adverse Media" && 
                                           a.SourceList == request.Source &&
                                           a.Status != "Closed");

                if (existingAlert != null)
                {
                    return BadRequest(new { error = "Alert already exists for this customer and source" });
                }

                // Determine priority and risk level
                var priority = DeterminePriority(request.Severity);
                var riskLevel = DetermineRiskLevel(request.Severity);

                // Create new alert
                var alert = new Alert
                {
                    Id = Guid.NewGuid(),
                    CustomerId = customer.Id,
                    Context = "AdverseMedia",
                    AlertType = "Adverse Media",
                    Status = "Open",
                    Priority = priority,
                    RiskLevel = riskLevel,
                    WorkflowStatus = "PendingReview",
                    SimilarityScore = request.SimilarityScore,
                    SourceList = request.Source,
                    SourceCategory = request.Category,
                    MatchingDetails = JsonSerializer.Serialize(new
                    {
                        headline = request.Headline,
                        summary = request.Summary,
                        url = request.Url,
                        publicationDate = request.PublicationDate,
                        severity = request.Severity
                    }),
                    CreatedAtUtc = DateTime.UtcNow,
                    UpdatedAtUtc = DateTime.UtcNow,
                    CreatedBy = request.CreatedBy ?? "AdverseMediaSystem",
                    DueDate = DateTime.UtcNow.AddHours(GetSlaHours(priority)),
                    SlaHours = GetSlaHours(priority),
                    SlaStatus = "OnTime",
                    EscalationLevel = 0,
                    LastActionType = "Created",
                    LastActionDateUtc = DateTime.UtcNow
                };

                // Assign to appropriate reviewer based on risk level
                if (riskLevel == "High" || riskLevel == "Critical")
                {
                    var seniorReviewer = await _context.OrganizationUsers
                        .FirstOrDefaultAsync(u => (u.Role == "Manager" || u.Role == "ComplianceOfficer") && u.IsActive);
                    
                    if (seniorReviewer != null)
                    {
                        alert.AssignedTo = seniorReviewer.Id.ToString();
                        alert.CurrentReviewer = seniorReviewer.Email;
                        alert.WorkflowStatus = "UnderReview";
                    }
                }

                _context.Alerts.Add(alert);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created adverse media alert {AlertId} for customer {CustomerName}", 
                    alert.Id, request.CustomerName);

                return Ok(new { 
                    alertId = alert.Id, 
                    message = "Adverse media alert created successfully",
                    priority = priority,
                    riskLevel = riskLevel,
                    assignedTo = alert.CurrentReviewer
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating adverse media alert for customer {CustomerName}", request.CustomerName);
                return StatusCode(500, new { error = "Failed to create adverse media alert", details = ex.Message });
            }
        }

        [HttpGet("sources")]
        public async Task<IActionResult> GetAdverseMediaSources()
        {
            try
            {
                var sources = new[]
                {
                    new { Name = "Reuters", Type = "News Agency", Coverage = "Global" },
                    new { Name = "BBC News", Type = "News Agency", Coverage = "Global" },
                    new { Name = "Financial Times", Type = "Financial News", Coverage = "Global" },
                    new { Name = "Bloomberg", Type = "Financial News", Coverage = "Global" },
                    new { Name = "Wall Street Journal", Type = "Financial News", Coverage = "Global" },
                    new { Name = "Associated Press", Type = "News Agency", Coverage = "Global" },
                    new { Name = "CNN", Type = "News Network", Coverage = "Global" },
                    new { Name = "CNBC", Type = "Financial News", Coverage = "Global" }
                };

                return Ok(sources);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving adverse media sources");
                return StatusCode(500, new { error = "Failed to retrieve sources" });
            }
        }

        private async Task CreateAdverseMediaAlert(AdverseMediaScreeningRequest request, List<AdverseMediaResult> results)
        {
            try
            {
                // Find customer
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.FullName == request.FullName && !c.IsDeleted);

                if (customer == null) return;

                // Create alert for the highest severity match
                var highestSeverityResult = results
                    .OrderByDescending(r => GetSeverityWeight(r.Severity))
                    .First();

                var alert = new Alert
                {
                    Id = Guid.NewGuid(),
                    CustomerId = customer.Id,
                    Context = "AdverseMedia",
                    AlertType = "Adverse Media",
                    Status = "Open",
                    Priority = DeterminePriority(highestSeverityResult.Severity),
                    RiskLevel = DetermineRiskLevel(highestSeverityResult.Severity),
                    WorkflowStatus = "PendingReview",
                    SimilarityScore = highestSeverityResult.SimilarityScore,
                    SourceList = highestSeverityResult.Source,
                    SourceCategory = highestSeverityResult.Category,
                    MatchingDetails = JsonSerializer.Serialize(new
                    {
                        totalMatches = results.Count,
                        highestSeverity = highestSeverityResult.Severity,
                        sources = results.Select(r => r.Source).Distinct().ToArray(),
                        categories = results.Select(r => r.Category).Distinct().ToArray(),
                        topMatch = highestSeverityResult
                    }),
                    CreatedAtUtc = DateTime.UtcNow,
                    UpdatedAtUtc = DateTime.UtcNow,
                    CreatedBy = "AdverseMediaSystem",
                    DueDate = DateTime.UtcNow.AddHours(GetSlaHours(DeterminePriority(highestSeverityResult.Severity))),
                    SlaHours = GetSlaHours(DeterminePriority(highestSeverityResult.Severity)),
                    SlaStatus = "OnTime",
                    EscalationLevel = 0,
                    LastActionType = "Created",
                    LastActionDateUtc = DateTime.UtcNow
                };

                _context.Alerts.Add(alert);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating adverse media alert during screening");
            }
        }

        private string DetermineOverallRisk(List<AdverseMediaResult> results)
        {
            if (!results.Any()) return "Low";
            
            var maxSeverityWeight = results.Max(r => GetSeverityWeight(r.Severity));
            return maxSeverityWeight switch
            {
                4 => "Critical",
                3 => "High",
                2 => "Medium",
                _ => "Low"
            };
        }

        private string DetermineRecommendedAction(List<AdverseMediaResult> results)
        {
            if (!results.Any()) return "Proceed";
            
            var maxSeverityWeight = results.Max(r => GetSeverityWeight(r.Severity));
            return maxSeverityWeight switch
            {
                4 => "Reject",
                3 => "Enhanced Due Diligence",
                2 => "Additional Review",
                _ => "Proceed with Caution"
            };
        }

        private string DeterminePriority(string severity)
        {
            return severity switch
            {
                "Critical" => "Critical",
                "High" => "High",
                "Medium" => "Medium",
                _ => "Low"
            };
        }

        private string DetermineRiskLevel(string severity)
        {
            return severity switch
            {
                "Critical" => "Critical",
                "High" => "High",
                "Medium" => "Medium",
                _ => "Low"
            };
        }

        private int GetSeverityWeight(string severity)
        {
            return severity switch
            {
                "Critical" => 4,
                "High" => 3,
                "Medium" => 2,
                "Low" => 1,
                _ => 0
            };
        }

        private int GetSlaHours(string priority)
        {
            return priority switch
            {
                "Critical" => 4,
                "High" => 8,
                "Medium" => 24,
                _ => 48
            };
        }
    }

    // Request/Response DTOs
    public class AdverseMediaScreeningRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string? Country { get; set; }
        public string? DateOfBirth { get; set; }
        public string? Nationality { get; set; }
        public int Threshold { get; set; } = 75;
        public string[]? Sources { get; set; }
        public string[]? Categories { get; set; }
    }

    public class AdverseMediaScreeningResponse
    {
        public Guid ScreeningId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Country { get; set; }
        public DateTime ScreeningDate { get; set; }
        public int TotalMatches { get; set; }
        public int HighRiskMatches { get; set; }
        public List<AdverseMediaResult> Results { get; set; } = new();
        public string OverallRiskLevel { get; set; } = string.Empty;
        public string RecommendedAction { get; set; } = string.Empty;
    }

    public class AdverseMediaResult
    {
        public Guid Id { get; set; }
        public string MatchedName { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public double SimilarityScore { get; set; }
        public DateTime PublicationDate { get; set; }
        public string Headline { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string? Country { get; set; }
        public DateTime ScreeningDate { get; set; }
    }

    public class CreateAdverseMediaAlertRequest
    {
        public string CustomerName { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public double SimilarityScore { get; set; }
        public string Headline { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public DateTime PublicationDate { get; set; }
        public string? CreatedBy { get; set; }
    }
}