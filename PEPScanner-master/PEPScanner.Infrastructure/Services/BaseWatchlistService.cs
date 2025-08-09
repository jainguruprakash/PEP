using Microsoft.EntityFrameworkCore;
using PEPScanner.Domain.Entities;
using PEPScanner.Application.Abstractions;
using CsvHelper;
using System.Globalization;
using System.Text.Json;

namespace PEPScanner.Infrastructure.Services
{
    public abstract class BaseWatchlistService : IBaseWatchlistService
    {
        protected readonly PepScannerDbContext _context;
        protected readonly ILogger _logger;
        protected readonly IConfiguration _configuration;
        protected readonly HttpClient _httpClient;

        protected BaseWatchlistService(
            PepScannerDbContext context,
            ILogger logger,
            IConfiguration configuration,
            HttpClient httpClient)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
            _httpClient = httpClient;
        }

        // ... (rest of the implementation remains unchanged)
    }
}
