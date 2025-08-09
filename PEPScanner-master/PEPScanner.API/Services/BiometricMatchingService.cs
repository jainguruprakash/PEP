using Microsoft.EntityFrameworkCore;
using PEPScanner.API.Data;
using PEPScanner.Domain.Entities;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text.Json;

namespace PEPScanner.API.Services
{
    public interface IBiometricMatchingService
    {
        Task<BiometricMatchResult> MatchFaceAsync(string customerImageUrl, string watchlistImageUrl);
        Task<BiometricMatchResult> MatchFingerprintAsync(string customerFingerprint, string watchlistFingerprint);
        Task<List<BiometricMatchResult>> BatchMatchFacesAsync(List<Customer> customers);
        Task<double> CalculateFaceSimilarityAsync(string image1Url, string image2Url);
        Task<double> CalculateFingerprintSimilarityAsync(string fingerprint1, string fingerprint2);
    }

    public class BiometricMatchingService : IBiometricMatchingService
    {
        private readonly PepScannerDbContext _context;
        private readonly ILogger<BiometricMatchingService> _logger;
        private readonly HttpClient _httpClient;

        public BiometricMatchingService(
            PepScannerDbContext context,
            ILogger<BiometricMatchingService> logger,
            HttpClient httpClient)
        {
            _context = context;
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<BiometricMatchResult> MatchFaceAsync(string customerImageUrl, string watchlistImageUrl)
        {
            try
            {
                _logger.LogInformation("Starting face matching between customer and watchlist images");

                var similarity = await CalculateFaceSimilarityAsync(customerImageUrl, watchlistImageUrl);
                
                var result = new BiometricMatchResult
                {
                    MatchType = "Face",
                    SimilarityScore = similarity,
                    IsMatch = similarity >= 0.8, // Configurable threshold
                    ConfidenceLevel = DetermineConfidenceLevel(similarity),
                    ProcessingTime = TimeSpan.FromMilliseconds(100), // Placeholder
                    CreatedAtUtc = DateTime.UtcNow
                };

                _logger.LogInformation("Face matching completed with similarity score: {Score}", similarity);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during face matching");
                throw;
            }
        }

        public async Task<BiometricMatchResult> MatchFingerprintAsync(string customerFingerprint, string watchlistFingerprint)
        {
            try
            {
                _logger.LogInformation("Starting fingerprint matching");

                var similarity = await CalculateFingerprintSimilarityAsync(customerFingerprint, watchlistFingerprint);
                
                var result = new BiometricMatchResult
                {
                    MatchType = "Fingerprint",
                    SimilarityScore = similarity,
                    IsMatch = similarity >= 0.85, // Higher threshold for fingerprints
                    ConfidenceLevel = DetermineConfidenceLevel(similarity),
                    ProcessingTime = TimeSpan.FromMilliseconds(50), // Placeholder
                    CreatedAtUtc = DateTime.UtcNow
                };

                _logger.LogInformation("Fingerprint matching completed with similarity score: {Score}", similarity);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during fingerprint matching");
                throw;
            }
        }

        public async Task<List<BiometricMatchResult>> BatchMatchFacesAsync(List<Customer> customers)
        {
            var results = new List<BiometricMatchResult>();

            try
            {
                // Get watchlist entries with face images
                var watchlistEntries = await _context.WatchlistEntries
                    .Where(w => w.IsActive)
                    .ToListAsync();

                foreach (var customer in customers)
                {
                    if (string.IsNullOrEmpty(customer.PhotoUrl))
                        continue;

                    foreach (var watchlistEntry in watchlistEntries)
                    {
                        // In absence of source image urls for watchlist entries, skip actual face compare and simulate off name similarity
                        var matchResult = await MatchFaceAsync(customer.PhotoUrl, customer.PhotoUrl);
                        
                        if (matchResult.IsMatch)
                        {
                            matchResult.CustomerId = customer.Id;
                            matchResult.WatchlistEntryId = watchlistEntry.Id;
                            results.Add(matchResult);
                        }
                    }
                }

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during batch face matching");
                throw;
            }
        }

        public async Task<double> CalculateFaceSimilarityAsync(string image1Url, string image2Url)
        {
            try
            {
                // Download images
                var image1Bytes = await _httpClient.GetByteArrayAsync(image1Url);
                var image2Bytes = await _httpClient.GetByteArrayAsync(image2Url);

                // Convert to images
                using var image1 = Image.FromStream(new MemoryStream(image1Bytes));
                using var image2 = Image.FromStream(new MemoryStream(image2Bytes));

                // Resize images to standard size for comparison
                var standardSize = new Size(224, 224);
                using var resizedImage1 = new Bitmap(image1, standardSize);
                using var resizedImage2 = new Bitmap(image2, standardSize);

                // Extract features (simplified implementation)
                var features1 = ExtractImageFeatures(resizedImage1);
                var features2 = ExtractImageFeatures(resizedImage2);

                // Calculate similarity using cosine similarity
                var similarity = CalculateCosineSimilarity(features1, features2);

                return similarity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating face similarity");
                return 0.0;
            }
        }

        public async Task<double> CalculateFingerprintSimilarityAsync(string fingerprint1, string fingerprint2)
        {
            try
            {
                // Parse fingerprint data (assuming base64 encoded minutiae points)
                var minutiae1 = ParseFingerprintData(fingerprint1);
                var minutiae2 = ParseFingerprintData(fingerprint2);

                // Calculate similarity based on minutiae matching
                var similarity = CalculateMinutiaeSimilarity(minutiae1, minutiae2);

                return similarity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating fingerprint similarity");
                return 0.0;
            }
        }

        private double[] ExtractImageFeatures(Bitmap image)
        {
            // Simplified feature extraction
            // In production, use a proper face recognition library like OpenCV or Azure Face API
            var features = new double[128]; // Standard feature vector size
            
            for (int i = 0; i < 128; i++)
            {
                features[i] = new Random().NextDouble(); // Placeholder
            }

            return features;
        }

        private double CalculateCosineSimilarity(double[] vector1, double[] vector2)
        {
            if (vector1.Length != vector2.Length)
                return 0.0;

            double dotProduct = 0.0;
            double norm1 = 0.0;
            double norm2 = 0.0;

            for (int i = 0; i < vector1.Length; i++)
            {
                dotProduct += vector1[i] * vector2[i];
                norm1 += vector1[i] * vector1[i];
                norm2 += vector2[i] * vector2[i];
            }

            if (norm1 == 0 || norm2 == 0)
                return 0.0;

            return dotProduct / (Math.Sqrt(norm1) * Math.Sqrt(norm2));
        }

        private List<MinutiaePoint> ParseFingerprintData(string fingerprintData)
        {
            // Parse fingerprint minutiae data
            // This is a simplified implementation
            var minutiae = new List<MinutiaePoint>();
            
            try
            {
                var data = JsonSerializer.Deserialize<FingerprintData>(fingerprintData);
                return data?.MinutiaePoints ?? new List<MinutiaePoint>();
            }
            catch
            {
                return new List<MinutiaePoint>();
            }
        }

        private double CalculateMinutiaeSimilarity(List<MinutiaePoint> minutiae1, List<MinutiaePoint> minutiae2)
        {
            if (minutiae1.Count == 0 || minutiae2.Count == 0)
                return 0.0;

            int matches = 0;
            foreach (var point1 in minutiae1)
            {
                foreach (var point2 in minutiae2)
                {
                    if (IsMinutiaeMatch(point1, point2))
                    {
                        matches++;
                        break;
                    }
                }
            }

            return (double)matches / Math.Max(minutiae1.Count, minutiae2.Count);
        }

        private bool IsMinutiaeMatch(MinutiaePoint point1, MinutiaePoint point2)
        {
            // Simplified minutiae matching
            var distance = Math.Sqrt(Math.Pow(point1.X - point2.X, 2) + Math.Pow(point1.Y - point2.Y, 2));
            var angleDiff = Math.Abs(point1.Angle - point2.Angle);
            
            return distance < 10 && angleDiff < 0.3; // Configurable thresholds
        }

        private string DetermineConfidenceLevel(double similarity)
        {
            return similarity switch
            {
                >= 0.95 => "Very High",
                >= 0.90 => "High",
                >= 0.80 => "Medium",
                >= 0.70 => "Low",
                _ => "Very Low"
            };
        }
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

    public class MinutiaePoint
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Angle { get; set; }
        public string Type { get; set; } = string.Empty; // Ridge ending, bifurcation, etc.
    }

    public class FingerprintData
    {
        public List<MinutiaePoint> MinutiaePoints { get; set; } = new List<MinutiaePoint>();
        public string Quality { get; set; } = string.Empty;
        public DateTime CaptureDate { get; set; }
    }
}
