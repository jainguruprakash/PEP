using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PEPScanner.Infrastructure.Data;
using PEPScanner.Domain.Entities;
using Hangfire;

namespace PEPScanner.Application.Services
{
    public interface ICustomerMediaScanningService
    {
        Task<CustomerScanResult> ScanCustomerAsync(Guid customerId);
        Task<BulkScanResult> ScanAllCustomersAsync();
        Task<BulkScanResult> ScanHighRiskCustomersAsync();
        Task<BulkScanResult> ScanCustomersBatchAsync(List<Guid> customerIds);
        Task SchedulePeriodicScansAsync();
        Task<List<CustomerScanStatus>> GetScanStatusAsync();
        Task<CustomerScanResult> RescanCustomerAsync(Guid customerId);
    }

    public class CustomerMediaScanningService : ICustomerMediaScanningService
    {
        private readonly PepScannerDbContext _context;
        private readonly ILogger<CustomerMediaScanningService> _logger;
        private readonly IAdverseMediaService _adverseMediaService;
        private readonly IAlertService _alertService;

        public CustomerMediaScanningService(
            PepScannerDbContext context,
            ILogger<CustomerMediaScanningService> logger,
            IAdverseMediaService adverseMediaService,
            IAlertService alertService)
        {
            _context = context;
            _logger = logger;
            _adverseMediaService = adverseMediaService;
            _alertService = alertService;
        }

        public async Task<CustomerScanResult> ScanCustomerAsync(Guid customerId)
        {
            try
            {
                _logger.LogInformation("Starting adverse media scan for customer {CustomerId}", customerId);

                var customer = await _context.Customers.FindAsync(customerId);
                if (customer == null)
                {
                    throw new ArgumentException($"Customer {customerId} not found");
                }

                var scanResult = new CustomerScanResult
                {
                    CustomerId = customerId,
                    CustomerName = customer.FullName,
                    ScanStartTime = DateTime.UtcNow,
                    Status = "InProgress"
                };

                // Perform adverse media scan
                var mediaResults = await _adverseMediaService.ScanEntityAsync(new AdverseMediaScanRequest
                {
                    EntityName = customer.FullName,
                    EntityType = "Individual",
                    DateRange = "90", // Last 90 days
                    RiskThreshold = "medium",
                    IncludeFinancialCrime = true,
                    IncludeCorruption = true,
                    IncludeTerrorism = true,
                    IncludeFraud = true,
                    IncludeSanctions = true,
                    IncludeMoneyLaundering = true
                });

                scanResult.MediaResultsFound = mediaResults.Count;
                scanResult.HighRiskResults = mediaResults.Count(r => r.RiskScore >= 75);
                scanResult.MediumRiskResults = mediaResults.Count(r => r.RiskScore >= 50 && r.RiskScore < 75);
                scanResult.LowRiskResults = mediaResults.Count(r => r.RiskScore < 50);

                // Create alerts for high-risk findings
                var alertsCreated = 0;
                foreach (var result in mediaResults.Where(r => r.RiskScore >= 75))
                {
                    try
                    {
                        await _alertService.CreateFromAdverseMediaAsync(new CreateMediaAlertRequest
                        {
                            CustomerId = customerId,
                            EntityName = customer.FullName,
                            EntityType = "Individual",
                            RiskScore = result.RiskScore,
                            MediaHeadline = result.Headline,
                            MediaSource = result.Source,
                            PublishedDate = result.PublishedDate,
                            RiskCategories = result.RiskCategories,
                            Excerpt = result.Excerpt,
                            ArticleUrl = result.Url,
                            Sentiment = result.Sentiment
                        });
                        alertsCreated++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to create alert for customer {CustomerId} media result", customerId);
                    }
                }

                scanResult.AlertsCreated = alertsCreated;
                scanResult.ScanEndTime = DateTime.UtcNow;
                scanResult.Status = "Completed";

                // Update customer's last scan date and risk level
                customer.LastMediaScanDate = DateTime.UtcNow;
                if (scanResult.HighRiskResults > 0)
                {
                    customer.RiskLevel = "High";
                }
                else if (scanResult.MediumRiskResults > 0)
                {
                    customer.RiskLevel = "Medium";
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Completed adverse media scan for customer {CustomerId}. Found {ResultsCount} results, created {AlertsCount} alerts",
                    customerId, scanResult.MediaResultsFound, scanResult.AlertsCreated);

                return scanResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scanning customer {CustomerId} for adverse media", customerId);
                throw;
            }
        }

