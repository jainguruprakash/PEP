using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using PEPScanner.Infrastructure.Data;
using PEPScanner.Domain.Entities;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using FluentAssertions;

namespace PEPScanner.Tests.IntegrationTests.Controllers;

public class CustomerMediaScanControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public CustomerMediaScanControllerTests(WebApplicationFactory<Program> factory)
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
                    options.UseInMemoryDatabase("CustomerMediaScanTestDb");
                });
            });
        });

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task ScanCustomer_ShouldReturnSuccess_WhenCustomerExists()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PepScannerDbContext>();

        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            FullName = "John Doe",
            IsActive = true,
            RiskLevel = "Medium"
        };

        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        // Act
        var response = await _client.PostAsync($"/api/customer-media-scan/scan/{customer.Id}", null);

        // Assert
        response.Should().BeSuccessful();
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content);
        
        result.GetProperty("success").GetBoolean().Should().BeTrue();
        result.GetProperty("data").GetProperty("customerId").GetString().Should().Be(customer.Id.ToString());
        result.GetProperty("data").GetProperty("customerName").GetString().Should().Be("John Doe");
        result.GetProperty("data").GetProperty("status").GetString().Should().Be("Completed");
    }

    [Fact]
    public async Task ScanCustomer_ShouldReturnNotFound_WhenCustomerDoesNotExist()
    {
        // Arrange
        var nonExistentCustomerId = Guid.NewGuid();

        // Act
        var response = await _client.PostAsync($"/api/customer-media-scan/scan/{nonExistentCustomerId}", null);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ScanAllCustomers_ShouldReturnSuccess()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PepScannerDbContext>();

        var customers = new List<Customer>
        {
            new() { Id = Guid.NewGuid(), FullName = "Customer 1", IsActive = true },
            new() { Id = Guid.NewGuid(), FullName = "Customer 2", IsActive = true },
            new() { Id = Guid.NewGuid(), FullName = "Inactive Customer", IsActive = false }
        };

        context.Customers.AddRange(customers);
        await context.SaveChangesAsync();

        // Act
        var response = await _client.PostAsync("/api/customer-media-scan/scan/bulk/all", null);

        // Assert
        response.Should().BeSuccessful();
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content);
        
        result.GetProperty("success").GetBoolean().Should().BeTrue();
        result.GetProperty("data").GetProperty("totalCustomers").GetInt32().Should().Be(2); // Only active customers
        result.GetProperty("data").GetProperty("status").GetString().Should().Be("Completed");
    }

    [Fact]
    public async Task ScanHighRiskCustomers_ShouldReturnSuccess()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PepScannerDbContext>();

        var customers = new List<Customer>
        {
            new() { Id = Guid.NewGuid(), FullName = "High Risk", IsActive = true, RiskLevel = "High" },
            new() { Id = Guid.NewGuid(), FullName = "Medium Risk", IsActive = true, RiskLevel = "Medium" },
            new() { Id = Guid.NewGuid(), FullName = "Low Risk", IsActive = true, RiskLevel = "Low" }
        };

        context.Customers.AddRange(customers);
        await context.SaveChangesAsync();

        // Act
        var response = await _client.PostAsync("/api/customer-media-scan/scan/bulk/high-risk", null);

        // Assert
        response.Should().BeSuccessful();
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content);
        
        result.GetProperty("success").GetBoolean().Should().BeTrue();
        result.GetProperty("data").GetProperty("totalCustomers").GetInt32().Should().Be(2); // High and Medium risk only
    }

    [Fact]
    public async Task ScanCustomersBatch_ShouldReturnSuccess_WhenValidCustomerIds()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PepScannerDbContext>();

        var customers = new List<Customer>
        {
            new() { Id = Guid.NewGuid(), FullName = "Batch Customer 1", IsActive = true },
            new() { Id = Guid.NewGuid(), FullName = "Batch Customer 2", IsActive = true }
        };

        context.Customers.AddRange(customers);
        await context.SaveChangesAsync();

        var request = new
        {
            CustomerIds = customers.Select(c => c.Id).ToList()
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/customer-media-scan/scan/bulk/batch", content);

        // Assert
        response.Should().BeSuccessful();
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
        
        result.GetProperty("success").GetBoolean().Should().BeTrue();
        result.GetProperty("data").GetProperty("totalCustomers").GetInt32().Should().Be(2);
    }

    [Fact]
    public async Task ScanCustomersBatch_ShouldReturnBadRequest_WhenNoCustomerIds()
    {
        // Arrange
        var request = new { CustomerIds = new List<Guid>() };
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/customer-media-scan/scan/bulk/batch", content);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetScanStatus_ShouldReturnCustomerStatuses()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PepScannerDbContext>();

        var customers = new List<Customer>
        {
            new() 
            { 
                Id = Guid.NewGuid(), 
                FullName = "Recent Scan", 
                IsActive = true, 
                RiskLevel = "High",
                LastMediaScanDate = DateTime.UtcNow.AddDays(-5)
            },
            new() 
            { 
                Id = Guid.NewGuid(), 
                FullName = "Old Scan", 
                IsActive = true, 
                RiskLevel = "Medium",
                LastMediaScanDate = DateTime.UtcNow.AddDays(-45)
            },
            new() 
            { 
                Id = Guid.NewGuid(), 
                FullName = "Never Scanned", 
                IsActive = true, 
                RiskLevel = "Low",
                LastMediaScanDate = null
            }
        };

        context.Customers.AddRange(customers);
        await context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/customer-media-scan/status");

        // Assert
        response.Should().BeSuccessful();
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content);
        
        result.GetProperty("success").GetBoolean().Should().BeTrue();
        result.GetProperty("summary").GetProperty("totalCustomers").GetInt32().Should().Be(3);
        result.GetProperty("summary").GetProperty("highRisk").GetInt32().Should().Be(1);
        result.GetProperty("summary").GetProperty("neverScanned").GetInt32().Should().Be(1);
        
        var customerArray = result.GetProperty("customers").EnumerateArray().ToList();
        customerArray.Should().HaveCount(3);
    }

    [Fact]
    public async Task SetupPeriodicScans_ShouldReturnSuccess()
    {
        // Act
        var response = await _client.PostAsync("/api/customer-media-scan/schedule/setup", null);

        // Assert
        response.Should().BeSuccessful();
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content);
        
        result.GetProperty("success").GetBoolean().Should().BeTrue();
        result.GetProperty("message").GetString().Should().Contain("scheduled successfully");
        result.GetProperty("schedules").GetProperty("dailyHighRisk").GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task RescanCustomer_ShouldReturnSuccess_WhenCustomerExists()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PepScannerDbContext>();

        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            FullName = "Rescan Customer",
            IsActive = true,
            LastMediaScanDate = DateTime.UtcNow.AddDays(-30)
        };

        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        // Act
        var response = await _client.PostAsync($"/api/customer-media-scan/rescan/{customer.Id}", null);

        // Assert
        response.Should().BeSuccessful();
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content);
        
        result.GetProperty("success").GetBoolean().Should().BeTrue();
        result.GetProperty("data").GetProperty("customerId").GetString().Should().Be(customer.Id.ToString());
    }

    [Fact]
    public async Task OnDemandScan_ShouldReturnSuccess_ForAllScanTypes()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PepScannerDbContext>();

        var customers = new List<Customer>
        {
            new() { Id = Guid.NewGuid(), FullName = "Customer 1", IsActive = true, RiskLevel = "High" },
            new() { Id = Guid.NewGuid(), FullName = "Customer 2", IsActive = true, RiskLevel = "Low" }
        };

        context.Customers.AddRange(customers);
        await context.SaveChangesAsync();

        var testCases = new[]
        {
            new { ScanType = "all", CustomerIds = (List<Guid>?)null },
            new { ScanType = "high-risk", CustomerIds = (List<Guid>?)null },
            new { ScanType = "batch", CustomerIds = customers.Select(c => c.Id).ToList() }
        };

        foreach (var testCase in testCases)
        {
            var request = new
            {
                ScanType = testCase.ScanType,
                CustomerIds = testCase.CustomerIds
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/customer-media-scan/scan/on-demand", content);

            // Assert
            response.Should().BeSuccessful($"Failed for scan type: {testCase.ScanType}");
            
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
            
            result.GetProperty("success").GetBoolean().Should().BeTrue();
            result.GetProperty("message").GetString().Should().Contain(testCase.ScanType);
        }
    }

    [Fact]
    public async Task OnDemandScan_ShouldReturnBadRequest_ForInvalidScanType()
    {
        // Arrange
        var request = new
        {
            ScanType = "invalid-type",
            CustomerIds = (List<Guid>?)null
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/customer-media-scan/scan/on-demand", content);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetScheduleStatus_ShouldReturnScheduleInformation()
    {
        // Act
        var response = await _client.GetAsync("/api/customer-media-scan/schedule/status");

        // Assert
        response.Should().BeSuccessful();
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content);
        
        result.GetProperty("success").GetBoolean().Should().BeTrue();
        result.GetProperty("schedules").GetProperty("dailyHighRiskScan").GetProperty("enabled").GetBoolean().Should().BeTrue();
        result.GetProperty("schedules").GetProperty("weeklyAllCustomersScan").GetProperty("enabled").GetBoolean().Should().BeTrue();
        result.GetProperty("schedules").GetProperty("monthlyDormantScan").GetProperty("enabled").GetBoolean().Should().BeTrue();
    }
}
