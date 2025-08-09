using AutoFixture;
using AutoFixture.Xunit2;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using PEPScanner.Application.Abstractions;
using PEPScanner.Infrastructure.Services;

namespace PEPScanner.Tests.UnitTests.Services;

public class UnSanctionsServiceTests
{
    private readonly Mock<ILogger<UnSanctionsService>> _mockLogger;
    private readonly Mock<HttpMessageHandler> _mockHttpHandler;
    private readonly HttpClient _httpClient;
    private readonly UnSanctionsService _service;
    private readonly Fixture _fixture;

    public UnSanctionsServiceTests()
    {
        _mockLogger = new Mock<ILogger<UnSanctionsService>>();
        _mockHttpHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpHandler.Object);
        _service = new UnSanctionsService(_httpClient, _mockLogger.Object);
        _fixture = new Fixture();
    }

    [Fact]
    public async Task FetchUnSanctionsDataAsync_WithValidResponse_ShouldReturnData()
    {
        // Arrange
        var jsonContent = JsonSerializer.Serialize(new
        {
            results = new[]
            {
                new { first_name = "John", last_name = "Doe", un_list_type = "Al-Qaida", reference_number = "QDi.001" }
            }
        });

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonContent)
        };

        _mockHttpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        // Act
        var result = await _service.FetchUnSanctionsDataAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task FetchUnSanctionsDataAsync_WithHttpError_ShouldThrowException()
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
        var act = async () => await _service.FetchUnSanctionsDataAsync();
        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task FetchUnSanctionsDataAsync_WithInvalidJson_ShouldHandleGracefully()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("invalid json content")
        };

        _mockHttpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        // Act & Assert
        var act = async () => await _service.FetchUnSanctionsDataAsync();
        await act.Should().ThrowAsync<JsonException>();
    }

    [Fact]
    public void ParseUnSanctionsJson_WithValidJson_ShouldReturnParsedData()
    {
        // Arrange
        var jsonContent = JsonSerializer.Serialize(new
        {
            results = new[]
            {
                new 
                { 
                    first_name = "John", 
                    last_name = "Doe", 
                    un_list_type = "Al-Qaida", 
                    reference_number = "QDi.001",
                    date_of_birth = "1980-01-01"
                },
                new 
                { 
                    first_name = "Jane", 
                    last_name = "Smith", 
                    un_list_type = "Taliban", 
                    reference_number = "TAi.002"
                }
            }
        });

        // Act
        var result = _service.ParseUnSanctionsJson(jsonContent);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.First().FirstName.Should().Be("John");
        result.First().LastName.Should().Be("Doe");
        result.First().UnListType.Should().Be("Al-Qaida");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ParseUnSanctionsJson_WithInvalidInput_ShouldReturnEmptyList(string jsonContent)
    {
        // Act
        var result = _service.ParseUnSanctionsJson(jsonContent);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public void ParseUnSanctionsJson_WithMalformedJson_ShouldThrowException()
    {
        // Arrange
        var malformedJson = "{ invalid json structure";

        // Act & Assert
        var act = () => _service.ParseUnSanctionsJson(malformedJson);
        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void ParseUnSanctionsJson_WithMissingResultsProperty_ShouldReturnEmptyList()
    {
        // Arrange
        var jsonContent = JsonSerializer.Serialize(new { data = "some other structure" });

        // Act
        var result = _service.ParseUnSanctionsJson(jsonContent);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Theory]
    [AutoData]
    public void SearchUnSanctionsData_WithValidCriteria_ShouldReturnResults(string searchTerm)
    {
        // Arrange
        var unData = new List<UnSanctionsRecord>
        {
            new() { FirstName = "John", LastName = "Doe", ReferenceNumber = "QDi.001" },
            new() { FirstName = "Jane", LastName = "Smith", ReferenceNumber = "TAi.002" },
            new() { FirstName = "Bob", LastName = "Johnson", ReferenceNumber = "QDi.003" }
        };

        // Act
        var result = _service.SearchUnSanctionsData(unData, searchTerm);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeAssignableTo<IEnumerable<UnSanctionsRecord>>();
    }

    [Fact]
    public void SearchUnSanctionsData_WithExactMatch_ShouldReturnExactResult()
    {
        // Arrange
        var unData = new List<UnSanctionsRecord>
        {
            new() { FirstName = "John", LastName = "Doe", ReferenceNumber = "QDi.001" },
            new() { FirstName = "Jane", LastName = "Smith", ReferenceNumber = "TAi.002" }
        };
        var searchTerm = "John Doe";

        // Act
        var result = _service.SearchUnSanctionsData(unData, searchTerm);

        // Assert
        result.Should().NotBeNull();
        result.Should().Contain(r => r.FirstName == "John" && r.LastName == "Doe");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void SearchUnSanctionsData_WithInvalidSearchTerm_ShouldReturnEmptyResults(string searchTerm)
    {
        // Arrange
        var unData = new List<UnSanctionsRecord>
        {
            new() { FirstName = "John", LastName = "Doe", ReferenceNumber = "QDi.001" }
        };

        // Act
        var result = _service.SearchUnSanctionsData(unData, searchTerm);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public void SearchUnSanctionsData_WithNullData_ShouldReturnEmptyResults()
    {
        // Arrange
        List<UnSanctionsRecord> unData = null;
        var searchTerm = "John Doe";

        // Act
        var result = _service.SearchUnSanctionsData(unData, searchTerm);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public void SearchUnSanctionsData_CaseInsensitive_ShouldReturnResults()
    {
        // Arrange
        var unData = new List<UnSanctionsRecord>
        {
            new() { FirstName = "John", LastName = "Doe", ReferenceNumber = "QDi.001" }
        };
        var searchTerm = "john doe";

        // Act
        var result = _service.SearchUnSanctionsData(unData, searchTerm);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public void SearchUnSanctionsData_ByReferenceNumber_ShouldReturnResults()
    {
        // Arrange
        var unData = new List<UnSanctionsRecord>
        {
            new() { FirstName = "John", LastName = "Doe", ReferenceNumber = "QDi.001" },
            new() { FirstName = "Jane", LastName = "Smith", ReferenceNumber = "TAi.002" }
        };
        var searchTerm = "QDi.001";

        // Act
        var result = _service.SearchUnSanctionsData(unData, searchTerm);

        // Assert
        result.Should().NotBeNull();
        result.Should().Contain(r => r.ReferenceNumber == "QDi.001");
    }

    [Fact]
    public void SearchUnSanctionsData_ByListType_ShouldReturnResults()
    {
        // Arrange
        var unData = new List<UnSanctionsRecord>
        {
            new() { FirstName = "John", LastName = "Doe", UnListType = "Al-Qaida", ReferenceNumber = "QDi.001" },
            new() { FirstName = "Jane", LastName = "Smith", UnListType = "Taliban", ReferenceNumber = "TAi.002" }
        };
        var searchTerm = "Al-Qaida";

        // Act
        var result = _service.SearchUnSanctionsData(unData, searchTerm);

        // Assert
        result.Should().NotBeNull();
        result.Should().Contain(r => r.UnListType == "Al-Qaida");
    }

    [Fact]
    public async Task UpdateUnSanctionsDataAsync_ShouldFetchAndProcessData()
    {
        // Arrange
        var jsonContent = JsonSerializer.Serialize(new
        {
            results = new[]
            {
                new { first_name = "John", last_name = "Doe", un_list_type = "Al-Qaida", reference_number = "QDi.001" }
            }
        });

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonContent)
        };

        _mockHttpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        // Act
        var act = async () => await _service.UpdateUnSanctionsDataAsync();

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public void ValidateUnSanctionsRecord_WithValidRecord_ShouldReturnTrue()
    {
        // Arrange
        var record = new UnSanctionsRecord
        {
            ReferenceNumber = "QDi.001",
            FirstName = "John",
            LastName = "Doe",
            UnListType = "Al-Qaida"
        };

        // Act
        var result = _service.ValidateUnSanctionsRecord(record);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "John", "Doe")]
    [InlineData("QDi.001", "", "Doe")]
    [InlineData("QDi.001", "John", "")]
    [InlineData(null, "John", "Doe")]
    public void ValidateUnSanctionsRecord_WithInvalidRecord_ShouldReturnFalse(string referenceNumber, string firstName, string lastName)
    {
        // Arrange
        var record = new UnSanctionsRecord
        {
            ReferenceNumber = referenceNumber,
            FirstName = firstName,
            LastName = lastName
        };

        // Act
        var result = _service.ValidateUnSanctionsRecord(record);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ValidateUnSanctionsRecord_WithNullRecord_ShouldReturnFalse()
    {
        // Act
        var result = _service.ValidateUnSanctionsRecord(null);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ParseUnSanctionsJson_WithSpecialCharacters_ShouldHandleCorrectly()
    {
        // Arrange
        var jsonContent = JsonSerializer.Serialize(new
        {
            results = new[]
            {
                new 
                { 
                    first_name = "José María", 
                    last_name = "O'Connor-Smith", 
                    un_list_type = "Al-Qaida", 
                    reference_number = "QDi.001"
                }
            }
        });

        // Act
        var result = _service.ParseUnSanctionsJson(jsonContent);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.First().FirstName.Should().Be("José María");
        result.First().LastName.Should().Be("O'Connor-Smith");
    }

    [Fact]
    public void ParseUnSanctionsJson_WithNullFields_ShouldHandleGracefully()
    {
        // Arrange
        var jsonContent = JsonSerializer.Serialize(new
        {
            results = new[]
            {
                new 
                { 
                    first_name = (string)null, 
                    last_name = "Doe", 
                    un_list_type = "Al-Qaida", 
                    reference_number = "QDi.001"
                }
            }
        });

        // Act
        var result = _service.ParseUnSanctionsJson(jsonContent);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.First().FirstName.Should().BeNullOrEmpty();
        result.First().LastName.Should().Be("Doe");
    }

    [Fact]
    public async Task FetchUnSanctionsDataAsync_WithTimeout_ShouldThrowException()
    {
        // Arrange
        _mockHttpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException("Request timeout"));

        // Act & Assert
        var act = async () => await _service.FetchUnSanctionsDataAsync();
        await act.Should().ThrowAsync<TaskCanceledException>();
    }
}
