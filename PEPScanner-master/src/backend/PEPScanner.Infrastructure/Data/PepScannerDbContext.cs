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
        public DbSet<AlertAction> AlertActions => Set<AlertAction>();
        public DbSet<User> Users => Set<User>();
        public DbSet<CustomerRelationship> CustomerRelationships => Set<CustomerRelationship>();
        public DbSet<CustomerDocument> CustomerDocuments => Set<CustomerDocument>();
        public DbSet<ScreeningJob> ScreeningJobs => Set<ScreeningJob>();

        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
        public DbSet<NotificationLog> NotificationLogs => Set<NotificationLog>();

        // Reports
        public DbSet<SuspiciousActivityReport> SuspiciousActivityReports => Set<SuspiciousActivityReport>();
        public DbSet<SarComment> SarComments => Set<SarComment>();
        public DbSet<SarStatusHistory> SarStatusHistories => Set<SarStatusHistory>();
        public DbSet<SuspiciousTransactionReport> SuspiciousTransactionReports => Set<SuspiciousTransactionReport>();
        public DbSet<StrComment> StrComments => Set<StrComment>();
        public DbSet<StrStatusHistory> StrStatusHistories => Set<StrStatusHistory>();

        // Dashboard & Analytics
        public DbSet<DashboardMetrics> DashboardMetrics => Set<DashboardMetrics>();
        public DbSet<AlertMetrics> AlertMetrics => Set<AlertMetrics>();
        public DbSet<ScreeningMetrics> ScreeningMetrics => Set<ScreeningMetrics>();
        public DbSet<ComplianceReport> ComplianceReports => Set<ComplianceReport>();

        // OpenSanctions Integration
        public DbSet<OpenSanctionsEntity> OpenSanctionsEntities => Set<OpenSanctionsEntity>();

        // System Configuration
        public DbSet<SystemConfiguration> SystemConfigurations => Set<SystemConfiguration>();

        // Organization Custom Lists
        public DbSet<OrganizationCustomList> OrganizationCustomLists => Set<OrganizationCustomList>();
        public DbSet<OrganizationCustomListEntry> OrganizationCustomListEntries => Set<OrganizationCustomListEntry>();

        // Organization and Multi-tenant support
        public DbSet<Organization> Organizations => Set<Organization>();
        public DbSet<OrganizationUser> OrganizationUsers => Set<OrganizationUser>();
        public DbSet<OrganizationWatchlist> OrganizationWatchlists => Set<OrganizationWatchlist>();
        public DbSet<OrganizationConfiguration> OrganizationConfigurations => Set<OrganizationConfiguration>();

        // AI Risk Scoring and Real-time Notifications
        public DbSet<RiskAssessmentEntity> RiskAssessments => Set<RiskAssessmentEntity>();
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<AIModelMetricsEntity> AIModelMetrics => Set<AIModelMetricsEntity>();
        public DbSet<RiskFactorTemplateEntity> RiskFactorTemplates => Set<RiskFactorTemplateEntity>();
        public DbSet<CustomerRiskProfileEntity> CustomerRiskProfiles => Set<CustomerRiskProfileEntity>();

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

            // Configure OpenSanctions relationships
            ConfigureOpenSanctionsRelationships(modelBuilder);
            ConfigureReportRelationships(modelBuilder);
            ConfigureDashboardRelationships(modelBuilder);

            // Configure AI Risk Scoring relationships
            ConfigureAIRiskScoringRelationships(modelBuilder);
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
            modelBuilder.Entity<Organization>()
                .HasMany(o => o.Users)
                .WithOne(ou => ou.Organization)
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

        private void ConfigureOpenSanctionsRelationships(ModelBuilder modelBuilder)
        {
            // Configure OpenSanctionsEntity indexes
            modelBuilder.Entity<OpenSanctionsEntity>()
                .HasIndex(os => os.Name)
                .HasDatabaseName("IX_OpenSanctionsEntity_Name");

            modelBuilder.Entity<OpenSanctionsEntity>()
                .HasIndex(os => os.Schema)
                .HasDatabaseName("IX_OpenSanctionsEntity_Schema");

            modelBuilder.Entity<OpenSanctionsEntity>()
                .HasIndex(os => os.LastChange)
                .HasDatabaseName("IX_OpenSanctionsEntity_LastChange");

            // Configure Alert OpenSanctions indexes
            modelBuilder.Entity<Alert>()
                .HasIndex(a => a.OpenSanctionsEntityId)
                .HasDatabaseName("IX_Alert_OpenSanctionsEntityId");

            modelBuilder.Entity<Alert>()
                .HasIndex(a => a.OpenSanctionsScore)
                .HasDatabaseName("IX_Alert_OpenSanctionsScore");
        }

        private void ConfigureReportRelationships(ModelBuilder modelBuilder)
        {
            // SAR Relationships
            modelBuilder.Entity<SuspiciousActivityReport>()
                .HasOne(sar => sar.Organization)
                .WithMany()
                .HasForeignKey(sar => sar.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SuspiciousActivityReport>()
                .HasOne(sar => sar.ReportedBy)
                .WithMany()
                .HasForeignKey(sar => sar.ReportedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SuspiciousActivityReport>()
                .HasOne(sar => sar.ReviewedBy)
                .WithMany()
                .HasForeignKey(sar => sar.ReviewedById)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<SuspiciousActivityReport>()
                .HasOne(sar => sar.Customer)
                .WithMany()
                .HasForeignKey(sar => sar.CustomerId)
                .OnDelete(DeleteBehavior.SetNull);

            // SAR Comments
            modelBuilder.Entity<SarComment>()
                .HasOne(sc => sc.Sar)
                .WithMany(sar => sar.Comments)
                .HasForeignKey(sc => sc.SarId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SarComment>()
                .HasOne(sc => sc.User)
                .WithMany()
                .HasForeignKey(sc => sc.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // SAR Status History
            modelBuilder.Entity<SarStatusHistory>()
                .HasOne(ssh => ssh.Sar)
                .WithMany(sar => sar.StatusHistory)
                .HasForeignKey(ssh => ssh.SarId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SarStatusHistory>()
                .HasOne(ssh => ssh.ChangedBy)
                .WithMany()
                .HasForeignKey(ssh => ssh.ChangedById)
                .OnDelete(DeleteBehavior.Restrict);

            // STR Relationships (similar to SAR)
            modelBuilder.Entity<SuspiciousTransactionReport>()
                .HasOne(str => str.Organization)
                .WithMany()
                .HasForeignKey(str => str.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SuspiciousTransactionReport>()
                .HasOne(str => str.ReportedBy)
                .WithMany()
                .HasForeignKey(str => str.ReportedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SuspiciousTransactionReport>()
                .HasOne(str => str.ReviewedBy)
                .WithMany()
                .HasForeignKey(str => str.ReviewedById)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<SuspiciousTransactionReport>()
                .HasOne(str => str.Customer)
                .WithMany()
                .HasForeignKey(str => str.CustomerId)
                .OnDelete(DeleteBehavior.SetNull);

            // STR Comments
            modelBuilder.Entity<StrComment>()
                .HasOne(sc => sc.Str)
                .WithMany(str => str.Comments)
                .HasForeignKey(sc => sc.StrId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StrComment>()
                .HasOne(sc => sc.User)
                .WithMany()
                .HasForeignKey(sc => sc.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // STR Status History
            modelBuilder.Entity<StrStatusHistory>()
                .HasOne(ssh => ssh.Str)
                .WithMany(str => str.StatusHistory)
                .HasForeignKey(ssh => ssh.StrId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StrStatusHistory>()
                .HasOne(ssh => ssh.ChangedBy)
                .WithMany()
                .HasForeignKey(ssh => ssh.ChangedById)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes for performance
            modelBuilder.Entity<SuspiciousActivityReport>()
                .HasIndex(sar => sar.ReportNumber)
                .IsUnique()
                .HasDatabaseName("IX_SAR_ReportNumber");

            modelBuilder.Entity<SuspiciousActivityReport>()
                .HasIndex(sar => new { sar.OrganizationId, sar.Status })
                .HasDatabaseName("IX_SAR_Organization_Status");

            modelBuilder.Entity<SuspiciousTransactionReport>()
                .HasIndex(str => str.ReportNumber)
                .IsUnique()
                .HasDatabaseName("IX_STR_ReportNumber");

            modelBuilder.Entity<SuspiciousTransactionReport>()
                .HasIndex(str => new { str.OrganizationId, str.Status })
                .HasDatabaseName("IX_STR_Organization_Status");
        }

        private void ConfigureDashboardRelationships(ModelBuilder modelBuilder)
        {
            // Dashboard Metrics
            modelBuilder.Entity<DashboardMetrics>()
                .HasOne(dm => dm.Organization)
                .WithMany()
                .HasForeignKey(dm => dm.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DashboardMetrics>()
                .HasIndex(dm => new { dm.OrganizationId, dm.MetricDate, dm.MetricType })
                .IsUnique()
                .HasDatabaseName("IX_DashboardMetrics_Org_Date_Type");

            // Alert Metrics
            modelBuilder.Entity<AlertMetrics>()
                .HasOne(am => am.Organization)
                .WithMany()
                .HasForeignKey(am => am.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AlertMetrics>()
                .HasIndex(am => new { am.OrganizationId, am.Date })
                .IsUnique()
                .HasDatabaseName("IX_AlertMetrics_Org_Date");

            // Screening Metrics
            modelBuilder.Entity<ScreeningMetrics>()
                .HasOne(sm => sm.Organization)
                .WithMany()
                .HasForeignKey(sm => sm.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ScreeningMetrics>()
                .HasIndex(sm => new { sm.OrganizationId, sm.Date })
                .IsUnique()
                .HasDatabaseName("IX_ScreeningMetrics_Org_Date");

            // Compliance Reports
            modelBuilder.Entity<ComplianceReport>()
                .HasOne(cr => cr.Organization)
                .WithMany()
                .HasForeignKey(cr => cr.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ComplianceReport>()
                .HasOne(cr => cr.GeneratedBy)
                .WithMany()
                .HasForeignKey(cr => cr.GeneratedById)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ComplianceReport>()
                .HasIndex(cr => new { cr.OrganizationId, cr.ReportType, cr.ReportPeriodStart })
                .HasDatabaseName("IX_ComplianceReport_Org_Type_Period");
        }

        private void ConfigureAIRiskScoringRelationships(ModelBuilder modelBuilder)
        {
            // Risk Assessment relationships
            modelBuilder.Entity<RiskAssessmentEntity>()
                .HasOne(ra => ra.Customer)
                .WithMany()
                .HasForeignKey(ra => ra.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RiskAssessmentEntity>()
                .HasIndex(ra => ra.CustomerId)
                .HasDatabaseName("IX_RiskAssessment_CustomerId");

            modelBuilder.Entity<RiskAssessmentEntity>()
                .HasIndex(ra => ra.CalculatedAt)
                .HasDatabaseName("IX_RiskAssessment_CalculatedAt");

            modelBuilder.Entity<RiskAssessmentEntity>()
                .HasIndex(ra => new { ra.CustomerId, ra.CalculatedAt })
                .HasDatabaseName("IX_RiskAssessment_Customer_Date");

            // Notification relationships - using simple Notification entity
            modelBuilder.Entity<Notification>()
                .HasIndex(n => n.CreatedAtUtc)
                .HasDatabaseName("IX_Notification_CreatedAtUtc");

            modelBuilder.Entity<Notification>()
                .HasIndex(n => new { n.Type, n.Priority })
                .HasDatabaseName("IX_Notification_Type_Priority");

            modelBuilder.Entity<Notification>()
                .HasIndex(n => n.IsRead)
                .HasDatabaseName("IX_Notification_IsRead");

            // Customer Risk Profile relationships
            modelBuilder.Entity<CustomerRiskProfileEntity>()
                .HasOne(crp => crp.Customer)
                .WithMany()
                .HasForeignKey(crp => crp.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CustomerRiskProfileEntity>()
                .HasIndex(crp => crp.CustomerId)
                .IsUnique()
                .HasDatabaseName("IX_CustomerRiskProfile_CustomerId");

            modelBuilder.Entity<CustomerRiskProfileEntity>()
                .HasIndex(crp => crp.RiskLevel)
                .HasDatabaseName("IX_CustomerRiskProfile_RiskLevel");

            modelBuilder.Entity<CustomerRiskProfileEntity>()
                .HasIndex(crp => crp.NextAssessmentDue)
                .HasDatabaseName("IX_CustomerRiskProfile_NextAssessmentDue");

            // AI Model Metrics indexes
            modelBuilder.Entity<AIModelMetricsEntity>()
                .HasIndex(amm => new { amm.ModelVersion, amm.MetricDate })
                .HasDatabaseName("IX_AIModelMetrics_Version_Date");

            // Risk Factor Templates indexes
            modelBuilder.Entity<RiskFactorTemplateEntity>()
                .HasIndex(rft => rft.Category)
                .HasDatabaseName("IX_RiskFactorTemplate_Category");

            modelBuilder.Entity<RiskFactorTemplateEntity>()
                .HasIndex(rft => rft.IsActive)
                .HasDatabaseName("IX_RiskFactorTemplate_IsActive");
        }
    }
}
