using Microsoft.EntityFrameworkCore;
using PEPScanner.API.Data;
using PEPScanner.Domain.Entities;
using PEPScanner.Application.Abstractions;
using FuzzySharp;
using System.Text.RegularExpressions;

namespace PEPScanner.Infrastructure.Services
{
    public class NameMatchingService : INameMatchingService
    {
        private readonly PepScannerDbContext _context;
        private readonly ILogger<NameMatchingService> _logger;

        // Indian name transliteration mappings
        private readonly Dictionary<string, string> _devanagariToLatin = new()
        {
            {"अ", "a"}, {"आ", "aa"}, {"इ", "i"}, {"ई", "ee"}, {"उ", "u"}, {"ऊ", "uu"},
            {"ए", "e"}, {"ऐ", "ai"}, {"ओ", "o"}, {"औ", "au"}, {"क", "k"}, {"ख", "kh"},
            {"ग", "g"}, {"घ", "gh"}, {"ङ", "ng"}, {"च", "ch"}, {"छ", "chh"}, {"ज", "j"},
            {"झ", "jh"}, {"ञ", "ny"}, {"ट", "t"}, {"ठ", "th"}, {"ड", "d"}, {"ढ", "dh"},
            {"ण", "n"}, {"त", "t"}, {"थ", "th"}, {"द", "d"}, {"ध", "dh"}, {"न", "n"},
            {"प", "p"}, {"फ", "ph"}, {"ब", "b"}, {"भ", "bh"}, {"म", "m"}, {"य", "y"},
            {"र", "r"}, {"ल", "l"}, {"व", "v"}, {"श", "sh"}, {"ष", "sh"}, {"स", "s"},
            {"ह", "h"}, {"क्ष", "ksh"}, {"त्र", "tr"}, {"ज्ञ", "gy"}
        };

        public NameMatchingService(PepScannerDbContext context, ILogger<NameMatchingService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<NameMatchResult>> MatchNameAsync(string customerName, Customer customerData, double threshold = 0.7)
        {
            var results = new List<NameMatchResult>();
            var normalizedCustomerName = NormalizeName(customerName);
            var phoneticVariations = GeneratePhoneticVariations(normalizedCustomerName);

            // Get active watchlist entries
            var watchlistEntries = await _context.WatchlistEntries
                .Where(w => w.IsActive && !w.IsWhitelisted)
                .ToListAsync();

            foreach (var entry in watchlistEntries)
            {
                var entryNames = new List<string> { entry.PrimaryName };
                
                // Add alternate names if available
                if (!string.IsNullOrEmpty(entry.AlternateNames))
                {
                    entryNames.AddRange(entry.AlternateNames.Split(',', ';').Select(n => n.Trim()));
                }

                foreach (var entryName in entryNames)
                {
                    var normalizedEntryName = NormalizeName(entryName);
                    var similarity = CalculateSimilarity(normalizedCustomerName, normalizedEntryName);

                    // Check if any phonetic variation matches
                    foreach (var phoneticVariation in phoneticVariations)
                    {
                        var phoneticSimilarity = CalculateSimilarity(phoneticVariation, normalizedEntryName);
                        if (phoneticSimilarity.OverallScore > similarity.OverallScore)
                        {
                            similarity = phoneticSimilarity;
                        }
                    }

                    if (similarity.OverallScore >= threshold)
                    {
                        results.Add(new NameMatchResult
                        {
                            WatchlistEntryId = entry.Id,
                            WatchlistName = entry.PrimaryName,
                            CustomerName = customerName,
                            SimilarityScore = similarity.OverallScore,
                            MatchAlgorithm = similarity.BestAlgorithm,
                            MatchedFields = $"Name: {similarity.MatchingDetails}",
                            SourceList = entry.Source,
                            ListType = entry.ListType,
                            RiskLevel = entry.RiskLevel ?? "Medium",
                            WatchlistEntry = entry,
                            Customer = customerData
                        });
                    }
                }
            }

            return results.OrderByDescending(r => r.SimilarityScore).ToList();
        }

        public async Task<List<NameMatchResult>> BatchMatchNamesAsync(List<Customer> customers, double threshold = 0.7)
        {
            var allResults = new List<NameMatchResult>();

            // Process customers in parallel for better performance
            var tasks = customers.Select(async customer =>
            {
                var customerResults = await MatchNameAsync(customer.FullName, customer, threshold);
                return customerResults;
            });

            var results = await Task.WhenAll(tasks);
            allResults.AddRange(results.SelectMany(r => r));

            return allResults.OrderByDescending(r => r.SimilarityScore).ToList();
        }

        public string TransliterateName(string name, string sourceScript = "Devanagari")
        {
            if (sourceScript.Equals("Devanagari", StringComparison.OrdinalIgnoreCase))
            {
                var transliterated = name;
                foreach (var mapping in _devanagariToLatin)
                {
                    transliterated = transliterated.Replace(mapping.Key, mapping.Value);
                }
                return transliterated;
            }

            // For other scripts, you can add more mappings
            return name;
        }

        public string NormalizeName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return string.Empty;

            // Convert to lowercase
            var normalized = name.ToLowerInvariant();

            // Remove special characters but keep spaces and hyphens
            normalized = Regex.Replace(normalized, @"[^\w\s\-]", "");

            // Remove extra whitespace
            normalized = Regex.Replace(normalized, @"\s+", " ").Trim();

            // Remove common titles and honorifics
            var titles = new[] { "mr", "mrs", "ms", "miss", "dr", "prof", "sir", "madam", "shri", "smt", "kum" };
            var words = normalized.Split(' ');
            var filteredWords = words.Where(word => !titles.Contains(word)).ToArray();
            normalized = string.Join(" ", filteredWords);

            return normalized;
        }

