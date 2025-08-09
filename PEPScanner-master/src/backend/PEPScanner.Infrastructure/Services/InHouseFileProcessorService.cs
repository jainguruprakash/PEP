using Microsoft.EntityFrameworkCore;
using PEPScanner.Domain.Entities;
using PEPScanner.Application.Abstractions;
using CsvHelper;
using System.Globalization;
using System.Text.Json;
using OfficeOpenXml;
using System.Data;

namespace PEPScanner.Infrastructure.Services
{
    public class InHouseFileProcessorService : IInHouseFileProcessorService
    {
        private readonly PepScannerDbContext _context;
        private readonly ILogger<InHouseFileProcessorService> _logger;

        public InHouseFileProcessorService(
            PepScannerDbContext context,
            ILogger<InHouseFileProcessorService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ... (rest of the implementation remains unchanged)
    }
}
