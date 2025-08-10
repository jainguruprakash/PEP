using Microsoft.EntityFrameworkCore;
using PEPScanner.Infrastructure.Data;
using PEPScanner.Domain.Entities;

namespace PEPScanner.API.Services
{
    public interface IAutomatedScreeningService
    {
        Task<bool> ScreenCustomerAsync(Guid customerId, string triggeredBy = "System");
        Task<List<Alert>> PerformAdverseMediaScanAsync(Customer customer, string triggeredBy = "System");
    }

    public class AutomatedScreeningService : IAutomatedScreeningService
    {
        private readonly PepScannerDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly ILogger<AutomatedScreeningService> _logger;

        public AutomatedScreeningService(PepScannerDbContext context, INotificationService notificationService, ILogger<AutomatedScreeningService> logger)
        {
            _context = context;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<bool> ScreenCustomerAsync(Guid customerId, string triggeredBy = "System")
        {
            try
            {
                var customer = await _context.Customers.FindAsync(customerId);
                if (customer == null)
                {
                    _logger.LogWarning("Customer not found for screening: {CustomerId}", customerId);
                    return false;
                }

                _logger.LogInformation("Starting automated screening for customer: {CustomerId}", customerId);

                // Perform adverse media scan
                var alerts = await PerformAdverseMediaScanAsync(customer, triggeredBy);

                _logger.LogInformation("Automated screening completed for customer: {CustomerId}. Generated {AlertCount} alerts", 
                    customerId, alerts.Count);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during automated screening for customer: {CustomerId}", customerId);
                return false;
            }
        }

        public async Task<List<Alert>> PerformAdverseMediaScanAsync(Customer customer, string triggeredBy = "System")
        {
            var alerts = new List<Alert>();

            try
            {
                // Simulate adverse media scanning logic
                var adverseMediaMatches = await SimulateAdverseMediaScan(customer);

                foreach (var match in adverseMediaMatches)
                {
                    var alert = new Alert
                    {
                        Id = Guid.NewGuid(),
                        CustomerId = customer.Id,
                        Context = "Onboarding",
                        AlertType = "Adverse Media",
                        SimilarityScore = match.Score,
                        Status = "Open",
                        Priority = DeterminePriority(match.Score),
                        WorkflowStatus = "PendingReview",
                        MatchingDetails = match.Details,
                        SourceList = match.Source,
                        SourceCategory = "Adverse Media",
                        CreatedBy = triggeredBy,
                        CreatedAtUtc = DateTime.UtcNow,
                        UpdatedAtUtc = DateTime.UtcNow
                    };

                    _context.Alerts.Add(alert);
                    alerts.Add(alert);
                }

                await _context.SaveChangesAsync();

                // Create notifications for each alert
                foreach (var alert in alerts)
                {
                    await _notificationService.CreateAlertNotificationAsync(alert);
                }

                _logger.LogInformation("Created {AlertCount} adverse media alerts for customer: {CustomerId}", 
                    alerts.Count, customer.Id);

                return alerts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing adverse media scan for customer: {CustomerId}", customer.Id);
                return alerts;
            }
        }

        private async Task<List<AdverseMediaMatch>> SimulateAdverseMediaScan(Customer customer)
        {
            // Simulate adverse media scanning - replace with actual implementation
            var matches = new List<AdverseMediaMatch>();

            // Simulate some matches based on customer name
            var fullName = $"{customer.FirstName} {customer.LastName}".ToLower();
            
            // High-risk keywords that might trigger alerts
            var riskKeywords = new[] { "fraud", "money laundering", "corruption", "sanctions", "criminal" };
            
            // Simulate finding matches (in real implementation, this would call external APIs)
            if (fullName.Contains("test") || fullName.Contains("demo"))
            {
                matches.Add(new AdverseMediaMatch
                {
                    Score = 0.85,
                    Source = "News Source A",
                    Details = $"Potential adverse media match found for {customer.FirstName} {customer.LastName} in financial news articles"
                });
            }

            // Simulate random matches for demonstration
            var random = new Random();
            if (random.NextDouble() > 0.7) // 30% chance of generating an alert
            {
                matches.Add(new AdverseMediaMatch
                {
                    Score = random.NextDouble() * 0.5 + 0.5, // Score between 0.5 and 1.0
                    Source = "Global News Database",
                    Details = $"Adverse media reference found for {customer.FirstName} {customer.LastName}"
                });
            }

            return matches;
        }

        private string DeterminePriority(double score)
        {
            return score switch
            {
                >= 0.9 => "Critical",
                >= 0.8 => "High",
                >= 0.6 => "Medium",
                _ => "Low"
            };
        }
    }

    public class AdverseMediaMatch
    {
        public double Score { get; set; }
        public string Source { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
    }
}