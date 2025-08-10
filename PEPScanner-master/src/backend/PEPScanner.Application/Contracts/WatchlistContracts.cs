namespace PEPScanner.Application.Contracts
{
    public class WatchlistUpdateResult
    {
        public bool Success { get; set; }
        public string Source { get; set; } = string.Empty;
        public int TotalRecords { get; set; }
        public int NewRecords { get; set; }
        public int UpdatedRecords { get; set; }
        public int DeletedRecords { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime ProcessingDate { get; set; } = DateTime.UtcNow;
        public TimeSpan ProcessingTime { get; set; }
    }

    public class FileValidationResult
    {
        public bool IsValid { get; set; }
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
    }

    public class BiometricMatchResult
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid? CustomerId { get; set; }
        public Guid? WatchlistEntryId { get; set; }
        public string MatchType { get; set; } = string.Empty; // Face, Fingerprint
        public double SimilarityScore { get; set; }
        public bool IsMatch { get; set; }
        public string ConfidenceLevel { get; set; } = string.Empty;
        public TimeSpan ProcessingTime { get; set; }
        public DateTime CreatedAtUtc { get; set; }
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
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public DateTime PublishedDate { get; set; }
        public string Content { get; set; } = string.Empty;
        public double SentimentScore { get; set; }
        public string SentimentLabel { get; set; } = string.Empty; // Positive, Negative, Neutral
        public List<string> Keywords { get; set; } = new List<string>();
        public string Language { get; set; } = "en";
        public string Country { get; set; } = string.Empty;
    }
}
