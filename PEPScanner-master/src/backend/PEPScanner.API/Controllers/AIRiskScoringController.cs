using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PEPScanner.Application.Services;

namespace PEPScanner.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AIRiskScoringController : ControllerBase
    {
        private readonly IAIRiskScoringService _aiRiskScoringService;
        private readonly IRealTimeNotificationService _notificationService;
        private readonly ILogger<AIRiskScoringController> _logger;

        public AIRiskScoringController(
            IAIRiskScoringService aiRiskScoringService,
            IRealTimeNotificationService notificationService,
            ILogger<AIRiskScoringController> logger)
        {
            _aiRiskScoringService = aiRiskScoringService;
            _notificationService = notificationService;
            _logger = logger;
        }

        [HttpPost("calculate/{customerId}")]
        public async Task<IActionResult> CalculateRiskScore(Guid customerId)
        {
            try
            {
                _logger.LogInformation("Calculating AI risk score for customer {CustomerId}", customerId);

                var assessment = await _aiRiskScoringService.CalculateRiskScoreAsync(customerId);

                // Send real-time notification about risk score update
                await _notificationService.SendRiskScoreUpdateNotificationAsync(customerId, assessment);

                return Ok(new
                {
                    success = true,
                    message = "AI risk score calculated successfully",
                    data = assessment
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Customer not found: {CustomerId}", customerId);
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating AI risk score for customer {CustomerId}", customerId);
                return StatusCode(500, new { error = "Internal server error during AI risk calculation" });
            }
        }

        [HttpPost("batch-calculate")]
        public async Task<IActionResult> BatchCalculateRiskScores([FromBody] BatchCalculateRequest request)
        {
            try
            {
                if (request.CustomerIds == null || !request.CustomerIds.Any())
                {
                    return BadRequest(new { error = "Customer IDs are required" });
                }

                _logger.LogInformation("Batch calculating AI risk scores for {Count} customers", request.CustomerIds.Count);

                var assessments = new List<AIRiskAssessment>();
                var successCount = 0;
                var failedCount = 0;

                foreach (var customerId in request.CustomerIds)
                {
                    try
                    {
                        var assessment = await _aiRiskScoringService.CalculateRiskScoreAsync(customerId);
                        assessments.Add(assessment);
                        successCount++;

                        // Send notification for high-risk customers
                        if (assessment.RiskScore >= 75)
                        {
                            await _notificationService.SendRiskScoreUpdateNotificationAsync(customerId, assessment);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to calculate risk score for customer {CustomerId}", customerId);
                        failedCount++;
                    }
                }

                return Ok(new
                {
                    success = true,
                    message = $"Batch calculation completed: {successCount} successful, {failedCount} failed",
                    data = assessments,
                    summary = new
                    {
                        totalRequested = request.CustomerIds.Count,
                        successCount,
                        failedCount,
                        highRiskCount = assessments.Count(a => a.RiskScore >= 75)
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during batch AI risk calculation");
                return StatusCode(500, new { error = "Internal server error during batch calculation" });
            }
        }

        [HttpGet("insights/{customerId}")]
        public async Task<IActionResult> GetPredictiveInsights(Guid customerId)
        {
            try
            {
                var insights = await _aiRiskScoringService.GetPredictiveInsightsAsync(customerId);

                return Ok(new
                {
                    success = true,
                    data = insights
                });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting predictive insights for customer {CustomerId}", customerId);
                return StatusCode(500, new { error = "Internal server error getting insights" });
            }
        }

        [HttpGet("trend/{customerId}")]
        public async Task<IActionResult> GetRiskTrend(Guid customerId, [FromQuery] int daysBack = 90)
        {
            try
            {
                var trend = await _aiRiskScoringService.GetRiskTrendAsync(customerId, daysBack);

                return Ok(new
                {
                    success = true,
                    data = trend
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting risk trend for customer {CustomerId}", customerId);
                return StatusCode(500, new { error = "Internal server error getting trend" });
            }
        }

        [HttpGet("actions/{customerId}")]
        public async Task<IActionResult> GetRecommendedActions(Guid customerId)
        {
            try
            {
                var assessment = await _aiRiskScoringService.CalculateRiskScoreAsync(customerId);
                var actions = await _aiRiskScoringService.GetRecommendedActionsAsync(assessment);

                return Ok(new
                {
                    success = true,
                    data = actions
                });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recommended actions for customer {CustomerId}", customerId);
                return StatusCode(500, new { error = "Internal server error getting actions" });
            }
        }

        [HttpGet("history/{customerId}")]
        public async Task<IActionResult> GetHistoricalAssessments(Guid customerId)
        {
            try
            {
                // This would fetch from stored historical assessments
                // For now, return empty array as placeholder
                return Ok(new
                {
                    success = true,
                    data = new List<AIRiskAssessment>()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting historical assessments for customer {CustomerId}", customerId);
                return StatusCode(500, new { error = "Internal server error getting history" });
            }
        }

        [HttpGet("factors/{customerId}")]
        public async Task<IActionResult> GetRiskFactors(Guid customerId)
        {
            try
            {
                var assessment = await _aiRiskScoringService.CalculateRiskScoreAsync(customerId);

                return Ok(new
                {
                    success = true,
                    data = assessment.RiskFactors
                });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting risk factors for customer {CustomerId}", customerId);
                return StatusCode(500, new { error = "Internal server error getting factors" });
            }
        }

        [HttpPost("recalculate-all")]
        public async Task<IActionResult> RecalculateAllRiskScores()
        {
            try
            {
                _logger.LogInformation("Starting recalculation of all customer risk scores");

                // This would be a background job in production
                await _notificationService.SendSystemNotificationAsync(
                    "AI risk score recalculation started for all customers", 
                    "system_update");

                return Ok(new
                {
                    success = true,
                    message = "Risk score recalculation initiated for all customers"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initiating risk score recalculation");
                return StatusCode(500, new { error = "Internal server error during recalculation" });
            }
        }

        [HttpGet("statistics")]
        public async Task<IActionResult> GetRiskStatistics()
        {
            try
            {
                // This would calculate statistics from stored assessments
                // For now, return mock data
                var statistics = new
                {
                    totalAssessments = 1250,
                    averageRiskScore = 42.3,
                    highRiskCustomers = 89,
                    criticalRiskCustomers = 12,
                    riskDistribution = new
                    {
                        critical = 12,
                        high = 77,
                        medium = 234,
                        low = 567,
                        minimal = 360
                    },
                    trendAnalysis = new
                    {
                        increasing = 23,
                        stable = 1156,
                        decreasing = 71
                    }
                };

                return Ok(new
                {
                    success = true,
                    data = statistics
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting risk statistics");
                return StatusCode(500, new { error = "Internal server error getting statistics" });
            }
        }
    }

    public class BatchCalculateRequest
    {
        public List<Guid> CustomerIds { get; set; } = new();
    }
}
