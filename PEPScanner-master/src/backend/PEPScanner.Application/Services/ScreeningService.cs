using Microsoft.EntityFrameworkCore;
using PEPScanner.Infrastructure.Data;
using PEPScanner.Domain.Entities;

namespace PEPScanner.Application.Services
{
    public class ScreeningService : IScreeningService
    {
        private readonly PepScannerDbContext _context;
        private readonly INameMatchingService _nameMatchingService;
        private readonly ILogger<ScreeningService> _logger;

        public ScreeningService(
            PepScannerDbContext context,
            INameMatchingService nameMatchingService,
            ILogger<ScreeningService> logger)
        {
            _context = context;
            _nameMatchingService = nameMatchingService;
            _logger = logger;
        }

        public async Task<ScreeningResult> ScreenCustomerAsync(Customer customer, string context = "Onboarding")
        {
            var startTime = DateTime.UtcNow;
            var result = new ScreeningResult
            {
                CustomerId = customer.Id,
                CustomerName = customer.FullName,
                ScreeningContext = context,
                ScreeningDate = startTime
            };

            try
            {
                // Perform name matching
                var matches = await _nameMatchingService.MatchNameAsync(customer.FullName, customer);
                result.HasMatches = matches.Any();

                if (result.HasMatches)
                {
                    // Create alerts for each match
                    foreach (var match in matches)
                    {
                        var alert = new Alert
                        {
                            Id = Guid.NewGuid(),
                            CustomerId = customer.Id,
                            WatchlistEntryId = match.WatchlistEntryId,
                            Context = context,
                            AlertType = match.ListType,
                            SimilarityScore = match.SimilarityScore,
                            MatchAlgorithm = match.MatchAlgorithm,
                            Status = "Open",
                            Priority = DeterminePriority(match.SimilarityScore, match.RiskLevel),
                            RiskLevel = match.RiskLevel,
                            MatchedFields = match.MatchedFields,
                            SourceList = match.SourceList,
                            SourceCategory = match.ListType,
                            CreatedAtUtc = DateTime.UtcNow,
                            CreatedBy = "System"
                        };

                        // Determine compliance actions based on match type and risk level
                        DetermineComplianceActions(alert, match);

                        result.Alerts.Add(alert);
                        _context.Alerts.Add(alert);
                    }

                    // Calculate risk score and level
                    result.RiskScore = CalculateRiskScore(matches);
                    result.RiskLevel = DetermineRiskLevel(result.RiskScore);
                    result.RequiresEdd = result.Alerts.Any(a => a.RequiresEdd);
                    result.RequiresStr = result.Alerts.Any(a => a.RequiresStr);
                    result.RequiresSar = result.Alerts.Any(a => a.RequiresSar);

                    // Update customer risk information
                    customer.RiskScore = result.RiskScore;
                    customer.RiskLevel = result.RiskLevel;
                    customer.RequiresEdd = result.RequiresEdd;
                    customer.LastScreeningDate = startTime;
                    customer.NextScreeningDate = CalculateNextScreeningDate(customer.ScreeningFrequency);
                    customer.UpdatedAtUtc = DateTime.UtcNow;
                    customer.UpdatedBy = "System";

                    _context.Customers.Update(customer);
                }

                await _context.SaveChangesAsync();

                result.ProcessingTime = DateTime.UtcNow - startTime;
                _logger.LogInformation("Screening completed for customer {CustomerName} in {ProcessingTime}ms", 
                    customer.FullName, result.ProcessingTime.TotalMilliseconds);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during screening for customer {CustomerName}", customer.FullName);
                throw;
            }
        }

        public async Task<List<ScreeningResult>> BatchScreenCustomersAsync(List<Customer> customers, Guid jobId)
        {
            var results = new List<ScreeningResult>();
            var job = await _context.ScreeningJobs.FindAsync(jobId);

            if (job != null)
            {
                job.Status = "Running";
                job.StartedAtUtc = DateTime.UtcNow;
                job.TotalRecords = customers.Count;
                _context.ScreeningJobs.Update(job);
            }

            try
            {
                // Process customers in parallel batches
                const int batchSize = 50;
                for (int i = 0; i < customers.Count; i += batchSize)
                {
                    var batch = customers.Skip(i).Take(batchSize).ToList();
                    var batchTasks = batch.Select(customer => ScreenCustomerAsync(customer, "Batch"));
                    var batchResults = await Task.WhenAll(batchTasks);
                    results.AddRange(batchResults);

                    if (job != null)
                    {
                        job.ProcessedRecords = Math.Min(i + batchSize, customers.Count);
                        job.MatchesFound = results.Count(r => r.HasMatches);
                        job.AlertsGenerated = results.Sum(r => r.Alerts.Count);
                        _context.ScreeningJobs.Update(job);
                        await _context.SaveChangesAsync();
                    }
                }

                if (job != null)
                {
                    job.Status = "Completed";
                    job.CompletedAtUtc = DateTime.UtcNow;
                    _context.ScreeningJobs.Update(job);
                    await _context.SaveChangesAsync();
                }

                return results;
            }
            catch (Exception ex)
            {
                if (job != null)
                {
                    job.Status = "Failed";
                    job.ErrorMessage = ex.Message;
                    job.CompletedAtUtc = DateTime.UtcNow;
                    _context.ScreeningJobs.Update(job);
                    await _context.SaveChangesAsync();
                }

                _logger.LogError(ex, "Error during batch screening for job {JobId}", jobId);
                throw;
            }
        }

