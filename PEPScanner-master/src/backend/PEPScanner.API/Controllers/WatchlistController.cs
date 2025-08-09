using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PEPScanner.API.Services;
using PEPScanner.Domain.Entities;

namespace PEPScanner.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WatchlistController : ControllerBase
    {
        private readonly IOfacDataService _ofacService;
        private readonly IUnSanctionsService _unService;
        private readonly IRbiWatchlistService _rbiService;
        private readonly IInHouseFileProcessorService _inHouseService;
        private readonly SebiWatchlistService _sebiService;
        private readonly IndianParliamentWatchlistService _parliamentService;
        private readonly ILogger<WatchlistController> _logger;

        public WatchlistController(
            IOfacDataService ofacService,
            IUnSanctionsService unService,
            IRbiWatchlistService rbiService,
            IInHouseFileProcessorService inHouseService,
            SebiWatchlistService sebiService,
            IndianParliamentWatchlistService parliamentService,
            ILogger<WatchlistController> logger)
        {
            _ofacService = ofacService;
            _unService = unService;
            _rbiService = rbiService;
            _inHouseService = inHouseService;
            _sebiService = sebiService;
            _parliamentService = parliamentService;
            _logger = logger;
        }

        /// <summary>
        /// Update OFAC watchlist from official source
        /// </summary>
        [HttpPost("ofac/update")]
        [Authorize(Policy = "Manager")]
        public async Task<ActionResult<WatchlistUpdateResult>> UpdateOfacWatchlist()
        {
            try
            {
                _logger.LogInformation("Starting OFAC watchlist update");
                var result = await _ofacService.UpdateWatchlistFromOfacAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating OFAC watchlist");
                return StatusCode(500, new { error = "Failed to update OFAC watchlist", details = ex.Message });
            }
        }

        /// <summary>
        /// Update UN sanctions watchlist from official source
        /// </summary>
        [HttpPost("un/update")]
        [Authorize(Policy = "Manager")]
        public async Task<ActionResult<WatchlistUpdateResult>> UpdateUnWatchlist()
        {
            try
            {
                _logger.LogInformation("Starting UN sanctions watchlist update");
                var result = await _unService.UpdateWatchlistFromUnAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating UN sanctions watchlist");
                return StatusCode(500, new { error = "Failed to update UN sanctions watchlist", details = ex.Message });
            }
        }

        /// <summary>
        /// Update RBI watchlist from official source
        /// </summary>
        [HttpPost("rbi/update")]
        [Authorize(Policy = "Manager")]
        public async Task<ActionResult<WatchlistUpdateResult>> UpdateRbiWatchlist()
        {
            try
            {
                _logger.LogInformation("Starting RBI watchlist update");
                var result = await _rbiService.UpdateWatchlistFromRbiAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating RBI watchlist");
                return StatusCode(500, new { error = "Failed to update RBI watchlist", details = ex.Message });
            }
        }

        /// <summary>
        /// Upload and process RBI watchlist file
        /// </summary>
        [HttpPost("rbi/upload")]
        [Authorize(Policy = "ComplianceOfficer")]
        public async Task<ActionResult<WatchlistUpdateResult>> UploadRbiFile(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { error = "No file provided" });

                _logger.LogInformation("Processing RBI file upload: {FileName}", file.FileName);
                var result = await _rbiService.ProcessRbiFileAsync(file);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing RBI file upload");
                return StatusCode(500, new { error = "Failed to process RBI file", details = ex.Message });
            }
        }

        /// <summary>
        /// Perform advanced RBI web scraping
        /// </summary>
        [HttpPost("rbi/scrape-advanced")]
        [Authorize(Policy = "Manager")]
        public async Task<ActionResult<WatchlistUpdateResult>> ScrapeRbiAdvanced()
        {
            try
            {
                _logger.LogInformation("Starting advanced RBI scraping");
                var result = await _rbiService.ScrapeRbiAdvancedAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing advanced RBI scraping");
                return StatusCode(500, new { error = "Failed to perform advanced RBI scraping", details = ex.Message });
            }
        }

        /// <summary>
        /// Search RBI data by category
        /// </summary>
        [HttpGet("rbi/search-by-category")]
        [Authorize(Policy = "ComplianceOfficer")]
        public async Task<ActionResult<List<RbiEntry>>> SearchRbiByCategory([FromQuery] string category)
        {
            try
            {
                if (string.IsNullOrEmpty(category))
                {
                    return BadRequest(new { error = "Category parameter is required" });
                }

                _logger.LogInformation("Searching RBI data by category: {Category}", category);
                var result = await _rbiService.SearchRbiByCategoryAsync(category);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching RBI data by category: {Category}", category);
                return StatusCode(500, new { error = "Failed to search RBI data by category", details = ex.Message });
            }
        }

        // SEBI Endpoints
        /// <summary>
        /// Update SEBI watchlist from official source
        /// </summary>
        [HttpPost("sebi/update")]
        [Authorize(Policy = "Manager")]
        public async Task<ActionResult<WatchlistUpdateResult>> UpdateSebiWatchlist()
        {
            try
            {
                _logger.LogInformation("Starting SEBI watchlist update");
                var result = await _sebiService.UpdateWatchlistAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating SEBI watchlist");
                return StatusCode(500, new { error = "Failed to update SEBI watchlist", details = ex.Message });
            }
        }

        /// <summary>
        /// Search SEBI data by name
        /// </summary>
        [HttpGet("sebi/search")]
        [Authorize(Policy = "ComplianceOfficer")]
        public async Task<ActionResult<List<WatchlistEntry>>> SearchSebiByName([FromQuery] string name)
        {
            try
            {
                if (string.IsNullOrEmpty(name))
                {
                    return BadRequest(new { error = "Name parameter is required" });
                }

                _logger.LogInformation("Searching SEBI data for name: {Name}", name);
                var result = await _sebiService.SearchByNameAsync(name);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching SEBI data for name: {Name}", name);
                return StatusCode(500, new { error = "Failed to search SEBI data", details = ex.Message });
            }
        }

        /// <summary>
        /// Search SEBI data by category
        /// </summary>
        [HttpGet("sebi/search-by-category")]
        [Authorize(Policy = "ComplianceOfficer")]
        public async Task<ActionResult<List<WatchlistEntry>>> SearchSebiByCategory([FromQuery] string category)
        {
            try
            {
                if (string.IsNullOrEmpty(category))
                {
                    return BadRequest(new { error = "Category parameter is required" });
                }

                _logger.LogInformation("Searching SEBI data by category: {Category}", category);
                var result = await _sebiService.SearchSebiByCategoryAsync(category);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching SEBI data by category: {Category}", category);
                return StatusCode(500, new { error = "Failed to search SEBI data by category", details = ex.Message });
            }
        }

        /// <summary>
        /// Perform advanced SEBI web scraping
        /// </summary>
        [HttpPost("sebi/scrape-advanced")]
        [Authorize(Policy = "Manager")]
        public async Task<ActionResult<WatchlistUpdateResult>> ScrapeSebiAdvanced()
        {
            try
            {
                _logger.LogInformation("Starting advanced SEBI scraping");
                var result = await _sebiService.ScrapeSebiAdvancedAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing advanced SEBI scraping");
                return StatusCode(500, new { error = "Failed to perform advanced SEBI scraping", details = ex.Message });
            }
        }

        // Parliament Endpoints
        /// <summary>
        /// Update Indian Parliament watchlist from official source
        /// </summary>
        [HttpPost("parliament/update")]
        [Authorize(Policy = "Manager")]
        public async Task<ActionResult<WatchlistUpdateResult>> UpdateParliamentWatchlist()
        {
            try
            {
                _logger.LogInformation("Starting Indian Parliament watchlist update");
                var result = await _parliamentService.UpdateWatchlistAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Indian Parliament watchlist");
                return StatusCode(500, new { error = "Failed to update Indian Parliament watchlist", details = ex.Message });
            }
        }

        /// <summary>
        /// Search Indian Parliament data by name
        /// </summary>
        [HttpGet("parliament/search")]
        [Authorize(Policy = "ComplianceOfficer")]
        public async Task<ActionResult<List<WatchlistEntry>>> SearchParliamentByName([FromQuery] string name)
        {
            try
            {
                if (string.IsNullOrEmpty(name))
                {
                    return BadRequest(new { error = "Name parameter is required" });
                }

                _logger.LogInformation("Searching Indian Parliament data for name: {Name}", name);
                var result = await _parliamentService.SearchByNameAsync(name);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching Indian Parliament data for name: {Name}", name);
                return StatusCode(500, new { error = "Failed to search Indian Parliament data", details = ex.Message });
            }
        }

        /// <summary>
        /// Search Indian Parliament data by category
        /// </summary>
        [HttpGet("parliament/search-by-category")]
        [Authorize(Policy = "ComplianceOfficer")]
        public async Task<ActionResult<List<WatchlistEntry>>> SearchParliamentByCategory([FromQuery] string category)
        {
            try
            {
                if (string.IsNullOrEmpty(category))
                {
                    return BadRequest(new { error = "Category parameter is required" });
                }

                _logger.LogInformation("Searching Indian Parliament data by category: {Category}", category);
                var result = await _parliamentService.SearchParliamentByCategoryAsync(category);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching Indian Parliament data by category: {Category}", category);
                return StatusCode(500, new { error = "Failed to search Indian Parliament data by category", details = ex.Message });
            }
        }

        /// <summary>
        /// Perform advanced Indian Parliament web scraping
        /// </summary>
        [HttpPost("parliament/scrape-advanced")]
        [Authorize(Policy = "Manager")]
        public async Task<ActionResult<WatchlistUpdateResult>> ScrapeParliamentAdvanced()
        {
            try
            {
                _logger.LogInformation("Starting advanced Indian Parliament scraping");
                var result = await _parliamentService.ScrapeParliamentAdvancedAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing advanced Indian Parliament scraping");
                return StatusCode(500, new { error = "Failed to perform advanced Indian Parliament scraping", details = ex.Message });
            }
        }

        /// <summary>
        /// Upload and process in-house watchlist file
        /// </summary>
        [HttpPost("inhouse/upload")]
        [Authorize(Policy = "ComplianceOfficer")]
        public async Task<ActionResult<WatchlistUpdateResult>> UploadInHouseFile(
            IFormFile file, 
            [FromForm] string sourceName,
            [FromForm] string? fileFormat = null)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { error = "No file provided" });

                if (string.IsNullOrEmpty(sourceName))
                    return BadRequest(new { error = "Source name is required" });

                _logger.LogInformation("Processing in-house file upload: {FileName} for source: {SourceName}", file.FileName, sourceName);
                var result = await _inHouseService.ProcessFileAsync(file, sourceName, fileFormat);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing in-house file upload");
                return StatusCode(500, new { error = "Failed to process in-house file", details = ex.Message });
            }
        }

        /// <summary>
        /// Validate in-house watchlist file before processing
        /// </summary>
        [HttpPost("inhouse/validate")]
        [Authorize(Policy = "ComplianceOfficer")]
        public async Task<ActionResult<FileValidationResult>> ValidateInHouseFile(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { error = "No file provided" });

                _logger.LogInformation("Validating in-house file: {FileName}", file.FileName);
                var result = await _inHouseService.ValidateFileAsync(file);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating in-house file");
                return StatusCode(500, new { error = "Failed to validate file", details = ex.Message });
            }
        }

        /// <summary>
        /// Get supported file formats for in-house uploads
        /// </summary>
        [HttpGet("inhouse/formats")]
        [Authorize(Policy = "ComplianceOfficer")]
        public async Task<ActionResult<List<string>>> GetSupportedFormats()
        {
            try
            {
                var formats = await _inHouseService.GetSupportedFormatsAsync();
                return Ok(formats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting supported formats");
                return StatusCode(500, new { error = "Failed to get supported formats", details = ex.Message });
            }
        }

        /// <summary>
        /// Search OFAC data by name
        /// </summary>
        [HttpGet("ofac/search")]
        [Authorize(Policy = "ComplianceOfficer")]
        public async Task<ActionResult<List<OfacEntry>>> SearchOfacByName([FromQuery] string name)
        {
            try
            {
                if (string.IsNullOrEmpty(name))
                    return BadRequest(new { error = "Name parameter is required" });

                _logger.LogInformation("Searching OFAC data for name: {Name}", name);
                var results = await _ofacService.SearchByNameAsync(name);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching OFAC data");
                return StatusCode(500, new { error = "Failed to search OFAC data", details = ex.Message });
            }
        }

        /// <summary>
        /// Search UN sanctions data by name
        /// </summary>
        [HttpGet("un/search")]
        [Authorize(Policy = "ComplianceOfficer")]
        public async Task<ActionResult<List<UnSanctionsEntry>>> SearchUnByName([FromQuery] string name)
        {
            try
            {
                if (string.IsNullOrEmpty(name))
                    return BadRequest(new { error = "Name parameter is required" });

                _logger.LogInformation("Searching UN sanctions data for name: {Name}", name);
                var results = await _unService.SearchByNameAsync(name);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching UN sanctions data");
                return StatusCode(500, new { error = "Failed to search UN sanctions data", details = ex.Message });
            }
        }

        /// <summary>
        /// Search RBI data by name
        /// </summary>
        [HttpGet("rbi/search")]
        [Authorize(Policy = "ComplianceOfficer")]
        public async Task<ActionResult<List<RbiEntry>>> SearchRbiByName([FromQuery] string name)
        {
            try
            {
                if (string.IsNullOrEmpty(name))
                    return BadRequest(new { error = "Name parameter is required" });

                _logger.LogInformation("Searching RBI data for name: {Name}", name);
                var results = await _rbiService.SearchByNameAsync(name);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching RBI data");
                return StatusCode(500, new { error = "Failed to search RBI data", details = ex.Message });
            }
        }

        /// <summary>
        /// Get last update timestamps for all watchlist sources
        /// </summary>
        [HttpGet("last-updates")]
        [Authorize(Policy = "ComplianceOfficer")]
        public async Task<ActionResult<Dictionary<string, DateTime?>>> GetLastUpdateTimestamps()
        {
            try
            {
                var timestamps = new Dictionary<string, DateTime?>
                {
                    ["OFAC"] = await _ofacService.GetLastUpdateTimestampAsync(),
                    ["UN"] = null, // UN service doesn't have this method yet
                    ["RBI"] = null  // RBI service doesn't have this method yet
                };

                return Ok(timestamps);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting last update timestamps");
                return StatusCode(500, new { error = "Failed to get last update timestamps", details = ex.Message });
            }
        }

        /// <summary>
        /// Get available watchlist sources
        /// </summary>
        [HttpGet("sources")]
        [Authorize(Policy = "ComplianceOfficer")]
        public ActionResult<List<WatchlistSource>> GetAvailableSources()
        {
            try
            {
                var sources = new List<WatchlistSource>
                {
                    new WatchlistSource
                    {
                        Name = "OFAC",
                        DisplayName = "OFAC Sanctions",
                        Type = "Global",
                        Country = "United States",
                        Description = "Office of Foreign Assets Control sanctions list",
                        UpdateFrequency = "Daily",
                        DataFormat = "CSV",
                        FileUrl = "https://www.treasury.gov/ofac/downloads/sdn.csv"
                    },
                    new WatchlistSource
                    {
                        Name = "UN",
                        DisplayName = "UN Sanctions",
                        Type = "Global",
                        Country = "International",
                        Description = "United Nations Security Council sanctions list",
                        UpdateFrequency = "Daily",
                        DataFormat = "JSON",
                        FileUrl = "https://scsanctions.un.org/resources/json/en/consolidated.json"
                    },
                    new WatchlistSource
                    {
                        Name = "RBI",
                        DisplayName = "RBI Watchlist",
                        Type = "Local",
                        Country = "India",
                        Description = "Reserve Bank of India wilful defaulters and fraud lists",
                        UpdateFrequency = "Weekly",
                        DataFormat = "CSV/Excel",
                        FileUrl = "https://www.rbi.org.in/Scripts/bs_viewcontent.aspx?Id=2009"
                    },
                    new WatchlistSource
                    {
                        Name = "InHouse",
                        DisplayName = "In-House Lists",
                        Type = "InHouse",
                        Country = "India",
                        Description = "Bank's internal watchlists and blacklists",
                        UpdateFrequency = "As needed",
                        DataFormat = "CSV/Excel/JSON"
                    }
                };

                return Ok(sources);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available sources");
                return StatusCode(500, new { error = "Failed to get available sources", details = ex.Message });
            }
        }
    }
}
