using Microsoft.EntityFrameworkCore;
using PEPScanner.API.Data;
using PEPScanner.Domain.Entities;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace PEPScanner.API.Services
{
    public interface IAdverseMediaService
    {
        Task<AdverseMediaResult> ScanCustomerMediaAsync(Customer customer);
        Task<List<AdverseMediaResult>> BatchScanMediaAsync(List<Customer> customers);
        Task<AdverseMediaResult> ScanSpecificSourceAsync(string customerName, string sourceUrl);
        Task<List<MediaArticle>> SearchNewsAsync(string searchTerm, DateTime? fromDate = null);
        Task<double> AnalyzeSentimentAsync(string text);
    }

    public class AdverseMediaService : IAdverseMediaService
    {
        private readonly PepScannerDbContext _context;
        private readonly ILogger<AdverseMediaService> _logger;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        // News sources for Indian context
        private readonly List<string> _newsSources = new()
        {
            "https://timesofindia.indiatimes.com",
            "https://www.hindustantimes.com",
            "https://www.thehindu.com",
            "https://www.ndtv.com",
            "https://www.indianexpress.com",
            "https://www.financialexpress.com",
            "https://www.business-standard.com",
            "https://www.moneycontrol.com"
        };

        // Negative keywords for adverse media detection
        private readonly List<string> _negativeKeywords = new()
        {
            "fraud", "scam", "corruption", "money laundering", "terrorism", "sanctions",
            "arrest", "conviction", "investigation", "probe", "chargesheet", "fir",
            "default", "bankruptcy", "insolvency", "wilful defaulter", "blacklisted",
            "banned", "suspended", "penalty", "fine", "illegal", "criminal"
        };

        public AdverseMediaService(
            PepScannerDbContext context,
            ILogger<AdverseMediaService> logger,
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<AdverseMediaResult> ScanCustomerMediaAsync(Customer customer)
        {
            var result = new AdverseMediaResult
            {
                CustomerId = customer.Id,
                CustomerName = customer.FullName,
                ScanDate = DateTime.UtcNow
            };

            try
            {
                _logger.LogInformation("Starting adverse media scan for customer: {CustomerName}", customer.FullName);

                var articles = new List<MediaArticle>();

                // Search across multiple news sources
                foreach (var source in _newsSources.Take(3)) // Limit to 3 sources for performance
                {
                    try
                    {
                        var sourceArticles = await SearchNewsAsync(customer.FullName, DateTime.UtcNow.AddDays(-30));
                        articles.AddRange(sourceArticles);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to search source: {Source}", source);
                    }
                }

                // Analyze articles for adverse content
                foreach (var article in articles)
                {
                    var sentiment = await AnalyzeSentimentAsync(article.Content);
                    article.SentimentScore = sentiment;

                    if (sentiment < 0.3 || ContainsNegativeKeywords(article.Content))
                    {
                        article.IsAdverse = true;
                        result.AdverseArticles.Add(article);
                    }
                }

                result.TotalArticlesFound = articles.Count;
                result.AdverseArticlesCount = result.AdverseArticles.Count;
                result.OverallSentiment = articles.Any() ? articles.Average(a => a.SentimentScore) : 0.5;
                result.HasAdverseMedia = result.AdverseArticles.Any();
                result.RiskLevel = DetermineRiskLevel(result.AdverseArticlesCount, result.OverallSentiment);

                // Store results in database
                await StoreAdverseMediaResultAsync(result);

                _logger.LogInformation("Adverse media scan completed for {CustomerName}. Found {Count} adverse articles", 
                    customer.FullName, result.AdverseArticlesCount);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during adverse media scan for customer: {CustomerName}", customer.FullName);
                result.ErrorMessage = ex.Message;
                return result;
            }
        }

        public async Task<List<AdverseMediaResult>> BatchScanMediaAsync(List<Customer> customers)
        {
            var results = new List<AdverseMediaResult>();

            // Process customers in parallel batches
            const int batchSize = 10;
            for (int i = 0; i < customers.Count; i += batchSize)
            {
                var batch = customers.Skip(i).Take(batchSize).ToList();
                var batchTasks = batch.Select(customer => ScanCustomerMediaAsync(customer));
                var batchResults = await Task.WhenAll(batchTasks);
                results.AddRange(batchResults);

                // Add delay between batches to avoid rate limiting
                if (i + batchSize < customers.Count)
                {
                    await Task.Delay(2000);
                }
            }

            return results;
        }

        public async Task<AdverseMediaResult> ScanSpecificSourceAsync(string customerName, string sourceUrl)
        {
            var result = new AdverseMediaResult
            {
                CustomerName = customerName,
                ScanDate = DateTime.UtcNow
            };

            try
            {
                var articles = await SearchNewsAsync(customerName);
                var relevantArticles = articles.Where(a => a.SourceUrl.Contains(sourceUrl)).ToList();

                foreach (var article in relevantArticles)
                {
                    var sentiment = await AnalyzeSentimentAsync(article.Content);
                    article.SentimentScore = sentiment;

                    if (sentiment < 0.3 || ContainsNegativeKeywords(article.Content))
                    {
                        article.IsAdverse = true;
                        result.AdverseArticles.Add(article);
                    }
                }

                result.TotalArticlesFound = relevantArticles.Count;
                result.AdverseArticlesCount = result.AdverseArticles.Count;
                result.HasAdverseMedia = result.AdverseArticles.Any();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scanning specific source for {CustomerName}", customerName);
                result.ErrorMessage = ex.Message;
                return result;
            }
        }

        public async Task<List<MediaArticle>> SearchNewsAsync(string searchTerm, DateTime? fromDate = null)
        {
            var articles = new List<MediaArticle>();

            try
            {
                // Use a news API service (example with NewsAPI)
                var apiKey = _configuration["NewsApi:ApiKey"];
                if (!string.IsNullOrEmpty(apiKey))
                {
                    var dateFilter = fromDate?.ToString("yyyy-MM-dd") ?? DateTime.UtcNow.AddDays(-7).ToString("yyyy-MM-dd");
                    var url = $"https://newsapi.org/v2/everything?q={Uri.EscapeDataString(searchTerm)}&from={dateFilter}&language=en&sortBy=relevancy&apiKey={apiKey}";

                    var response = await _httpClient.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var newsResponse = JsonSerializer.Deserialize<NewsApiResponse>(content);

                        if (newsResponse?.Articles != null)
                        {
                            articles.AddRange(newsResponse.Articles.Select(a => new MediaArticle
                            {
                                Title = a.Title,
                                Content = a.Description,
                                SourceUrl = a.Url,
                                PublishedDate = a.PublishedAt,
                                Source = a.Source?.Name ?? "Unknown"
                            }));
                        }
                    }
                }
                else
                {
                    // Fallback: Simulate news search results
                    articles.AddRange(GenerateMockArticles(searchTerm));
                }

                return articles;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching news for term: {SearchTerm}", searchTerm);
                return new List<MediaArticle>();
            }
        }

        public async Task<double> AnalyzeSentimentAsync(string text)
        {
            try
            {
                if (string.IsNullOrEmpty(text))
                    return 0.5; // Neutral sentiment

                // Simple sentiment analysis based on keyword counting
                var words = text.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var positiveWords = CountPositiveWords(words);
                var negativeWords = CountNegativeWords(words);
                var totalWords = words.Length;

                if (totalWords == 0)
                    return 0.5;

                var sentiment = (positiveWords - negativeWords) / (double)totalWords;
                return Math.Max(0, Math.Min(1, (sentiment + 1) / 2)); // Normalize to 0-1 range
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing sentiment");
                return 0.5; // Neutral sentiment on error
            }
        }

        private bool ContainsNegativeKeywords(string content)
        {
            var lowerContent = content.ToLower();
            return _negativeKeywords.Any(keyword => lowerContent.Contains(keyword));
        }

        private int CountPositiveWords(string[] words)
        {
            var positiveWords = new[] { "good", "great", "excellent", "positive", "success", "profit", "growth", "improve" };
            return words.Count(word => positiveWords.Contains(word));
        }

        private int CountNegativeWords(string[] words)
        {
            return words.Count(word => _negativeKeywords.Contains(word));
        }

        private string DetermineRiskLevel(int adverseCount, double sentiment)
        {
            if (adverseCount >= 5 || sentiment < 0.2)
                return "Critical";
            if (adverseCount >= 3 || sentiment < 0.4)
                return "High";
            if (adverseCount >= 1 || sentiment < 0.6)
                return "Medium";
            return "Low";
        }

        private async Task StoreAdverseMediaResultAsync(AdverseMediaResult result)
        {
            try
            {
                // Store in database for audit trail
                var auditLog = new AuditLog
                {
                    Action = "AdverseMediaScan",
                    EntityType = "Customer",
                    EntityId = result.CustomerId,
                    AdditionalData = JsonSerializer.Serialize(result),
                    Severity = result.HasAdverseMedia ? "Warning" : "Info",
                    TimestampUtc = DateTime.UtcNow
                };

                _context.AuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing adverse media result");
            }
        }

        private List<MediaArticle> GenerateMockArticles(string searchTerm)
        {
            // Generate mock articles for testing
            return new List<MediaArticle>
            {
                new MediaArticle
                {
                    Title = $"News about {searchTerm}",
                    Content = $"This is a sample news article about {searchTerm}.",
                    SourceUrl = "https://example.com/news/1",
                    PublishedDate = DateTime.UtcNow.AddDays(-1),
                    Source = "Mock News Source"
                }
            };
        }
    }

    public class AdverseMediaResult
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid? CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public DateTime ScanDate { get; set; }
        public int TotalArticlesFound { get; set; }
        public int AdverseArticlesCount { get; set; }
        public double OverallSentiment { get; set; }
        public bool HasAdverseMedia { get; set; }
        public string RiskLevel { get; set; } = "Low";
        public string? ErrorMessage { get; set; }
        public List<MediaArticle> AdverseArticles { get; set; } = new List<MediaArticle>();
    }

    public class MediaArticle
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string SourceUrl { get; set; } = string.Empty;
        public DateTime? PublishedDate { get; set; }
        public string Source { get; set; } = string.Empty;
        public double SentimentScore { get; set; }
        public bool IsAdverse { get; set; }
    }

    public class NewsApiResponse
    {
        public string Status { get; set; } = string.Empty;
        public int TotalResults { get; set; }
        public List<NewsApiArticle> Articles { get; set; } = new List<NewsApiArticle>();
    }

    public class NewsApiArticle
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public DateTime? PublishedAt { get; set; }
        public NewsApiSource? Source { get; set; }
    }

    public class NewsApiSource
    {
        public string Name { get; set; } = string.Empty;
    }
}
