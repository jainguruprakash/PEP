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
                var customers = await _context.Customers
                    .Where(c => !c.IsDeleted)
                    .Select(c => new
                    {
                        c.Id,
                        c.FullName,
                        c.EmailAddress,
                        c.Country,
                        c.RiskLevel,
                        c.CreatedAtUtc,
                        c.LastScreeningDate,
                        c.RequiresEdd
                    })
                    .OrderByDescending(c => c.CreatedAtUtc)
                    .ToListAsync();

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
                var customer = new Customer
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = Guid.NewGuid(),
                    FullName = $"{request.FirstName} {request.LastName}".Trim(),
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    EmailAddress = request.Email,
                    Country = request.Country,
                    PhoneNumber = request.PhoneNumber,
                    DateOfBirth = request.DateOfBirth,
                    Nationality = request.Nationality,
                    IdentificationNumber = request.IdentificationNumber,
                    IdentificationType = request.IdentificationType,
                    Address = request.Address,
                    City = request.City,
                    State = request.State,
                    PostalCode = request.PostalCode,
                    RiskLevel = "Low",
                    Status = "Active",
                    IsActive = true,
                    CreatedAtUtc = DateTime.UtcNow,
                    UpdatedAtUtc = DateTime.UtcNow,
                    CreatedBy = "System"
                };

                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Customer created: {Name} with ID: {Id}", customer.FullName, customer.Id);
                return Ok(new { message = "Customer created successfully", customerId = customer.Id });
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
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
                if (customer == null)
                {
                    return NotFound(new { error = "Customer not found" });
                }

                customer.FullName = $"{request.FirstName} {request.LastName}".Trim();
                customer.EmailAddress = request.Email;
                customer.Country = request.Country;
                customer.PhoneNumber = request.PhoneNumber;
                customer.DateOfBirth = request.DateOfBirth;
                customer.Nationality = request.Nationality;
                customer.IdentificationNumber = request.IdentificationNumber;
                customer.IdentificationType = request.IdentificationType;
                customer.Address = request.Address;
                customer.City = request.City;
                customer.State = request.State;
                customer.PostalCode = request.PostalCode;
                customer.UpdatedAtUtc = DateTime.UtcNow;
                customer.UpdatedBy = "System"; // TODO: Get from user context

                await _context.SaveChangesAsync();
                return Ok(new { message = "Customer updated successfully" });
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
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
                if (customer == null)
                {
                    return NotFound(new { error = "Customer not found" });
                }

                // Soft delete
                customer.IsDeleted = true;
                customer.DeletedAtUtc = DateTime.UtcNow;
                customer.UpdatedAtUtc = DateTime.UtcNow;
                customer.UpdatedBy = "System"; // TODO: Get from user context

                await _context.SaveChangesAsync();
                return Ok(new { message = "Customer deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting customer {Id}", id);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpPost("test-create")]
        public async Task<IActionResult> TestCreate()
        {
            try
            {
                var testCustomer = new Customer
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = Guid.NewGuid(),
                    FullName = "Test Customer",
                    EmailAddress = "test@example.com",
                    Country = "India",
                    RiskLevel = "Low",
                    IsActive = true,
                    CreatedAtUtc = DateTime.UtcNow,
                    UpdatedAtUtc = DateTime.UtcNow,
                    CreatedBy = "TestEndpoint"
                };

                _context.Customers.Add(testCustomer);
                var result = await _context.SaveChangesAsync();

                return Ok(new { 
                    message = "Test customer created successfully", 
                    customerId = testCustomer.Id,
                    recordsAffected = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating test customer");
                return StatusCode(500, new { error = "Internal server error", details = ex.Message, stackTrace = ex.StackTrace });
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

                var customersToAdd = new List<Customer>();
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
                        if (fields.Length < 3)
                        {
                            result.FailedCount++;
                            result.Errors.Add($"Line {lineNumber}: Insufficient data fields (minimum: FullName, Email, Country)");
                            continue;
                        }

                        var fullName = fields[0].Trim();
                        var email = fields[1].Trim();
                        var country = fields[2].Trim();
                        var phoneNumber = fields.Length > 3 ? fields[3].Trim() : null;
                        var nationality = fields.Length > 4 ? fields[4].Trim() : null;

                        if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(email))
                        {
                            result.FailedCount++;
                            result.Errors.Add($"Line {lineNumber}: Missing required fields (FullName, Email)");
                            continue;
                        }

                        // Check for duplicate email
                        var existingCustomer = await _context.Customers
                            .FirstOrDefaultAsync(c => c.EmailAddress == email && !c.IsDeleted);
                        
                        if (existingCustomer != null)
                        {
                            result.FailedCount++;
                            result.Errors.Add($"Line {lineNumber}: Customer with email {email} already exists");
                            continue;
                        }

                        var nameParts = fullName.Split(' ', 2);
                        var customer = new Customer
                        {
                            Id = Guid.NewGuid(),
                            OrganizationId = Guid.NewGuid(),
                            FullName = fullName,
                            FirstName = nameParts[0],
                            LastName = nameParts.Length > 1 ? nameParts[1] : "",
                            EmailAddress = email,
                            Country = country,
                            PhoneNumber = phoneNumber,
                            Nationality = nationality,
                            RiskLevel = "Low",
                            Status = "Active",
                            IsActive = true,
                            CreatedAtUtc = DateTime.UtcNow,
                            UpdatedAtUtc = DateTime.UtcNow,
                            CreatedBy = "BulkUpload"
                        };

                        customersToAdd.Add(customer);
                        result.SuccessCount++;
                        _logger.LogInformation("Prepared customer for bulk insert: {Name}", fullName);
                    }
                    catch (Exception ex)
                    {
                        result.FailedCount++;
                        result.Errors.Add($"Line {lineNumber}: {ex.Message}");
                    }
                }

                // Bulk insert customers
                if (customersToAdd.Any())
                {
                    _context.Customers.AddRange(customersToAdd);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Bulk inserted {Count} customers", customersToAdd.Count);
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
        public string? PhoneNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Nationality { get; set; }
        public string? IdentificationNumber { get; set; }
        public string? IdentificationType { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
    }

    public class UpdateCustomerRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Nationality { get; set; }
        public string? IdentificationNumber { get; set; }
        public string? IdentificationType { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
    }

    public class BulkUploadResult
    {
        public int TotalRecords { get; set; }
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
}
