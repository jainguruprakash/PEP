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
            base.OnModelCreating(modelBuilder);

            // Configure Customer relationships
            ConfigureCustomerRelationships(modelBuilder);

            // Configure Organization relationships
            ConfigureOrganizationRelationships(modelBuilder);

            // Configure Watchlist relationships
            ConfigureWatchlistRelationships(modelBuilder);

            // Configure Alert relationships
            ConfigureAlertRelationships(modelBuilder);

            // Configure Screening relationships
            ConfigureScreeningRelationships(modelBuilder);
        }

        private void ConfigureCustomerRelationships(ModelBuilder modelBuilder)
        {
            // Customer -> Organization (Many-to-One)
            modelBuilder.Entity<Customer>()
                .HasOne(c => c.Organization)
                .WithMany()
                .HasForeignKey(c => c.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            // Customer -> CustomerRelationships (One-to-Many)
            modelBuilder.Entity<Customer>()
                .HasMany(c => c.Relationships)
                .WithOne(cr => cr.Customer)
                .HasForeignKey(cr => cr.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Customer -> CustomerDocuments (One-to-Many)
            modelBuilder.Entity<Customer>()
                .HasMany(c => c.Documents)
                .WithOne(cd => cd.Customer)
                .HasForeignKey(cd => cd.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Customer -> Alerts (One-to-Many)
            modelBuilder.Entity<Customer>()
                .HasMany(c => c.Alerts)
                .WithOne(a => a.Customer)
                .HasForeignKey(a => a.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            // CustomerRelationship self-referencing relationship
            modelBuilder.Entity<CustomerRelationship>()
                .HasOne(cr => cr.RelatedCustomer)
                .WithMany()
                .HasForeignKey(cr => cr.RelatedCustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        private void ConfigureOrganizationRelationships(ModelBuilder modelBuilder)
        {
            // Organization -> OrganizationUsers (One-to-Many)
            modelBuilder.Entity<OrganizationUser>()
                .HasOne<Organization>()
                .WithMany()
                .HasForeignKey(ou => ou.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            // Organization -> OrganizationConfigurations (One-to-Many)
            modelBuilder.Entity<OrganizationConfiguration>()
                .HasOne<Organization>()
                .WithMany()
                .HasForeignKey(oc => oc.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            // Organization -> OrganizationWatchlists (One-to-Many)
            modelBuilder.Entity<OrganizationWatchlist>()
                .HasOne<Organization>()
                .WithMany()
                .HasForeignKey(ow => ow.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        private void ConfigureWatchlistRelationships(ModelBuilder modelBuilder)
        {
            // Configure WatchlistEntry indexes for better performance
            modelBuilder.Entity<WatchlistEntry>()
                .HasIndex(w => w.PrimaryName)
                .HasDatabaseName("IX_WatchlistEntry_PrimaryName");

            modelBuilder.Entity<WatchlistEntry>()
                .HasIndex(w => w.Source)
                .HasDatabaseName("IX_WatchlistEntry_Source");

            modelBuilder.Entity<WatchlistEntry>()
                .HasIndex(w => new { w.Source, w.PrimaryName })
                .HasDatabaseName("IX_WatchlistEntry_Source_PrimaryName");
        }

        private void ConfigureAlertRelationships(ModelBuilder modelBuilder)
        {
            // Alert -> Customer (Many-to-One, optional)
            modelBuilder.Entity<Alert>()
                .HasOne(a => a.Customer)
                .WithMany(c => c.Alerts)
                .HasForeignKey(a => a.CustomerId)
                .OnDelete(DeleteBehavior.SetNull);

            // Alert -> WatchlistEntry (Many-to-One, optional)
            modelBuilder.Entity<Alert>()
                .HasOne(a => a.WatchlistEntry)
                .WithMany()
                .HasForeignKey(a => a.WatchlistEntryId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure Alert indexes
            modelBuilder.Entity<Alert>()
                .HasIndex(a => a.Status)
                .HasDatabaseName("IX_Alert_Status");

            modelBuilder.Entity<Alert>()
                .HasIndex(a => a.CreatedAtUtc)
                .HasDatabaseName("IX_Alert_CreatedAtUtc");
        }

        private void ConfigureScreeningRelationships(ModelBuilder modelBuilder)
        {
            // Configure ScreeningJob indexes
            modelBuilder.Entity<ScreeningJob>()
                .HasIndex(sj => sj.Status)
                .HasDatabaseName("IX_ScreeningJob_Status");

            modelBuilder.Entity<ScreeningJob>()
                .HasIndex(sj => sj.CreatedAtUtc)
                .HasDatabaseName("IX_ScreeningJob_CreatedAtUtc");
        }
    }
}
