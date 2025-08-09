using PEPScanner.API.Models;

namespace PEPScanner.API.Services
{
    public interface IScreeningService
    {
        /// <summary>
        /// Performs real-time screening of a customer during onboarding
        /// </summary>
        /// <param name="customer">Customer to screen</param>
        /// <param name="context">Screening context (Onboarding, Transaction, etc.)</param>
        /// <returns>Screening result with alerts</returns>
        Task<ScreeningResult> ScreenCustomerAsync(Customer customer, string context = "Onboarding");

        /// <summary>
        /// Performs batch screening of multiple customers
        /// </summary>
        /// <param name="customers">List of customers to screen</param>
        /// <param name="jobId">Screening job ID</param>
        /// <returns>Screening results</returns>
        Task<List<ScreeningResult>> BatchScreenCustomersAsync(List<Customer> customers, Guid jobId);

        /// <summary>
        /// Screens a transaction against PEP/sanctions lists
        /// </summary>
        /// <param name="transaction">Transaction details</param>
        /// <returns>Screening result</returns>
        Task<ScreeningResult> ScreenTransactionAsync(TransactionScreeningRequest transaction);

        /// <summary>
        /// Performs ad-hoc name search against watchlists
        /// </summary>
        /// <param name="searchRequest">Search parameters</param>
        /// <returns>Search results</returns>
        Task<List<NameMatchResult>> SearchNamesAsync(NameSearchRequest searchRequest);

        /// <summary>
        /// Updates customer screening status and schedules next screening
        /// </summary>
        /// <param name="customerId">Customer ID</param>
        /// <param name="screeningDate">Date of screening</param>
        /// <returns>Updated customer</returns>
        Task<Customer> UpdateScreeningStatusAsync(Guid customerId, DateTime screeningDate);

        /// <summary>
        /// Gets screening statistics for reporting
        /// </summary>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>Screening statistics</returns>
        Task<ScreeningStatistics> GetScreeningStatisticsAsync(DateTime startDate, DateTime endDate);
    }

    public class ScreeningResult
    {
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public bool HasMatches { get; set; }
        public List<Alert> Alerts { get; set; } = new List<Alert>();
        public int RiskScore { get; set; }
        public string RiskLevel { get; set; } = "Low";
        public bool RequiresEdd { get; set; }
        public bool RequiresStr { get; set; }
        public bool RequiresSar { get; set; }
        public DateTime ScreeningDate { get; set; } = DateTime.UtcNow;
        public string ScreeningContext { get; set; } = string.Empty;
        public TimeSpan ProcessingTime { get; set; }
    }

    public class TransactionScreeningRequest
    {
        public string TransactionId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string TransactionType { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public string SenderId { get; set; } = string.Empty;
        public string BeneficiaryName { get; set; } = string.Empty;
        public string BeneficiaryId { get; set; } = string.Empty;
        public string SourceCountry { get; set; } = string.Empty;
        public string DestinationCountry { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    }

    public class NameSearchRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Country { get; set; }
        public string? Nationality { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public List<string> Sources { get; set; } = new List<string>();
        public List<string> ListTypes { get; set; } = new List<string>();
        public double Threshold { get; set; } = 0.7;
        public int MaxResults { get; set; } = 100;
    }

    public class ScreeningStatistics
    {
        public int TotalScreenings { get; set; }
        public int MatchesFound { get; set; }
        public int AlertsGenerated { get; set; }
        public int PepMatches { get; set; }
        public int SanctionMatches { get; set; }
        public int AdverseMediaMatches { get; set; }
        public int EddRequired { get; set; }
        public int StrRequired { get; set; }
        public int SarRequired { get; set; }
        public double AverageProcessingTime { get; set; }
        public Dictionary<string, int> MatchesBySource { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> MatchesByRiskLevel { get; set; } = new Dictionary<string, int>();
    }
}
