using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PEPScanner.Application.Services;

namespace PEPScanner.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CustomerMediaScanController : ControllerBase
    {
        private readonly ICustomerMediaScanningService _scanningService;
        private readonly ILogger<CustomerMediaScanController> _logger;

        public CustomerMediaScanController(
            ICustomerMediaScanningService scanningService,
            ILogger<CustomerMediaScanController> logger)
        {
            _scanningService = scanningService;
            _logger = logger;
        }

        [HttpPost("scan/{customerId}")]
        public async Task<IActionResult> ScanCustomer(Guid customerId)
        {
            try
            {
                _logger.LogInformation("Starting adverse media scan for customer {CustomerId}", customerId);
                
                var result = await _scanningService.ScanCustomerAsync(customerId);
                
                return Ok(new
                {
                    success = true,
                    message = $"Customer scan completed successfully",
                    data = result
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Customer not found: {CustomerId}", customerId);
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scanning customer {CustomerId}", customerId);
                return StatusCode(500, new { error = "Internal server error during customer scan" });
            }
        }

        [HttpPost("scan/bulk/all")]
        public async Task<IActionResult> ScanAllCustomers()
        {
            try
            {
                _logger.LogInformation("Starting bulk scan for all customers");
                
                var result = await _scanningService.ScanAllCustomersAsync();
                
                return Ok(new
                {
                    success = true,
                    message = "Bulk scan of all customers completed",
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during bulk scan of all customers");
                return StatusCode(500, new { error = "Internal server error during bulk scan" });
            }
        }

        [HttpPost("scan/bulk/high-risk")]
        public async Task<IActionResult> ScanHighRiskCustomers()
        {
            try
            {
                _logger.LogInformation("Starting bulk scan for high-risk customers");
                
                var result = await _scanningService.ScanHighRiskCustomersAsync();
                
                return Ok(new
                {
                    success = true,
                    message = "Bulk scan of high-risk customers completed",
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during bulk scan of high-risk customers");
                return StatusCode(500, new { error = "Internal server error during high-risk scan" });
            }
        }

        [HttpPost("scan/bulk/batch")]
        public async Task<IActionResult> ScanCustomersBatch([FromBody] ScanBatchRequest request)
        {
            try
            {
                if (request.CustomerIds == null || !request.CustomerIds.Any())
                {
                    return BadRequest(new { error = "Customer IDs are required" });
                }

                _logger.LogInformation("Starting batch scan for {Count} customers", request.CustomerIds.Count);
                
                var result = await _scanningService.ScanCustomersBatchAsync(request.CustomerIds);
                
                return Ok(new
                {
                    success = true,
                    message = $"Batch scan completed for {request.CustomerIds.Count} customers",
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during batch scan");
                return StatusCode(500, new { error = "Internal server error during batch scan" });
            }
        }

        [HttpPost("schedule/setup")]
        public async Task<IActionResult> SetupPeriodicScans()
        {
            try
            {
                _logger.LogInformation("Setting up periodic customer media scanning");
                
                await _scanningService.SchedulePeriodicScansAsync();
                
                return Ok(new
                {
                    success = true,
                    message = "Periodic scanning jobs scheduled successfully",
                    schedules = new
                    {
                        dailyHighRisk = "Daily at 2:00 AM UTC",
                        weeklyAll = "Weekly on Sunday at 3:00 AM UTC",
                        monthlyDormant = "Monthly on 1st at 4:00 AM UTC"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting up periodic scans");
                return StatusCode(500, new { error = "Internal server error during schedule setup" });
            }
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetScanStatus()
        {
            try
            {
                var status = await _scanningService.GetScanStatusAsync();
                
                var summary = new
                {
                    totalCustomers = status.Count,
                    requiresRescan = status.Count(s => s.RequiresRescan),
                    highRisk = status.Count(s => s.RiskLevel == "High"),
                    mediumRisk = status.Count(s => s.RiskLevel == "Medium"),
                    lowRisk = status.Count(s => s.RiskLevel == "Low"),
                    neverScanned = status.Count(s => !s.LastScanDate.HasValue)
                };
                
                return Ok(new
                {
                    success = true,
                    summary,
                    customers = status
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting scan status");
                return StatusCode(500, new { error = "Internal server error getting scan status" });
            }
        }

        [HttpPost("rescan/{customerId}")]
        public async Task<IActionResult> RescanCustomer(Guid customerId)
        {
            try
            {
                _logger.LogInformation("Starting rescan for customer {CustomerId}", customerId);
                
                var result = await _scanningService.RescanCustomerAsync(customerId);
                
                return Ok(new
                {
                    success = true,
                    message = "Customer rescan completed successfully",
                    data = result
                });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rescanning customer {CustomerId}", customerId);
                return StatusCode(500, new { error = "Internal server error during rescan" });
            }
        }

        [HttpGet("schedule/status")]
        public IActionResult GetScheduleStatus()
        {
            try
            {
                // This would integrate with Hangfire to get actual job status
                var scheduleStatus = new
                {
                    dailyHighRiskScan = new
                    {
                        jobId = "daily-high-risk-customer-scan",
                        schedule = "0 2 * * *",
                        description = "Daily scan for high-risk customers",
                        nextRun = DateTime.UtcNow.Date.AddDays(1).AddHours(2),
                        enabled = true
                    },
                    weeklyAllCustomersScan = new
                    {
                        jobId = "weekly-all-customers-scan",
                        schedule = "0 3 * * 0",
                        description = "Weekly scan for all customers",
                        nextRun = GetNextSunday().AddHours(3),
                        enabled = true
                    },
                    monthlyDormantScan = new
                    {
                        jobId = "monthly-dormant-customers-scan",
                        schedule = "0 4 1 * *",
                        description = "Monthly scan for dormant customers",
                        nextRun = GetNextFirstOfMonth().AddHours(4),
                        enabled = true
                    }
                };

                return Ok(new
                {
                    success = true,
                    message = "Schedule status retrieved successfully",
                    schedules = scheduleStatus
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting schedule status");
                return StatusCode(500, new { error = "Internal server error getting schedule status" });
            }
        }

        [HttpPost("scan/on-demand")]
        public async Task<IActionResult> OnDemandScan([FromBody] OnDemandScanRequest request)
        {
            try
            {
                _logger.LogInformation("Starting on-demand scan with criteria: {Criteria}", request.ScanType);

                BulkScanResult result = request.ScanType.ToLower() switch
                {
                    "all" => await _scanningService.ScanAllCustomersAsync(),
                    "high-risk" => await _scanningService.ScanHighRiskCustomersAsync(),
                    "batch" when request.CustomerIds?.Any() == true => 
                        await _scanningService.ScanCustomersBatchAsync(request.CustomerIds),
                    _ => throw new ArgumentException("Invalid scan type or missing customer IDs for batch scan")
                };

                return Ok(new
                {
                    success = true,
                    message = $"On-demand {request.ScanType} scan completed",
                    data = result
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during on-demand scan");
                return StatusCode(500, new { error = "Internal server error during on-demand scan" });
            }
        }

        private DateTime GetNextSunday()
        {
            var today = DateTime.UtcNow.Date;
            var daysUntilSunday = ((int)DayOfWeek.Sunday - (int)today.DayOfWeek + 7) % 7;
            if (daysUntilSunday == 0) daysUntilSunday = 7; // If today is Sunday, get next Sunday
            return today.AddDays(daysUntilSunday);
        }

        private DateTime GetNextFirstOfMonth()
        {
            var today = DateTime.UtcNow.Date;
            var nextMonth = today.AddMonths(1);
            return new DateTime(nextMonth.Year, nextMonth.Month, 1);
        }
    }

    public class ScanBatchRequest
    {
        public List<Guid> CustomerIds { get; set; } = new();
    }

    public class OnDemandScanRequest
    {
        public string ScanType { get; set; } = string.Empty; // "all", "high-risk", "batch"
        public List<Guid>? CustomerIds { get; set; }
    }
}
