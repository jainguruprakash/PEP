using PEPScanner.Domain.Entities;

namespace PEPScanner.Application.Abstractions
{
    public interface IBiometricMatchingService
    {
        Task<BiometricMatchResult> MatchBiometricDataAsync(Customer customer, BiometricData biometricData);
        Task<List<BiometricMatchResult>> BatchMatchBiometricsAsync(List<Customer> customers);
        Task<double> CalculateBiometricSimilarityAsync(BiometricData data1, BiometricData data2);
        Task<BiometricMatchResult> MatchFingerprintAsync(string customerFingerprint, string watchlistFingerprint);
        Task<BiometricMatchResult> MatchFaceAsync(byte[] customerPhoto, byte[] watchlistPhoto);
    }

    public class BiometricData
    {
        public string? FingerprintData { get; set; }
        public byte[]? PhotoData { get; set; }
        public string? IrisData { get; set; }
        public string? VoicePrint { get; set; }
        public Dictionary<string, string> AdditionalBiometrics { get; set; } = new();
    }

    public class BiometricMatchResult
    {
        public Guid CustomerId { get; set; }
        public Guid WatchlistEntryId { get; set; }
        public string MatchType { get; set; } = string.Empty; // Fingerprint, Face, Iris, Voice
        public double SimilarityScore { get; set; }
        public string Algorithm { get; set; } = string.Empty;
        public bool IsMatch { get; set; }
        public string MatchDetails { get; set; } = string.Empty;
        public DateTime MatchDate { get; set; } = DateTime.UtcNow;
    }
}
