using PEPScanner.API.Models;

namespace PEPScanner.API.Services
{
    public interface INameMatchingService
    {
        /// <summary>
        /// Performs comprehensive name matching against watchlist entries
        /// </summary>
        /// <param name="customerName">The customer name to match</param>
        /// <param name="customerData">Additional customer data for context</param>
        /// <param name="threshold">Minimum similarity score (0.0 to 1.0)</param>
        /// <returns>List of potential matches with scores</returns>
        Task<List<NameMatchResult>> MatchNameAsync(string customerName, Customer customerData, double threshold = 0.7);

        /// <summary>
        /// Performs batch name matching for multiple customers
        /// </summary>
        /// <param name="customers">List of customers to screen</param>
        /// <param name="threshold">Minimum similarity score</param>
        /// <returns>List of matches for all customers</returns>
        Task<List<NameMatchResult>> BatchMatchNamesAsync(List<Customer> customers, double threshold = 0.7);

        /// <summary>
        /// Transliterates names from Indian scripts to Latin script
        /// </summary>
        /// <param name="name">Name in original script</param>
        /// <param name="sourceScript">Source script (e.g., Devanagari, Telugu)</param>
        /// <returns>Transliterated name</returns>
        string TransliterateName(string name, string sourceScript = "Devanagari");

        /// <summary>
        /// Normalizes a name for better matching (removes special characters, standardizes format)
        /// </summary>
        /// <param name="name">Original name</param>
        /// <returns>Normalized name</returns>
        string NormalizeName(string name);

        /// <summary>
        /// Generates phonetic variations of a name
        /// </summary>
        /// <param name="name">Original name</param>
        /// <returns>List of phonetic variations</returns>
        List<string> GeneratePhoneticVariations(string name);

        /// <summary>
        /// Calculates similarity score between two names using multiple algorithms
        /// </summary>
        /// <param name="name1">First name</param>
        /// <param name="name2">Second name</param>
        /// <returns>Similarity score and algorithm details</returns>
        SimilarityScore CalculateSimilarity(string name1, string name2);
    }

    public class NameMatchResult
    {
        public Guid WatchlistEntryId { get; set; }
        public string WatchlistName { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public double SimilarityScore { get; set; }
        public string MatchAlgorithm { get; set; } = string.Empty;
        public string MatchedFields { get; set; } = string.Empty;
        public string SourceList { get; set; } = string.Empty;
        public string ListType { get; set; } = string.Empty;
        public string RiskLevel { get; set; } = string.Empty;
        public WatchlistEntry WatchlistEntry { get; set; } = null!;
        public Customer Customer { get; set; } = null!;
    }

    public class SimilarityScore
    {
        public double OverallScore { get; set; }
        public double LevenshteinScore { get; set; }
        public double JaroWinklerScore { get; set; }
        public double SoundexScore { get; set; }
        public double MetaphoneScore { get; set; }
        public string BestAlgorithm { get; set; } = string.Empty;
        public string MatchingDetails { get; set; } = string.Empty;
    }
}
