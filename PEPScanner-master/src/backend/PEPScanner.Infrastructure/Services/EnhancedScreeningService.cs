using PEPScanner.Domain.Entities;
using PEPScanner.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace PEPScanner.Infrastructure.Services
{
    public interface IEnhancedScreeningService
    {
        Task<List<Alert>> ScreenCustomerAsync(Customer customer);
        Task<Alert?> EnhanceAlertWithOpenSanctionsAsync(Alert alert);
        Task<List<Alert>> GetAlertsWithOpenSanctionsDataAsync(int skip = 0, int take = 50);
        Task<Alert?> GetAlertWithOpenSanctionsDetailsAsync(Guid alertId);
    }

    public class EnhancedScreeningService : IEnhancedScreeningService
    {
        private readonly PepScannerDbContext _context;
        private readonly IOpenSanctionsDataService _openSanctionsDataService;
        private readonly IOrganizationCustomListService _customListService;
        private readonly ILogger<EnhancedScreeningService> _logger;

        public EnhancedScreeningService(
            PepScannerDbContext context,
            IOpenSanctionsDataService openSanctionsDataService,
            IOrganizationCustomListService customListService,
            ILogger<EnhancedScreeningService> logger)
        {
            _context = context;
            _openSanctionsDataService = openSanctionsDataService;
            _customListService = customListService;
            _logger = logger;
        }

        public async Task<List<Alert>> ScreenCustomerAsync(Customer customer)
        {
            var alerts = new List<Alert>();

            try
            {
                _logger.LogInformation("Starting enhanced screening for customer: {CustomerId}", customer.Id);

                // Step 1: Traditional screening (existing logic)
                var traditionalAlerts = await PerformTraditionalScreeningAsync(customer);
                alerts.AddRange(traditionalAlerts);

                // Step 2: OpenSanctions screening
                var openSanctionsAlerts = await PerformOpenSanctionsScreeningAsync(customer);
                alerts.AddRange(openSanctionsAlerts);

                // Step 3: Enhance existing alerts with OpenSanctions data
                foreach (var alert in traditionalAlerts)
                {
                    await EnhanceAlertWithOpenSanctionsAsync(alert);
                }

                // Step 4: Save all alerts
                _context.Alerts.AddRange(alerts);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Enhanced screening completed for customer: {CustomerId}. Generated {AlertCount} alerts", 
                    customer.Id, alerts.Count);

                return alerts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during enhanced screening for customer: {CustomerId}", customer.Id);
                throw;
            }
        }

        public async Task<Alert?> EnhanceAlertWithOpenSanctionsAsync(Alert alert)
        {
            try
            {
                if (alert.Customer == null)
                {
                    alert.Customer = await _context.Customers.FindAsync(alert.CustomerId);
                }

                if (alert.Customer == null)
                {
                    _logger.LogWarning("Cannot enhance alert {AlertId} - customer not found", alert.Id);
                    return alert;
                }

                _logger.LogInformation("Enhancing alert {AlertId} with OpenSanctions data", alert.Id);

                // For now, treat all customers as persons since we don't have organization type
                var matches = await _openSanctionsDataService.MatchPersonAsync(
                    alert.Customer.FullName,
                    alert.Customer.DateOfBirth,
                    alert.Customer.Country);

                if (matches.Any())
                {
                    var bestMatch = matches.OrderByDescending(m => m.Score).First();
                    
                    // Update alert with OpenSanctions data
                    alert.OpenSanctionsEntityId = bestMatch.Id;
                    alert.OpenSanctionsScore = bestMatch.Score;
                    alert.OpenSanctionsEntityType = bestMatch.Schema;
                    alert.OpenSanctionsDatasets = bestMatch.Datasets; // Already JSON string
                    alert.OpenSanctionsAliases = bestMatch.Aliases; // Already JSON string
                    alert.OpenSanctionsSanctions = bestMatch.Sanctions; // Already JSON string
                    alert.OpenSanctionsCountries = bestMatch.Countries; // Already JSON string
                    alert.OpenSanctionsAddresses = bestMatch.Addresses; // Already JSON string
                    alert.OpenSanctionsFirstSeen = bestMatch.FirstSeen;
                    alert.OpenSanctionsLastSeen = bestMatch.LastSeen;
                    alert.OpenSanctionsLastChange = bestMatch.LastChange;
                    alert.OpenSanctionsLastChecked = DateTime.UtcNow;

                    // Calculate match features
                    var matchFeatures = CalculateMatchFeatures(alert.Customer, bestMatch);
                    alert.OpenSanctionsMatchFeatures = JsonSerializer.Serialize(matchFeatures);

                    // Update alert priority based on OpenSanctions score
                    if (bestMatch.Score >= 0.95)
                    {
                        alert.Priority = "Critical";
                        alert.RiskLevel = "Critical";
                    }
                    else if (bestMatch.Score >= 0.85)
                    {
                        alert.Priority = "High";
                        alert.RiskLevel = "High";
                    }

                    _logger.LogInformation("Enhanced alert {AlertId} with OpenSanctions match {EntityId} (Score: {Score})", 
                        alert.Id, bestMatch.Id, bestMatch.Score);
                }
                else
                {
                    alert.OpenSanctionsLastChecked = DateTime.UtcNow;
                    _logger.LogInformation("No OpenSanctions matches found for alert {AlertId}", alert.Id);
                }

                // Check against organization's custom lists
                var customMatches = await _customListService.MatchAgainstCustomListsAsync(
                    alert.Customer.OrganizationId,
                    alert.Customer.FullName,
                    alert.Customer.DateOfBirth,
                    alert.Customer.Country);

                if (customMatches.Any())
                {
                    var customMatch = customMatches.First();
                    var customListNote = $"\n[Custom List Match] Found in {customMatch.EntryType} list: {customMatch.PrimaryName}";
                    alert.OutcomeNotes = (alert.OutcomeNotes ?? "") + customListNote;

                    // Escalate risk level for custom list matches
                    if (customMatch.RiskCategory == "Critical")
                    {
                        alert.RiskLevel = "Critical";
                        alert.Priority = "Critical";
                    }
                    else if (customMatch.RiskCategory == "High" && alert.RiskLevel != "Critical")
                    {
                        alert.RiskLevel = "High";
                        alert.Priority = "High";
                    }

                    _logger.LogInformation("Enhanced alert {AlertId} with custom list match: {EntryId} (Type: {EntryType})",
                        alert.Id, customMatch.Id, customMatch.EntryType);
                }

                return alert;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enhancing alert {AlertId} with OpenSanctions data", alert.Id);
                return alert;
            }
        }

        public async Task<List<Alert>> GetAlertsWithOpenSanctionsDataAsync(int skip = 0, int take = 50)
        {
            return await _context.Alerts
                .Where(a => !string.IsNullOrEmpty(a.OpenSanctionsEntityId))
                .Include(a => a.Customer)
                .Include(a => a.Actions)
                .OrderByDescending(a => a.OpenSanctionsScore)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<Alert?> GetAlertWithOpenSanctionsDetailsAsync(Guid alertId)
        {
            return await _context.Alerts
                .Include(a => a.Customer)
                .Include(a => a.Actions)
                .FirstOrDefaultAsync(a => a.Id == alertId);
        }

        private async Task<List<Alert>> PerformTraditionalScreeningAsync(Customer customer)
        {
            // This would contain your existing screening logic
            // For now, return empty list as placeholder
            var alerts = new List<Alert>();

            // Example traditional screening logic
            var watchlistMatches = await _context.WatchlistEntries
                .Where(w => w.PrimaryName.Contains(customer.FullName))
                .ToListAsync();

            foreach (var match in watchlistMatches)
            {
                var similarity = CalculateNameSimilarity(customer.FullName, match.PrimaryName);

                if (similarity > 0.7) // 70% threshold
                {
                    alerts.Add(new Alert
                    {
                        Id = Guid.NewGuid(),
                        CustomerId = customer.Id,
                        WatchlistEntryId = match.Id,
                        Context = "Onboarding",
                        AlertType = "PEP", // Default since Category doesn't exist
                        SimilarityScore = similarity,
                        MatchAlgorithm = "Levenshtein",
                        Status = "Open",
                        WorkflowStatus = "PendingReview",
                        Priority = similarity > 0.9 ? "High" : "Medium",
                        RiskLevel = similarity > 0.9 ? "High" : "Medium",
                        SourceList = match.Source,
                        SourceCategory = "PEP", // Default since Category doesn't exist
                        MatchingDetails = $"Name similarity: {similarity:P2}",
                        CreatedAtUtc = DateTime.UtcNow,
                        CreatedBy = "System"
                    });
                }
            }

            return alerts;
        }

        private async Task<List<Alert>> PerformOpenSanctionsScreeningAsync(Customer customer)
        {
            var alerts = new List<Alert>();

            try
            {
                // For now, treat all customers as persons
                var matches = await _openSanctionsDataService.MatchPersonAsync(
                    customer.FullName,
                    customer.DateOfBirth,
                    customer.Country);

                foreach (var match in matches.Where(m => m.Score > 0.7))
                {
                    var alert = new Alert
                    {
                        Id = Guid.NewGuid(),
                        CustomerId = customer.Id,
                        Context = "Onboarding",
                        AlertType = "OpenSanctions",
                        SimilarityScore = match.Score,
                        MatchAlgorithm = "OpenSanctions",
                        Status = "Open",
                        WorkflowStatus = "PendingReview",
                        Priority = match.Score > 0.9 ? "Critical" : match.Score > 0.8 ? "High" : "Medium",
                        RiskLevel = match.Score > 0.9 ? "Critical" : match.Score > 0.8 ? "High" : "Medium",
                        SourceList = "OpenSanctions",
                        SourceCategory = match.Schema,
                        MatchingDetails = $"OpenSanctions match: {match.Score:P2}",
                        CreatedAtUtc = DateTime.UtcNow,
                        CreatedBy = "System",
                        
                        // OpenSanctions specific data
                        OpenSanctionsEntityId = match.Id,
                        OpenSanctionsScore = match.Score,
                        OpenSanctionsEntityType = match.Schema,
                        OpenSanctionsDatasets = match.Datasets, // Already JSON string
                        OpenSanctionsAliases = match.Aliases, // Already JSON string
                        OpenSanctionsSanctions = match.Sanctions, // Already JSON string
                        OpenSanctionsCountries = match.Countries, // Already JSON string
                        OpenSanctionsAddresses = match.Addresses, // Already JSON string
                        OpenSanctionsFirstSeen = match.FirstSeen,
                        OpenSanctionsLastSeen = match.LastSeen,
                        OpenSanctionsLastChange = match.LastChange,
                        OpenSanctionsLastChecked = DateTime.UtcNow
                    };

                    // Calculate match features
                    var matchFeatures = CalculateMatchFeatures(customer, match);
                    alert.OpenSanctionsMatchFeatures = JsonSerializer.Serialize(matchFeatures);

                    alerts.Add(alert);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing OpenSanctions screening for customer: {CustomerId}", customer.Id);
            }

            return alerts;
        }

        private OpenSanctionsMatchFeatures CalculateMatchFeatures(Customer customer, OpenSanctionsEntity match)
        {
            var features = new OpenSanctionsMatchFeatures();

            // Name matching
            features.NameScore = CalculateNameSimilarity(customer.FullName, match.Name);

            // Date of birth matching
            if (customer.DateOfBirth.HasValue && match.BirthDate.HasValue)
            {
                features.DateOfBirthScore = customer.DateOfBirth.Value.Date == match.BirthDate.Value.Date ? 1.0 : 0.0;
            }

            // Nationality matching
            if (!string.IsNullOrEmpty(customer.Country) && !string.IsNullOrEmpty(match.Countries))
            {
                try
                {
                    var countries = JsonSerializer.Deserialize<List<string>>(match.Countries) ?? new List<string>();
                    features.NationalityScore = countries.Any(c => c.Equals(customer.Country, StringComparison.OrdinalIgnoreCase)) ? 1.0 : 0.0;
                }
                catch
                {
                    features.NationalityScore = 0.0;
                }
            }

            // Calculate overall score
            var scores = new List<double> { features.NameScore };
            if (features.DateOfBirthScore.HasValue) scores.Add(features.DateOfBirthScore.Value);
            if (features.NationalityScore.HasValue) scores.Add(features.NationalityScore.Value);
            
            features.OverallScore = scores.Average();

            return features;
        }

        private double CalculateNameSimilarity(string name1, string name2)
        {
            // Simple Levenshtein distance implementation
            if (string.IsNullOrEmpty(name1) || string.IsNullOrEmpty(name2))
                return 0.0;

            name1 = name1.ToLowerInvariant();
            name2 = name2.ToLowerInvariant();

            var distance = LevenshteinDistance(name1, name2);
            var maxLength = Math.Max(name1.Length, name2.Length);
            
            return maxLength == 0 ? 1.0 : 1.0 - (double)distance / maxLength;
        }

        private int LevenshteinDistance(string s1, string s2)
        {
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
