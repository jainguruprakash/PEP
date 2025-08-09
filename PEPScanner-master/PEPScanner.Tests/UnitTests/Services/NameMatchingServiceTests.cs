using AutoFixture;
using AutoFixture.Xunit2;
using Microsoft.Extensions.Logging;
using PEPScanner.Application.Abstractions;
using PEPScanner.Infrastructure.Services;

namespace PEPScanner.Tests.UnitTests.Services;

public class NameMatchingServiceTests
{
    private readonly Mock<ILogger<NameMatchingService>> _mockLogger;
    private readonly NameMatchingService _service;
    private readonly Fixture _fixture;

    public NameMatchingServiceTests()
    {
        _mockLogger = new Mock<ILogger<NameMatchingService>>();
        _service = new NameMatchingService(_mockLogger.Object);
        _fixture = new Fixture();
    }

    [Theory]
    [InlineData("John Doe", "John Doe", 1.0)]
    [InlineData("John Doe", "JOHN DOE", 1.0)]
    [InlineData("John Doe", "john doe", 1.0)]
    [InlineData("John Doe", "Jon Doe", 0.8)]
    [InlineData("John Doe", "Jane Smith", 0.0)]
    [InlineData("", "", 1.0)]
    [InlineData("John", "J", 0.2)]
    public void CalculateSimilarity_ShouldReturnExpectedScore(string name1, string name2, double expectedMinScore)
    {
        // Act
        var result = _service.CalculateSimilarity(name1, name2);

        // Assert
        result.Should().BeGreaterOrEqualTo(expectedMinScore - 0.1);
        result.Should().BeLessOrEqualTo(1.0);
        result.Should().BeGreaterOrEqualTo(0.0);
    }

    [Theory]
    [InlineData("John Doe")]
    [InlineData("JOHN DOE")]
    [InlineData("john doe")]
    [InlineData("John-Doe")]
    [InlineData("John.Doe")]
    public void NormalizeName_ShouldReturnConsistentFormat(string input)
    {
        // Act
        var result = _service.NormalizeName(input);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().BeUpperCased();
        result.Should().NotContain(".");
        result.Should().NotContain("-");
    }

