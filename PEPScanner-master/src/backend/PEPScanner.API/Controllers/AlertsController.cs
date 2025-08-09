using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PEPScanner.Infrastructure.Data;
using PEPScanner.Domain.Entities;

namespace PEPScanner.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AlertsController : ControllerBase
    {
        private readonly PepScannerDbContext _context;
        private readonly ILogger<AlertsController> _logger;

        public AlertsController(PepScannerDbContext context, ILogger<AlertsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? status = null)
        {
            try
            {
                var alertsQuery = _context.Alerts.AsQueryable();

                if (!string.IsNullOrEmpty(status))
                {
                    alertsQuery = alertsQuery.Where(a => a.Status == status);
                }

                var alerts = await alertsQuery
                    .Select(a => new
                    {
                        a.Id,
                        a.AlertType,
                        a.Status,
                        a.Priority,
                        a.Message,
                        a.CreatedAt,
                        a.UpdatedAt
                    })
                    .OrderByDescending(a => a.CreatedAt)
                    .ToListAsync();

                return Ok(alerts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting alerts");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var alert = await _context.Alerts.FindAsync(id);
                if (alert == null)
                {
                    return NotFound(new { error = "Alert not found" });
                }

                return Ok(alert);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting alert {Id}", id);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAlertRequest request)
        {
            try
            {
                var alert = await _context.Alerts.FindAsync(id);
                if (alert == null)
                {
                    return NotFound(new { error = "Alert not found" });
                }

                alert.Status = request.Status;
                alert.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating alert {Id}", id);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetByStatus(string status)
        {
            try
            {
                var alerts = await _context.Alerts
                    .Where(a => a.Status == status)
                    .Select(a => new
                    {
                        a.Id,
                        a.AlertType,
                        a.Status,
                        a.Priority,
                        a.Message,
                        a.CreatedAt
                    })
                    .OrderByDescending(a => a.CreatedAt)
                    .ToListAsync();

                return Ok(alerts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting alerts by status {Status}", status);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }
    }

    public class UpdateAlertRequest
    {
        public string Status { get; set; } = string.Empty;
    }
}
