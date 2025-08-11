using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PEPScanner.Infrastructure.Data;
using PEPScanner.Domain.Entities;

namespace PEPScanner.API.Controllers
{
    [ApiController]
    [Route("api/customer-media-scan")]
    public class CustomerMediaScanController : ControllerBase
    {
        private readonly PepScannerDbContext _context;
        private readonly ILogger<CustomerMediaScanController> _logger;

        public CustomerMediaScanController(PepScannerDbContext context, ILogger<CustomerMediaScanController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost("scan/bulk/all")]
        public async Task<IActionResult> BulkScanAllCustomers()
        {
            try
            {
                var customers = await _context.Customers
                    .Where(c => !c.IsDeleted && c.Status == "Active")
                    .ToListAsync();

                var scanResults = new List<object>();

                foreach (var customer in customers)
                {
                    var result = await PerformMediaScan(customer);
                    scanResults.Add(result);
                }

                return Ok(new
                {
                    totalCustomers = customers.Count,
                    scannedCustomers = scanResults.Count,
                    results = scanResults,
                    scanDate = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during bulk customer media scan");
                return StatusCode(500, new { error = "Bulk scan failed", details = ex.Message });
            }
        }

        [HttpPost("scan/{customerId}")]
        public async Task<IActionResult> ScanCustomer(Guid customerId)
        {
            try
            {
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.Id == customerId && !c.IsDeleted);

                if (customer == null)
                {
                    return NotFound(new { error = "Customer not found" });
                }

                var result = await PerformMediaScan(customer);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scanning customer {CustomerId}", customerId);
                return StatusCode(500, new { error = "Customer scan failed", details = ex.Message });
            }
        }

        private async Task<object> PerformMediaScan(Customer customer)
        {
            // Simulate media scanning
            var riskScore = Random.Shared.Next(1, 100);
            var hasAdverseMedia = riskScore > 70;

            var result = new
            {
                customerId = customer.Id,
                customerName = customer.FullName,
                riskScore = riskScore,
                riskLevel = GetRiskLevel(riskScore),
                hasAdverseMedia = hasAdverseMedia,
                mediaMatches = hasAdverseMedia ? Random.Shared.Next(1, 5) : 0,
                scanDate = DateTime.UtcNow,
                sources = hasAdverseMedia ? new[] { "Reuters", "BBC", "Financial Times" } : new string[0],
                categories = hasAdverseMedia ? new[] { "Financial Crime", "Sanctions" } : new string[0]
            };

            // Create alert if high risk
            if (riskScore > 80)
            {
                await CreateMediaAlert(customer, riskScore);
            }

            return result;
        }

        private async Task CreateMediaAlert(Customer customer, int riskScore)
        {
            try
            {
                var alert = new Alert
                {
                    Id = Guid.NewGuid(),
                    CustomerId = customer.Id,
                    Context = "CustomerMediaScan",
                    AlertType = "Media Screening",
                    Status = "Open",
                    Priority = riskScore > 90 ? "Critical" : "High",
                    RiskLevel = riskScore > 90 ? "Critical" : "High",
                    WorkflowStatus = "PendingReview",
                    SimilarityScore = riskScore,
                    SourceList = "Media Scan System",
                    SourceCategory = "Automated Screening",
                    MatchingDetails = $"{{\"riskScore\": {riskScore}, \"scanType\": \"bulk_media_scan\"}}",
                    CreatedAtUtc = DateTime.UtcNow,
                    UpdatedAtUtc = DateTime.UtcNow,
                    CreatedBy = "MediaScanSystem",
                    DueDate = DateTime.UtcNow.AddHours(riskScore > 90 ? 4 : 8),
                    SlaHours = riskScore > 90 ? 4 : 8,
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
                _logger.LogError(ex, "Error creating media alert for customer {CustomerId}", customer.Id);
            }
        }

        private string GetRiskLevel(int score)
        {
            return score switch
            {
                > 90 => "Critical",
                > 70 => "High",
                > 40 => "Medium",
                _ => "Low"
            };
        }
    }
}