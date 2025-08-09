using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PEPScanner.Application.Abstractions;
using PEPScanner.Application.Contracts;
using PEPScanner.API.Models;

namespace PEPScanner.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ScreeningController : ControllerBase
    {
        private readonly IScreeningService _screeningService;
        private readonly ILogger<ScreeningController> _logger;

        public ScreeningController(IScreeningService screeningService, ILogger<ScreeningController> logger)
        {
            _screeningService = screeningService;
            _logger = logger;
        }

        /// <summary>
        /// Screen a customer in real-time during onboarding
        /// </summary>
        [HttpPost("customer")]
        [Authorize(Policy = "ComplianceOfficer")]
        public async Task<ActionResult<ScreeningResult>> ScreenCustomer([FromBody] CustomerScreeningRequest customer)
        {
            try
            {
                var result = await _screeningService.ScreenCustomerAsync(customer, "Onboarding");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error screening customer {CustomerName}", customer.FullName);
                return StatusCode(500, new { error = "Screening failed", message = ex.Message });
            }
        }

        /// <summary>
        /// Screen a transaction against PEP/sanctions lists
        /// </summary>
        [HttpPost("transaction")]
        [Authorize(Policy = "ComplianceOfficer")]
        public async Task<ActionResult<ScreeningResult>> ScreenTransaction([FromBody] TransactionScreeningRequest request)
        {
            try
            {
                var result = await _screeningService.ScreenTransactionAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error screening transaction {TransactionId}", request.TransactionId);
                return StatusCode(500, new { error = "Transaction screening failed", message = ex.Message });
            }
        }

        /// <summary>
        /// Search names against watchlists
        /// </summary>
        [HttpPost("search")]
        [Authorize(Policy = "ComplianceOfficer")]
        public async Task<ActionResult<List<NameMatchResult>>> SearchNames([FromBody] NameSearchRequest request)
        {
            try
            {
                var results = await _screeningService.SearchNamesAsync(request);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching names for {Name}", request.Name);
                return StatusCode(500, new { error = "Name search failed", message = ex.Message });
            }
        }

        /// <summary>
        /// Get screening statistics for reporting
        /// </summary>
        [HttpGet("statistics")]
        [Authorize(Policy = "Manager")]
        public async Task<ActionResult<ScreeningStatistics>> GetStatistics(
            [FromQuery] DateTime startDate, 
            [FromQuery] DateTime endDate)
        {
            try
            {
                var statistics = await _screeningService.GetScreeningStatisticsAsync(startDate, endDate);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting screening statistics from {StartDate} to {EndDate}", startDate, endDate);
                return StatusCode(500, new { error = "Failed to get statistics", message = ex.Message });
            }
        }

        /// <summary>
        /// Update customer screening status
        /// </summary>
        [HttpPut("customer/{customerId}/status")]
        [Authorize(Policy = "ComplianceOfficer")]
        public async Task<ActionResult<object>> UpdateScreeningStatus(
            Guid customerId, 
            [FromBody] DateTime screeningDate)
        {
            try
            {
                var updated = await _screeningService.UpdateScreeningStatusAsync(customerId, screeningDate);
                return Ok(updated);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { error = "Customer not found", message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating screening status for customer {CustomerId}", customerId);
                return StatusCode(500, new { error = "Failed to update screening status", message = ex.Message });
            }
        }
    }
}
