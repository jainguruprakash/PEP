using Microsoft.AspNetCore.Mvc;

namespace PEPScanner.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CibilController : ControllerBase
    {
        private readonly ILogger<CibilController> _logger;

        public CibilController(ILogger<CibilController> logger)
        {
            _logger = logger;
        }

        [HttpPost("report")]
        public async Task<IActionResult> GetCreditReport([FromBody] CibilRequest request)
        {
            try
            {
                _logger.LogInformation("Fetching CIBIL report for PAN: {PAN}", request.PanNumber);

                // Mock CIBIL response - replace with actual CIBIL API integration
                var response = new CibilResponse
                {
                    PanNumber = request.PanNumber,
                    CreditScore = 742,
                    ReportDate = DateTime.UtcNow,
                    Accounts = new List<CibilAccount>
                    {
                        new CibilAccount
                        {
                            AccountNumber = "****1234",
                            AccountType = "Credit Card",
                            BankName = "HDFC Bank",
                            CurrentBalance = 25000,
                            CreditLimit = 100000,
                            PaymentStatus = "Current",
                            Dpd = 0,
                            OpenDate = DateTime.UtcNow.AddYears(-2),
                            LastPaymentDate = DateTime.UtcNow.AddDays(-15)
                        },
                        new CibilAccount
                        {
                            AccountNumber = "****5678",
                            AccountType = "Personal Loan",
                            BankName = "ICICI Bank",
                            CurrentBalance = 150000,
                            CreditLimit = 300000,
                            PaymentStatus = "Current",
                            Dpd = 0,
                            OpenDate = DateTime.UtcNow.AddYears(-1),
                            LastPaymentDate = DateTime.UtcNow.AddDays(-10)
                        }
                    },
                    Enquiries = new List<CibilEnquiry>
                    {
                        new CibilEnquiry
                        {
                            Date = DateTime.UtcNow.AddMonths(-2),
                            BankName = "SBI",
                            EnquiryType = "Credit Card",
                            Amount = 50000
                        }
                    },
                    Summary = new CibilSummary
                    {
                        TotalAccounts = 2,
                        ActiveAccounts = 2,
                        ClosedAccounts = 0,
                        TotalCreditLimit = 400000,
                        TotalCurrentBalance = 175000,
                        UtilizationRatio = 44
                    },
                    RiskAnalysis = new RiskAnalysis
                    {
                        RiskLevel = "MEDIUM",
                        RiskScore = 65,
                        RiskFactors = new List<string> { "High utilization ratio", "Recent credit enquiry" },
                        Recommendations = new List<string> 
                        { 
                            "Reduce credit utilization below 30%",
                            "Maintain consistent payment history",
                            "Avoid multiple credit enquiries"
                        }
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching CIBIL report for PAN: {PAN}", request.PanNumber);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("validate-pan/{panNumber}")]
        public async Task<IActionResult> ValidatePAN(string panNumber)
        {
            try
            {
                _logger.LogInformation("Validating PAN: {PAN}", panNumber);

                // Mock PAN validation - replace with actual PAN validation API
                var isValid = System.Text.RegularExpressions.Regex.IsMatch(panNumber, @"^[A-Z]{5}[0-9]{4}[A-Z]{1}$");
                
                var response = new
                {
                    valid = isValid,
                    name = isValid ? "AMIT SHAH" : null // Mock name
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating PAN: {PAN}", panNumber);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }
    }

    public class CibilRequest
    {
        public string PanNumber { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? DateOfBirth { get; set; }
        public string? MobileNumber { get; set; }
    }

    public class CibilResponse
    {
        public string PanNumber { get; set; } = string.Empty;
        public int CreditScore { get; set; }
        public DateTime ReportDate { get; set; }
        public List<CibilAccount> Accounts { get; set; } = new();
        public List<CibilEnquiry> Enquiries { get; set; } = new();
        public CibilSummary Summary { get; set; } = new();
        public RiskAnalysis RiskAnalysis { get; set; } = new();
    }

    public class CibilAccount
    {
        public string AccountNumber { get; set; } = string.Empty;
        public string AccountType { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public decimal CurrentBalance { get; set; }
        public decimal CreditLimit { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public int Dpd { get; set; }
        public DateTime OpenDate { get; set; }
        public DateTime LastPaymentDate { get; set; }
    }

    public class CibilEnquiry
    {
        public DateTime Date { get; set; }
        public string BankName { get; set; } = string.Empty;
        public string EnquiryType { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }

    public class CibilSummary
    {
        public int TotalAccounts { get; set; }
        public int ActiveAccounts { get; set; }
        public int ClosedAccounts { get; set; }
        public decimal TotalCreditLimit { get; set; }
        public decimal TotalCurrentBalance { get; set; }
        public int UtilizationRatio { get; set; }
    }

    public class RiskAnalysis
    {
        public string RiskLevel { get; set; } = string.Empty;
        public int RiskScore { get; set; }
        public List<string> RiskFactors { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
    }
}