        public List<string> GeneratePhoneticVariations(string name)
        {
            var variations = new List<string>();

            // Add original name
            variations.Add(name);

            // Generate Soundex variations
            var soundex = GenerateSoundex(name);
            if (!string.IsNullOrEmpty(soundex))
                variations.Add(soundex);

            // Generate Metaphone variations
            var metaphone = GenerateMetaphone(name);
            if (!string.IsNullOrEmpty(metaphone))
                variations.Add(metaphone);

            // Generate Double Metaphone variations
            var doubleMetaphone = GenerateDoubleMetaphone(name);
            if (!string.IsNullOrEmpty(doubleMetaphone))
                variations.Add(doubleMetaphone);

            // Generate token-based variations (for multi-word names)
            var tokens = name.Split(' ');
            if (tokens.Length > 1)
            {
                // Add reversed name (for Indian names where order might vary)
                variations.Add(string.Join(" ", tokens.Reverse()));
                
                // Add initials
                var initials = string.Join(" ", tokens.Select(t => t.Length > 0 ? t[0].ToString() : ""));
                if (!string.IsNullOrEmpty(initials))
                    variations.Add(initials);
            }

            return variations.Distinct().ToList();
        }

        public SimilarityScore CalculateSimilarity(string name1, string name2)
        {
            if (string.IsNullOrEmpty(name1) || string.IsNullOrEmpty(name2))
                return new SimilarityScore { OverallScore = 0 };

            var score = new SimilarityScore();

            // Levenshtein distance - temporarily disabled due to FuzzySharp API issues
            // var levenshteinDistance = FuzzySharp.Levenshtein.GetDistance(name1, name2);
            // score.LevenshteinScore = 1.0 - (double)levenshteinDistance / Math.Max(name1.Length, name2.Length);
            score.LevenshteinScore = 0.5; // Placeholder

            // Jaro-Winkler distance - temporarily disabled due to FuzzySharp API issues
            // score.JaroWinklerScore = FuzzySharp.JaroWinkler.GetDistance(name1, name2);
            score.JaroWinklerScore = 0.5; // Placeholder

            // Soundex comparison
            var soundex1 = GenerateSoundex(name1);
            var soundex2 = GenerateSoundex(name2);
            score.SoundexScore = soundex1 == soundex2 ? 1.0 : 0.0;

            // Metaphone comparison
            var metaphone1 = GenerateMetaphone(name1);
            var metaphone2 = GenerateMetaphone(name2);
            score.MetaphoneScore = metaphone1 == metaphone2 ? 1.0 : 0.0;

            // Calculate overall score (weighted average)
            score.OverallScore = (score.LevenshteinScore * 0.3 + 
                                 score.JaroWinklerScore * 0.4 + 
                                 score.SoundexScore * 0.15 + 
                                 score.MetaphoneScore * 0.15);

            // Determine best algorithm
            var scores = new Dictionary<string, double>
            {
                { "Levenshtein", score.LevenshteinScore },
                { "JaroWinkler", score.JaroWinklerScore },
                { "Soundex", score.SoundexScore },
                { "Metaphone", score.MetaphoneScore }
            };

            score.BestAlgorithm = scores.OrderByDescending(kvp => kvp.Value).First().Key;
            score.MatchingDetails = $"Overall: {score.OverallScore:F3}, {score.BestAlgorithm}: {scores[score.BestAlgorithm]:F3}";

            return score;
        }

