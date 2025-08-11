using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PEPScanner.Infrastructure.Data;

namespace PEPScanner.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StrController : ControllerBase
    {
        private readonly PepScannerDbContext _context;

        public StrController(PepScannerDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var strs = await _context.SuspiciousTransactionReports
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
            return Ok(strs);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var str = await _context.SuspiciousTransactionReports
                .Include(s => s.Customer)
                .FirstOrDefaultAsync(s => s.Id == id);
            
            if (str == null) return NotFound();
            return Ok(str);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateStrRequest request)
        {
            var str = new SuspiciousTransactionReport
            {
                Id = Guid.NewGuid(),
                ReportNumber = $"STR-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}",
                CustomerId = request.CustomerId,
                TransactionType = request.TransactionType,
                TransactionAmount = request.TransactionAmount,
                TransactionDate = request.TransactionDate,
                TransactionMode = request.TransactionMode,
                BeneficiaryDetails = request.BeneficiaryDetails,
                ReasonForSuspicion = request.ReasonForSuspicion,
                LocationOfTransaction = request.LocationOfTransaction,
                ReportingOfficer = request.ReportingOfficer,
                BranchCode = request.BranchCode,
                AccountNumber = request.AccountNumber,
                Status = request.Status ?? "Draft",
                Priority = request.Priority ?? "Medium",
                CreatedAtUtc = DateTime.UtcNow,
                CreatedBy = request.CreatedBy ?? "System"
            };
            
            _context.SuspiciousTransactionReports.Add(str);
            await _context.SaveChangesAsync();
            
            return Ok(new { id = str.Id, reportNumber = str.ReportNumber, message = "STR created successfully" });
        }
    }
    
    public class CreateStrRequest
    {
        public Guid CustomerId { get; set; }
        public string TransactionType { get; set; } = string.Empty;
        public decimal TransactionAmount { get; set; }
        public DateTime TransactionDate { get; set; }
        public string TransactionMode { get; set; } = string.Empty;
        public string BeneficiaryDetails { get; set; } = string.Empty;
        public string ReasonForSuspicion { get; set; } = string.Empty;
        public string LocationOfTransaction { get; set; } = string.Empty;
        public string ReportingOfficer { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string? Status { get; set; }
        public string? Priority { get; set; }
        public string? CreatedBy { get; set; }
    }
    }
}