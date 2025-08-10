using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using PEPScanner.Infrastructure.Data;
using PEPScanner.Domain.Entities;
using System.Text.Json;

// AI Risk Scoring DTOs
namespace PEPScanner.Application.Services
{
    public class AIRiskAssessment
    {
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public double RiskScore { get; set; }
        public double ConfidenceLevel { get; set; }
        public List<RiskFactor> RiskFactors { get; set; } = new();
        public PredictiveRiskInsight PredictiveInsights { get; set; } = new();
        public List<RecommendedAction> RecommendedActions { get; set; } = new();
        public DateTime CalculatedAt { get; set; }
        public string ModelVersion { get; set; } = string.Empty;
        public string RiskTrend { get; set; } = string.Empty;
        public RiskComponentScores ComponentScores { get; set; } = new();
    }

    public class RiskFactor
    {
        public string Category { get; set; } = string.Empty; // 'behavioral', 'transactional', 'geographical', 'network'
        public double Weight { get; set; }
        public string Description { get; set; } = string.Empty;
        public List<Evidence> Evidence { get; set; } = new();
    }

    public class Evidence
    {
        public string Type { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public DateTime? Date { get; set; }
        public string Source { get; set; } = string.Empty;
    }

    public class PredictiveRiskInsight
    {
        public double PredictedRiskIn30Days { get; set; }
        public double PredictedRiskIn90Days { get; set; }
        public string RiskTrend { get; set; } = string.Empty; // 'increasing', 'stable', 'decreasing'
        public List<string> KeyDrivers { get; set; } = new();
        public List<string> EarlyWarningSignals { get; set; } = new();
        public string RecommendedMonitoringFrequency { get; set; } = string.Empty;
    }

    public class RecommendedAction
    {
        public string Priority { get; set; } = string.Empty; // 'Critical', 'High', 'Medium', 'Low'
        public string Action { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string Timeline { get; set; } = string.Empty;
        public string AssignTo { get; set; } = string.Empty;
    }

    public class RiskComponentScores
    {
        public double BaseRisk { get; set; }
        public double MediaRisk { get; set; }
        public double BehavioralRisk { get; set; }
        public double NetworkRisk { get; set; }
    }

    public class RiskTrendAnalysis
    {
        public string Trend { get; set; } = string.Empty;
        public double TrendStrength { get; set; }
        public int DataPoints { get; set; }
        public double AverageRisk { get; set; }
        public double MinRisk { get; set; }
        public double MaxRisk { get; set; }
        public double Volatility { get; set; }
    }

    public class HistoricalRiskData
    {
        public DateTime Date { get; set; }
        public double RiskScore { get; set; }
        public double ConfidenceLevel { get; set; }
    }

    public class TrendAnalysis
    {
        public string Trend { get; set; } = string.Empty;
        public double Strength { get; set; }
        public double PredictedRisk30Days { get; set; }
        public double PredictedRisk90Days { get; set; }
        public List<string> KeyDrivers { get; set; } = new();
        public List<string> EarlyWarnings { get; set; } = new();
        public string RecommendedFrequency { get; set; } = string.Empty;
    }

    public class AdverseMediaResult
    {
        public double RiskScore { get; set; }
        public string Headline { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string Sentiment { get; set; } = string.Empty;
        public DateTime PublishedDate { get; set; }
        public List<string> RiskCategories { get; set; } = new();
        public string Excerpt { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }
}

namespace PEPScanner.Application.Services
{
    public interface IAIRiskScoringService
    {
        Task<AIRiskAssessment> CalculateRiskScoreAsync(Guid customerId);
        Task<AIRiskAssessment> CalculateRiskScoreAsync(Customer customer, List<AdverseMediaResult> mediaResults);
        Task<PredictiveRiskInsight> GetPredictiveInsightsAsync(Guid customerId);
        Task<List<RiskFactor>> AnalyzeRiskFactorsAsync(Customer customer);
        Task<RiskTrendAnalysis> GetRiskTrendAsync(Guid customerId, int daysBack = 90);
        Task<List<RecommendedAction>> GetRecommendedActionsAsync(AIRiskAssessment assessment);
    }

