using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PEPScanner.API.Services;

namespace PEPScanner.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TestController : ControllerBase
    {
        private readonly IRbiWatchlistService _rbiService;
        private readonly SebiWatchlistService _sebiService;
        private readonly IndianParliamentWatchlistService _parliamentService;
        private readonly ILogger<TestController> _logger;

        public TestController(
            IRbiWatchlistService rbiService,
            SebiWatchlistService sebiService,
            IndianParliamentWatchlistService parliamentService,
            ILogger<TestController> logger)
        {
            _rbiService = rbiService;
            _sebiService = sebiService;
            _parliamentService = parliamentService;
            _logger = logger;
        }

        /// <summary>
        /// Test RBI scraping functionality
        /// </summary>
        [HttpGet("rbi-scraping")]
        [Authorize(Policy = "Manager")]
        public async Task<ActionResult<object>> TestRbiScraping()
        {
            try
            {
                _logger.LogInformation("Testing RBI scraping functionality");
                
                // Test basic scraping
                var basicResults = await _rbiService.FetchRbiWatchlistAsync();
                
                // Test category-specific scraping
                var wilfulDefaulters = await _rbiService.SearchRbiByCategoryAsync("Wilful Defaulter");
                var fraudMaster = await _rbiService.SearchRbiByCategoryAsync("Fraud Master");
                var cautionList = await _rbiService.SearchRbiByCategoryAsync("Caution List");
                
                var result = new
                {
                    Timestamp = DateTime.UtcNow,
                    BasicScraping = new
                    {
                        TotalEntries = basicResults.Count,
                        SampleEntries = basicResults.Take(5).Select(e => new { e.Name, e.Category, e.Designation })
                    },
                    CategoryScraping = new
                    {
                        WilfulDefaulters = new { Count = wilfulDefaulters.Count, Sample = wilfulDefaulters.Take(3).Select(e => e.Name) },
                        FraudMaster = new { Count = fraudMaster.Count, Sample = fraudMaster.Take(3).Select(e => e.Name) },
                        CautionList = new { Count = cautionList.Count, Sample = cautionList.Take(3).Select(e => e.Name) }
                    },
                    Status = "Success"
                };
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing RBI scraping");
                return StatusCode(500, new { error = "Failed to test RBI scraping", details = ex.Message });
            }
        }

        /// <summary>
        /// Test RBI name search functionality
        /// </summary>
        [HttpGet("rbi-search")]
        [Authorize(Policy = "ComplianceOfficer")]
        public async Task<ActionResult<object>> TestRbiSearch([FromQuery] string name = "test")
        {
            try
            {
                _logger.LogInformation("Testing RBI search functionality for name: {Name}", name);
                
                var searchResults = await _rbiService.SearchByNameAsync(name);
                
                var result = new
                {
                    Timestamp = DateTime.UtcNow,
                    SearchTerm = name,
                    ResultsCount = searchResults.Count,
                    Results = searchResults.Take(10).Select(e => new { e.PrimaryName, e.RiskCategory, e.PositionOrRole, e.Address })
                };
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing RBI search");
                return StatusCode(500, new { error = "Failed to test RBI search", details = ex.Message });
            }
        }

        /// <summary>
        /// Test SEBI scraping functionality
        /// </summary>
        [HttpGet("sebi-scraping")]
        [Authorize(Policy = "Manager")]
        public async Task<ActionResult<object>> TestSebiScraping()
        {
            try
            {
                _logger.LogInformation("Testing SEBI scraping functionality");
                
                // Test basic scraping
                var basicResults = await _sebiService.FetchWatchlistDataAsync();
                
                // Test category-specific scraping
                var debarredEntities = await _sebiService.SearchSebiByCategoryAsync("Debarred Entity");
                var defaulters = await _sebiService.SearchSebiByCategoryAsync("Defaulter");
                var suspendedEntities = await _sebiService.SearchSebiByCategoryAsync("Suspended Entity");
                
                var result = new
                {
                    Timestamp = DateTime.UtcNow,
                    BasicScraping = new
                    {
                        TotalEntries = basicResults.Count,
                        SampleEntries = basicResults.Take(5).Select(e => new { e.PrimaryName, e.RiskCategory, e.PositionOrRole })
                    },
                    CategoryScraping = new
                    {
                        DebarredEntities = new { Count = debarredEntities.Count, Sample = debarredEntities.Take(3).Select(e => e.PrimaryName) },
                        Defaulters = new { Count = defaulters.Count, Sample = defaulters.Take(3).Select(e => e.PrimaryName) },
                        SuspendedEntities = new { Count = suspendedEntities.Count, Sample = suspendedEntities.Take(3).Select(e => e.PrimaryName) }
                    },
                    Status = "Success"
                };
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing SEBI scraping");
                return StatusCode(500, new { error = "Failed to test SEBI scraping", details = ex.Message });
            }
        }

        /// <summary>
        /// Test SEBI search functionality
        /// </summary>
        [HttpGet("sebi-search")]
        [Authorize(Policy = "ComplianceOfficer")]
        public async Task<ActionResult<object>> TestSebiSearch([FromQuery] string name = "test")
        {
            try
            {
                _logger.LogInformation("Testing SEBI search functionality for name: {Name}", name);
                
                var searchResults = await _sebiService.SearchByNameAsync(name);
                
                var result = new
                {
                    Timestamp = DateTime.UtcNow,
                    SearchTerm = name,
                    ResultsCount = searchResults.Count,
                    Results = searchResults.Take(10).Select(e => new { e.PrimaryName, e.RiskCategory, e.PositionOrRole, e.Country })
                };
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing SEBI search");
                return StatusCode(500, new { error = "Failed to test SEBI search", details = ex.Message });
            }
        }

        /// <summary>
        /// Test Indian Parliament scraping functionality
        /// </summary>
        [HttpGet("parliament-scraping")]
        [Authorize(Policy = "Manager")]
        public async Task<ActionResult<object>> TestParliamentScraping()
        {
            try
            {
                _logger.LogInformation("Testing Indian Parliament scraping functionality");
                
                // Test basic scraping
                var basicResults = await _parliamentService.FetchWatchlistDataAsync();
                
                // Test category-specific scraping
                var lokSabhaMembers = await _parliamentService.SearchParliamentByCategoryAsync("Lok Sabha Member");
                var rajyaSabhaMembers = await _parliamentService.SearchParliamentByCategoryAsync("Rajya Sabha Member");
                var ministers = await _parliamentService.SearchParliamentByCategoryAsync("Minister");
                
                var result = new
                {
                    Timestamp = DateTime.UtcNow,
                    BasicScraping = new
                    {
                        TotalEntries = basicResults.Count,
                        SampleEntries = basicResults.Take(5).Select(e => new { e.PrimaryName, e.RiskCategory, e.PositionOrRole })
                    },
                    CategoryScraping = new
                    {
                        LokSabhaMembers = new { Count = lokSabhaMembers.Count, Sample = lokSabhaMembers.Take(3).Select(e => e.PrimaryName) },
                        RajyaSabhaMembers = new { Count = rajyaSabhaMembers.Count, Sample = rajyaSabhaMembers.Take(3).Select(e => e.PrimaryName) },
                        Ministers = new { Count = ministers.Count, Sample = ministers.Take(3).Select(e => e.PrimaryName) }
                    },
                    Status = "Success"
                };
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing Indian Parliament scraping");
                return StatusCode(500, new { error = "Failed to test Indian Parliament scraping", details = ex.Message });
            }
        }

        /// <summary>
        /// Test Indian Parliament search functionality
        /// </summary>
        [HttpGet("parliament-search")]
        [Authorize(Policy = "ComplianceOfficer")]
        public async Task<ActionResult<object>> TestParliamentSearch([FromQuery] string name = "test")
        {
            try
            {
                _logger.LogInformation("Testing Indian Parliament search functionality for name: {Name}", name);
                
                var searchResults = await _parliamentService.SearchByNameAsync(name);
                
                var result = new
                {
                    Timestamp = DateTime.UtcNow,
                    SearchTerm = name,
                    ResultsCount = searchResults.Count,
                    Results = searchResults.Take(10).Select(e => new { e.PrimaryName, e.RiskCategory, e.PositionOrRole, e.Country })
                };
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing Indian Parliament search");
                return StatusCode(500, new { error = "Failed to test Indian Parliament search", details = ex.Message });
            }
        }
    }
}