    [Fact]
    public void NormalizeName_WithNullInput_ShouldReturnEmptyString()
    {
        // Act
        var result = _service.NormalizeName(null);

        // Assert
        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\n")]
    public void NormalizeName_WithEmptyOrWhitespaceInput_ShouldReturnEmptyString(string input)
    {
        // Act
        var result = _service.NormalizeName(input);

        // Assert
        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData("John", "J")]
    [InlineData("Smith", "S")]
    [InlineData("McDonald", "M")]
    [InlineData("O'Connor", "O")]
    public void GeneratePhoneticVariations_ShouldIncludeFirstLetter(string input, string expectedFirstLetter)
    {
        // Act
        var result = _service.GeneratePhoneticVariations(input);

        // Assert
        result.Should().NotBeEmpty();
        result.Should().Contain(variation => variation.StartsWith(expectedFirstLetter));
    }

    [Fact]
    public void GeneratePhoneticVariations_WithEmptyInput_ShouldReturnEmptyList()
    {
        // Act
        var result = _service.GeneratePhoneticVariations("");

        // Assert
        result.Should().BeEmpty();
    }

    [Theory]
    [AutoData]
    public void PerformFuzzyMatching_WithValidInputs_ShouldReturnResults(
        string searchName,
        List<string> candidateNames)
    {
        // Arrange
        var threshold = 0.5;

        // Act
        var result = _service.PerformFuzzyMatching(searchName, candidateNames, threshold);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeAssignableTo<IEnumerable<NameMatchResult>>();
        result.All(r => r.Score >= threshold).Should().BeTrue();
    }

    [Fact]
    public void PerformFuzzyMatching_WithHighThreshold_ShouldReturnFewerResults()
    {
        // Arrange
        var searchName = "John Doe";
        var candidateNames = new List<string> { "John Doe", "Jon Doe", "Jane Smith", "John Smith" };
        var lowThreshold = 0.3;
        var highThreshold = 0.8;

        // Act
        var lowThresholdResults = _service.PerformFuzzyMatching(searchName, candidateNames, lowThreshold);
        var highThresholdResults = _service.PerformFuzzyMatching(searchName, candidateNames, highThreshold);

        // Assert
        lowThresholdResults.Count().Should().BeGreaterOrEqualTo(highThresholdResults.Count());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void PerformFuzzyMatching_WithInvalidSearchName_ShouldReturnEmptyResults(string searchName)
    {
        // Arrange
        var candidateNames = new List<string> { "John Doe", "Jane Smith" };
        var threshold = 0.5;

        // Act
        var result = _service.PerformFuzzyMatching(searchName, candidateNames, threshold);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void PerformFuzzyMatching_WithNullCandidateNames_ShouldReturnEmptyResults()
    {
        // Arrange
        var searchName = "John Doe";
        List<string> candidateNames = null;
        var threshold = 0.5;

        // Act
        var result = _service.PerformFuzzyMatching(searchName, candidateNames, threshold);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void PerformFuzzyMatching_WithEmptyCandidateNames_ShouldReturnEmptyResults()
    {
        // Arrange
        var searchName = "John Doe";
        var candidateNames = new List<string>();
        var threshold = 0.5;

        // Act
        var result = _service.PerformFuzzyMatching(searchName, candidateNames, threshold);

        // Assert
        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.1)]
    [InlineData(2.0)]
    public void PerformFuzzyMatching_WithInvalidThreshold_ShouldHandleGracefully(double threshold)
    {
        // Arrange
        var searchName = "John Doe";
        var candidateNames = new List<string> { "John Doe", "Jane Smith" };

        // Act & Assert
        var act = () => _service.PerformFuzzyMatching(searchName, candidateNames, threshold);
        act.Should().NotThrow();
    }

    [Fact]
    public void CalculateSimilarity_WithSpecialCharacters_ShouldHandleCorrectly()
    {
        // Arrange
        var name1 = "O'Connor-Smith";
        var name2 = "OConnor Smith";

        // Act
        var result = _service.CalculateSimilarity(name1, name2);

        // Assert
        result.Should().BeGreaterThan(0.7);
    }

    [Fact]
    public void CalculateSimilarity_WithUnicodeCharacters_ShouldHandleCorrectly()
    {
        // Arrange
        var name1 = "José María";
        var name2 = "Jose Maria";

        // Act
        var result = _service.CalculateSimilarity(name1, name2);

        // Assert
        result.Should().BeGreaterThan(0.8);
    }

    [Theory]
    [InlineData("John Doe Jr.", "John Doe")]
    [InlineData("Dr. John Doe", "John Doe")]
    [InlineData("John Doe III", "John Doe")]
    public void CalculateSimilarity_WithTitlesAndSuffixes_ShouldMatchWell(string name1, string name2)
    {
        // Act
        var result = _service.CalculateSimilarity(name1, name2);

        // Assert
        result.Should().BeGreaterThan(0.7);
    }

    [Fact]
    public void PerformFuzzyMatching_ShouldOrderResultsByScore()
    {
        // Arrange
        var searchName = "John Doe";
        var candidateNames = new List<string> 
        { 
            "Jane Smith",    // Low similarity
            "Jon Doe",       // Medium similarity
            "John Doe",      // High similarity
            "John David"     // Medium similarity
        };
        var threshold = 0.1;

        // Act
        var result = _service.PerformFuzzyMatching(searchName, candidateNames, threshold).ToList();

        // Assert
        result.Should().BeInDescendingOrder(r => r.Score);
        result.First().MatchedName.Should().Be("John Doe");
    }
}