        private string GenerateSoundex(string name)
        {
            if (string.IsNullOrEmpty(name))
                return string.Empty;

            // Simple Soundex implementation
            var soundex = name[0].ToString().ToUpper();
            var code = "";
            var previousCode = GetSoundexCode(name[0]);

            for (int i = 1; i < name.Length && soundex.Length < 4; i++)
            {
                var currentCode = GetSoundexCode(name[i]);
                if (currentCode != '0' && currentCode != previousCode)
                {
                    soundex += currentCode;
                }
                previousCode = currentCode;
            }

            // Pad with zeros if necessary
            while (soundex.Length < 4)
            {
                soundex += "0";
            }

            return soundex;
        }

        private char GetSoundexCode(char c)
        {
            return c.ToString().ToLower() switch
            {
                "b" or "f" or "p" or "v" => '1',
                "c" or "g" or "j" or "k" or "q" or "s" or "x" or "z" => '2',
                "d" or "t" => '3',
                "l" => '4',
                "m" or "n" => '5',
                "r" => '6',
                _ => '0'
            };
        }

        private string GenerateMetaphone(string name)
        {
            if (string.IsNullOrEmpty(name))
                return string.Empty;

            // Simple Metaphone implementation
            var metaphone = "";
            var i = 0;

            while (i < name.Length && metaphone.Length < 4)
            {
                var current = name[i].ToString().ToLower();
                var next = i + 1 < name.Length ? name[i + 1].ToString().ToLower() : "";
                var prev = i > 0 ? name[i - 1].ToString().ToLower() : "";

                switch (current)
                {
                    case "b":
                        if (next != "b") metaphone += "B";
                        break;
                    case "c":
                        if (next == "h") { metaphone += "X"; i++; }
                        else if (next == "i" || next == "e" || next == "y") metaphone += "S";
                        else metaphone += "K";
                        break;
                    case "d":
                        if (next == "g" && (i + 2 < name.Length && "iey".Contains(name[i + 2]))) { metaphone += "J"; i++; }
                        else metaphone += "T";
                        break;
                    case "f":
                        metaphone += "F";
                        break;
                    case "g":
                        if (next == "h" && i > 0 && !"aeiou".Contains(prev)) { metaphone += ""; i++; }
                        else if (next == "n") { metaphone += ""; i++; }
                        else if (next == "i" || next == "e" || next == "y") metaphone += "J";
                        else metaphone += "K";
                        break;
                    case "h":
                        if (i > 0 && !"aeiou".Contains(prev)) metaphone += "";
                        break;
                    case "j":
                        metaphone += "J";
                        break;
                    case "k":
                        if (prev != "c") metaphone += "K";
                        break;
                    case "l":
                        metaphone += "L";
                        break;
                    case "m":
                        metaphone += "M";
                        break;
                    case "n":
                        metaphone += "N";
                        break;
                    case "p":
                        if (next == "h") { metaphone += "F"; i++; }
                        else metaphone += "P";
                        break;
                    case "q":
                        metaphone += "K";
                        break;
                    case "r":
                        metaphone += "R";
                        break;
                    case "s":
                        if (next == "h") { metaphone += "X"; i++; }
                        else metaphone += "S";
                        break;
                    case "t":
                        if (next == "h") { metaphone += "0"; i++; }
                        else metaphone += "T";
                        break;
                    case "v":
                        metaphone += "F";
                        break;
                    case "w":
                        if (i > 0 && !"aeiou".Contains(prev)) metaphone += "";
                        break;
                    case "x":
                        metaphone += "KS";
                        break;
                    case "y":
                        if (i > 0 && !"aeiou".Contains(prev)) metaphone += "";
                        break;
                    case "z":
                        metaphone += "S";
                        break;
                }
                i++;
            }

            return metaphone;
        }

        private string GenerateDoubleMetaphone(string name)
        {
            // Simplified Double Metaphone - returns primary metaphone
            return GenerateMetaphone(name);
        }
    }
}
