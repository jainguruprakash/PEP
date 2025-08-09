using Microsoft.AspNetCore.Mvc;
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
                _logger.LogInformation("Screening customer: {CustomerName}", request.CustomerName);
                
                // Basic screening logic - replace with actual implementation
                var result = new
                {
                    customerId = request.CustomerId,
                    customerName = request.CustomerName,
                    riskScore = 0.2,
                    matches = new List<object>(),
                    status = "Clear",
                    screenedAt = DateTime.UtcNow
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
    }

    public class CustomerScreeningRequest
    {
        public string CustomerId { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string EntityType { get; set; } = "Individual";
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
