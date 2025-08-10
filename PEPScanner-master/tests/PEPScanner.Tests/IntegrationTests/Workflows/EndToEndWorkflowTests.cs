using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using PEPScanner.Infrastructure.Data;
using PEPScanner.Domain.Entities;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using FluentAssertions;

namespace PEPScanner.Tests.IntegrationTests.Workflows;

public class EndToEndWorkflowTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public EndToEndWorkflowTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<PepScannerDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                services.AddDbContext<PepScannerDbContext>(options =>
                {
                    options.UseInMemoryDatabase("EndToEndWorkflowTestDb");
                });
            });
        });

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task CompleteCustomerOnboardingToAlertResolution_ShouldWorkEndToEnd()
    {
        // Step 1: Create a new customer
        var newCustomer = new
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            DateOfBirth = "1980-01-15",
            Country = "US",
            RiskLevel = "Medium"
        };

        var customerJson = JsonSerializer.Serialize(newCustomer);
        var customerContent = new StringContent(customerJson, Encoding.UTF8, "application/json");

        var customerResponse = await _client.PostAsync("/api/customers", customerContent);
        customerResponse.Should().BeSuccessful();

        var customerResponseContent = await customerResponse.Content.ReadAsStringAsync();
        var createdCustomer = JsonSerializer.Deserialize<JsonElement>(customerResponseContent);
        var customerId = createdCustomer.GetProperty("id").GetString();

        // Step 2: Perform initial PEP/Sanctions screening
        var screeningResponse = await _client.PostAsync($"/api/screening/screen/{customerId}", null);
        screeningResponse.Should().BeSuccessful();

        // Step 3: Perform adverse media scan
        var mediaScanResponse = await _client.PostAsync($"/api/customer-media-scan/scan/{customerId}", null);
        mediaScanResponse.Should().BeSuccessful();

        var mediaScanContent = await mediaScanResponse.Content.ReadAsStringAsync();
        var mediaScanResult = JsonSerializer.Deserialize<JsonElement>(mediaScanContent);
        
        mediaScanResult.GetProperty("success").GetBoolean().Should().BeTrue();
        mediaScanResult.GetProperty("data").GetProperty("status").GetString().Should().Be("Completed");

        // Step 4: Check if alerts were created
        var alertsResponse = await _client.GetAsync("/api/alerts");
        alertsResponse.Should().BeSuccessful();

        var alertsContent = await alertsResponse.Content.ReadAsStringAsync();
        var alerts = JsonSerializer.Deserialize<JsonElement>(alertsContent);

        // Step 5: If alerts exist, test the approval workflow
        if (alerts.EnumerateArray().Any())
        {
            var firstAlert = alerts.EnumerateArray().First();
            var alertId = firstAlert.GetProperty("id").GetString();

            // Assign alert to reviewer
            var assignRequest = new
            {
                AssignedTo = "reviewer@bank.com",
                AssignedBy = "supervisor@bank.com",
                Comments = "Assigned for review"
            };

            var assignJson = JsonSerializer.Serialize(assignRequest);
            var assignContent = new StringContent(assignJson, Encoding.UTF8, "application/json");

            var assignResponse = await _client.PostAsync($"/api/alerts/{alertId}/assign", assignContent);
            assignResponse.Should().BeSuccessful();

            // Approve the alert
            var approveRequest = new
            {
                ApprovedBy = "compliance.officer@bank.com",
                Comments = "Reviewed and approved - low risk",
                Outcome = "Approved"
            };

            var approveJson = JsonSerializer.Serialize(approveRequest);
            var approveContent = new StringContent(approveJson, Encoding.UTF8, "application/json");

            var approveResponse = await _client.PostAsync($"/api/alerts/{alertId}/approve", approveContent);
            approveResponse.Should().BeSuccessful();

            // Verify alert status is updated
            var updatedAlertResponse = await _client.GetAsync($"/api/alerts/{alertId}");
            updatedAlertResponse.Should().BeSuccessful();

            var updatedAlertContent = await updatedAlertResponse.Content.ReadAsStringAsync();
            var updatedAlert = JsonSerializer.Deserialize<JsonElement>(updatedAlertContent);
            
            updatedAlert.GetProperty("workflowStatus").GetString().Should().Be("Approved");
        }

        // Step 6: Verify customer status is updated
        var finalCustomerResponse = await _client.GetAsync($"/api/customers/{customerId}");
        finalCustomerResponse.Should().BeSuccessful();

        var finalCustomerContent = await finalCustomerResponse.Content.ReadAsStringAsync();
        var finalCustomer = JsonSerializer.Deserialize<JsonElement>(finalCustomerContent);
        
        finalCustomer.GetProperty("id").GetString().Should().Be(customerId);
        // Customer should have been scanned
        finalCustomer.TryGetProperty("lastMediaScanDate", out _).Should().BeTrue();
    }

    [Fact]
    public async Task BulkCustomerProcessing_ShouldWorkEndToEnd()
    {
        // Step 1: Bulk upload customers
        var customers = new[]
        {
            new { FirstName = "Alice", LastName = "Johnson", Email = "alice@example.com", Country = "US" },
            new { FirstName = "Bob", LastName = "Smith", Email = "bob@example.com", Country = "UK" },
            new { FirstName = "Charlie", LastName = "Brown", Email = "charlie@example.com", Country = "CA" }
        };

        // Create customers individually (simulating bulk upload)
        var customerIds = new List<string>();
        foreach (var customer in customers)
        {
            var customerJson = JsonSerializer.Serialize(customer);
            var customerContent = new StringContent(customerJson, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("/api/customers", customerContent);
            response.Should().BeSuccessful();

            var content = await response.Content.ReadAsStringAsync();
            var createdCustomer = JsonSerializer.Deserialize<JsonElement>(content);
            customerIds.Add(createdCustomer.GetProperty("id").GetString()!);
        }

        // Step 2: Bulk adverse media scan
        var bulkScanRequest = new { CustomerIds = customerIds };
        var bulkScanJson = JsonSerializer.Serialize(bulkScanRequest);
        var bulkScanContent = new StringContent(bulkScanJson, Encoding.UTF8, "application/json");

        var bulkScanResponse = await _client.PostAsync("/api/customer-media-scan/scan/bulk/batch", bulkScanContent);
        bulkScanResponse.Should().BeSuccessful();

        var bulkScanResponseContent = await bulkScanResponse.Content.ReadAsStringAsync();
        var bulkScanResult = JsonSerializer.Deserialize<JsonElement>(bulkScanResponseContent);
        
        bulkScanResult.GetProperty("success").GetBoolean().Should().BeTrue();
        bulkScanResult.GetProperty("data").GetProperty("totalCustomers").GetInt32().Should().Be(3);

        // Step 3: Check scan status
        var statusResponse = await _client.GetAsync("/api/customer-media-scan/status");
        statusResponse.Should().BeSuccessful();

        var statusContent = await statusResponse.Content.ReadAsStringAsync();
        var status = JsonSerializer.Deserialize<JsonElement>(statusContent);
        
        status.GetProperty("success").GetBoolean().Should().BeTrue();
        status.GetProperty("summary").GetProperty("totalCustomers").GetInt32().Should().BeGreaterOrEqualTo(3);
    }

    [Fact]
    public async Task PeriodicScanningWorkflow_ShouldWorkEndToEnd()
    {
        // Step 1: Setup periodic scans
        var setupResponse = await _client.PostAsync("/api/customer-media-scan/schedule/setup", null);
        setupResponse.Should().BeSuccessful();

        var setupContent = await setupResponse.Content.ReadAsStringAsync();
        var setupResult = JsonSerializer.Deserialize<JsonElement>(setupContent);
        
        setupResult.GetProperty("success").GetBoolean().Should().BeTrue();
        setupResult.GetProperty("message").GetString().Should().Contain("scheduled successfully");

        // Step 2: Check schedule status
        var scheduleStatusResponse = await _client.GetAsync("/api/customer-media-scan/schedule/status");
        scheduleStatusResponse.Should().BeSuccessful();

        var scheduleContent = await scheduleStatusResponse.Content.ReadAsStringAsync();
        var scheduleStatus = JsonSerializer.Deserialize<JsonElement>(scheduleContent);
        
        scheduleStatus.GetProperty("success").GetBoolean().Should().BeTrue();
        scheduleStatus.GetProperty("schedules").GetProperty("dailyHighRiskScan").GetProperty("enabled").GetBoolean().Should().BeTrue();

        // Step 3: Trigger on-demand high-risk scan
        var onDemandRequest = new { ScanType = "high-risk" };
        var onDemandJson = JsonSerializer.Serialize(onDemandRequest);
        var onDemandContent = new StringContent(onDemandJson, Encoding.UTF8, "application/json");

        var onDemandResponse = await _client.PostAsync("/api/customer-media-scan/scan/on-demand", onDemandContent);
        onDemandResponse.Should().BeSuccessful();

        var onDemandResponseContent = await onDemandResponse.Content.ReadAsStringAsync();
        var onDemandResult = JsonSerializer.Deserialize<JsonElement>(onDemandResponseContent);
        
        onDemandResult.GetProperty("success").GetBoolean().Should().BeTrue();
        onDemandResult.GetProperty("message").GetString().Should().Contain("high-risk scan completed");
    }

    [Fact]
    public async Task WatchlistDataFetching_ShouldWorkEndToEnd()
    {
        // Step 1: Fetch RBI data with current year filter
        var rbiResponse = await _client.PostAsync("/api/watchlistdata/fetch/rbi?currentYearOnly=true", null);
        rbiResponse.Should().BeSuccessful();

        var rbiContent = await rbiResponse.Content.ReadAsStringAsync();
        var rbiResult = JsonSerializer.Deserialize<JsonElement>(rbiContent);
        
        rbiResult.GetProperty("Success").GetBoolean().Should().BeTrue();
        rbiResult.GetProperty("Source").GetString().Should().Be("RBI");

        // Step 2: Fetch OFAC data
        var ofacResponse = await _client.PostAsync("/api/watchlistdata/fetch/ofac", null);
        ofacResponse.Should().BeSuccessful();

        // Step 3: Fetch all data sources
        var allResponse = await _client.PostAsync("/api/watchlistdata/fetch/all", null);
        allResponse.Should().BeSuccessful();

        var allContent = await allResponse.Content.ReadAsStringAsync();
        var allResult = JsonSerializer.Deserialize<JsonElement>(allContent);
        
        allResult.GetProperty("message").GetString().Should().Contain("All watchlist sources updated");

        // Step 4: Check data status
        var statusResponse = await _client.GetAsync("/api/watchlistdata/status");
        statusResponse.Should().BeSuccessful();

        var statusContent = await statusResponse.Content.ReadAsStringAsync();
        var status = JsonSerializer.Deserialize<JsonElement>(statusContent);
        
        status.GetProperty("sources").EnumerateArray().Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task AdverseMediaAlertCreation_ShouldWorkEndToEnd()
    {
        // Step 1: Create adverse media alert
        var alertRequest = new
        {
            EntityName = "Test Entity",
            EntityType = "Individual",
            RiskScore = 85.0,
            MediaHeadline = "Test Entity under investigation",
            MediaSource = "Test News",
            PublishedDate = DateTime.UtcNow.AddDays(-1),
            RiskCategories = new[] { "Financial Crime" },
            Excerpt = "Investigation details...",
            ArticleUrl = "https://example.com/article",
            Sentiment = "Negative"
        };

        var alertJson = JsonSerializer.Serialize(alertRequest);
        var alertContent = new StringContent(alertJson, Encoding.UTF8, "application/json");

        var alertResponse = await _client.PostAsync("/api/alerts/create-from-media", alertContent);
        alertResponse.Should().BeSuccessful();

        var alertResponseContent = await alertResponse.Content.ReadAsStringAsync();
        var createdAlert = JsonSerializer.Deserialize<JsonElement>(alertResponseContent);
        
        createdAlert.GetProperty("AlertType").GetString().Should().Be("Adverse Media");
        createdAlert.GetProperty("Priority").GetString().Should().Be("High");
        createdAlert.GetProperty("WorkflowStatus").GetString().Should().Be("PendingReview");

        // Step 2: Get pending review alerts
        var pendingResponse = await _client.GetAsync("/api/alerts/pending-review");
        pendingResponse.Should().BeSuccessful();

        var pendingContent = await pendingResponse.Content.ReadAsStringAsync();
        var pendingAlerts = JsonSerializer.Deserialize<JsonElement>(pendingContent);
        
        pendingAlerts.EnumerateArray().Should().HaveCountGreaterOrEqualTo(1);

        // Step 3: Bulk create alerts
        var bulkAlertRequest = new
        {
            MediaResults = new[]
            {
                new
                {
                    EntityName = "Bulk Entity 1",
                    EntityType = "Individual",
                    RiskScore = 90.0,
                    MediaHeadline = "Bulk Entity 1 news",
                    MediaSource = "News Source",
                    PublishedDate = DateTime.UtcNow,
                    RiskCategories = new[] { "Sanctions" },
                    Excerpt = "News excerpt...",
                    ArticleUrl = "https://example.com/bulk1",
                    Sentiment = "Negative"
                },
                new
                {
                    EntityName = "Bulk Entity 2",
                    EntityType = "Individual",
                    RiskScore = 45.0, // Below threshold
                    MediaHeadline = "Bulk Entity 2 news",
                    MediaSource = "News Source",
                    PublishedDate = DateTime.UtcNow,
                    RiskCategories = new[] { "General" },
                    Excerpt = "Low risk news...",
                    ArticleUrl = "https://example.com/bulk2",
                    Sentiment = "Neutral"
                }
            },
            MinimumRiskThreshold = 75.0
        };

        var bulkAlertJson = JsonSerializer.Serialize(bulkAlertRequest);
        var bulkAlertContent = new StringContent(bulkAlertJson, Encoding.UTF8, "application/json");

        var bulkAlertResponse = await _client.PostAsync("/api/alerts/bulk-create-from-media", bulkAlertContent);
        bulkAlertResponse.Should().BeSuccessful();

        var bulkAlertResponseContent = await bulkAlertResponse.Content.ReadAsStringAsync();
        var bulkAlertResult = JsonSerializer.Deserialize<JsonElement>(bulkAlertResponseContent);
        
        bulkAlertResult.GetProperty("createdCount").GetInt32().Should().Be(1); // Only high-risk entity
        bulkAlertResult.GetProperty("skippedCount").GetInt32().Should().Be(1); // Low-risk entity skipped
    }

    [Fact]
    public async Task CompleteAlertWorkflow_ShouldWorkEndToEnd()
    {
        // Step 1: Create an alert
        var alertRequest = new
        {
            EntityName = "Workflow Test Entity",
            EntityType = "Individual",
            RiskScore = 95.0,
            MediaHeadline = "Critical news about entity",
            MediaSource = "Major News",
            PublishedDate = DateTime.UtcNow,
            RiskCategories = new[] { "Financial Crime", "Sanctions" },
            Excerpt = "Critical investigation details...",
            ArticleUrl = "https://example.com/critical",
            Sentiment = "Negative"
        };

        var alertJson = JsonSerializer.Serialize(alertRequest);
        var alertContent = new StringContent(alertJson, Encoding.UTF8, "application/json");

        var alertResponse = await _client.PostAsync("/api/alerts/create-from-media", alertContent);
        alertResponse.Should().BeSuccessful();

        var alertResponseContent = await alertResponse.Content.ReadAsStringAsync();
        var createdAlert = JsonSerializer.Deserialize<JsonElement>(alertResponseContent);
        var alertId = createdAlert.GetProperty("Id").GetString();

        // Step 2: Assign alert
        var assignRequest = new
        {
            AssignedTo = "analyst@bank.com",
            AssignedBy = "supervisor@bank.com",
            Comments = "High priority - investigate immediately"
        };

        var assignJson = JsonSerializer.Serialize(assignRequest);
        var assignContent = new StringContent(assignJson, Encoding.UTF8, "application/json");

        var assignResponse = await _client.PostAsync($"/api/alerts/{alertId}/assign", assignContent);
        assignResponse.Should().BeSuccessful();

        // Step 3: Approve alert
        var approveRequest = new
        {
            ApprovedBy = "compliance.manager@bank.com",
            Comments = "Investigated and resolved - customer relationship terminated",
            Outcome = "Relationship Terminated"
        };

        var approveJson = JsonSerializer.Serialize(approveRequest);
        var approveContent = new StringContent(approveJson, Encoding.UTF8, "application/json");

        var approveResponse = await _client.PostAsync($"/api/alerts/{alertId}/approve", approveContent);
        approveResponse.Should().BeSuccessful();

        // Step 4: Verify final alert status
        var finalAlertResponse = await _client.GetAsync($"/api/alerts/{alertId}");
        finalAlertResponse.Should().BeSuccessful();

        var finalAlertContent = await finalAlertResponse.Content.ReadAsStringAsync();
        var finalAlert = JsonSerializer.Deserialize<JsonElement>(finalAlertContent);
        
        finalAlert.GetProperty("workflowStatus").GetString().Should().Be("Approved");
        finalAlert.GetProperty("priority").GetString().Should().Be("Critical");
    }
}
