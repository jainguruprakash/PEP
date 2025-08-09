using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text;
using System.Text.Json;
using PEPScanner.API;
using PEPScanner.Application.Abstractions;

namespace PEPScanner.Tests.IntegrationTests.Controllers;

public class WatchlistControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public WatchlistControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Add InMemory database for testing
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb_Watchlist");
                });

                // Override services with mocks if needed
                services.AddScoped<IOfacDataService, MockOfacDataService>();
                services.AddScoped<IUnSanctionsService, MockUnSanctionsService>();
            });
        });

        _client = _factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    [Fact]
    public async Task UpdateOfac_ShouldReturnOk()
    {
        // Act
        var response = await _client.PostAsync("/api/watchlist/ofac/update", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
        result.TryGetProperty("success", out _).Should().BeTrue();
    }

    [Fact]
    public async Task SearchOfac_WithValidName_ShouldReturnResults()
    {
        // Arrange
        var searchName = "John Doe";

        // Act
        var response = await _client.GetAsync($"/api/watchlist/ofac/search?name={Uri.EscapeDataString(searchName)}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
        result.TryGetProperty("results", out _).Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SearchOfac_WithInvalidName_ShouldReturnBadRequest(string searchName)
    {
        // Act
        var response = await _client.GetAsync($"/api/watchlist/ofac/search?name={Uri.EscapeDataString(searchName)}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SearchOfac_WithoutNameParameter_ShouldReturnBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/watchlist/ofac/search");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateUn_ShouldReturnOk()
    {
        // Act
        var response = await _client.PostAsync("/api/watchlist/un/update", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
        result.TryGetProperty("success", out _).Should().BeTrue();
    }

    [Fact]
    public async Task SearchUn_WithValidName_ShouldReturnResults()
    {
        // Arrange
        var searchName = "Terrorist Name";

        // Act
        var response = await _client.GetAsync($"/api/watchlist/un/search?name={Uri.EscapeDataString(searchName)}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
        result.TryGetProperty("results", out _).Should().BeTrue();
    }

    [Fact]
    public async Task SearchUn_WithSpecialCharacters_ShouldHandleCorrectly()
    {
        // Arrange
        var searchName = "José María O'Connor";

        // Act
        var response = await _client.GetAsync($"/api/watchlist/un/search?name={Uri.EscapeDataString(searchName)}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetLastUpdates_ShouldReturnUpdateInformation()
    {
        // Act
        var response = await _client.GetAsync("/api/watchlist/last-updates");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
        result.ValueKind.Should().Be(JsonValueKind.Object);
    }

    [Fact]
    public async Task UpdateAll_ShouldReturnOk()
    {
        // Act
        var response = await _client.PostAsync("/api/watchlist/update-all", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SearchOfac_ConcurrentRequests_ShouldHandleCorrectly()
    {
        // Arrange
        var tasks = new List<Task<HttpResponseMessage>>();
        var searchName = "John Doe";

        // Act
        for (int i = 0; i < 5; i++)
        {
            tasks.Add(_client.GetAsync($"/api/watchlist/ofac/search?name={Uri.EscapeDataString(searchName)}"));
        }

        var responses = await Task.WhenAll(tasks);

        // Assert
        responses.Should().AllSatisfy(response => 
            response.StatusCode.Should().Be(HttpStatusCode.OK));
    }

    [Fact]
    public async Task SearchOfac_WithLongName_ShouldHandleCorrectly()
    {
        // Arrange
        var longName = new string('A', 500); // Very long name

        // Act
        var response = await _client.GetAsync($"/api/watchlist/ofac/search?name={Uri.EscapeDataString(longName)}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("John Doe")]
    [InlineData("JOHN DOE")]
    [InlineData("john doe")]
    public async Task SearchOfac_CaseInsensitive_ShouldReturnResults(string searchName)
    {
        // Act
        var response = await _client.GetAsync($"/api/watchlist/ofac/search?name={Uri.EscapeDataString(searchName)}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}

// Mock services for testing
public class MockOfacDataService : IOfacDataService
{
    public Task<List<OfacRecord>> FetchOfacDataAsync()
    {
        var data = new List<OfacRecord>
        {
            new() { Uid = "12345", FirstName = "John", LastName = "Doe", EntityType = "Individual" },
            new() { Uid = "67890", FirstName = "Jane", LastName = "Smith", EntityType = "Individual" }
        };
        return Task.FromResult(data);
    }

    public List<OfacRecord> ParseOfacCsv(string csvContent)
    {
        return new List<OfacRecord>
        {
            new() { Uid = "12345", FirstName = "John", LastName = "Doe", EntityType = "Individual" }
        };
    }

    public IEnumerable<OfacRecord> SearchOfacData(IEnumerable<OfacRecord> data, string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm) || data == null)
            return Enumerable.Empty<OfacRecord>();

        return data.Where(r => 
            r.FirstName?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true ||
            r.LastName?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true);
    }

    public Task UpdateOfacDataAsync()
    {
        return Task.CompletedTask;
    }

    public bool ValidateOfacRecord(OfacRecord record)
    {
        return record != null && 
               !string.IsNullOrEmpty(record.Uid) && 
               !string.IsNullOrEmpty(record.FirstName) && 
               !string.IsNullOrEmpty(record.LastName);
    }
}

public class MockUnSanctionsService : IUnSanctionsService
{
    public Task<List<UnSanctionsRecord>> FetchUnSanctionsDataAsync()
    {
        var data = new List<UnSanctionsRecord>
        {
            new() { ReferenceNumber = "QDi.001", FirstName = "Terrorist", LastName = "Name", UnListType = "Al-Qaida" },
            new() { ReferenceNumber = "TAi.002", FirstName = "Another", LastName = "Person", UnListType = "Taliban" }
        };
        return Task.FromResult(data);
    }

    public List<UnSanctionsRecord> ParseUnSanctionsJson(string jsonContent)
    {
        return new List<UnSanctionsRecord>
        {
            new() { ReferenceNumber = "QDi.001", FirstName = "Terrorist", LastName = "Name", UnListType = "Al-Qaida" }
        };
    }

    public IEnumerable<UnSanctionsRecord> SearchUnSanctionsData(IEnumerable<UnSanctionsRecord> data, string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm) || data == null)
            return Enumerable.Empty<UnSanctionsRecord>();

        return data.Where(r => 
            r.FirstName?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true ||
            r.LastName?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true ||
            r.ReferenceNumber?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true);
    }

    public Task UpdateUnSanctionsDataAsync()
    {
        return Task.CompletedTask;
    }

    public bool ValidateUnSanctionsRecord(UnSanctionsRecord record)
    {
        return record != null && 
               !string.IsNullOrEmpty(record.ReferenceNumber) && 
               !string.IsNullOrEmpty(record.FirstName) && 
               !string.IsNullOrEmpty(record.LastName);
    }
}