        public async Task<BulkScanResult> ScanAllCustomersAsync()
        {
            try
            {
                _logger.LogInformation("Starting bulk adverse media scan for all customers");

                var customers = await _context.Customers
                    .Where(c => c.IsActive)
                    .Select(c => c.Id)
                    .ToListAsync();

                return await ScanCustomersBatchAsync(customers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during bulk scan of all customers");
                throw;
            }
        }

        public async Task<BulkScanResult> ScanHighRiskCustomersAsync()
        {
            try
            {
                _logger.LogInformation("Starting bulk adverse media scan for high-risk customers");

                var highRiskCustomers = await _context.Customers
                    .Where(c => c.IsActive && (c.RiskLevel == "High" || c.RiskLevel == "Medium"))
                    .Select(c => c.Id)
                    .ToListAsync();

                return await ScanCustomersBatchAsync(highRiskCustomers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during bulk scan of high-risk customers");
                throw;
            }
        }

        public async Task<BulkScanResult> ScanCustomersBatchAsync(List<Guid> customerIds)
        {
            var bulkResult = new BulkScanResult
            {
                TotalCustomers = customerIds.Count,
                StartTime = DateTime.UtcNow,
                Status = "InProgress"
            };

            var successfulScans = 0;
            var failedScans = 0;
            var totalAlertsCreated = 0;

            foreach (var customerId in customerIds)
            {
                try
                {
                    var scanResult = await ScanCustomerAsync(customerId);
                    successfulScans++;
                    totalAlertsCreated += scanResult.AlertsCreated;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to scan customer {CustomerId}", customerId);
                    failedScans++;
                }
            }

            bulkResult.SuccessfulScans = successfulScans;
            bulkResult.FailedScans = failedScans;
            bulkResult.TotalAlertsCreated = totalAlertsCreated;
            bulkResult.EndTime = DateTime.UtcNow;
            bulkResult.Status = "Completed";

            _logger.LogInformation("Bulk scan completed: {Successful} successful, {Failed} failed, {Alerts} alerts created",
                successfulScans, failedScans, totalAlertsCreated);

            return bulkResult;
        }

        public async Task SchedulePeriodicScansAsync()
        {
            _logger.LogInformation("Setting up periodic customer media scanning jobs");

            // Daily scan for high-risk customers
            RecurringJob.AddOrUpdate(
                "daily-high-risk-customer-scan",
                () => ScanHighRiskCustomersAsync(),
                "0 2 * * *", // Daily at 2 AM
                TimeZoneInfo.Utc);

            // Weekly scan for all customers
            RecurringJob.AddOrUpdate(
                "weekly-all-customers-scan",
                () => ScanAllCustomersAsync(),
                "0 3 * * 0", // Weekly on Sunday at 3 AM
                TimeZoneInfo.Utc);

            // Monthly deep scan for dormant customers
            RecurringJob.AddOrUpdate(
                "monthly-dormant-customers-scan",
                () => ScanDormantCustomersAsync(),
                "0 4 1 * *", // Monthly on 1st at 4 AM
                TimeZoneInfo.Utc);

            _logger.LogInformation("Periodic customer media scanning jobs scheduled successfully");
        }

        public async Task<List<CustomerScanStatus>> GetScanStatusAsync()
        {
            var customers = await _context.Customers
                .Where(c => c.IsActive)
                .Select(c => new CustomerScanStatus
                {
                    CustomerId = c.Id,
                    CustomerName = c.FullName,
                    RiskLevel = c.RiskLevel,
                    LastScanDate = c.LastMediaScanDate,
                    DaysSinceLastScan = c.LastMediaScanDate.HasValue 
                        ? (int)(DateTime.UtcNow - c.LastMediaScanDate.Value).TotalDays 
                        : null,
                    RequiresRescan = !c.LastMediaScanDate.HasValue || 
                        (DateTime.UtcNow - c.LastMediaScanDate.Value).TotalDays > GetRescanIntervalDays(c.RiskLevel)
                })
                .OrderByDescending(c => c.RequiresRescan)
                .ThenBy(c => c.LastScanDate)
                .ToListAsync();

            return customers;
        }

        public async Task<CustomerScanResult> RescanCustomerAsync(Guid customerId)
        {
            _logger.LogInformation("Performing rescan for customer {CustomerId}", customerId);
            return await ScanCustomerAsync(customerId);
        }

        private async Task<BulkScanResult> ScanDormantCustomersAsync()
        {
            var dormantCustomers = await _context.Customers
                .Where(c => c.IsActive && 
                    (!c.LastMediaScanDate.HasValue || 
                     (DateTime.UtcNow - c.LastMediaScanDate.Value).TotalDays > 90))
                .Select(c => c.Id)
                .ToListAsync();

            return await ScanCustomersBatchAsync(dormantCustomers);
        }

        private int GetRescanIntervalDays(string riskLevel)
        {
            return riskLevel switch
            {
                "High" => 7,     // Weekly for high risk
                "Medium" => 30,  // Monthly for medium risk
                "Low" => 90,     // Quarterly for low risk
                _ => 30          // Default monthly
            };
        }
    }

    // DTOs
    public class CustomerScanResult
    {
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public DateTime ScanStartTime { get; set; }
        public DateTime? ScanEndTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public int MediaResultsFound { get; set; }
        public int HighRiskResults { get; set; }
        public int MediumRiskResults { get; set; }
        public int LowRiskResults { get; set; }
        public int AlertsCreated { get; set; }
    }

    public class BulkScanResult
    {
        public int TotalCustomers { get; set; }
        public int SuccessfulScans { get; set; }
        public int FailedScans { get; set; }
        public int TotalAlertsCreated { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class CustomerScanStatus
    {
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string RiskLevel { get; set; } = string.Empty;
        public DateTime? LastScanDate { get; set; }
        public int? DaysSinceLastScan { get; set; }
        public bool RequiresRescan { get; set; }
    }

    public class CreateMediaAlertRequest
    {
        public Guid? CustomerId { get; set; }
        public string EntityName { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public double RiskScore { get; set; }
        public string MediaHeadline { get; set; } = string.Empty;
        public string MediaSource { get; set; } = string.Empty;
        public DateTime PublishedDate { get; set; }
        public List<string> RiskCategories { get; set; } = new();
        public string Excerpt { get; set; } = string.Empty;
        public string ArticleUrl { get; set; } = string.Empty;
        public string Sentiment { get; set; } = string.Empty;
    }

    public class AdverseMediaScanRequest
    {
        public string EntityName { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string DateRange { get; set; } = string.Empty;
        public string RiskThreshold { get; set; } = string.Empty;
        public bool IncludeFinancialCrime { get; set; }
        public bool IncludeCorruption { get; set; }
        public bool IncludeTerrorism { get; set; }
        public bool IncludeFraud { get; set; }
        public bool IncludeSanctions { get; set; }
        public bool IncludeMoneyLaundering { get; set; }
    }
}
