using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PEPScanner.API.Data;
using PEPScanner.API.Models;

namespace PEPScanner.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AlertsController : ControllerBase
    {
        private readonly PepScannerDbContext _context;

        public AlertsController(PepScannerDbContext context)
        {
            _context = context;
        }

        // GET: api/Alerts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Alert>>> GetAlerts()
        {
            return await _context.Alerts
                .Include(a => a.Customer)
                .Include(a => a.WatchlistEntry)
                .ToListAsync();
        }

        // GET: api/Alerts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Alert>> GetAlert(Guid id)
        {
            var alert = await _context.Alerts
                .Include(a => a.Customer)
                .Include(a => a.WatchlistEntry)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (alert == null)
            {
                return NotFound();
            }

            return alert;
        }

        // POST: api/Alerts
        [HttpPost]
        public async Task<ActionResult<Alert>> CreateAlert(Alert alert)
        {
            alert.Id = Guid.NewGuid();
            alert.CreatedAtUtc = DateTime.UtcNow;
            alert.UpdatedAtUtc = DateTime.UtcNow;

            _context.Alerts.Add(alert);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAlert), new { id = alert.Id }, alert);
        }

        // PUT: api/Alerts/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAlert(Guid id, Alert alert)
        {
            if (id != alert.Id)
            {
                return BadRequest();
            }

            alert.UpdatedAtUtc = DateTime.UtcNow;
            _context.Entry(alert).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AlertExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // GET: api/Alerts/status/{status}
        [HttpGet("status/{status}")]
        public async Task<ActionResult<IEnumerable<Alert>>> GetAlertsByStatus(string status)
        {
            return await _context.Alerts
                .Include(a => a.Customer)
                .Include(a => a.WatchlistEntry)
                .Where(a => a.Status == status)
                .ToListAsync();
        }

        private bool AlertExists(Guid id)
        {
            return _context.Alerts.Any(e => e.Id == id);
        }
    }
}
