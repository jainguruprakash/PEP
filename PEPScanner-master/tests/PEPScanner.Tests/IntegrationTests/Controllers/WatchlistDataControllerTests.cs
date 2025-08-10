using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using PEPScanner.Infrastructure.Data;
using System.Net.Http;
using System.Text.Json;
using FluentAssertions;

namespace PEPScanner.Tests.IntegrationTests.Controllers;

public class WatchlistDataControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public WatchlistDataControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the real database context
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<PepScannerDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                // Add in-memory database for testing
                services.AddDbContext<PepScannerDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb");
                });
            });
        });

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task FetchRbiData_ShouldReturnSuccess_WhenCalledOnDemand()
    {
        // Act
        var response = await _client.PostAsync("/api/watchlistdata/fetch/rbi", null);

        // Assert
        response.Should().BeSuccessful();
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content);
        
        result.GetProperty("Success").GetBoolean().Should().BeTrue();
        result.GetProperty("Source").GetString().Should().Be("RBI");
        result.GetProperty("TotalRecords").GetInt32().Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task FetchRbiData_ShouldFilterCurrentYearData_WhenRequested()
    {
        // Arrange
        var currentYear = DateTime.Now.Year;
        var url = $"/api/watchlistdata/fetch/rbi?currentYearOnly=true";

        // Act
        var response = await _client.PostAsync(url, null);

        // Assert
        response.Should().BeSuccessful();
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content);
        
        result.GetProperty("Success").GetBoolean().Should().BeTrue();
        result.GetProperty("Source").GetString().Should().Be("RBI");
        
        // Verify that the processing date is current
        var processingDate = result.GetProperty("ProcessingDate").GetDateTime();
        processingDate.Year.Should().Be(currentYear);
    }

    [Fact]
    public async Task FetchOfacData_ShouldReturnSuccess_WhenCalledOnDemand()
    {
        // Act
        var response = await _client.PostAsync("/api/watchlistdata/fetch/ofac", null);

        // Assert
        response.Should().BeSuccessful();
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content);
        
        result.GetProperty("Success").GetBoolean().Should().BeTrue();
        result.GetProperty("Source").GetString().Should().Be("OFAC");
    }

    [Fact]
    public async Task FetchUnSanctionsData_ShouldReturnSuccess_WhenCalledOnDemand()
    {
        // Act
        var response = await _client.PostAsync("/api/watchlistdata/fetch/un", null);

        // Assert
        response.Should().BeSuccessful();
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content);
        
        result.GetProperty("Success").GetBoolean().Should().BeTrue();
        result.GetProperty("Source").GetString().Should().Be("UN Sanctions");
    }

    [Fact]
    public async Task FetchEuData_ShouldReturnSuccess_WhenCalledOnDemand()
    {
        // Act
        var response = await _client.PostAsync("/api/watchlistdata/fetch/eu", null);

        // Assert
        response.Should().BeSuccessful();
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content);
        
        result.GetProperty("Success").GetBoolean().Should().BeTrue();
        result.GetProperty("Source").GetString().Should().Be("EU Sanctions");
    }

    [Fact]
    public async Task FetchUkData_ShouldReturnSuccess_WhenCalledOnDemand()
    {
        // Act
        var response = await _client.PostAsync("/api/watchlistdata/fetch/uk", null);

        // Assert
        response.Should().BeSuccessful();
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content);
        
        result.GetProperty("Success").GetBoolean().Should().BeTrue();
        result.GetProperty("Source").GetString().Should().Be("UK Sanctions");
    }

    [Fact]
    public async Task FetchSebiData_ShouldReturnSuccess_WhenCalledOnDemand()
    {
        // Act
        var response = await _client.PostAsync("/api/watchlistdata/fetch/sebi", null);

        // Assert
        response.Should().BeSuccessful();
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content);
        
        result.GetProperty("Success").GetBoolean().Should().BeTrue();
        result.GetProperty("Source").GetString().Should().Be("SEBI");
    }

    [Fact]
    public async Task GetDataStatus_ShouldReturnCurrentStatus()
    {
        // Act
        var response = await _client.GetAsync("/api/watchlistdata/status");

        // Assert
        response.Should().BeSuccessful();
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content);
        
        result.GetProperty("sources").EnumerateArray().Should().HaveCountGreaterThan(0);
        
        foreach (var source in result.GetProperty("sources").EnumerateArray())
        {
            source.GetProperty("name").GetString().Should().NotBeNullOrEmpty();
            source.GetProperty("recordCount").GetInt32().Should().BeGreaterOrEqualTo(0);
            source.TryGetProperty("lastUpdated", out _).Should().BeTrue();
        }
    }

    [Fact]
    public async Task FetchAllData_ShouldUpdateAllSources_WhenCalledOnDemand()
    {
        // Act
        var response = await _client.PostAsync("/api/watchlistdata/fetch/all", null);

        // Assert
        response.Should().BeSuccessful();
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content);
        
        result.GetProperty("message").GetString().Should().Contain("All watchlist sources updated");
        result.GetProperty("sources").EnumerateArray().Should().HaveCountGreaterThan(0);
        
        foreach (var source in result.GetProperty("sources").EnumerateArray())
        {
            source.GetProperty("success").GetBoolean().Should().BeTrue();
            source.GetProperty("recordCount").GetInt32().Should().BeGreaterOrEqualTo(0);
        }
    }

    [Theory]
    [InlineData("/api/watchlistdata/fetch/rbi")]
    [InlineData("/api/watchlistdata/fetch/ofac")]
    [InlineData("/api/watchlistdata/fetch/un")]
    [InlineData("/api/watchlistdata/fetch/eu")]
    [InlineData("/api/watchlistdata/fetch/uk")]
    [InlineData("/api/watchlistdata/fetch/sebi")]
    public async Task FetchData_ShouldHandleMultipleConcurrentRequests(string endpoint)
    {
        // Arrange
        var tasks = new List<Task<HttpResponseMessage>>();
        
        // Act - Make 3 concurrent requests
        for (int i = 0; i < 3; i++)
        {
            tasks.Add(_client.PostAsync(endpoint, null));
        }
        
        var responses = await Task.WhenAll(tasks);

        // Assert
        responses.Should().AllSatisfy(response => 
        {
            response.Should().BeSuccessful();
        });
    }

    [Fact]
    public async Task FetchRbiData_ShouldPersistDataToDatabase()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PepScannerDbContext>();

        // Clear existing data
        context.WatchlistEntries.RemoveRange(context.WatchlistEntries.Where(w => w.Source == "RBI"));
        await context.SaveChangesAsync();

        // Act
        var response = await _client.PostAsync("/api/watchlistdata/fetch/rbi", null);

        // Assert
        response.Should().BeSuccessful();
        
        // Verify data was persisted
        var rbiEntries = await context.WatchlistEntries
            .Where(w => w.Source == "RBI")
            .ToListAsync();
        
        rbiEntries.Should().NotBeEmpty();
        rbiEntries.Should().AllSatisfy(entry => 
        {
            entry.Source.Should().Be("RBI");
            entry.PrimaryName.Should().NotBeNullOrEmpty();
            entry.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(5));
        });
    }

    [Fact]
    public async Task FetchData_ShouldReturnError_WhenInvalidSourceRequested()
    {
        // Act
        var response = await _client.PostAsync("/api/watchlistdata/fetch/invalid-source", null);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }
}
