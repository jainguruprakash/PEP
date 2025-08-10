using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PEPScanner.Infrastructure.Data;

namespace PEPScanner.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class McaController : ControllerBase
    {
        private readonly PepScannerDbContext _context;
        private readonly ILogger<McaController> _logger;

        public McaController(PepScannerDbContext context, ILogger<McaController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost("directors/search")]
        public async Task<IActionResult> SearchDirectors([FromBody] McaDirectorRequest request)
        {
            try
            {
                _logger.LogInformation("Searching MCA directors for: {Name}", request.Name);

                // Mock MCA director data - replace with actual MCA API integration
                var directors = new List<McaDirector>();

                if (!string.IsNullOrEmpty(request.Name))
                {
                    // Simulate director search based on name
                    var searchName = request.Name.ToUpper();
                    
                    if (searchName.Contains("AMIT"))
                    {
                        directors.Add(new McaDirector
                        {
                            Din = "00123456",
                            Name = "Amit Shah",
                            CompanyName = "ABC Private Limited",
                            Cin = "U12345MH2020PTC123456",
                            Designation = "Managing Director",
                            AppointmentDate = "2020-01-15",
                            Status = "Active",
                            PanNumber = "ABCDE1234F",
                            Address = "Mumbai, Maharashtra",
                            Nationality = "Indian",
                            RiskLevel = "Medium"
                        });
                    }

                    if (searchName.Contains("JOHN"))
                    {
                        directors.Add(new McaDirector
                        {
                            Din = "00789012",
                            Name = "John Doe",
                            CompanyName = "XYZ Industries Limited",
                            Cin = "L67890DL2018PLC345678",
                            Designation = "Director",
                            AppointmentDate = "2018-06-20",
                            Status = "Active",
                            Address = "Delhi, India",
                            Nationality = "Indian",
                            RiskLevel = "Low"
                        });
                    }
                }

                return Ok(directors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching MCA directors");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("company/{cin}/directors")]
        public async Task<IActionResult> GetCompanyDirectors(string cin)
        {
            try
            {
                _logger.LogInformation("Fetching directors for company: {CIN}", cin);

                // Mock company with directors data
                var company = new McaCompany
                {
                    Cin = cin,
                    CompanyName = "Sample Company Limited",
                    Status = "Active",
                    IncorporationDate = "2020-01-15",
                    AuthorizedCapital = 10000000,
                    PaidUpCapital = 5000000,
                    Directors = new List<McaDirector>
                    {
                        new McaDirector
                        {
                            Din = "00123456",
                            Name = "Amit Shah",
                            CompanyName = "Sample Company Limited",
                            Cin = cin,
                            Designation = "Managing Director",
                            AppointmentDate = "2020-01-15",
                            Status = "Active",
                            PanNumber = "ABCDE1234F",
                            Address = "Mumbai, Maharashtra",
                            Nationality = "Indian",
                            RiskLevel = "Medium"
                        },
                        new McaDirector
                        {
                            Din = "00789012",
                            Name = "Priya Sharma",
                            CompanyName = "Sample Company Limited",
                            Cin = cin,
                            Designation = "Director",
                            AppointmentDate = "2020-03-10",
                            Status = "Active",
                            Address = "Bangalore, Karnataka",
                            Nationality = "Indian",
                            RiskLevel = "Low"
                        }
                    }
                };

                return Ok(company);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching company directors for CIN: {CIN}", cin);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("director/{din}")]
        public async Task<IActionResult> GetDirectorDetails(string din)
        {
            try
            {
                _logger.LogInformation("Fetching director details for DIN: {DIN}", din);

                // Mock director details
                var director = new McaDirector
                {
                    Din = din,
                    Name = "Amit Shah",
                    CompanyName = "ABC Private Limited",
                    Cin = "U12345MH2020PTC123456",
                    Designation = "Managing Director",
                    AppointmentDate = "2020-01-15",
                    Status = "Active",
                    PanNumber = "ABCDE1234F",
                    Address = "Mumbai, Maharashtra",
                    Nationality = "Indian",
                    RiskLevel = "Medium"
                };

                return Ok(director);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching director details for DIN: {DIN}", din);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }
    }

    public class McaDirectorRequest
    {
        public string? Name { get; set; }
        public string? Cin { get; set; }
        public string? CompanyName { get; set; }
        public string? Din { get; set; }
    }

    public class McaDirector
    {
        public string Din { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string Cin { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public string AppointmentDate { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? PanNumber { get; set; }
        public string? Address { get; set; }
        public string Nationality { get; set; } = string.Empty;
        public string RiskLevel { get; set; } = string.Empty;
    }

    public class McaCompany
    {
        public string Cin { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string IncorporationDate { get; set; } = string.Empty;
        public List<McaDirector> Directors { get; set; } = new();
        public decimal AuthorizedCapital { get; set; }
        public decimal PaidUpCapital { get; set; }
    }
}