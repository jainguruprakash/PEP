using AutoFixture;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using PEPScanner.Application.Services;
using PEPScanner.Infrastructure.Data;
using PEPScanner.Domain.Entities;
using Moq;
using FluentAssertions;

namespace PEPScanner.Tests.UnitTests.Services;

public class CustomerMediaScanningServiceTests : IDisposable
{
    private readonly Mock<ILogger<CustomerMediaScanningService>> _mockLogger;
    private readonly Mock<IAdverseMediaService> _mockAdverseMediaService;
    private readonly Mock<IAlertService> _mockAlertService;
    private readonly PepScannerDbContext _context;
    private readonly CustomerMediaScanningService _service;
    private readonly Fixture _fixture;

    public CustomerMediaScanningServiceTests()
    {
        _mockLogger = new Mock<ILogger<CustomerMediaScanningService>>();
        _mockAdverseMediaService = new Mock<IAdverseMediaService>();
        _mockAlertService = new Mock<IAlertService>();
        
        var options = new DbContextOptionsBuilder<PepScannerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new PepScannerDbContext(options);
        
        _service = new CustomerMediaScanningService(
            _context, 
            _mockLogger.Object, 
            _mockAdverseMediaService.Object, 
            _mockAlertService.Object);
        
        _fixture = new Fixture();
    }

    [Fact]
    public async Task ScanCustomerAsync_ShouldReturnScanResult_WhenCustomerExists()
    {
        // Arrange
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            FullName = "John Doe",
            IsActive = true,
            RiskLevel = "Medium"
        };
        
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var mockMediaResults = new List<AdverseMediaResult>
        {
            new() { RiskScore = 85, Headline = "High risk news", Source = "News Source", Sentiment = "Negative" },
            new() { RiskScore = 45, Headline = "Low risk news", Source = "News Source", Sentiment = "Neutral" }
        };

        _mockAdverseMediaService
            .Setup(x => x.ScanEntityAsync(It.IsAny<AdverseMediaScanRequest>()))
            .ReturnsAsync(mockMediaResults);

        _mockAlertService
            .Setup(x => x.CreateFromAdverseMediaAsync(It.IsAny<CreateMediaAlertRequest>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.ScanCustomerAsync(customer.Id);

        // Assert
        result.Should().NotBeNull();
        result.CustomerId.Should().Be(customer.Id);
        result.CustomerName.Should().Be("John Doe");
        result.Status.Should().Be("Completed");
        result.MediaResultsFound.Should().Be(2);
        result.HighRiskResults.Should().Be(1);
        result.LowRiskResults.Should().Be(1);
        result.AlertsCreated.Should().Be(1); // Only high-risk results create alerts
    }

    [Fact]
    public async Task ScanCustomerAsync_ShouldThrowException_WhenCustomerNotFound()
    {
        // Arrange
        var nonExistentCustomerId = Guid.NewGuid();

        // Act & Assert
        var act = async () => await _service.ScanCustomerAsync(nonExistentCustomerId);
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage($"Customer {nonExistentCustomerId} not found");
    }

    [Fact]
    public async Task ScanCustomerAsync_ShouldUpdateCustomerRiskLevel_WhenHighRiskResultsFound()
    {
        // Arrange
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            FullName = "Jane Smith",
            IsActive = true,
            RiskLevel = "Low"
        };
        
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var mockMediaResults = new List<AdverseMediaResult>
        {
            new() { RiskScore = 90, Headline = "Critical risk news", Source = "News Source", Sentiment = "Negative" }
        };

        _mockAdverseMediaService
            .Setup(x => x.ScanEntityAsync(It.IsAny<AdverseMediaScanRequest>()))
            .ReturnsAsync(mockMediaResults);

        // Act
        await _service.ScanCustomerAsync(customer.Id);

        // Assert
        var updatedCustomer = await _context.Customers.FindAsync(customer.Id);
        updatedCustomer!.RiskLevel.Should().Be("High");
        updatedCustomer.LastMediaScanDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task ScanAllCustomersAsync_ShouldScanAllActiveCustomers()
    {
        // Arrange
        var customers = new List<Customer>
        {
            new() { Id = Guid.NewGuid(), FullName = "Customer 1", IsActive = true },
            new() { Id = Guid.NewGuid(), FullName = "Customer 2", IsActive = true },
            new() { Id = Guid.NewGuid(), FullName = "Customer 3", IsActive = false } // Inactive
        };
        
        _context.Customers.AddRange(customers);
        await _context.SaveChangesAsync();

        _mockAdverseMediaService
            .Setup(x => x.ScanEntityAsync(It.IsAny<AdverseMediaScanRequest>()))
            .ReturnsAsync(new List<AdverseMediaResult>());

        // Act
        var result = await _service.ScanAllCustomersAsync();

        // Assert
        result.Should().NotBeNull();
        result.TotalCustomers.Should().Be(2); // Only active customers
        result.Status.Should().Be("Completed");
    }

    [Fact]
    public async Task ScanHighRiskCustomersAsync_ShouldScanOnlyHighAndMediumRiskCustomers()
    {
        // Arrange
        var customers = new List<Customer>
        {
            new() { Id = Guid.NewGuid(), FullName = "High Risk", IsActive = true, RiskLevel = "High" },
            new() { Id = Guid.NewGuid(), FullName = "Medium Risk", IsActive = true, RiskLevel = "Medium" },
            new() { Id = Guid.NewGuid(), FullName = "Low Risk", IsActive = true, RiskLevel = "Low" }
        };
        
        _context.Customers.AddRange(customers);
        await _context.SaveChangesAsync();

        _mockAdverseMediaService
            .Setup(x => x.ScanEntityAsync(It.IsAny<AdverseMediaScanRequest>()))
            .ReturnsAsync(new List<AdverseMediaResult>());

        // Act
        var result = await _service.ScanHighRiskCustomersAsync();

        // Assert
        result.Should().NotBeNull();
        result.TotalCustomers.Should().Be(2); // Only high and medium risk
        result.Status.Should().Be("Completed");
    }

    [Fact]
    public async Task GetScanStatusAsync_ShouldReturnCustomerScanStatuses()
    {
        // Arrange
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
        
        _context.Customers.AddRange(customers);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetScanStatusAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(s => s.CustomerName == "Recent Scan" && !s.RequiresRescan);
        result.Should().Contain(s => s.CustomerName == "Old Scan" && s.RequiresRescan);
        result.Should().Contain(s => s.CustomerName == "Never Scanned" && s.RequiresRescan);
    }

    [Fact]
    public async Task ScanCustomersBatchAsync_ShouldHandlePartialFailures()
    {
        // Arrange
        var validCustomer = new Customer
        {
            Id = Guid.NewGuid(),
            FullName = "Valid Customer",
            IsActive = true
        };
        
        _context.Customers.Add(validCustomer);
        await _context.SaveChangesAsync();

        var customerIds = new List<Guid> { validCustomer.Id, Guid.NewGuid() }; // One valid, one invalid

        _mockAdverseMediaService
            .Setup(x => x.ScanEntityAsync(It.IsAny<AdverseMediaScanRequest>()))
            .ReturnsAsync(new List<AdverseMediaResult>());

        // Act
        var result = await _service.ScanCustomersBatchAsync(customerIds);

        // Assert
        result.Should().NotBeNull();
        result.TotalCustomers.Should().Be(2);
        result.SuccessfulScans.Should().Be(1);
        result.FailedScans.Should().Be(1);
        result.Status.Should().Be("Completed");
    }

    [Theory]
    [InlineData("High", 7)]
    [InlineData("Medium", 30)]
    [InlineData("Low", 90)]
    [InlineData("Unknown", 30)]
    public async Task GetScanStatusAsync_ShouldCalculateCorrectRescanRequirement(string riskLevel, int expectedInterval)
    {
        // Arrange
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            FullName = "Test Customer",
            IsActive = true,
            RiskLevel = riskLevel,
            LastMediaScanDate = DateTime.UtcNow.AddDays(-(expectedInterval + 1)) // Past due
        };
        
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetScanStatusAsync();

        // Assert
        var customerStatus = result.First();
        customerStatus.RequiresRescan.Should().BeTrue();
        customerStatus.DaysSinceLastScan.Should().BeGreaterThan(expectedInterval);
    }

