using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PEPScanner.Infrastructure.Data;

namespace PEPScanner.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SarController : ControllerBase
    {
        private readonly PepScannerDbContext _context;

        public SarController(PepScannerDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var sars = await _context.SuspiciousActivityReports
                .OrderByDescending(s => s.CreatedAtUtc)
                .Select(s => new
                {
                    s.Id,
                    s.ReportNumber,
                    s.Status,
                    s.Priority,
                    s.CreatedAtUtc,
                    s.SubmittedAtUtc,
                    CustomerName = s.Customer != null ? s.Customer.FullName : "Unknown"
                })
                .ToListAsync();
            return Ok(sars);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var sar = await _context.SuspiciousActivityReports
                .Include(s => s.Customer)
                .FirstOrDefaultAsync(s => s.Id == id);
            
            if (sar == null) return NotFound();
            return Ok(sar);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSarRequest request)
        {
            var sar = new SuspiciousActivityReport
            {
                Id = Guid.NewGuid(),
                ReportNumber = $"SAR-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}",
                CustomerId = request.CustomerId,
                SuspiciousActivity = request.SuspiciousActivity,
                TransactionDetails = request.TransactionDetails,
                AmountInvolved = request.AmountInvolved,
                DateOfSuspiciousActivity = request.DateOfSuspiciousActivity,
                ReasonForSuspicion = request.ReasonForSuspicion,
                ActionTaken = request.ActionTaken,
                ReportingOfficer = request.ReportingOfficer,
                BranchCode = request.BranchCode,
                AccountNumber = request.AccountNumber,
                Status = request.Status ?? "Draft",
                Priority = request.Priority ?? "Medium",
                CreatedAtUtc = DateTime.UtcNow,
                CreatedBy = request.CreatedBy ?? "System"
            };
            
            _context.SuspiciousActivityReports.Add(sar);
            await _context.SaveChangesAsync();
            
            return Ok(new { id = sar.Id, reportNumber = sar.ReportNumber, message = "SAR created successfully" });
        }
    }
    
    public class CreateSarRequest
    {
        public Guid CustomerId { get; set; }
        public string SuspiciousActivity { get; set; } = string.Empty;
        public string TransactionDetails { get; set; } = string.Empty;
        public decimal AmountInvolved { get; set; }
        public DateTime DateOfSuspiciousActivity { get; set; }
        public string ReasonForSuspicion { get; set; } = string.Empty;
        public string ActionTaken { get; set; } = string.Empty;
        public string ReportingOfficer { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string? Status { get; set; }
        public string? Priority { get; set; }
        public string? CreatedBy { get; set; }
    }
    }
}