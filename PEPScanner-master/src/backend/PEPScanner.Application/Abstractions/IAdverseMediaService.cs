using PEPScanner.Domain.Entities;

namespace PEPScanner.Application.Abstractions
{
    public interface IAdverseMediaService
    {
        Task<List<AdverseMediaResult>> SearchAdverseMediaAsync(string entityName, string entityType = "Individual");
        Task<List<AdverseMediaResult>> BatchSearchAdverseMediaAsync(List<Customer> customers);
        Task<AdverseMediaResult> AnalyzeNewsArticleAsync(string articleUrl, string entityName);
        Task<List<AdverseMediaSource>> GetConfiguredSourcesAsync();
        Task<AdverseMediaSummary> GenerateAdverseMediaSummaryAsync(Guid customerId);
    }

    public class AdverseMediaResult
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid CustomerId { get; set; }
        public string EntityName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string SourceUrl { get; set; } = string.Empty;
        public string SourceName { get; set; } = string.Empty;
        public DateTime PublishedDate { get; set; }
        public string RiskLevel { get; set; } = "Medium"; // Low, Medium, High, Critical
        public double RelevanceScore { get; set; }
        public List<string> Keywords { get; set; } = new();
        public string Category { get; set; } = string.Empty; // Corruption, Fraud, Terrorism, etc.
        public DateTime SearchDate { get; set; } = DateTime.UtcNow;
    }

    public class AdverseMediaSource
    {
        public string Name { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public string Country { get; set; } = string.Empty;
        public string Language { get; set; } = "English";
    }

    public class AdverseMediaSummary
    {
        public Guid CustomerId { get; set; }
        public int TotalArticles { get; set; }
        public int HighRiskArticles { get; set; }
        public int MediumRiskArticles { get; set; }
        public int LowRiskArticles { get; set; }
        public string OverallRiskLevel { get; set; } = string.Empty;
        public List<string> TopCategories { get; set; } = new();
        public DateTime LastSearchDate { get; set; }
    }
}
