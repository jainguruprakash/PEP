using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using PEPScanner.Application.Services;
using PEPScanner.Domain.Entities;
using PEPScanner.Infrastructure.Data;
using Xunit;

namespace PEPScanner.Tests.UnitTests.Services
{
    public class ScreeningServiceTests
    {
        private readonly Mock<ILogger<ScreeningService>> _mockLogger;
        private readonly Mock<INameMatchingService> _mockNameMatchingService;
        private readonly PepScannerDbContext _context;

        public ScreeningServiceTests()
        {
            _mockLogger = new Mock<ILogger<ScreeningService>>();
            _mockNameMatchingService = new Mock<INameMatchingService>();
            
            // Create in-memory database for testing
            var options = new DbContextOptionsBuilder<PepScannerDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new PepScannerDbContext(options);
        }

        [Fact]
        public async Task ScreenCustomerAsync_WithValidCustomer_ShouldReturnScreeningResult()
        {
            // Arrange
            var service = new ScreeningService(_context, _mockNameMatchingService.Object, _mockLogger.Object);
            var customer = new Customer
            {
                Id = Guid.NewGuid(),
                FullName = "Test Customer",
                Country = "India"
            };

            _mockNameMatchingService
                .Setup(x => x.MatchNameAsync(It.IsAny<string>(), It.IsAny<Customer>(), It.IsAny<double>()))
                .ReturnsAsync(new List<NameMatchResult>());

            // Act
            var result = await service.ScreenCustomerAsync(customer);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(customer.Id, result.CustomerId);
            Assert.Equal(customer.FullName, result.CustomerName);
            Assert.False(result.HasMatches);
        }

        [Fact]
        public async Task ScreenCustomerAsync_WithMatches_ShouldCreateAlerts()
        {
            // Arrange
            var service = new ScreeningService(_context, _mockNameMatchingService.Object, _mockLogger.Object);
            var customer = new Customer
            {
                Id = Guid.NewGuid(),
                FullName = "Test Customer",
                Country = "India"
            };

            var matchResults = new List<NameMatchResult>
            {
                new NameMatchResult
                {
                    WatchlistEntryId = Guid.NewGuid(),
                    ListType = "PEP",
                    SimilarityScore = 0.9,
                    RiskLevel = "High",
                    SourceList = "RBI",
                    MatchAlgorithm = "Fuzzy",
                    MatchedFields = "Name"
                }
            };

            _mockNameMatchingService
                .Setup(x => x.MatchNameAsync(It.IsAny<string>(), It.IsAny<Customer>(), It.IsAny<double>()))
                .ReturnsAsync(matchResults);

            // Act
            var result = await service.ScreenCustomerAsync(customer);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.HasMatches);
            Assert.Single(result.Alerts);
            Assert.Equal("PEP", result.Alerts.First().AlertType);
        }

        [Fact]
        public async Task SearchNamesAsync_WithRbiSource_ShouldFilterBySource()
        {
            // Arrange
            var service = new ScreeningService(_context, _mockNameMatchingService.Object, _mockLogger.Object);
            var searchRequest = new NameSearchRequest
            {
                Name = "Test Name",
                Sources = new List<string> { "RBI" },
                Threshold = 0.7,
                MaxResults = 50
            };

            var matchResults = new List<NameMatchResult>
            {
                new NameMatchResult
                {
                    SourceList = "RBI",
                    ListType = "PEP",
                    SimilarityScore = 0.8
                }
            };

            _mockNameMatchingService
                .Setup(x => x.MatchNameAsync(It.IsAny<string>(), It.IsAny<Customer>(), It.IsAny<double>()))
                .ReturnsAsync(matchResults);

            // Act
            var results = await service.SearchNamesAsync(searchRequest);

            // Assert
            Assert.NotNull(results);
            Assert.All(results, r => Assert.Equal("RBI", r.SourceList));
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }

    // Mock classes for testing
    public class NameMatchResult
    {
        public Guid WatchlistEntryId { get; set; }
        public string ListType { get; set; } = string.Empty;
        public double SimilarityScore { get; set; }
        public string RiskLevel { get; set; } = string.Empty;
        public string SourceList { get; set; } = string.Empty;
        public string MatchAlgorithm { get; set; } = string.Empty;
        public string MatchedFields { get; set; } = string.Empty;
    }

    public class NameSearchRequest
    {
        public string Name { get; set; } = string.Empty;
        public List<string> Sources { get; set; } = new();
        public double Threshold { get; set; }
        public int MaxResults { get; set; }
        public string Country { get; set; } = string.Empty;
        public string Nationality { get; set; } = string.Empty;
        public string DateOfBirth { get; set; } = string.Empty;
        public List<string> ListTypes { get; set; } = new();
    }
}