    public class AIRiskScoringService : IAIRiskScoringService
    {
        private readonly PepScannerDbContext _context;
        private readonly ILogger<AIRiskScoringService> _logger;

        public AIRiskScoringService(
            PepScannerDbContext context,
            ILogger<AIRiskScoringService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<AIRiskAssessment> CalculateRiskScoreAsync(Guid customerId)
        {
            try
            {
                var customer = await _context.Customers
                    .Include(c => c.Alerts)
                    .FirstOrDefaultAsync(c => c.Id == customerId);

                if (customer == null)
                    throw new ArgumentException($"Customer {customerId} not found");

                // Get recent media results (simulated for now)
                var mediaResults = await GetRecentMediaResultsAsync(customerId);
                
                return await CalculateRiskScoreAsync(customer, mediaResults);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating AI risk score for customer {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<AIRiskAssessment> CalculateRiskScoreAsync(Customer customer, List<AdverseMediaResult> mediaResults)
        {
            try
            {
                _logger.LogInformation("Calculating AI risk score for customer {CustomerId}", customer.Id);

                var riskFactors = await AnalyzeRiskFactorsAsync(customer);
                var baseRiskScore = CalculateBaseRiskScore(customer, riskFactors);
                var mediaRiskScore = CalculateMediaRiskScore(mediaResults);
                var behavioralRiskScore = await CalculateBehavioralRiskScoreAsync(customer);
                var networkRiskScore = await CalculateNetworkRiskScoreAsync(customer);

                // AI-weighted combination of risk factors
                var finalRiskScore = CombineRiskScores(baseRiskScore, mediaRiskScore, behavioralRiskScore, networkRiskScore);
                var confidenceLevel = CalculateConfidenceLevel(riskFactors, mediaResults);

                var assessment = new AIRiskAssessment
                {
                    CustomerId = customer.Id,
                    CustomerName = customer.FullName,
                    RiskScore = finalRiskScore,
                    ConfidenceLevel = confidenceLevel,
                    RiskFactors = riskFactors,
                    PredictiveInsights = await GetPredictiveInsightsAsync(customer.Id),
                    RecommendedActions = await GetRecommendedActionsAsync(finalRiskScore, riskFactors),
                    CalculatedAt = DateTime.UtcNow,
                    ModelVersion = "v2.1.0",
                    RiskTrend = await CalculateRiskTrendAsync(customer.Id),
                    ComponentScores = new RiskComponentScores
                    {
                        BaseRisk = baseRiskScore,
                        MediaRisk = mediaRiskScore,
                        BehavioralRisk = behavioralRiskScore,
                        NetworkRisk = networkRiskScore
                    }
                };

                // Store assessment for historical tracking
                await StoreRiskAssessmentAsync(assessment);

                _logger.LogInformation("AI risk score calculated for customer {CustomerId}: {RiskScore} (confidence: {Confidence}%)",
                    customer.Id, finalRiskScore, confidenceLevel);

                return assessment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AI risk calculation for customer {CustomerId}", customer.Id);
                throw;
            }
        }

        public async Task<List<RiskFactor>> AnalyzeRiskFactorsAsync(Customer customer)
        {
            var riskFactors = new List<RiskFactor>();

            // Geographic Risk Analysis
            var geographicRisk = AnalyzeGeographicRisk(customer);
            if (geographicRisk.Weight > 0)
                riskFactors.Add(geographicRisk);

            // Transaction Pattern Analysis
            var transactionalRisk = await AnalyzeTransactionalRiskAsync(customer);
            if (transactionalRisk.Weight > 0)
                riskFactors.Add(transactionalRisk);

            // Historical Alert Analysis
            var alertHistoryRisk = await AnalyzeAlertHistoryRiskAsync(customer);
            if (alertHistoryRisk.Weight > 0)
                riskFactors.Add(alertHistoryRisk);

            // Industry/Occupation Risk
            var occupationalRisk = AnalyzeOccupationalRisk(customer);
            if (occupationalRisk.Weight > 0)
                riskFactors.Add(occupationalRisk);

            // Age and Profile Risk
            var profileRisk = AnalyzeProfileRisk(customer);
            if (profileRisk.Weight > 0)
                riskFactors.Add(profileRisk);

            return riskFactors;
        }

        public async Task<PredictiveRiskInsight> GetPredictiveInsightsAsync(Guid customerId)
        {
            // AI-powered predictive analysis
            var historicalData = await GetHistoricalRiskDataAsync(customerId);
            var trendAnalysis = AnalyzeTrends(historicalData);
            
            return new PredictiveRiskInsight
            {
                PredictedRiskIn30Days = trendAnalysis.PredictedRisk30Days,
                PredictedRiskIn90Days = trendAnalysis.PredictedRisk90Days,
                RiskTrend = trendAnalysis.Trend,
                KeyDrivers = trendAnalysis.KeyDrivers,
                EarlyWarningSignals = trendAnalysis.EarlyWarnings,
                RecommendedMonitoringFrequency = trendAnalysis.RecommendedFrequency
            };
        }

        public async Task<RiskTrendAnalysis> GetRiskTrendAsync(Guid customerId, int daysBack = 90)
        {
            var historicalAssessments = await _context.RiskAssessments
                .Where(r => r.CustomerId == customerId && r.CalculatedAt >= DateTime.UtcNow.AddDays(-daysBack))
                .OrderBy(r => r.CalculatedAt)
                .ToListAsync();

            if (!historicalAssessments.Any())
            {
                return new RiskTrendAnalysis
                {
                    Trend = "stable",
                    TrendStrength = 0,
                    DataPoints = 0
                };
            }

            var riskScores = historicalAssessments.Select(a => a.RiskScore).ToList();
            var trend = CalculateTrend(riskScores);

            return new RiskTrendAnalysis
            {
                Trend = trend.Direction,
                TrendStrength = trend.Strength,
                DataPoints = riskScores.Count,
                AverageRisk = riskScores.Average(),
                MinRisk = riskScores.Min(),
                MaxRisk = riskScores.Max(),
                Volatility = CalculateVolatility(riskScores)
            };
        }

        public async Task<List<RecommendedAction>> GetRecommendedActionsAsync(AIRiskAssessment assessment)
        {
            return await GetRecommendedActionsAsync(assessment.RiskScore, assessment.RiskFactors);
        }

        private async Task<List<RecommendedAction>> GetRecommendedActionsAsync(double riskScore, List<RiskFactor> riskFactors)
        {
            var actions = new List<RecommendedAction>();

            // High-risk recommendations
            if (riskScore >= 80)
            {
                actions.Add(new RecommendedAction
                {
                    Priority = "Critical",
                    Action = "Immediate enhanced due diligence required",
                    Reason = "High AI risk score detected",
                    Timeline = "Within 24 hours",
                    AssignTo = "Senior Compliance Officer"
                });

                actions.Add(new RecommendedAction
                {
                    Priority = "High",
                    Action = "Escalate to compliance committee",
                    Reason = "Risk score exceeds institutional threshold",
                    Timeline = "Within 48 hours",
                    AssignTo = "Compliance Manager"
                });
            }

            // Medium-risk recommendations
            if (riskScore >= 60 && riskScore < 80)
            {
                actions.Add(new RecommendedAction
                {
                    Priority = "Medium",
                    Action = "Enhanced monitoring for 90 days",
                    Reason = "Elevated risk indicators detected",
                    Timeline = "Within 1 week",
                    AssignTo = "Compliance Analyst"
                });
            }

            // Factor-specific recommendations
            foreach (var factor in riskFactors.Where(f => f.Weight > 0.3))
            {
                actions.AddRange(GetFactorSpecificActions(factor));
            }

            return actions.OrderByDescending(a => GetPriorityWeight(a.Priority)).ToList();
        }

        // Private helper methods
        private double CalculateBaseRiskScore(Customer customer, List<RiskFactor> riskFactors)
        {
            var baseScore = 30.0; // Starting baseline
            
            // Apply risk factors
            foreach (var factor in riskFactors)
            {
                baseScore += factor.Weight * 20; // Scale factor weights
            }

            return Math.Min(baseScore, 100);
        }

        private double CalculateMediaRiskScore(List<AdverseMediaResult> mediaResults)
        {
            if (!mediaResults.Any()) return 0;

            var avgRiskScore = mediaResults.Average(m => m.RiskScore);
            var recentResults = mediaResults.Where(m => m.PublishedDate >= DateTime.UtcNow.AddDays(-30)).Count();
            var sentimentPenalty = mediaResults.Count(m => m.Sentiment == "Negative") * 5;

            return Math.Min(avgRiskScore + (recentResults * 2) + sentimentPenalty, 100);
        }

        private async Task<double> CalculateBehavioralRiskScoreAsync(Customer customer)
        {
            // Analyze behavioral patterns (simulated)
            var alertCount = await _context.Alerts
                .CountAsync(a => a.CustomerId == customer.Id && a.CreatedAtUtc >= DateTime.UtcNow.AddDays(-90));

            var behavioralScore = alertCount * 10; // 10 points per alert
            
            // Add other behavioral factors
            if (customer.LastMediaScanDate.HasValue)
            {
                var daysSinceLastScan = (DateTime.UtcNow - customer.LastMediaScanDate.Value).TotalDays;
                if (daysSinceLastScan > 30) behavioralScore += 15;
            }

            return Math.Min(behavioralScore, 100);
        }

        private async Task<double> CalculateNetworkRiskScoreAsync(Customer customer)
        {
            // Network analysis (simulated)
            // In real implementation, this would analyze connections to other high-risk entities
            return 0; // Placeholder
        }

        private double CombineRiskScores(double baseRisk, double mediaRisk, double behavioralRisk, double networkRisk)
        {
            // AI-weighted combination
            var weights = new { Base = 0.3, Media = 0.4, Behavioral = 0.2, Network = 0.1 };
            
            var combinedScore = (baseRisk * weights.Base) + 
                               (mediaRisk * weights.Media) + 
                               (behavioralRisk * weights.Behavioral) + 
                               (networkRisk * weights.Network);

            return Math.Round(Math.Min(combinedScore, 100), 2);
        }

        private double CalculateConfidenceLevel(List<RiskFactor> riskFactors, List<AdverseMediaResult> mediaResults)
        {
            var dataPoints = riskFactors.Count + mediaResults.Count;
            var baseConfidence = Math.Min(dataPoints * 10, 80); // Max 80% base confidence
            
            // Boost confidence if we have recent data
            if (mediaResults.Any(m => m.PublishedDate >= DateTime.UtcNow.AddDays(-7)))
                baseConfidence += 15;

            return Math.Min(baseConfidence, 95); // Max 95% confidence
        }

        private RiskFactor AnalyzeGeographicRisk(Customer customer)
        {
            var highRiskCountries = new[] { "AF", "IR", "KP", "SY" }; // Example high-risk countries
            var mediumRiskCountries = new[] { "PK", "BD", "MM" }; // Example medium-risk countries

            if (highRiskCountries.Contains(customer.Country))
            {
                return new RiskFactor
                {
                    Category = "geographical",
                    Weight = 0.8,
                    Description = $"Customer from high-risk jurisdiction: {customer.Country}",
                    Evidence = new List<Evidence> { new Evidence { Type = "Country", Value = customer.Country } }
                };
            }

            if (mediumRiskCountries.Contains(customer.Country))
            {
                return new RiskFactor
                {
                    Category = "geographical",
                    Weight = 0.4,
                    Description = $"Customer from medium-risk jurisdiction: {customer.Country}",
                    Evidence = new List<Evidence> { new Evidence { Type = "Country", Value = customer.Country } }
                };
            }

            return new RiskFactor { Weight = 0 };
        }

        private async Task<RiskFactor> AnalyzeTransactionalRiskAsync(Customer customer)
        {
            // Placeholder for transaction analysis
            // In real implementation, analyze transaction patterns
            return new RiskFactor { Weight = 0 };
        }

        private async Task<RiskFactor> AnalyzeAlertHistoryRiskAsync(Customer customer)
        {
            var alertCount = await _context.Alerts
                .CountAsync(a => a.CustomerId == customer.Id);

            if (alertCount >= 5)
            {
                return new RiskFactor
                {
                    Category = "behavioral",
                    Weight = 0.6,
                    Description = $"High alert history: {alertCount} alerts",
                    Evidence = new List<Evidence> { new Evidence { Type = "AlertCount", Value = alertCount.ToString() } }
                };
            }

            return new RiskFactor { Weight = 0 };
        }

        private RiskFactor AnalyzeOccupationalRisk(Customer customer)
        {
            var highRiskOccupations = new[] { "politician", "government official", "arms dealer" };
            
            if (!string.IsNullOrEmpty(customer.Occupation) && 
                highRiskOccupations.Any(o => customer.Occupation.ToLower().Contains(o)))
            {
                return new RiskFactor
                {
                    Category = "occupational",
                    Weight = 0.7,
                    Description = $"High-risk occupation: {customer.Occupation}",
                    Evidence = new List<Evidence> { new Evidence { Type = "Occupation", Value = customer.Occupation } }
                };
            }

            return new RiskFactor { Weight = 0 };
        }

        private RiskFactor AnalyzeProfileRisk(Customer customer)
        {
            var riskWeight = 0.0;
            var reasons = new List<string>();

            // Age-based risk
            if (customer.DateOfBirth.HasValue)
            {
                var age = DateTime.Now.Year - customer.DateOfBirth.Value.Year;
                if (age > 80) { riskWeight += 0.1; reasons.Add("Advanced age"); }
                if (age < 18) { riskWeight += 0.3; reasons.Add("Minor"); }
            }

            if (riskWeight > 0)
            {
                return new RiskFactor
                {
                    Category = "demographic",
                    Weight = riskWeight,
                    Description = string.Join(", ", reasons),
                    Evidence = reasons.Select(r => new Evidence { Type = "Demographic", Value = r }).ToList()
                };
            }

            return new RiskFactor { Weight = 0 };
        }

        private async Task<List<AdverseMediaResult>> GetRecentMediaResultsAsync(Guid customerId)
        {
            // Placeholder - in real implementation, fetch from media scan results
            return new List<AdverseMediaResult>();
        }

        private async Task<List<HistoricalRiskData>> GetHistoricalRiskDataAsync(Guid customerId)
        {
            return await _context.RiskAssessments
                .Where(r => r.CustomerId == customerId)
                .OrderBy(r => r.CalculatedAt)
                .Select(r => new HistoricalRiskData
                {
                    Date = r.CalculatedAt,
                    RiskScore = r.RiskScore,
                    ConfidenceLevel = r.ConfidenceLevel
                })
                .ToListAsync();
        }

        private TrendAnalysis AnalyzeTrends(List<HistoricalRiskData> data)
        {
            if (data.Count < 2)
            {
                return new TrendAnalysis
                {
                    Trend = "stable",
                    PredictedRisk30Days = data.LastOrDefault()?.RiskScore ?? 50,
                    PredictedRisk90Days = data.LastOrDefault()?.RiskScore ?? 50
                };
            }

            var riskScores = data.Select(d => d.RiskScore).ToList();
            var trend = CalculateTrend(riskScores);

            return new TrendAnalysis
            {
                Trend = trend.Direction,
                PredictedRisk30Days = PredictFutureRisk(riskScores, 30),
                PredictedRisk90Days = PredictFutureRisk(riskScores, 90),
                KeyDrivers = new List<string> { "Historical pattern analysis" },
                EarlyWarnings = GenerateEarlyWarnings(trend),
                RecommendedFrequency = GetRecommendedFrequency(trend.Direction)
            };
        }

        private (string Direction, double Strength) CalculateTrend(List<double> scores)
        {
            if (scores.Count < 2) return ("stable", 0);

            var recent = scores.TakeLast(Math.Min(5, scores.Count)).ToList();
            var older = scores.Take(Math.Min(5, scores.Count)).ToList();

            var recentAvg = recent.Average();
            var olderAvg = older.Average();
            var difference = recentAvg - olderAvg;

            if (Math.Abs(difference) < 5) return ("stable", Math.Abs(difference));
            if (difference > 0) return ("increasing", difference);
            return ("decreasing", Math.Abs(difference));
        }

        private double PredictFutureRisk(List<double> historicalScores, int daysAhead)
        {
            // Simple linear prediction - in real implementation, use ML models
            if (historicalScores.Count < 2) return historicalScores.LastOrDefault();

            var trend = CalculateTrend(historicalScores);
            var currentRisk = historicalScores.Last();
            var trendAdjustment = (trend.Strength / 30) * daysAhead; // Adjust based on days

            if (trend.Direction == "increasing")
                return Math.Min(currentRisk + trendAdjustment, 100);
            if (trend.Direction == "decreasing")
                return Math.Max(currentRisk - trendAdjustment, 0);
            
            return currentRisk;
        }

        private List<string> GenerateEarlyWarnings(TrendAnalysis trend)
        {
            var warnings = new List<string>();
            
            if (trend.Direction == "increasing" && trend.Strength > 10)
                warnings.Add("Rapid risk increase detected");
            
            return warnings;
        }

        private string GetRecommendedFrequency(string trendDirection)
        {
            return trendDirection switch
            {
                "increasing" => "Weekly",
                "decreasing" => "Monthly",
                _ => "Bi-weekly"
            };
        }

        private double CalculateVolatility(List<double> scores)
        {
            if (scores.Count < 2) return 0;
            
            var mean = scores.Average();
            var variance = scores.Select(s => Math.Pow(s - mean, 2)).Average();
            return Math.Sqrt(variance);
        }

        private List<RecommendedAction> GetFactorSpecificActions(RiskFactor factor)
        {
            return factor.Category switch
            {
                "geographical" => new List<RecommendedAction>
                {
                    new RecommendedAction
                    {
                        Priority = "Medium",
                        Action = "Enhanced geographic due diligence",
                        Reason = factor.Description,
                        Timeline = "Within 1 week"
                    }
                },
                "behavioral" => new List<RecommendedAction>
                {
                    new RecommendedAction
                    {
                        Priority = "High",
                        Action = "Behavioral pattern analysis",
                        Reason = factor.Description,
                        Timeline = "Within 3 days"
                    }
                },
                _ => new List<RecommendedAction>()
            };
        }

        private int GetPriorityWeight(string priority)
        {
            return priority switch
            {
                "Critical" => 4,
                "High" => 3,
                "Medium" => 2,
                "Low" => 1,
                _ => 0
            };
        }

        private async Task StoreRiskAssessmentAsync(AIRiskAssessment assessment)
        {
            var entity = new RiskAssessmentEntity
            {
                Id = Guid.NewGuid(),
                CustomerId = assessment.CustomerId,
                RiskScore = assessment.RiskScore,
                ConfidenceLevel = assessment.ConfidenceLevel,
                ModelVersion = assessment.ModelVersion,
                CalculatedAt = assessment.CalculatedAt,
                RiskFactorsJson = JsonSerializer.Serialize(assessment.RiskFactors),
                ComponentScoresJson = JsonSerializer.Serialize(assessment.ComponentScores)
            };

            _context.RiskAssessments.Add(entity);
            await _context.SaveChangesAsync();
        }

        private async Task<string> CalculateRiskTrendAsync(Guid customerId)
        {
            var trendAnalysis = await GetRiskTrendAsync(customerId);
            return trendAnalysis.Trend;
        }
    }
}