        public async Task<ScreeningResult> ScreenTransactionAsync(TransactionScreeningRequest transaction)
        {
            var startTime = DateTime.UtcNow;
            var result = new ScreeningResult
            {
                CustomerName = $"{transaction.SenderName} -> {transaction.BeneficiaryName}",
                ScreeningContext = "Transaction",
                ScreeningDate = startTime
            };

            try
            {
                // Create temporary customer objects for screening
                var senderCustomer = new Customer
                {
                    Id = Guid.NewGuid(),
                    FullName = transaction.SenderName,
                    Country = transaction.SourceCountry
                };

                var beneficiaryCustomer = new Customer
                {
                    Id = Guid.NewGuid(),
                    FullName = transaction.BeneficiaryName,
                    Country = transaction.DestinationCountry
                };

                // Screen both sender and beneficiary
                var senderMatches = await _nameMatchingService.MatchNameAsync(transaction.SenderName, senderCustomer);
                var beneficiaryMatches = await _nameMatchingService.MatchNameAsync(transaction.BeneficiaryName, beneficiaryCustomer);

                result.HasMatches = senderMatches.Any() || beneficiaryMatches.Any();

                if (result.HasMatches)
                {
                    // Create alerts for sender matches
                    foreach (var match in senderMatches)
                    {
                        var alert = CreateTransactionAlert(match, transaction, "Sender");
                        result.Alerts.Add(alert);
                    }

                    // Create alerts for beneficiary matches
                    foreach (var match in beneficiaryMatches)
                    {
                        var alert = CreateTransactionAlert(match, transaction, "Beneficiary");
                        result.Alerts.Add(alert);
                    }

                    result.RiskScore = CalculateRiskScore(senderMatches.Concat(beneficiaryMatches).ToList());
                    result.RiskLevel = DetermineRiskLevel(result.RiskScore);
                    result.RequiresEdd = result.Alerts.Any(a => a.RequiresEdd);
                    result.RequiresStr = result.Alerts.Any(a => a.RequiresStr);
                    result.RequiresSar = result.Alerts.Any(a => a.RequiresSar);
                }

                result.ProcessingTime = DateTime.UtcNow - startTime;
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during transaction screening for transaction {TransactionId}", transaction.TransactionId);
                throw;
            }
        }

        public async Task<List<NameMatchResult>> SearchNamesAsync(NameSearchRequest searchRequest)
        {
            try
            {
                // Create a temporary customer for search
                var searchCustomer = new Customer
                {
                    Id = Guid.NewGuid(),
                    FullName = searchRequest.Name,
                    Country = searchRequest.Country,
                    Nationality = searchRequest.Nationality,
                    DateOfBirth = searchRequest.DateOfBirth
                };

                var matches = await _nameMatchingService.MatchNameAsync(searchRequest.Name, searchCustomer, searchRequest.Threshold);

                // Filter by sources if specified
                if (searchRequest.Sources.Any())
                {
                    matches = matches.Where(m => searchRequest.Sources.Contains(m.SourceList)).ToList();
                }

                // Filter by list types if specified
                if (searchRequest.ListTypes.Any())
                {
                    matches = matches.Where(m => searchRequest.ListTypes.Contains(m.ListType)).ToList();
                }

                // Limit results
                return matches.Take(searchRequest.MaxResults).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during name search for {Name}", searchRequest.Name);
                throw;
            }
        }

        public async Task<Customer> UpdateScreeningStatusAsync(Guid customerId, DateTime screeningDate)
        {
            var customer = await _context.Customers.FindAsync(customerId);
            if (customer == null)
                throw new ArgumentException($"Customer with ID {customerId} not found");

            customer.LastScreeningDate = screeningDate;
            customer.NextScreeningDate = CalculateNextScreeningDate(customer.ScreeningFrequency);
            customer.UpdatedAtUtc = DateTime.UtcNow;
            customer.UpdatedBy = "System";

            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();

            return customer;
        }

