using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PEPScanner.Infrastructure.Data;
using PEPScanner.Domain.Entities;

namespace PEPScanner.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly PepScannerDbContext _context;
        private readonly ILogger<CustomersController> _logger;

        public CustomersController(PepScannerDbContext context, ILogger<CustomersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                // Temporary mock data to test API
                var customers = new[]
                {
                    new
                    {
                        Id = Guid.NewGuid(),
                        FullName = "John Doe",
                        EmailAddress = "john.doe@email.com",
                        Country = "United States",
                        RiskLevel = "Low",
                        CreatedAtUtc = DateTime.UtcNow
                    },
                    new
                    {
                        Id = Guid.NewGuid(),
                        FullName = "Jane Smith",
                        EmailAddress = "jane.smith@email.com",
                        Country = "United Kingdom",
                        RiskLevel = "Medium",
                        CreatedAtUtc = DateTime.UtcNow
                    }
                };

                return Ok(customers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customers");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var customer = await _context.Customers.FindAsync(id);
                if (customer == null)
                {
                    return NotFound(new { error = "Customer not found" });
                }

                return Ok(customer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer {Id}", id);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCustomerRequest request)
        {
            try
            {
                // Temporary mock response to test API
                var customer = new
                {
                    Id = Guid.NewGuid(),
                    FullName = $"{request.FirstName} {request.LastName}".Trim(),
                    EmailAddress = request.Email,
                    Country = request.Country,
                    RiskLevel = "Low",
                    CreatedAtUtc = DateTime.UtcNow
                };

                _logger.LogInformation("Customer created: {Name}", customer.FullName);
                return Ok(new { message = "Customer created successfully", customer });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating customer");
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCustomerRequest request)
        {
            try
            {
                var customer = await _context.Customers.FindAsync(id);
                if (customer == null)
                {
                    return NotFound(new { error = "Customer not found" });
                }

                customer.FullName = $"{request.FirstName} {request.LastName}".Trim();
                customer.EmailAddress = request.Email;
                customer.Country = request.Country;
                customer.UpdatedAtUtc = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer {Id}", id);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var customer = await _context.Customers.FindAsync(id);
                if (customer == null)
                {
                    return NotFound(new { error = "Customer not found" });
                }

                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting customer {Id}", id);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpPost("bulk-upload")]
        public async Task<IActionResult> BulkUpload(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { error = "No file uploaded" });
                }

                var result = new BulkUploadResult
                {
                    TotalRecords = 0,
                    SuccessCount = 0,
                    FailedCount = 0,
                    Errors = new List<string>()
                };

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
                    result.TotalRecords++;

                    try
                    {
                        var fields = line.Split(',');
                        if (fields.Length < 4)
                        {
                            result.FailedCount++;
                            result.Errors.Add($"Line {lineNumber}: Insufficient data fields");
                            continue;
                        }

                        // Mock processing - just validate the data
                        var customerName = $"{fields[0].Trim()} {fields[1].Trim()}".Trim();
                        var email = fields[2].Trim();
                        var country = fields[3].Trim();

                        if (string.IsNullOrEmpty(customerName) || string.IsNullOrEmpty(email))
                        {
                            result.FailedCount++;
                            result.Errors.Add($"Line {lineNumber}: Missing required fields");
                            continue;
                        }

                        // Simulate successful processing
                        result.SuccessCount++;
                        _logger.LogInformation("Processed customer: {Name}", customerName);
                    }
                    catch (Exception ex)
                    {
                        result.FailedCount++;
                        result.Errors.Add($"Line {lineNumber}: {ex.Message}");
                    }
                }

                _logger.LogInformation("Bulk upload completed: {Success} success, {Failed} failed",
                    result.SuccessCount, result.FailedCount);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during bulk upload");
                return StatusCode(500, new { error = "Internal server error during upload", details = ex.Message });
            }
        }
    }

    public class CreateCustomerRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
    }

    public class UpdateCustomerRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
    }

    public class BulkUploadResult
    {
        public int TotalRecords { get; set; }
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
}
