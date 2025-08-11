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
                var result = new
                {
                    customerId = request.CustomerId ?? "",
                    customerName = request.FullName ?? "",
                    riskScore = 0.1,
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

        [HttpPost("batch-file")]
        public async Task<IActionResult> ScreenBatchFile(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { error = "No file uploaded" });
                }

                var results = new List<object>
                {
                    new
                    {
                        customerName = "Sample Customer",
                        riskScore = 10,
                        status = "Clear",
                        screenedAt = DateTime.UtcNow
                    }
                };

                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing batch file");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }
    }

    public class CustomerScreeningRequest
    {
        public string? CustomerId { get; set; }
        public string? FullName { get; set; }
        public string? DateOfBirth { get; set; }
        public string? Nationality { get; set; }
        public string? Country { get; set; }
        public int? Threshold { get; set; } = 70;
        public List<string>? Sources { get; set; }
        public bool? IncludeAliases { get; set; } = true;
        public bool? IncludeFuzzyMatching { get; set; } = true;
        public bool? IncludePhoneticMatching { get; set; } = true;
    }
}