        public async Task<ScreeningStatistics> GetScreeningStatisticsAsync(DateTime startDate, DateTime endDate)
        {
            var statistics = new ScreeningStatistics();

            try
            {
                // Get alerts within date range
                var alerts = await _context.Alerts
                    .Where(a => a.CreatedAtUtc >= startDate && a.CreatedAtUtc <= endDate)
                    .ToListAsync();

                statistics.TotalScreenings = alerts.Count;
                statistics.MatchesFound = alerts.Count;
                statistics.AlertsGenerated = alerts.Count;
                statistics.PepMatches = alerts.Count(a => a.AlertType == "PEP");
                statistics.SanctionMatches = alerts.Count(a => a.AlertType == "Sanctions");
                statistics.AdverseMediaMatches = alerts.Count(a => a.AlertType == "Adverse Media");
                statistics.EddRequired = alerts.Count(a => a.RequiresEdd);
                statistics.StrRequired = alerts.Count(a => a.RequiresStr);
                statistics.SarRequired = alerts.Count(a => a.RequiresSar);

                // Group by source
                statistics.MatchesBySource = alerts
                    .GroupBy(a => a.SourceList)
                    .ToDictionary(g => g.Key ?? "Unknown", g => g.Count());

                // Group by risk level
                statistics.MatchesByRiskLevel = alerts
                    .GroupBy(a => a.RiskLevel)
                    .ToDictionary(g => g.Key ?? "Unknown", g => g.Count());

                return statistics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting screening statistics from {StartDate} to {EndDate}", startDate, endDate);
                throw;
            }
        }

        private string DeterminePriority(double similarityScore, string riskLevel)
        {
            if (similarityScore >= 0.95 || riskLevel == "Critical")
                return "Critical";
            if (similarityScore >= 0.85 || riskLevel == "High")
                return "High";
            if (similarityScore >= 0.75 || riskLevel == "Medium")
                return "Medium";
            return "Low";
        }

        private void DetermineComplianceActions(Alert alert, NameMatchResult match)
        {
            // PEP matches require EDD
            if (match.ListType == "PEP")
            {
                alert.RequiresEdd = true;
                alert.ComplianceAction = "EDD";
            }

            // Sanctions matches require immediate action
            if (match.ListType == "Sanctions")
            {
                alert.RequiresStr = true;
                alert.RequiresSar = true;
                alert.ComplianceAction = "STR/SAR";
            }

            // High-risk matches require STR
            if (match.RiskLevel == "High" || match.RiskLevel == "Critical")
            {
                alert.RequiresStr = true;
                alert.ComplianceAction = "STR";
            }

            // Set due dates based on priority
            alert.DueDate = alert.Priority switch
            {
                "Critical" => DateTime.UtcNow.AddHours(1),
                "High" => DateTime.UtcNow.AddHours(4),
                "Medium" => DateTime.UtcNow.AddHours(24),
                _ => DateTime.UtcNow.AddDays(3)
            };
        }

        private int CalculateRiskScore(List<NameMatchResult> matches)
        {
            var score = 0;
            foreach (var match in matches)
            {
                score += match.ListType switch
                {
                    "PEP" => 50,
                    "Sanctions" => 100,
                    "Adverse Media" => 30,
                    _ => 20
                };

                score += match.RiskLevel switch
                {
                    "Critical" => 50,
                    "High" => 30,
                    "Medium" => 15,
                    _ => 5
                };

                score += (int)(match.SimilarityScore * 20);
            }

            return Math.Min(score, 100);
        }

        private string DetermineRiskLevel(int riskScore)
        {
            return riskScore switch
            {
                >= 80 => "Critical",
                >= 60 => "High",
                >= 40 => "Medium",
                >= 20 => "Low",
                _ => "Minimal"
            };
        }

        private DateTime CalculateNextScreeningDate(string frequency)
        {
            return frequency switch
            {
                "Daily" => DateTime.UtcNow.AddDays(1),
                "Weekly" => DateTime.UtcNow.AddDays(7),
                "Monthly" => DateTime.UtcNow.AddMonths(1),
                "Quarterly" => DateTime.UtcNow.AddMonths(3),
                _ => DateTime.UtcNow.AddMonths(1)
            };
        }

        private Alert CreateTransactionAlert(NameMatchResult match, TransactionScreeningRequest transaction, string party)
        {
            return new Alert
            {
                Id = Guid.NewGuid(),
                Context = "Transaction",
                AlertType = match.ListType,
                SimilarityScore = match.SimilarityScore,
                MatchAlgorithm = match.MatchAlgorithm,
                Status = "Open",
                Priority = DeterminePriority(match.SimilarityScore, match.RiskLevel),
                RiskLevel = match.RiskLevel,
                MatchedFields = match.MatchedFields,
                SourceList = match.SourceList,
                SourceCategory = match.ListType,
                TransactionId = transaction.TransactionId,
                TransactionAmount = transaction.Amount,
                TransactionType = transaction.TransactionType,
                TransactionDate = transaction.TransactionDate,
                CreatedAtUtc = DateTime.UtcNow,
                CreatedBy = "System"
            };
        }
    }
}
