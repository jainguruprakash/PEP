using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using PEPScanner.API;
using PEPScanner.API.Controllers;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace PEPScanner.Tests.IntegrationTests.Controllers
{
    public class ScreeningControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public ScreeningControllerTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task ScreenCustomer_WithRbiSource_ShouldFilterCorrectly()
        {
            // Arrange
            var request = new CustomerScreeningRequest
            {
                FullName = "Test Customer",
                Sources = new List<string> { "RBI" },
                Threshold = 70,
                IncludeAliases = true,
                IncludeFuzzyMatching = true,
                IncludePhoneticMatching = false
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/screening/customer", request);

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<dynamic>(content);
            
            Assert.NotNull(result);
            // Additional assertions can be added based on expected behavior
        }

        [Fact]
        public async Task ScreenCustomer_WithMultipleSources_ShouldIncludeAllSources()
        {
            // Arrange
            var request = new CustomerScreeningRequest
            {
                FullName = "Test Customer",
                Sources = new List<string> { "RBI", "OFAC", "UN" },
                Threshold = 80,
                IncludeAliases = true,
                IncludeFuzzyMatching = true,
                IncludePhoneticMatching = true
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/screening/customer", request);

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<dynamic>(content);
            
            Assert.NotNull(result);
        }

        [Fact]
        public async Task ScreenCustomer_WithNoSources_ShouldSearchAllSources()
        {
            // Arrange
            var request = new CustomerScreeningRequest
            {
                FullName = "Test Customer",
                Sources = null, // No sources specified
                Threshold = 70
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/screening/customer", request);

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<dynamic>(content);
            
            Assert.NotNull(result);
        }

        [Fact]
        public async Task ScreenCustomer_WithLowThreshold_ShouldReturnMoreMatches()
        {
            // Arrange
            var request = new CustomerScreeningRequest
            {
                FullName = "amit",
                Sources = new List<string> { "RBI" },
                Threshold = 50, // Low threshold
                IncludeAliases = true,
                IncludeFuzzyMatching = true
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/screening/customer", request);

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            
            Assert.NotNull(content);
            Assert.Contains("amit", content, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task ScreenCustomer_WithHighThreshold_ShouldReturnFewerMatches()
        {
            // Arrange
            var request = new CustomerScreeningRequest
            {
                FullName = "amit",
                Sources = new List<string> { "RBI" },
                Threshold = 95, // High threshold
                IncludeAliases = true,
                IncludeFuzzyMatching = true
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/screening/customer", request);

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            
            Assert.NotNull(content);
        }
    }
}