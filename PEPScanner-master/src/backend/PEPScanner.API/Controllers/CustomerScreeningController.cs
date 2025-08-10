using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PEPScanner.Infrastructure.Data;
using PEPScanner.Domain.Entities;
using PEPScanner.API.Services;

namespace PEPScanner.API.Controllers
{
    [ApiController]
    [Route("api/customer-screening")]
    public class CustomerScreeningController : ControllerBase
    {
        private readonly PepScannerDbContext _context;
        private readonly IAutomatedScreeningService _screeningService;
        private readonly ILogger<CustomerScreeningController> _logger;

        public CustomerScreeningController(
            PepScannerDbContext context, 
            IAutomatedScreeningService screeningService,
            ILogger<CustomerScreeningController> logger)
        {
            _context = context;
            _screeningService = screeningService;
            _logger = logger;
        }

        [HttpPost("onboard-customer")]
        public async Task<IActionResult> OnboardCustomer([FromBody] CustomerOnboardingRequest request)
        {
            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                // Create customer
                var customer = new Customer
                {
                    Id = Guid.NewGuid(),
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    Phone = request.Phone,
                    DateOfBirth = request.DateOfBirth,
                    Nationality = request.Nationality,
                    Address = request.Address,
                    City = request.City,
                    State = request.State,
                    PostalCode = request.PostalCode,
                    Country = request.Country,
                    CustomerType = request.CustomerType,
                    RiskLevel = "Medium", // Default risk level
                    Status = "Active",
                    OnboardingDate = DateTime.UtcNow,
                    CreatedAtUtc = DateTime.UtcNow,
                    UpdatedAtUtc = DateTime.UtcNow
                };

                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();

                // Automatic screening based on configuration
                bool autoScreening = request.AutoScreening ?? true; // Default to true
                
                if (autoScreening)
                {
                    _logger.LogInformation("Starting automatic screening for customer: {CustomerId}", customer.Id);
                    await _screeningService.ScreenCustomerAsync(customer.Id, "OnboardingSystem");
                }

                await transaction.CommitAsync();

                var response = new
                {
                    customerId = customer.Id,
                    message = "Customer onboarded successfully",
                    autoScreeningEnabled = autoScreening,
                    screeningStatus = autoScreening ? "Initiated" : "Manual"
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error onboarding customer");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpPost("{customerId}/manual-screen")]
        public async Task<IActionResult> ManualScreenCustomer(Guid customerId, [FromBody] ManualScreeningRequest request)
        {
            try
            {
                var customer = await _context.Customers.FindAsync(customerId);
                if (customer == null)
                {
                    return NotFound(new { error = "Customer not found" });
                }

                _logger.LogInformation("Starting manual screening for customer: {CustomerId} by {User}", 
                    customerId, request.InitiatedBy);

                var success = await _screeningService.ScreenCustomerAsync(customerId, request.InitiatedBy);

                if (success)
                {
                    return Ok(new { message = "Manual screening initiated successfully" });
                }
                else
                {
                    return BadRequest(new { error = "Failed to initiate screening" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during manual screening");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("{customerId}/screening-results")]
        public async Task<IActionResult> GetScreeningResults(Guid customerId)
        {
            try
            {
                var customer = await _context.Customers.FindAsync(customerId);
                if (customer == null)
                {
                    return NotFound(new { error = "Customer not found" });
                }

                var alerts = await _context.Alerts
                    .Where(a => a.CustomerId == customerId)
                    .OrderByDescending(a => a.CreatedAtUtc)
                    .Select(a => new
                    {
                        a.Id,
                        a.AlertType,
                        a.Priority,
                        a.Status,
                        a.WorkflowStatus,
                        a.SimilarityScore,
                        a.SourceList,
                        a.CreatedAtUtc,
                        a.ReviewedBy,
                        a.ReviewedAtUtc,
                        a.ApprovedBy,
                        a.ApprovedAtUtc,
                        a.RejectedBy,
                        a.RejectedAtUtc
                    })
                    .ToListAsync();

                var summary = new
                {
                    customerId = customerId,
                    customerName = $"{customer.FirstName} {customer.LastName}",
                    totalAlerts = alerts.Count,
                    pendingReview = alerts.Count(a => a.WorkflowStatus == "PendingReview"),
                    underReview = alerts.Count(a => a.WorkflowStatus == "UnderReview"),
                    approved = alerts.Count(a => a.WorkflowStatus == "Approved"),
                    rejected = alerts.Count(a => a.WorkflowStatus == "Rejected"),
                    alerts = alerts
                };

                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching screening results");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpPost("configure-auto-screening")]
        public async Task<IActionResult> ConfigureAutoScreening([FromBody] AutoScreeningConfigRequest request)
        {
            try
            {
                // Store auto-screening configuration
                var config = new OrganizationConfiguration
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = request.OrganizationId,
                    Category = "Screening",
                    Key = "AutoScreeningEnabled",
                    Value = request.Enabled.ToString(),
                    CreatedAtUtc = DateTime.UtcNow
                };

                _context.OrganizationConfigurations.Add(config);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Auto-screening configuration updated" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error configuring auto-screening");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }
    }

    public class CustomerOnboardingRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string Nationality { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string CustomerType { get; set; } = "Individual";
        public bool? AutoScreening { get; set; } = true;
    }

    public class ManualScreeningRequest
    {
        public string InitiatedBy { get; set; } = string.Empty;
    }

    public class AutoScreeningConfigRequest
    {
        public Guid OrganizationId { get; set; }
        public bool Enabled { get; set; }
    }
}