    [Fact]
    public async Task ScanCustomerAsync_ShouldCreateAlertsOnlyForHighRiskResults()
    {
        // Arrange
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            FullName = "Test Customer",
            IsActive = true
        };
        
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var mockMediaResults = new List<AdverseMediaResult>
        {
            new() { RiskScore = 95, Headline = "Critical news", Source = "Source1", Sentiment = "Negative" },
            new() { RiskScore = 80, Headline = "High risk news", Source = "Source2", Sentiment = "Negative" },
            new() { RiskScore = 60, Headline = "Medium risk news", Source = "Source3", Sentiment = "Neutral" },
            new() { RiskScore = 30, Headline = "Low risk news", Source = "Source4", Sentiment = "Positive" }
        };

        _mockAdverseMediaService
            .Setup(x => x.ScanEntityAsync(It.IsAny<AdverseMediaScanRequest>()))
            .ReturnsAsync(mockMediaResults);

        _mockAlertService
            .Setup(x => x.CreateFromAdverseMediaAsync(It.IsAny<CreateMediaAlertRequest>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.ScanCustomerAsync(customer.Id);

        // Assert
        result.MediaResultsFound.Should().Be(4);
        result.HighRiskResults.Should().Be(2); // >= 75
        result.MediumRiskResults.Should().Be(1); // >= 50 && < 75
        result.LowRiskResults.Should().Be(1); // < 50
        result.AlertsCreated.Should().Be(2); // Only high-risk results

        // Verify alert service was called twice (for high-risk results only)
        _mockAlertService.Verify(
            x => x.CreateFromAdverseMediaAsync(It.IsAny<CreateMediaAlertRequest>()), 
            Times.Exactly(2));
    }

    [Fact]
    public async Task RescanCustomerAsync_ShouldCallScanCustomerAsync()
    {
        // Arrange
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            FullName = "Rescan Customer",
            IsActive = true
        };
        
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        _mockAdverseMediaService
            .Setup(x => x.ScanEntityAsync(It.IsAny<AdverseMediaScanRequest>()))
            .ReturnsAsync(new List<AdverseMediaResult>());

        // Act
        var result = await _service.RescanCustomerAsync(customer.Id);

        // Assert
        result.Should().NotBeNull();
        result.CustomerId.Should().Be(customer.Id);
        result.Status.Should().Be("Completed");
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}

// Mock classes for testing
public class AdverseMediaResult
{
    public double RiskScore { get; set; }
    public string Headline { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Sentiment { get; set; } = string.Empty;
    public DateTime PublishedDate { get; set; } = DateTime.UtcNow;
    public List<string> RiskCategories { get; set; } = new();
    public string Excerpt { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}
