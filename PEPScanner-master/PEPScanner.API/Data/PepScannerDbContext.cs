// This is a compatibility shim for migration and existing references.
// The actual DbContext implementation is in PEPScanner.Infrastructure/Persistence/PepScannerDbContext.cs

using Microsoft.EntityFrameworkCore;

namespace PEPScanner.API.Data
{
    // Empty partial class that allows existing migrations to reference this namespace
    public partial class PepScannerDbContext : DbContext
    {
        public PepScannerDbContext(DbContextOptions<PepScannerDbContext> options) : base(options) 
        {
            // This constructor is only kept for compatibility with existing migrations
            // Actual implementation is in Infrastructure layer
        }
    }
}
