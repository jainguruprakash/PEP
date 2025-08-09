using Microsoft.EntityFrameworkCore;
using PEPScanner.API.Models;

namespace PEPScanner.API.Data
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
            // paste of current mappings
            // NOTE: intentionally identical to previous API/Data/PepScannerDbContext.cs for compatibility
            // Customer Configuration
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FullName).HasMaxLength(300).IsRequired();
                entity.Property(e => e.AliasNames).HasMaxLength(1000);
                entity.Property(e => e.Nationality).HasMaxLength(100);
                entity.Property(e => e.IdentificationNumber).HasMaxLength(50);
                entity.Property(e => e.IdentificationType).HasMaxLength(50);
                entity.Property(e => e.Address).HasMaxLength(500);
                entity.Property(e => e.City).HasMaxLength(100);
                entity.Property(e => e.State).HasMaxLength(100);
                entity.Property(e => e.PostalCode).HasMaxLength(10);
                entity.Property(e => e.Country).HasMaxLength(100);
                entity.Property(e => e.Occupation).HasMaxLength(200);
                entity.Property(e => e.Employer).HasMaxLength(200);
                entity.Property(e => e.PhoneNumber).HasMaxLength(100);
                entity.Property(e => e.EmailAddress).HasMaxLength(100);
                entity.Property(e => e.RiskLevel).HasMaxLength(50);
                entity.Property(e => e.PepPosition).HasMaxLength(200);
                entity.Property(e => e.PepCountry).HasMaxLength(100);
                entity.Property(e => e.PhotoUrl).HasMaxLength(500);
                entity.Property(e => e.FingerprintData).HasMaxLength(500);
                entity.Property(e => e.FaceBiometricData).HasMaxLength(500);
                entity.Property(e => e.ScreeningFrequency).HasMaxLength(50);
                entity.Property(e => e.EddNotes).HasMaxLength(1000);
                entity.Property(e => e.AccountNumber).HasMaxLength(50);
                entity.Property(e => e.AccountType).HasMaxLength(100);
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.Property(e => e.UpdatedBy).HasMaxLength(100);
                entity.HasIndex(e => e.FullName);
                entity.HasIndex(e => e.IdentificationNumber);
                entity.HasIndex(e => e.IsPep);
                entity.HasIndex(e => e.RiskLevel);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.CreatedAtUtc);
            });

            modelBuilder.Entity<WatchlistEntry>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Source).HasMaxLength(100).IsRequired();
                entity.Property(e => e.ListType).HasMaxLength(100).IsRequired();
                entity.Property(e => e.PrimaryName).HasMaxLength(300).IsRequired();
                entity.Property(e => e.AlternateNames).HasMaxLength(1000);
                entity.Property(e => e.Country).HasMaxLength(100);
                entity.Property(e => e.Gender).HasMaxLength(50);
                entity.Property(e => e.PositionOrRole).HasMaxLength(200);
                entity.Property(e => e.RiskCategory).HasMaxLength(100);
                entity.Property(e => e.RiskLevel).HasMaxLength(100);
                entity.Property(e => e.EntityType).HasMaxLength(100);
                entity.Property(e => e.Nationality).HasMaxLength(100);
                entity.Property(e => e.Citizenship).HasMaxLength(100);
                entity.Property(e => e.Address).HasMaxLength(500);
                entity.Property(e => e.City).HasMaxLength(100);
                entity.Property(e => e.State).HasMaxLength(100);
                entity.Property(e => e.PostalCode).HasMaxLength(100);
                entity.Property(e => e.CountryOfResidence).HasMaxLength(100);
                entity.Property(e => e.PassportNumber).HasMaxLength(100);
                entity.Property(e => e.NationalIdNumber).HasMaxLength(100);
                entity.Property(e => e.TaxIdNumber).HasMaxLength(100);
                entity.Property(e => e.RegistrationNumber).HasMaxLength(100);
                entity.Property(e => e.LicenseNumber).HasMaxLength(100);
                entity.Property(e => e.VesselName).HasMaxLength(100);
                entity.Property(e => e.VesselType).HasMaxLength(100);
                entity.Property(e => e.VesselFlag).HasMaxLength(100);
                entity.Property(e => e.VesselCallSign).HasMaxLength(100);
                entity.Property(e => e.VesselImoNumber).HasMaxLength(100);
                entity.Property(e => e.AircraftType).HasMaxLength(100);
                entity.Property(e => e.AircraftRegistration).HasMaxLength(100);
                entity.Property(e => e.AircraftOperator).HasMaxLength(100);
                entity.Property(e => e.PepPosition).HasMaxLength(200);
                entity.Property(e => e.PepCountry).HasMaxLength(100);
                entity.Property(e => e.PepCategory).HasMaxLength(100);
                entity.Property(e => e.PepDescription).HasMaxLength(1000);
                entity.Property(e => e.SanctionType).HasMaxLength(100);
                entity.Property(e => e.SanctionAuthority).HasMaxLength(100);
                entity.Property(e => e.SanctionReference).HasMaxLength(100);
                entity.Property(e => e.SanctionReason).HasMaxLength(1000);
                entity.Property(e => e.MediaSource).HasMaxLength(100);
                entity.Property(e => e.MediaSummary).HasMaxLength(1000);
                entity.Property(e => e.MediaUrl).HasMaxLength(500);
                entity.Property(e => e.MediaCategory).HasMaxLength(100);
                entity.Property(e => e.ExternalId).HasMaxLength(100);
                entity.Property(e => e.ExternalReference).HasMaxLength(100);
                entity.Property(e => e.Comments).HasMaxLength(1000);
                entity.Property(e => e.DataQuality).HasMaxLength(100);
                entity.Property(e => e.AddedBy).HasMaxLength(100);
                entity.Property(e => e.RemovedBy).HasMaxLength(100);
                entity.Property(e => e.UpdatedBy).HasMaxLength(100);
                entity.HasIndex(e => e.PrimaryName);
                entity.HasIndex(e => e.Source);
                entity.HasIndex(e => e.ListType);
                entity.HasIndex(e => e.Country);
                entity.HasIndex(e => e.RiskLevel);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.DateAddedUtc);
            });

            modelBuilder.Entity<Alert>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Context).HasMaxLength(50).IsRequired();
                entity.Property(e => e.AlertType).HasMaxLength(100).IsRequired();
                entity.Property(e => e.MatchAlgorithm).HasMaxLength(100);
                entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Priority).HasMaxLength(50);
                entity.Property(e => e.AssignedTo).HasMaxLength(100);
                entity.Property(e => e.ReviewedBy).HasMaxLength(100);
                entity.Property(e => e.EscalatedTo).HasMaxLength(100);
                entity.Property(e => e.OutcomeNotes).HasMaxLength(1000);
                entity.Property(e => e.Outcome).HasMaxLength(50);
                entity.Property(e => e.RiskLevel).HasMaxLength(100);
                entity.Property(e => e.ComplianceAction).HasMaxLength(100);
                entity.Property(e => e.StrReference).HasMaxLength(100);
                entity.Property(e => e.SarReference).HasMaxLength(100);
                entity.Property(e => e.StrFiledBy).HasMaxLength(100);
                entity.Property(e => e.SarFiledBy).HasMaxLength(100);
                entity.Property(e => e.MatchedFields).HasMaxLength(1000);
                entity.Property(e => e.MatchingDetails).HasMaxLength(1000);
                entity.Property(e => e.SourceList).HasMaxLength(100);
                entity.Property(e => e.SourceCategory).HasMaxLength(100);
                entity.Property(e => e.TransactionId).HasMaxLength(100);
                entity.Property(e => e.TransactionType).HasMaxLength(100);
                entity.Property(e => e.SlaStatus).HasMaxLength(50);
                entity.Property(e => e.Tags).HasMaxLength(500);
                entity.Property(e => e.Category).HasMaxLength(100);
                entity.Property(e => e.SubCategory).HasMaxLength(100);
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.Property(e => e.UpdatedBy).HasMaxLength(100);
                entity.HasOne(e => e.Customer).WithMany(c => c.Alerts).HasForeignKey(e => e.CustomerId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.WatchlistEntry).WithMany(w => w.Alerts).HasForeignKey(e => e.WatchlistEntryId).OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(e => e.CustomerId);
                entity.HasIndex(e => e.WatchlistEntryId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.Priority);
                entity.HasIndex(e => e.AssignedTo);
                entity.HasIndex(e => e.CreatedAtUtc);
                entity.HasIndex(e => e.SimilarityScore);
            });

            modelBuilder.Entity<CustomerRelationship>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.RelationshipType).HasMaxLength(100).IsRequired();
                entity.Property(e => e.RelationshipDetails).HasMaxLength(500);
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.Property(e => e.UpdatedBy).HasMaxLength(100);
                entity.HasOne(e => e.Customer).WithMany(c => c.Relationships).HasForeignKey(e => e.CustomerId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.RelatedCustomer).WithMany().HasForeignKey(e => e.RelatedCustomerId).OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(e => e.CustomerId);
                entity.HasIndex(e => e.RelatedCustomerId);
                entity.HasIndex(e => e.RelationshipType);
            });

            modelBuilder.Entity<CustomerDocument>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.DocumentType).HasMaxLength(100).IsRequired();
                entity.Property(e => e.DocumentNumber).HasMaxLength(100).IsRequired();
                entity.Property(e => e.DocumentName).HasMaxLength(200);
                entity.Property(e => e.DocumentUrl).HasMaxLength(500);
                entity.Property(e => e.DocumentFormat).HasMaxLength(50);
                entity.Property(e => e.IssuingAuthority).HasMaxLength(100);
                entity.Property(e => e.IssuingCountry).HasMaxLength(100);
                entity.Property(e => e.VerifiedBy).HasMaxLength(100);
                entity.Property(e => e.VerificationNotes).HasMaxLength(1000);
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.Property(e => e.UpdatedBy).HasMaxLength(100);
                entity.HasOne(e => e.Customer).WithMany(c => c.Documents).HasForeignKey(e => e.CustomerId).OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(e => e.CustomerId);
                entity.HasIndex(e => e.DocumentType);
                entity.HasIndex(e => e.DocumentNumber);
                entity.HasIndex(e => e.IsVerified);
            });

            modelBuilder.Entity<ScreeningJob>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.JobName).HasMaxLength(100).IsRequired();
                entity.Property(e => e.JobType).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
                entity.Property(e => e.ErrorMessage).HasMaxLength(1000);
                entity.Property(e => e.InputFile).HasMaxLength(500);
                entity.Property(e => e.OutputFile).HasMaxLength(500);
                entity.Property(e => e.TriggeredBy).HasMaxLength(100);
                entity.Property(e => e.AssignedTo).HasMaxLength(100);
                entity.Property(e => e.Configuration).HasMaxLength(1000);
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.Property(e => e.UpdatedBy).HasMaxLength(100);
                entity.HasIndex(e => e.JobType);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.CreatedAtUtc);
                entity.HasIndex(e => e.StartedAtUtc);
            });

            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Action).HasMaxLength(100).IsRequired();
                entity.Property(e => e.EntityType).HasMaxLength(100).IsRequired();
                entity.Property(e => e.UserId).HasMaxLength(100);
                entity.Property(e => e.UserName).HasMaxLength(100);
                entity.Property(e => e.UserRole).HasMaxLength(100);
                entity.Property(e => e.IpAddress).HasMaxLength(50);
                entity.Property(e => e.UserAgent).HasMaxLength(500);
                entity.Property(e => e.SessionId).HasMaxLength(100);
                entity.Property(e => e.OldValues).HasMaxLength(1000);
                entity.Property(e => e.NewValues).HasMaxLength(1000);
                entity.Property(e => e.AdditionalData).HasMaxLength(1000);
                entity.Property(e => e.Severity).HasMaxLength(50);
                entity.Property(e => e.ErrorMessage).HasMaxLength(1000);
                entity.HasIndex(e => e.Action);
                entity.HasIndex(e => e.EntityType);
                entity.HasIndex(e => e.EntityId);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.TimestampUtc);
                entity.HasIndex(e => e.Date);
                entity.HasIndex(e => e.Hour);
            });

            modelBuilder.Entity<NotificationLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.NotificationType).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Payload).HasMaxLength(1000);
                entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
                entity.Property(e => e.ErrorMessage).HasMaxLength(500);
                entity.Property(e => e.Recipient).HasMaxLength(100);
                entity.Property(e => e.Subject).HasMaxLength(200);
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.HasIndex(e => e.AlertId);
                entity.HasIndex(e => e.NotificationType);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.CreatedAtUtc);
                entity.HasIndex(e => e.Recipient);
            });

            modelBuilder.Entity<Organization>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Code).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Type).HasMaxLength(100);
                entity.Property(e => e.Industry).HasMaxLength(100);
                entity.Property(e => e.Country).HasMaxLength(100);
                entity.Property(e => e.State).HasMaxLength(100);
                entity.Property(e => e.City).HasMaxLength(100);
                entity.Property(e => e.Address).HasMaxLength(500);
                entity.Property(e => e.PostalCode).HasMaxLength(20);
                entity.Property(e => e.ContactPerson).HasMaxLength(100);
                entity.Property(e => e.ContactEmail).HasMaxLength(100);
                entity.Property(e => e.ContactPhone).HasMaxLength(20);
                entity.Property(e => e.Website).HasMaxLength(100);
                entity.Property(e => e.LicenseNumber).HasMaxLength(100);
                entity.Property(e => e.RegulatoryBody).HasMaxLength(100);
                entity.Property(e => e.SubscriptionPlan).HasMaxLength(50);
                entity.Property(e => e.Configuration).HasMaxLength(1000);
                entity.Property(e => e.TimeZone).HasMaxLength(100);
                entity.Property(e => e.Currency).HasMaxLength(10);
                entity.Property(e => e.Language).HasMaxLength(10);
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.Property(e => e.UpdatedBy).HasMaxLength(100);
                entity.HasIndex(e => e.Code).IsUnique();
                entity.HasIndex(e => e.Name);
                entity.HasIndex(e => e.Type);
                entity.HasIndex(e => e.Country);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.CreatedAtUtc);
            });

            modelBuilder.Entity<OrganizationUser>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Email).HasMaxLength(100).IsRequired();
                entity.Property(e => e.FirstName).HasMaxLength(100);
                entity.Property(e => e.LastName).HasMaxLength(100);
                entity.Property(e => e.FullName).HasMaxLength(100);
                entity.Property(e => e.Role).HasMaxLength(100);
                entity.Property(e => e.Department).HasMaxLength(100);
                entity.Property(e => e.Position).HasMaxLength(100);
                entity.Property(e => e.PhoneNumber).HasMaxLength(20);
                entity.Property(e => e.Permissions).HasMaxLength(1000);
                entity.Property(e => e.TimeZone).HasMaxLength(100);
                entity.Property(e => e.Language).HasMaxLength(10);
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.Property(e => e.UpdatedBy).HasMaxLength(100);
                entity.HasOne(e => e.Organization).WithMany(o => o.Users).HasForeignKey(e => e.OrganizationId).OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(e => e.OrganizationId);
                entity.HasIndex(e => e.Username);
                entity.HasIndex(e => e.Email);
                entity.HasIndex(e => e.Role);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.CreatedAtUtc);
            });

            modelBuilder.Entity<OrganizationWatchlist>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.WatchlistSource).HasMaxLength(100).IsRequired();
                entity.Property(e => e.WatchlistType).HasMaxLength(100);
                entity.Property(e => e.RiskLevel).HasMaxLength(100);
                entity.Property(e => e.ReviewRole).HasMaxLength(100);
                entity.Property(e => e.UpdateFrequency).HasMaxLength(50);
                entity.Property(e => e.Configuration).HasMaxLength(1000);
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.Property(e => e.UpdatedBy).HasMaxLength(100);
                entity.HasOne(e => e.Organization).WithMany(o => o.Watchlists).HasForeignKey(e => e.OrganizationId).OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(e => e.OrganizationId);
                entity.HasIndex(e => e.WatchlistSource);
                entity.HasIndex(e => e.WatchlistType);
                entity.HasIndex(e => e.IsEnabled);
                entity.HasIndex(e => e.Priority);
            });

            modelBuilder.Entity<OrganizationConfiguration>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Category).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Key).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Value).HasMaxLength(1000);
                entity.Property(e => e.DataType).HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.Property(e => e.UpdatedBy).HasMaxLength(100);
                entity.HasOne(e => e.Organization).WithMany(o => o.Configurations).HasForeignKey(e => e.OrganizationId).OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(e => e.OrganizationId);
                entity.HasIndex(e => e.Category);
                entity.HasIndex(e => e.Key);
                entity.HasIndex(e => e.IsRequired);
            });

            // Tenant relation
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasOne(e => e.Organization)
                      .WithMany(o => o.Customers)
                      .HasForeignKey(e => e.OrganizationId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(e => e.OrganizationId);
            });
        }
    }
}


