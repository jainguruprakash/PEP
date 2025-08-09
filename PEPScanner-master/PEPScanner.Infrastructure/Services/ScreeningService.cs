using Microsoft.EntityFrameworkCore;
using PEPScanner.Domain.Entities;
using PEPScanner.Application.Abstractions;

namespace PEPScanner.Infrastructure.Services
{
    public class ScreeningService : IScreeningService
    {
        private readonly PepScannerDbContext _context;
        private readonly INameMatchingService _nameMatchingService;
        private readonly ILogger<ScreeningService> _logger;

        public ScreeningService(
            PepScannerDbContext context,
            INameMatchingService nameMatchingService,
            ILogger<ScreeningService> logger)
        {
            _context = context;
            _nameMatchingService = nameMatchingService;
            _logger = logger;
        }

        // ... (rest of the implementation remains unchanged)
    }
}
