using AutoFixture;
using AutoFixture.Xunit2;
using Microsoft.Extensions.Logging;
using System.Net;
using PEPScanner.Application.Abstractions;
using PEPScanner.Infrastructure.Services;

namespace PEPScanner.Tests.UnitTests.Services;

public class OfacDataServiceTests
{
    private readonly Mock<ILogger<OfacDataService>> _mockLogger;
    private readonly Mock<HttpMessageHandler> _mockHttpHandler;
    private readonly HttpClient _httpClient;
    private readonly OfacDataService _service;
    private readonly Fixture _fixture;

    public OfacDataServiceTests()
    {
        _mockLogger = new Mock<ILogger<OfacDataService>>();
        _mockHttpHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpHandler.Object);
        _service = new OfacDataService(_httpClient, _mockLogger.Object);
        _fixture = new Fixture();
    }

    [Fact]
    public async Task FetchOfacDataAsync_WithValidResponse_ShouldReturnData()
    {
        // Arrange
        var csvContent = "uid,first_name,last_name,title,entity_type\n12345,John,Doe,Mr,Individual";
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(csvContent)
        };

        _mockHttpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        // Act
        var result = await _service.FetchOfacDataAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task FetchOfacDataAsync_WithHttpError_ShouldThrowException()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);

        _mockHttpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        // Act & Assert
        var act = async () => await _service.FetchOfacDataAsync();
        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task FetchOfacDataAsync_WithNetworkTimeout_ShouldThrowException()
    {
        // Arrange
        _mockHttpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException("Request timeout"));

        // Act & Assert
        var act = async () => await _service.FetchOfacDataAsync();
        await act.Should().ThrowAsync<TaskCanceledException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("invalid,csv,format")]
    public async Task FetchOfacDataAsync_WithInvalidCsvContent_ShouldHandleGracefully(string csvContent)
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(csvContent)
        };

        _mockHttpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        // Act
        var result = await _service.FetchOfacDataAsync();

        // Assert
        result.Should().NotBeNull();
        // Should handle gracefully without throwing
    }

    [Fact]
    public void ParseOfacCsv_WithValidCsv_ShouldReturnParsedData()
    {
        // Arrange
        var csvContent = @"uid,first_name,last_name,title,entity_type,programs,remarks
12345,John,Doe,Mr,Individual,SDN,Terrorist
67890,Jane,Smith,Ms,Individual,NONSDN,Money Laundering";

        // Act
        var result = _service.ParseOfacCsv(csvContent);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCountGreaterThan(0);
        result.First().Should().NotBeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ParseOfacCsv_WithInvalidInput_ShouldReturnEmptyList(string csvContent)
    {
        // Act
        var result = _service.ParseOfacCsv(csvContent);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public void ParseOfacCsv_WithMalformedCsv_ShouldHandleGracefully()
    {
        // Arrange
        var malformedCsv = @"uid,first_name,last_name
12345,John
67890,Jane,Smith,Extra,Column";

        // Act
        var act = () => _service.ParseOfacCsv(malformedCsv);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void ParseOfacCsv_WithSpecialCharacters_ShouldHandleCorrectly()
    {
        // Arrange
        var csvContent = @"uid,first_name,last_name,title,entity_type
12345,José,María,Dr.,Individual
67890,O'Connor,Smith-Jones,Mr.,Individual";

        // Act
        var result = _service.ParseOfacCsv(csvContent);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(r => r.FirstName == "José");
        result.Should().Contain(r => r.LastName.Contains("O'Connor"));
    }

    [Fact]
    public void ParseOfacCsv_WithQuotedFields_ShouldHandleCorrectly()
    {
        // Arrange
        var csvContent = @"uid,first_name,last_name,remarks
12345,""John, Jr."",""Doe"",""Known as ""Johnny""""";

        // Act
        var result = _service.ParseOfacCsv(csvContent);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.First().FirstName.Should().Be("John, Jr.");
    }

    [Theory]
    [AutoData]
    public void SearchOfacData_WithValidCriteria_ShouldReturnResults(string searchTerm)
    {
        // Arrange
        var ofacData = new List<OfacRecord>
        {
            new() { FirstName = "John", LastName = "Doe", Uid = "12345" },
            new() { FirstName = "Jane", LastName = "Smith", Uid = "67890" },
            new() { FirstName = "Bob", LastName = "Johnson", Uid = "11111" }
        };

        // Act
        var result = _service.SearchOfacData(ofacData, searchTerm);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeAssignableTo<IEnumerable<OfacRecord>>();
    }

    [Fact]
    public void SearchOfacData_WithExactMatch_ShouldReturnExactResult()
    {
        // Arrange
        var ofacData = new List<OfacRecord>
        {
            new() { FirstName = "John", LastName = "Doe", Uid = "12345" },
            new() { FirstName = "Jane", LastName = "Smith", Uid = "67890" }
        };
        var searchTerm = "John Doe";

        // Act
        var result = _service.SearchOfacData(ofacData, searchTerm);

        // Assert
        result.Should().NotBeNull();
        result.Should().Contain(r => r.FirstName == "John" && r.LastName == "Doe");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void SearchOfacData_WithInvalidSearchTerm_ShouldReturnEmptyResults(string searchTerm)
    {
        // Arrange
        var ofacData = new List<OfacRecord>
        {
            new() { FirstName = "John", LastName = "Doe", Uid = "12345" }
        };

        // Act
        var result = _service.SearchOfacData(ofacData, searchTerm);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public void SearchOfacData_WithNullData_ShouldReturnEmptyResults()
    {
        // Arrange
        List<OfacRecord> ofacData = null;
        var searchTerm = "John Doe";

        // Act
        var result = _service.SearchOfacData(ofacData, searchTerm);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public void SearchOfacData_WithEmptyData_ShouldReturnEmptyResults()
    {
        // Arrange
        var ofacData = new List<OfacRecord>();
        var searchTerm = "John Doe";

        // Act
        var result = _service.SearchOfacData(ofacData, searchTerm);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public void SearchOfacData_WithPartialMatch_ShouldReturnRelevantResults()
    {
        // Arrange
        var ofacData = new List<OfacRecord>
        {
            new() { FirstName = "John", LastName = "Doe", Uid = "12345" },
            new() { FirstName = "Johnny", LastName = "Doe", Uid = "67890" },
            new() { FirstName = "Jane", LastName = "Smith", Uid = "11111" }
        };
        var searchTerm = "John";

        // Act
        var result = _service.SearchOfacData(ofacData, searchTerm).ToList();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCountGreaterThan(0);
        result.Should().Contain(r => r.FirstName.Contains("John"));
    }

    [Fact]
    public void SearchOfacData_CaseInsensitive_ShouldReturnResults()
    {
        // Arrange
        var ofacData = new List<OfacRecord>
        {
            new() { FirstName = "John", LastName = "Doe", Uid = "12345" }
        };
        var searchTerm = "john doe";

        // Act
        var result = _service.SearchOfacData(ofacData, searchTerm);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task UpdateOfacDataAsync_ShouldFetchAndProcessData()
    {
        // Arrange
        var csvContent = "uid,first_name,last_name,title,entity_type\n12345,John,Doe,Mr,Individual";
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(csvContent)
        };

        _mockHttpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        // Act
        var act = async () => await _service.UpdateOfacDataAsync();

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public void ValidateOfacRecord_WithValidRecord_ShouldReturnTrue()
    {
        // Arrange
        var record = new OfacRecord
        {
            Uid = "12345",
            FirstName = "John",
            LastName = "Doe",
            EntityType = "Individual"
        };

        // Act
        var result = _service.ValidateOfacRecord(record);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "John", "Doe")]
    [InlineData("12345", "", "Doe")]
    [InlineData("12345", "John", "")]
    [InlineData(null, "John", "Doe")]
    public void ValidateOfacRecord_WithInvalidRecord_ShouldReturnFalse(string uid, string firstName, string lastName)
    {
        // Arrange
        var record = new OfacRecord
        {
            Uid = uid,
            FirstName = firstName,
            LastName = lastName
        };

        // Act
        var result = _service.ValidateOfacRecord(record);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ValidateOfacRecord_WithNullRecord_ShouldReturnFalse()
    {
        // Act
        var result = _service.ValidateOfacRecord(null);

        // Assert
        result.Should().BeFalse();
    }
}
