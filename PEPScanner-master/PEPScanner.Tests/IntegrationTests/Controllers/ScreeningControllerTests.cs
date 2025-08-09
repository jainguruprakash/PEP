using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text;
using System.Text.Json;
using PEPScanner.API;
using PEPScanner.Application.Abstractions;

namespace PEPScanner.Tests.IntegrationTests.Controllers;

public class ScreeningControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public ScreeningControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the existing DbContext registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add InMemory database for testing
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb");
                });

                // Override services with mocks if needed
                services.AddScoped<IScreeningService, MockScreeningService>();
            });
        });

        _client = _factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    [Fact]
    public async Task ScreenCustomer_WithValidRequest_ShouldReturnOk()
    {
        // Arrange
        var request = new
        {
            FullName = "John Doe",
            DateOfBirth = "1990-01-01",
            Nationality = "US",
            DocumentNumber = "123456789"
        };

        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/screening/customer", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().NotBeNullOrEmpty();
        
        var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
        result.TryGetProperty("customerId", out _).Should().BeTrue();
    }

    [Fact]
    public async Task ScreenCustomer_WithInvalidRequest_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new
        {
            FullName = "", // Invalid empty name
            DateOfBirth = "invalid-date",
            Nationality = "US"
        };

        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/screening/customer", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ScreenCustomer_WithMissingRequiredFields_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new
        {
            DateOfBirth = "1990-01-01"
            // Missing FullName
        };

        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/screening/customer", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ScreenTransaction_WithValidRequest_ShouldReturnOk()
    {
        // Arrange
        var request = new
        {
            TransactionId = "TXN123456",
            Amount = 10000.50m,
            Currency = "USD",
            BeneficiaryName = "Jane Smith",
            BeneficiaryAccount = "ACC789",
            Purpose = "Business Payment"
        };

        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/screening/transaction", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
        result.TryGetProperty("transactionId", out _).Should().BeTrue();
    }

    [Theory]
    [InlineData(-1000)] // Negative amount
    [InlineData(0)] // Zero amount
    public async Task ScreenTransaction_WithInvalidAmount_ShouldReturnBadRequest(decimal amount)
    {
        // Arrange
        var request = new
        {
            TransactionId = "TXN123456",
            Amount = amount,
            Currency = "USD",
            BeneficiaryName = "Jane Smith"
        };

        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/screening/transaction", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Search_WithValidQuery_ShouldReturnResults()
    {
        // Arrange
        var request = new
        {
            SearchTerm = "John Doe",
            SearchType = "name",
            MaxResults = 10
        };

        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/screening/search", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
        result.TryGetProperty("results", out _).Should().BeTrue();
        result.TryGetProperty("totalCount", out _).Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Search_WithInvalidSearchTerm_ShouldReturnBadRequest(string searchTerm)
    {
        // Arrange
        var request = new
        {
            SearchTerm = searchTerm,
            SearchType = "name"
        };

        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/screening/search", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetStatistics_WithValidDateRange_ShouldReturnStatistics()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(-30).ToString("yyyy-MM-dd");
        var endDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

        // Act
        var response = await _client.GetAsync($"/api/screening/statistics?startDate={startDate}&endDate={endDate}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
        result.TryGetProperty("alertCount", out _).Should().BeTrue();
        result.TryGetProperty("customersScreened", out _).Should().BeTrue();
        result.TryGetProperty("averageRisk", out _).Should().BeTrue();
    }

    [Theory]
    [InlineData("invalid-date", "2024-01-31")]
    [InlineData("2024-01-01", "invalid-date")]
    [InlineData("2024-02-01", "2024-01-01")] // End date before start date
    public async Task GetStatistics_WithInvalidDateRange_ShouldReturnBadRequest(string startDate, string endDate)
    {
        // Act
        var response = await _client.GetAsync($"/api/screening/statistics?startDate={startDate}&endDate={endDate}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetStatistics_WithMissingParameters_ShouldReturnBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/screening/statistics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ScreenCustomer_WithLargePayload_ShouldHandleCorrectly()
    {
        // Arrange
        var request = new
        {
            FullName = "John Doe",
            DateOfBirth = "1990-01-01",
            Nationality = "US",
            DocumentNumber = "123456789",
            Address = new string('A', 1000), // Large address field
            AdditionalInfo = new string('B', 2000) // Large additional info
        };

        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/screening/customer", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ScreenCustomer_ConcurrentRequests_ShouldHandleCorrectly()
    {
        // Arrange
        var tasks = new List<Task<HttpResponseMessage>>();
        var request = new
        {
            FullName = "John Doe",
            DateOfBirth = "1990-01-01",
            Nationality = "US"
        };

        var json = JsonSerializer.Serialize(request, _jsonOptions);

        // Act
        for (int i = 0; i < 10; i++)
        {
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            tasks.Add(_client.PostAsync("/api/screening/customer", content));
        }

        var responses = await Task.WhenAll(tasks);

        // Assert
        responses.Should().AllSatisfy(response => 
            response.StatusCode.Should().Be(HttpStatusCode.OK));
    }

    [Fact]
    public async Task ScreenCustomer_WithSpecialCharacters_ShouldHandleCorrectly()
    {
        // Arrange
        var request = new
        {
            FullName = "José María O'Connor-Smith",
            DateOfBirth = "1990-01-01",
            Nationality = "ES",
            DocumentNumber = "ABC-123/456"
        };

        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/screening/customer", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Search_WithPagination_ShouldReturnPagedResults()
    {
        // Arrange
        var request = new
        {
            SearchTerm = "John",
            SearchType = "name",
            PageNumber = 1,
            PageSize = 5
        };

        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/screening/search", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
        
        if (result.TryGetProperty("results", out var results) && results.ValueKind == JsonValueKind.Array)
        {
            results.GetArrayLength().Should().BeLessOrEqualTo(5);
        }
    }
}

// Mock service for testing
public class MockScreeningService : IScreeningService
{
    public Task<ScreeningResult> ScreenCustomerAsync(CustomerScreeningRequest request)
    {
        var result = new ScreeningResult
        {
            CustomerId = Guid.NewGuid().ToString(),
            HasMatches = false,
            RiskScore = 0.1,
            ProcessedAt = DateTime.UtcNow
        };
        return Task.FromResult(result);
    }

    public Task<ScreeningResult> ScreenTransactionAsync(TransactionScreeningRequest request)
    {
        var result = new ScreeningResult
        {
            TransactionId = request.TransactionId,
            HasMatches = false,
            RiskScore = 0.2,
            ProcessedAt = DateTime.UtcNow
        };
        return Task.FromResult(result);
    }

    public Task<SearchResult> SearchAsync(SearchRequest request)
    {
        var result = new SearchResult
        {
            Results = new List<SearchResultItem>
            {
                new() { Name = "John Doe", Source = "OFAC", RiskLevel = "Low" }
            },
            TotalCount = 1
        };
        return Task.FromResult(result);
    }

    public Task<StatisticsResult> GetStatisticsAsync(DateTime startDate, DateTime endDate)
    {
        var result = new StatisticsResult
        {
            AlertCount = 25,
            CustomersScreened = 150,
            AverageRisk = 0.3
        };
        return Task.FromResult(result);
    }
}
