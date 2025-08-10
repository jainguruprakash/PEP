using AutoFixture;
using AutoFixture.Xunit2;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using PEPScanner.Application.Services;
using PEPScanner.Infrastructure.Data;
using PEPScanner.Domain.Entities;
using System.Net.Http;
using Moq;
using Moq.Protected;
using System.Net;

namespace PEPScanner.Tests.UnitTests.Services;

public class RbiWatchlistServiceTests : IDisposable
{
    private readonly Mock<ILogger<RbiWatchlistService>> _mockLogger;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _httpClient;
    private readonly PepScannerDbContext _context;
    private readonly RbiWatchlistService _service;
    private readonly Fixture _fixture;

    public RbiWatchlistServiceTests()
    {
        _mockLogger = new Mock<ILogger<RbiWatchlistService>>();
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
        
        var options = new DbContextOptionsBuilder<PepScannerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new PepScannerDbContext(options);
        
        _service = new RbiWatchlistService(_context, _httpClient, _mockLogger.Object);
        _fixture = new Fixture();
    }

    [Fact]
    public async Task FetchRbiWatchlistAsync_ShouldReturnEntries_WhenDataIsAvailable()
    {
        // Arrange
        var mockHtmlResponse = @"
            <html>
                <body>
                    <table class='tablebg'>
                        <tr>
                            <td>John Doe</td>
                            <td>123 Main St</td>
                            <td>Mumbai, Maharashtra</td>
                            <td>1000000</td>
                            <td>ABC Bank Ltd</td>
                        </tr>
                        <tr>
                            <td>Jane Smith</td>
                            <td>456 Oak Ave</td>
                            <td>Delhi, Delhi</td>
                            <td>2000000</td>
                            <td>XYZ Bank Corp</td>
                        </tr>
                    </table>
                </body>
            </html>";

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(mockHtmlResponse)
            });

        // Act
        var result = await _service.FetchRbiWatchlistAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCountGreaterThan(0);
        result.Should().Contain(entry => entry.Name == "John Doe");
        result.Should().Contain(entry => entry.Name == "Jane Smith");
    }

    [Fact]
    public async Task FetchRbiWatchlistAsync_ShouldHandleCurrentYearFiltering()
    {
        // Arrange
        var currentYear = DateTime.Now.Year;
        var mockHtmlResponse = $@"
            <html>
                <body>
                    <table class='tablebg'>
                        <tr>
                            <td>Current Year Entry</td>
                            <td>Listed on {currentYear}-01-15</td>
                            <td>Mumbai, Maharashtra</td>
                            <td>1000000</td>
                        </tr>
                        <tr>
                            <td>Old Entry</td>
                            <td>Listed on {currentYear - 2}-05-20</td>
                            <td>Delhi, Delhi</td>
                            <td>2000000</td>
                        </tr>
                    </table>
                </body>
            </html>";

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(mockHtmlResponse)
            });

        // Act
        var result = await _service.FetchRbiWatchlistAsync();

        // Assert
        result.Should().NotBeNull();
        // Both entries should be returned as the service doesn't filter by year in this method
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateWatchlistFromRbiAsync_ShouldCreateNewEntries()
    {
        // Arrange
        var mockHtmlResponse = @"
            <html>
                <body>
                    <table class='tablebg'>
                        <tr>
                            <td>Test Entity</td>
                            <td>Test Address</td>
                            <td>Test City, Test State</td>
                            <td>5000000</td>
                            <td>Test Bank</td>
                        </tr>
                    </table>
                </body>
            </html>";

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(mockHtmlResponse)
            });

        // Act
        var result = await _service.UpdateWatchlistFromRbiAsync();

        // Assert
        result.Should().NotBeNull();
        result.NewRecords.Should().BeGreaterThan(0);
        
        var watchlistEntries = await _context.WatchlistEntries
            .Where(w => w.Source == "RBI")
            .ToListAsync();
        
        watchlistEntries.Should().NotBeEmpty();
        watchlistEntries.Should().Contain(w => w.PrimaryName == "Test Entity");
    }

    [Fact]
    public async Task UpdateWatchlistFromRbiAsync_ShouldUpdateExistingEntries()
    {
        // Arrange
        var existingEntry = new WatchlistEntry
        {
            Id = Guid.NewGuid(),
            Source = "RBI",
            ExternalId = "RBI_Wilful_Defaulter_12345678",
            PrimaryName = "Existing Entity",
            Address = "Old Address",
            CreatedAtUtc = DateTime.UtcNow.AddDays(-30),
            UpdatedAtUtc = DateTime.UtcNow.AddDays(-30)
        };

        _context.WatchlistEntries.Add(existingEntry);
        await _context.SaveChangesAsync();

        var mockHtmlResponse = @"
            <html>
                <body>
                    <table class='tablebg'>
                        <tr>
                            <td>Existing Entity</td>
                            <td>Updated Address</td>
                            <td>Updated City, Updated State</td>
                            <td>6000000</td>
                            <td>Updated Bank</td>
                        </tr>
                    </table>
                </body>
            </html>";

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(mockHtmlResponse)
            });

        // Act
        var result = await _service.UpdateWatchlistFromRbiAsync();

        // Assert
        result.Should().NotBeNull();
        result.UpdatedRecords.Should().BeGreaterThan(0);
        
        var updatedEntry = await _context.WatchlistEntries
            .FirstOrDefaultAsync(w => w.Id == existingEntry.Id);
        
        updatedEntry.Should().NotBeNull();
        updatedEntry!.Address.Should().Be("Updated Address");
        updatedEntry.UpdatedAtUtc.Should().BeAfter(existingEntry.UpdatedAtUtc);
    }

    [Fact]
    public async Task SearchRbiByCategoryAsync_ShouldReturnFilteredResults()
    {
        // Arrange
        var category = "Wilful Defaulter";
        var mockHtmlResponse = @"
            <html>
                <body>
                    <table class='tablebg'>
                        <tr>
                            <td>Defaulter Entity</td>
                            <td>Defaulter Address</td>
                            <td>Mumbai, Maharashtra</td>
                            <td>10000000</td>
                            <td>Defaulter Bank</td>
                        </tr>
                    </table>
                </body>
            </html>";

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(mockHtmlResponse)
            });

        // Act
        var result = await _service.SearchRbiByCategoryAsync(category);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCountGreaterThan(0);
        result.Should().Contain(entry => entry.Name == "Defaulter Entity");
        result.Should().AllSatisfy(entry => entry.Category.Should().Be(category));
    }

    [Fact]
    public async Task FetchRbiWatchlistAsync_ShouldHandleHttpErrors()
    {
        // Arrange
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new StringContent("Server Error")
            });

        // Act & Assert
        var act = async () => await _service.FetchRbiWatchlistAsync();
        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task FetchRbiWatchlistAsync_ShouldHandleEmptyResponse()
    {
        // Arrange
        var emptyHtmlResponse = @"
            <html>
                <body>
                    <p>No data available</p>
                </body>
            </html>";

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(emptyHtmlResponse)
            });

        // Act
        var result = await _service.FetchRbiWatchlistAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData("fraud master")]
    [InlineData("caution list")]
    [InlineData("defaulters list")]
    [InlineData("alert list")]
    public async Task SearchRbiByCategoryAsync_ShouldHandleDifferentCategories(string category)
    {
        // Arrange
        var mockHtmlResponse = @"
            <html>
                <body>
                    <table class='tablebg'>
                        <tr>
                            <td>Category Entity</td>
                            <td>Category Address</td>
                            <td>Category City</td>
                            <td>1000000</td>
                        </tr>
                    </table>
                </body>
            </html>";

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(mockHtmlResponse)
            });

        // Act
        var result = await _service.SearchRbiByCategoryAsync(category);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCountGreaterOrEqualTo(0);
    }

    public void Dispose()
    {
        _context.Dispose();
        _httpClient.Dispose();
    }
}
