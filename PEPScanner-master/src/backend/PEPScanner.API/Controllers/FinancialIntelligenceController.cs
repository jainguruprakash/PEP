using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PEPScanner.Application.Services;

namespace PEPScanner.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FinancialIntelligenceController : ControllerBase
    {
        private readonly IFinancialIntelligenceService _financialService;
        private readonly IRealTimeNotificationService _notificationService;
        private readonly ILogger<FinancialIntelligenceController> _logger;

        public FinancialIntelligenceController(
            IFinancialIntelligenceService financialService,
            IRealTimeNotificationService notificationService,
            ILogger<FinancialIntelligenceController> logger)
        {
            _financialService = financialService;
            _notificationService = notificationService;
            _logger = logger;
        }

        [HttpPost("cibil-report")]
        public async Task<IActionResult> GetCibilReport([FromBody] CibilReportRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Pan) || string.IsNullOrEmpty(request.Name))
                {
                    return BadRequest(new { error = "PAN and Name are required" });
                }

                _logger.LogInformation("Fetching CIBIL report for PAN: {PAN}", request.Pan);

                var report = await _financialService.GetCibilReportAsync(request.Pan, request.Name);

                // Send notification for low credit scores
                if (report.CibilScore < 650)
                {
                    await _notificationService.SendSystemNotificationAsync(
                        $"Low CIBIL score detected for {request.Name}: {report.CibilScore}",
                        "financial_alert");
                }

                return Ok(new
                {
                    success = true,
                    message = "CIBIL report fetched successfully",
                    data = report
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching CIBIL report for PAN: {PAN}", request.Pan);
                return StatusCode(500, new { error = "Internal server error fetching CIBIL report" });
            }
        }

        [HttpPost("crisil-rating")]
        public async Task<IActionResult> GetCrisilRating([FromBody] CrisilRatingRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.CompanyName))
                {
                    return BadRequest(new { error = "Company name is required" });
                }

                _logger.LogInformation("Fetching CRISIL rating for company: {CompanyName}", request.CompanyName);

                var rating = await _financialService.GetCrisilRatingAsync(request.CompanyName, request.Cin ?? "");

                return Ok(new
                {
                    success = true,
                    message = "CRISIL rating fetched successfully",
                    data = rating
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching CRISIL rating for company: {CompanyName}", request.CompanyName);
                return StatusCode(500, new { error = "Internal server error fetching CRISIL rating" });
            }
        }

        [HttpPost("mca-profile")]
        public async Task<IActionResult> GetMcaProfile([FromBody] McaProfileRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Cin))
                {
                    return BadRequest(new { error = "CIN is required" });
                }

                _logger.LogInformation("Fetching MCA profile for CIN: {CIN}", request.Cin);

                var profile = await _financialService.GetMcaCompanyProfileAsync(request.Cin);

                return Ok(new
                {
                    success = true,
                    message = "MCA profile fetched successfully",
                    data = profile
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching MCA profile for CIN: {CIN}", request.Cin);
                return StatusCode(500, new { error = "Internal server error fetching MCA profile" });
            }
        }

        [HttpPost("gst-profile")]
        public async Task<IActionResult> GetGstProfile([FromBody] GstProfileRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Gstin))
                {
                    return BadRequest(new { error = "GSTIN is required" });
                }

                _logger.LogInformation("Fetching GST profile for GSTIN: {GSTIN}", request.Gstin);

                var profile = await _financialService.GetGstProfileAsync(request.Gstin);

                return Ok(new
                {
                    success = true,
                    message = "GST profile fetched successfully",
                    data = profile
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching GST profile for GSTIN: {GSTIN}", request.Gstin);
                return StatusCode(500, new { error = "Internal server error fetching GST profile" });
            }
        }

        [HttpPost("banking-profile")]
        public async Task<IActionResult> GetBankingProfile([FromBody] BankingProfileRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.AccountNumber) || string.IsNullOrEmpty(request.Ifsc))
                {
                    return BadRequest(new { error = "Account number and IFSC are required" });
                }

                _logger.LogInformation("Fetching banking profile for account: {AccountNumber}", 
                    request.AccountNumber.Substring(0, 4) + "****");

                var profile = await _financialService.GetBankingProfileAsync(request.AccountNumber, request.Ifsc);

                return Ok(new
                {
                    success = true,
                    message = "Banking profile fetched successfully",
                    data = profile
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching banking profile");
                return StatusCode(500, new { error = "Internal server error fetching banking profile" });
            }
        }

        [HttpGet("comprehensive-risk/{customerId}")]
        public async Task<IActionResult> GetComprehensiveFinancialRisk(Guid customerId)
        {
            try
            {
                _logger.LogInformation("Generating comprehensive financial risk for customer: {CustomerId}", customerId);

                var assessment = await _financialService.GetComprehensiveFinancialRiskAsync(customerId);

                return Ok(new
                {
                    success = true,
                    message = "Comprehensive financial risk assessment completed",
                    data = assessment
                });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating financial risk for customer: {CustomerId}", customerId);
                return StatusCode(500, new { error = "Internal server error generating financial risk" });
            }
        }

        [HttpGet("financial-anomalies/{customerId}")]
        public async Task<IActionResult> ScanFinancialAnomalies(Guid customerId)
        {
            try
            {
                _logger.LogInformation("Scanning financial anomalies for customer: {CustomerId}", customerId);

                var alerts = await _financialService.ScanFinancialAnomaliesAsync(customerId);

                // Send notifications for critical anomalies
                var criticalAlerts = alerts.Where(a => a.Severity == "Critical").ToList();
                foreach (var alert in criticalAlerts)
                {
                    await _notificationService.SendSystemNotificationAsync(
                        $"Critical financial anomaly detected: {alert.Title}",
                        "financial_anomaly");
                }

                return Ok(new
                {
                    success = true,
                    message = $"Financial anomaly scan completed. Found {alerts.Count} alerts",
                    data = new
                    {
                        totalAlerts = alerts.Count,
                        criticalAlerts = criticalAlerts.Count,
                        alerts = alerts
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scanning financial anomalies for customer: {CustomerId}", customerId);
                return StatusCode(500, new { error = "Internal server error scanning anomalies" });
            }
        }

        [HttpGet("financial-trends/{customerId}")]
        public async Task<IActionResult> GetFinancialTrends(Guid customerId, [FromQuery] int months = 12)
        {
            try
            {
                _logger.LogInformation("Getting financial trends for customer: {CustomerId}", customerId);

                var trends = await _financialService.GetFinancialTrendsAsync(customerId, months);

                return Ok(new
                {
                    success = true,
                    message = "Financial trends analysis completed",
                    data = trends
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting financial trends for customer: {CustomerId}", customerId);
                return StatusCode(500, new { error = "Internal server error getting trends" });
            }
        }

        [HttpPost("batch-financial-assessment")]
        public async Task<IActionResult> BatchFinancialAssessment([FromBody] BatchFinancialAssessmentRequest request)
        {
            try
            {
                if (request.CustomerIds == null || !request.CustomerIds.Any())
                {
                    return BadRequest(new { error = "Customer IDs are required" });
                }

                _logger.LogInformation("Starting batch financial assessment for {Count} customers", request.CustomerIds.Count);

                var assessments = new List<FinancialRiskAssessment>();
                var successCount = 0;
                var failedCount = 0;

                foreach (var customerId in request.CustomerIds)
                {
                    try
                    {
                        var assessment = await _financialService.GetComprehensiveFinancialRiskAsync(customerId);
                        assessments.Add(assessment);
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to assess customer: {CustomerId}", customerId);
                        failedCount++;
                    }
                }

                return Ok(new
                {
                    success = true,
                    message = $"Batch assessment completed: {successCount} successful, {failedCount} failed",
                    data = new
                    {
                        assessments,
                        summary = new
                        {
                            totalRequested = request.CustomerIds.Count,
                            successCount,
                            failedCount,
                            highRiskCount = assessments.Count(a => a.OverallRiskScore >= 75),
                            averageRiskScore = assessments.Any() ? assessments.Average(a => a.OverallRiskScore) : 0
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during batch financial assessment");
                return StatusCode(500, new { error = "Internal server error during batch assessment" });
            }
        }

        [HttpGet("financial-dashboard/{customerId}")]
        public async Task<IActionResult> GetFinancialDashboard(Guid customerId)
        {
            try
            {
                _logger.LogInformation("Loading financial dashboard for customer: {CustomerId}", customerId);

                // Get all financial data in parallel
                var riskAssessmentTask = _financialService.GetComprehensiveFinancialRiskAsync(customerId);
                var anomaliesTask = _financialService.ScanFinancialAnomaliesAsync(customerId);
                var trendsTask = _financialService.GetFinancialTrendsAsync(customerId);

                await Task.WhenAll(riskAssessmentTask, anomaliesTask, trendsTask);

                var dashboard = new
                {
                    riskAssessment = riskAssessmentTask.Result,
                    anomalies = anomaliesTask.Result,
                    trends = trendsTask.Result,
                    summary = new
                    {
                        overallRiskScore = riskAssessmentTask.Result.OverallRiskScore,
                        riskLevel = GetRiskLevel(riskAssessmentTask.Result.OverallRiskScore),
                        totalAnomalies = anomaliesTask.Result.Count,
                        criticalAnomalies = anomaliesTask.Result.Count(a => a.Severity == "Critical"),
                        complianceScore = riskAssessmentTask.Result.ComplianceScore,
                        lastAssessment = riskAssessmentTask.Result.AssessmentDate
                    }
                };

                return Ok(new
                {
                    success = true,
                    message = "Financial dashboard loaded successfully",
                    data = dashboard
                });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading financial dashboard for customer: {CustomerId}", customerId);
                return StatusCode(500, new { error = "Internal server error loading dashboard" });
            }
        }

        private string GetRiskLevel(double riskScore)
        {
            return riskScore switch
            {
                >= 90 => "Critical",
                >= 75 => "High",
                >= 50 => "Medium",
                >= 25 => "Low",
                _ => "Minimal"
            };
        }
    }

    // Request DTOs
    public class CibilReportRequest
    {
        public string Pan { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    public class CrisilRatingRequest
    {
        public string CompanyName { get; set; } = string.Empty;
        public string? Cin { get; set; }
    }

    public class McaProfileRequest
    {
        public string Cin { get; set; } = string.Empty;
    }

    public class GstProfileRequest
    {
        public string Gstin { get; set; } = string.Empty;
    }

    public class BankingProfileRequest
    {
        public string AccountNumber { get; set; } = string.Empty;
        public string Ifsc { get; set; } = string.Empty;
    }

    public class BatchFinancialAssessmentRequest
    {
        public List<Guid> CustomerIds { get; set; } = new();
    }
}
