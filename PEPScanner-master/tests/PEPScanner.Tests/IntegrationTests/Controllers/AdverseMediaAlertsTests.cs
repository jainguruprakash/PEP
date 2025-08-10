using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using PEPScanner.Infrastructure.Data;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using FluentAssertions;

namespace PEPScanner.Tests.IntegrationTests.Controllers;

public class AdverseMediaAlertsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AdverseMediaAlertsTests(WebApplicationFactory<Program> factory)
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
                    options.UseInMemoryDatabase("AdverseMediaTestDb");
                });
            });
        });

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task CreateFromAdverseMedia_ShouldCreateAlert_WhenValidDataProvided()
    {
        // Arrange
        var alertRequest = new
        {
            EntityName = "John Doe",
            EntityType = "Individual",
            RiskScore = 85.5,
            MediaHeadline = "John Doe faces regulatory investigation",
            MediaSource = "Financial Times",
            PublishedDate = DateTime.UtcNow.AddDays(-1),
            RiskCategories = new[] { "Financial Crime", "Regulatory" },
            Excerpt = "Recent developments suggest potential compliance issues...",
            ArticleUrl = "https://example.com/article1",
            Sentiment = "Negative"
        };

        var json = JsonSerializer.Serialize(alertRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/alerts/create-from-media", content);

        // Assert
        response.Should().BeSuccessful();
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
        
        result.GetProperty("AlertType").GetString().Should().Be("Adverse Media");
        result.GetProperty("EntityName").GetString().Should().Be("John Doe");
        result.GetProperty("RiskScore").GetDouble().Should().Be(85.5);
        result.GetProperty("Priority").GetString().Should().Be("High");
        result.GetProperty("Status").GetString().Should().Be("Open");
        result.GetProperty("WorkflowStatus").GetString().Should().Be("PendingReview");
    }

    [Fact]
    public async Task CreateFromAdverseMedia_ShouldPersistToDatabase()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PepScannerDbContext>();

        var alertRequest = new
        {
            EntityName = "Jane Smith",
            EntityType = "Individual",
            RiskScore = 92.0,
            MediaHeadline = "Jane Smith mentioned in sanctions report",
            MediaSource = "Reuters",
            PublishedDate = DateTime.UtcNow.AddDays(-2),
            RiskCategories = new[] { "Sanctions", "Legal" },
            Excerpt = "Government sources indicate ongoing investigation...",
            ArticleUrl = "https://example.com/article2",
            Sentiment = "Negative"
        };

        var json = JsonSerializer.Serialize(alertRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/alerts/create-from-media", content);

        // Assert
        response.Should().BeSuccessful();
        
        // Verify database persistence
        var alerts = await context.Alerts
            .Where(a => a.EntityName == "Jane Smith" && a.AlertType == "Adverse Media")
            .ToListAsync();
        
        alerts.Should().HaveCount(1);
        var alert = alerts.First();
        
        alert.EntityName.Should().Be("Jane Smith");
        alert.AlertType.Should().Be("Adverse Media");
        alert.RiskScore.Should().Be(92.0);
        alert.Priority.Should().Be("Critical");
        alert.Status.Should().Be("Open");
        alert.WorkflowStatus.Should().Be("PendingReview");
        alert.Source.Should().Be("Adverse Media Scan");
        alert.Description.Should().Contain("Jane Smith mentioned in sanctions report");
    }

    [Fact]
    public async Task BulkCreateFromAdverseMedia_ShouldCreateMultipleAlerts()
    {
        // Arrange
        var bulkRequest = new
        {
            MediaResults = new[]
            {
                new
                {
                    EntityName = "Company A",
                    EntityType = "Organization",
                    RiskScore = 88.0,
                    MediaHeadline = "Company A under investigation",
                    MediaSource = "Wall Street Journal",
                    PublishedDate = DateTime.UtcNow.AddDays(-1),
                    RiskCategories = new[] { "Financial Crime" },
                    Excerpt = "Investigation ongoing...",
                    ArticleUrl = "https://example.com/company-a",
                    Sentiment = "Negative"
                },
                new
                {
                    EntityName = "Company B",
                    EntityType = "Organization",
                    RiskScore = 76.5,
                    MediaHeadline = "Company B faces penalties",
                    MediaSource = "Bloomberg",
                    PublishedDate = DateTime.UtcNow.AddDays(-3),
                    RiskCategories = new[] { "Regulatory" },
                    Excerpt = "Regulatory penalties imposed...",
                    ArticleUrl = "https://example.com/company-b",
                    Sentiment = "Negative"
                }
            },
            MinimumRiskThreshold = 75.0
        };

        var json = JsonSerializer.Serialize(bulkRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/alerts/bulk-create-from-media", content);

        // Assert
        response.Should().BeSuccessful();
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
        
        result.GetProperty("createdCount").GetInt32().Should().Be(2);
        result.GetProperty("skippedCount").GetInt32().Should().Be(0);
        result.GetProperty("totalProcessed").GetInt32().Should().Be(2);
        
        var alerts = result.GetProperty("alerts").EnumerateArray().ToList();
        alerts.Should().HaveCount(2);
        alerts.Should().Contain(a => a.GetProperty("EntityName").GetString() == "Company A");
        alerts.Should().Contain(a => a.GetProperty("EntityName").GetString() == "Company B");
    }

    [Fact]
    public async Task BulkCreateFromAdverseMedia_ShouldSkipLowRiskEntries()
    {
        // Arrange
        var bulkRequest = new
        {
            MediaResults = new[]
            {
                new
                {
                    EntityName = "High Risk Entity",
                    EntityType = "Individual",
                    RiskScore = 85.0,
                    MediaHeadline = "High risk news",
                    MediaSource = "News Source",
                    PublishedDate = DateTime.UtcNow,
                    RiskCategories = new[] { "Financial Crime" },
                    Excerpt = "High risk content...",
                    ArticleUrl = "https://example.com/high-risk",
                    Sentiment = "Negative"
                },
                new
                {
                    EntityName = "Low Risk Entity",
                    EntityType = "Individual",
                    RiskScore = 45.0, // Below threshold
                    MediaHeadline = "Low risk news",
                    MediaSource = "News Source",
                    PublishedDate = DateTime.UtcNow,
                    RiskCategories = new[] { "General" },
                    Excerpt = "Low risk content...",
                    ArticleUrl = "https://example.com/low-risk",
                    Sentiment = "Neutral"
                }
            },
            MinimumRiskThreshold = 75.0
        };

        var json = JsonSerializer.Serialize(bulkRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/alerts/bulk-create-from-media", content);

        // Assert
        response.Should().BeSuccessful();
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
        
        result.GetProperty("createdCount").GetInt32().Should().Be(1);
        result.GetProperty("skippedCount").GetInt32().Should().Be(1);
        result.GetProperty("totalProcessed").GetInt32().Should().Be(2);
    }

    [Fact]
    public async Task BulkCreateFromAdverseMedia_ShouldSkipDuplicateAlerts()
    {
        // Arrange - First create an alert
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PepScannerDbContext>();

        var existingAlert = new PEPScanner.Domain.Entities.Alert
        {
            Id = Guid.NewGuid(),
            AlertType = "Adverse Media",
            EntityName = "Duplicate Entity",
            Description = "Duplicate Entity faces investigation",
            Status = "Open",
            CreatedAtUtc = DateTime.UtcNow
        };

        context.Alerts.Add(existingAlert);
        await context.SaveChangesAsync();

        var bulkRequest = new
        {
            MediaResults = new[]
            {
                new
                {
                    EntityName = "Duplicate Entity",
                    EntityType = "Individual",
                    RiskScore = 85.0,
                    MediaHeadline = "Duplicate Entity faces investigation",
                    MediaSource = "News Source",
                    PublishedDate = DateTime.UtcNow,
                    RiskCategories = new[] { "Financial Crime" },
                    Excerpt = "Investigation content...",
                    ArticleUrl = "https://example.com/duplicate",
                    Sentiment = "Negative"
                }
            },
            MinimumRiskThreshold = 75.0
        };

        var json = JsonSerializer.Serialize(bulkRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/alerts/bulk-create-from-media", content);

        // Assert
        response.Should().BeSuccessful();
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
        
        result.GetProperty("createdCount").GetInt32().Should().Be(0);
        result.GetProperty("skippedCount").GetInt32().Should().Be(1);
    }

    [Theory]
    [InlineData(95.0, "Critical")]
    [InlineData(85.0, "High")]
    [InlineData(65.0, "Medium")]
    [InlineData(45.0, "Low")]
    public async Task CreateFromAdverseMedia_ShouldSetCorrectPriority_BasedOnRiskScore(double riskScore, string expectedPriority)
    {
        // Arrange
        var alertRequest = new
        {
            EntityName = $"Test Entity {riskScore}",
            EntityType = "Individual",
            RiskScore = riskScore,
            MediaHeadline = "Test headline",
            MediaSource = "Test Source",
            PublishedDate = DateTime.UtcNow,
            RiskCategories = new[] { "Test" },
            Excerpt = "Test excerpt",
            ArticleUrl = "https://example.com/test",
            Sentiment = "Negative"
        };

        var json = JsonSerializer.Serialize(alertRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/alerts/create-from-media", content);

        // Assert
        response.Should().BeSuccessful();
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
        
        result.GetProperty("Priority").GetString().Should().Be(expectedPriority);
    }

    [Fact]
    public async Task CreateFromAdverseMedia_ShouldReturnBadRequest_WhenInvalidDataProvided()
    {
        // Arrange
        var invalidRequest = new
        {
            EntityName = "", // Empty name
            RiskScore = -1.0 // Invalid score
        };

        var json = JsonSerializer.Serialize(invalidRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/alerts/create-from-media", content);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }
}
