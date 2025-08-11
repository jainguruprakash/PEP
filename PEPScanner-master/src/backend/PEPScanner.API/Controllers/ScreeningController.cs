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
            var matches = new List<object>();
            var riskScore = 0.0;

            if (!string.IsNullOrEmpty(request.FullName))
            {
                var entries = await _context.WatchlistEntries.Take(1000).ToListAsync();

                foreach (var entry in entries)
                {
                    var score = CalculateScore(request.FullName, entry.PrimaryName ?? "");
                    
                    if (score >= (request.Threshold ?? 70))
                    {
                        matches.Add(new
                        {
                            name = entry.PrimaryName,
                            source = entry.Source,
                            country = entry.Country,
                            matchScore = score
                        });
                        riskScore = Math.Max(riskScore, score);
                    }
                }
            }

            return Ok(new
            {
                customerName = request.FullName,
                riskScore = riskScore,
                matches = matches.Take(10),
                status = riskScore >= 70 ? "Risk" : "Clear"
            });
        }

        [HttpPost("batch-file")]
        public async Task<IActionResult> ScreenBatchFile(IFormFile file)
        {
            if (file == null) return BadRequest("No file");

            var results = new List<object>();
            using var reader = new StreamReader(file.OpenReadStream());
            var content = await reader.ReadToEndAsync();
            var lines = content.Split('\n');

            foreach (var line in lines.Skip(1).Take(100))
            {
                var fields = line.Split(',');
                if (fields.Length >= 2)
                {
                    var name = fields[1]?.Trim();
                    if (!string.IsNullOrEmpty(name))
                    {
                        results.Add(new { name = name, status = "Processed" });
                    }
                }
            }

            return Ok(new { results = results });
        }

        private double CalculateScore(string name1, string name2)
        {
            name1 = name1.ToLower().Trim();
            name2 = name2.ToLower().Trim();

            if (name1 == name2) return 100;
            if (name1.Contains(name2) || name2.Contains(name1)) return 90;

            var words1 = name1.Split(' ');
            var words2 = name2.Split(' ');
            var matches = 0;

            foreach (var w1 in words1)
            {
                foreach (var w2 in words2)
                {
                    if (w1 == w2 && w1.Length > 2)
                    {
                        matches++;
                        break;
                    }
                }
            }

            return matches > 0 ? (double)matches / Math.Max(words1.Length, words2.Length) * 80 : 0;
        }
    }

    public class CustomerScreeningRequest
    {
        public string? CustomerId { get; set; }
        public string? FullName { get; set; }
        public string? DateOfBirth { get; set; }
        public string? Country { get; set; }
        public int? Threshold { get; set; } = 70;
        public List<string>? Sources { get; set; }
        public bool? IncludeFuzzyMatching { get; set; } = true;
        public bool? IncludePhoneticMatching { get; set; } = true;
    }
}