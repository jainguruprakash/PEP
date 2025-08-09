using Microsoft.EntityFrameworkCore;
using PEPScanner.Domain.Entities;

namespace PEPScanner.Infrastructure.Data
{
    public class PepScannerDbContext : DbContext
    {
        public PepScannerDbContext(DbContextOptions<PepScannerDbContext> options) : base(options)
        {
        }

        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<WatchlistEntry> WatchlistEntries => Set<WatchlistEntry>();
        public DbSet<Alert> Alerts => Set<Alert>();
        public DbSet<CustomerRelationship> CustomerRelationships => Set<CustomerRelationship>();
        public DbSet<CustomerDocument> CustomerDocuments => Set<CustomerDocument>();
        public DbSet<ScreeningJob> ScreeningJobs => Set<ScreeningJob>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
        public DbSet<NotificationLog> NotificationLogs => Set<NotificationLog>();
        
        // Organization and Multi-tenant support
        public DbSet<Organization> Organizations => Set<Organization>();
        public DbSet<OrganizationUser> OrganizationUsers => Set<OrganizationUser>();
        public DbSet<OrganizationWatchlist> OrganizationWatchlists => Set<OrganizationWatchlist>();
        public DbSet<OrganizationConfiguration> OrganizationConfigurations => Set<OrganizationConfiguration>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Apply all the entity configurations here
            // For now, let's use the data annotations on the entities
            base.OnModelCreating(modelBuilder);
        }
    }
}
