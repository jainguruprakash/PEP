using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PEPScanner.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgreSQL : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AIModelMetrics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ModelVersion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MetricDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Accuracy = table.Column<double>(type: "double precision", nullable: false),
                    Precision = table.Column<double>(type: "double precision", nullable: false),
                    Recall = table.Column<double>(type: "double precision", nullable: false),
                    F1Score = table.Column<double>(type: "double precision", nullable: false),
                    TotalPredictions = table.Column<int>(type: "integer", nullable: false),
                    TruePositives = table.Column<int>(type: "integer", nullable: false),
                    TrueNegatives = table.Column<int>(type: "integer", nullable: false),
                    FalsePositives = table.Column<int>(type: "integer", nullable: false),
                    FalseNegatives = table.Column<int>(type: "integer", nullable: false),
                    PerformanceMetricsJson = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIModelMetrics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NotificationLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AlertId = table.Column<Guid>(type: "uuid", nullable: true),
                    NotificationType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Payload = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Recipient = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Subject = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SentAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Data = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    TargetUserEmail = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TargetUserRole = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    ReadAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReadByUserEmail = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Priority = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OpenSanctionsEntities",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Schema = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Aliases = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    BirthDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Countries = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Addresses = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Identifiers = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Sanctions = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Datasets = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Score = table.Column<double>(type: "double precision", nullable: false),
                    FirstSeen = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastSeen = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastChange = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpenSanctionsEntities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Organizations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Industry = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    State = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    PostalCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ContactPerson = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ContactEmail = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ContactPhone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Website = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LicenseNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RegulatoryBody = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SubscriptionStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SubscriptionEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SubscriptionPlan = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MaxUsers = table.Column<int>(type: "integer", nullable: false),
                    MaxCustomers = table.Column<int>(type: "integer", nullable: false),
                    IsTrial = table.Column<bool>(type: "boolean", nullable: false),
                    Configuration = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    TimeZone = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Language = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RiskFactorTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    BaseWeight = table.Column<double>(type: "double precision", nullable: false),
                    Severity = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ConditionsJson = table.Column<string>(type: "text", nullable: false),
                    ActionsJson = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiskFactorTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScreeningJobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JobName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    JobType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TotalRecords = table.Column<int>(type: "integer", nullable: false),
                    ProcessedRecords = table.Column<int>(type: "integer", nullable: false),
                    MatchesFound = table.Column<int>(type: "integer", nullable: false),
                    AlertsGenerated = table.Column<int>(type: "integer", nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    InputFile = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    OutputFile = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TriggeredBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AssignedTo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Configuration = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScreeningJobs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemConfigurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemConfigurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Department = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ManagerEmail = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Phone = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    JobTitle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PasswordHash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    LastLoginUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CanCreateAlerts = table.Column<bool>(type: "boolean", nullable: false),
                    CanReviewAlerts = table.Column<bool>(type: "boolean", nullable: false),
                    CanApproveAlerts = table.Column<bool>(type: "boolean", nullable: false),
                    CanEscalateAlerts = table.Column<bool>(type: "boolean", nullable: false),
                    CanCloseAlerts = table.Column<bool>(type: "boolean", nullable: false),
                    CanViewAllAlerts = table.Column<bool>(type: "boolean", nullable: false),
                    CanAssignAlerts = table.Column<bool>(type: "boolean", nullable: false),
                    CanGenerateReports = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WatchlistEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Source = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ListType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PrimaryName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    AlternateNames = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Gender = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateOfBirthFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateOfBirthTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PositionOrRole = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RiskCategory = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RiskLevel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    EntityType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Nationality = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Citizenship = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    State = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PostalCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CountryOfResidence = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PlaceOfBirth = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PassportNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    NationalIdNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TaxIdNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RegistrationNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LicenseNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    VesselName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    VesselType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    VesselFlag = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    VesselCallSign = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    VesselImoNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AircraftType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AircraftRegistration = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AircraftOperator = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PepPosition = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PepCountry = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PepStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PepEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PepCategory = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PepDescription = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SanctionType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SanctionAuthority = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SanctionReference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SanctionStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SanctionEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SanctionReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    MediaSource = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MediaDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MediaSummary = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    MediaUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    MediaCategory = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ExternalId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ExternalReference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Comments = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DataQuality = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsWhitelisted = table.Column<bool>(type: "boolean", nullable: false),
                    DateAddedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateRemovedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateLastUpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AddedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RemovedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WatchlistEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AlertMetrics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TotalAlerts = table.Column<int>(type: "integer", nullable: false),
                    HighPriorityAlerts = table.Column<int>(type: "integer", nullable: false),
                    MediumPriorityAlerts = table.Column<int>(type: "integer", nullable: false),
                    LowPriorityAlerts = table.Column<int>(type: "integer", nullable: false),
                    ResolvedAlerts = table.Column<int>(type: "integer", nullable: false),
                    PendingAlerts = table.Column<int>(type: "integer", nullable: false),
                    EscalatedAlerts = table.Column<int>(type: "integer", nullable: false),
                    AverageResolutionTime = table.Column<decimal>(type: "numeric", nullable: false),
                    ComplianceScore = table.Column<decimal>(type: "numeric", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertMetrics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlertMetrics_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    AliasNames = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Nationality = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IdentificationNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IdentificationType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    State = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PostalCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Occupation = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Employer = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PhoneNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    EmailAddress = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FirstName = table.Column<string>(type: "text", nullable: true),
                    LastName = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    Phone = table.Column<string>(type: "text", nullable: true),
                    CustomerType = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: true),
                    OnboardingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsPep = table.Column<bool>(type: "boolean", nullable: false),
                    RiskScore = table.Column<int>(type: "integer", nullable: false),
                    RiskLevel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PepPosition = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PepCountry = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PepStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PepEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PhotoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FingerprintData = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FaceBiometricData = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    LastScreeningDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NextScreeningDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ScreeningFrequency = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresEdd = table.Column<bool>(type: "boolean", nullable: false),
                    EddNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    AccountNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AccountType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MonthlyTransactionVolume = table.Column<decimal>(type: "numeric", nullable: true),
                    AverageTransactionAmount = table.Column<decimal>(type: "numeric", nullable: true),
                    PanNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    GstNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CinNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    LastMediaScanDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OrganizationId1 = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Customers_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Customers_Organizations_OrganizationId1",
                        column: x => x.OrganizationId1,
                        principalTable: "Organizations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DashboardMetrics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    MetricDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MetricType = table.Column<string>(type: "text", nullable: false),
                    TotalCustomersScreened = table.Column<int>(type: "integer", nullable: false),
                    TotalTransactionsScreened = table.Column<int>(type: "integer", nullable: false),
                    HighRiskMatches = table.Column<int>(type: "integer", nullable: false),
                    MediumRiskMatches = table.Column<int>(type: "integer", nullable: false),
                    LowRiskMatches = table.Column<int>(type: "integer", nullable: false),
                    FalsePositives = table.Column<int>(type: "integer", nullable: false),
                    TruePositives = table.Column<int>(type: "integer", nullable: false),
                    TotalAlertsGenerated = table.Column<int>(type: "integer", nullable: false),
                    AlertsResolved = table.Column<int>(type: "integer", nullable: false),
                    AlertsPending = table.Column<int>(type: "integer", nullable: false),
                    AlertsEscalated = table.Column<int>(type: "integer", nullable: false),
                    AverageResolutionTimeHours = table.Column<decimal>(type: "numeric", nullable: false),
                    SarReportsCreated = table.Column<int>(type: "integer", nullable: false),
                    SarReportsSubmitted = table.Column<int>(type: "integer", nullable: false),
                    StrReportsCreated = table.Column<int>(type: "integer", nullable: false),
                    StrReportsSubmitted = table.Column<int>(type: "integer", nullable: false),
                    ComplianceScore = table.Column<decimal>(type: "numeric", nullable: false),
                    RegulatoryDeadlinesMissed = table.Column<int>(type: "integer", nullable: false),
                    RegulatoryDeadlinesMet = table.Column<int>(type: "integer", nullable: false),
                    SystemUptime = table.Column<decimal>(type: "numeric", nullable: false),
                    AverageScreeningTimeMs = table.Column<decimal>(type: "numeric", nullable: false),
                    ApiCallsCount = table.Column<int>(type: "integer", nullable: false),
                    ErrorsCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DashboardMetrics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DashboardMetrics_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrganizationConfigurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    DataType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    IsEncrypted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    OrganizationId1 = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationConfigurations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizationConfigurations_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrganizationConfigurations_Organizations_OrganizationId1",
                        column: x => x.OrganizationId1,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrganizationCustomLists",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ListName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ListType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsPrivate = table.Column<bool>(type: "boolean", nullable: false),
                    RiskLevel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MatchThreshold = table.Column<double>(type: "double precision", nullable: false),
                    AutoAlert = table.Column<bool>(type: "boolean", nullable: false),
                    RequireReview = table.Column<bool>(type: "boolean", nullable: false),
                    ReviewRole = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UpdateFrequency = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LastUpdateAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NextUpdateAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TotalEntries = table.Column<int>(type: "integer", nullable: false),
                    ActiveEntries = table.Column<int>(type: "integer", nullable: false),
                    SourceFormat = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SourceLocation = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Configuration = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationCustomLists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizationCustomLists_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrganizationUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FullName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Role = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Department = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Position = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsEmailVerified = table.Column<bool>(type: "boolean", nullable: false),
                    LastLoginAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PasswordChangedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    RefreshToken = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RefreshTokenExpiryTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Permissions = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    TimeZone = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Language = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizationUsers_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrganizationWatchlists",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    WatchlistSource = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    WatchlistType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    MatchThreshold = table.Column<double>(type: "double precision", nullable: false),
                    RiskLevel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AutoAlert = table.Column<bool>(type: "boolean", nullable: false),
                    RequireReview = table.Column<bool>(type: "boolean", nullable: false),
                    ReviewRole = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastUpdateAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NextUpdateAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateFrequency = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Configuration = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    OrganizationId1 = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationWatchlists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizationWatchlists_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrganizationWatchlists_Organizations_OrganizationId1",
                        column: x => x.OrganizationId1,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScreeningMetrics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CustomersScreened = table.Column<int>(type: "integer", nullable: false),
                    TransactionsScreened = table.Column<int>(type: "integer", nullable: false),
                    PepMatches = table.Column<int>(type: "integer", nullable: false),
                    SanctionMatches = table.Column<int>(type: "integer", nullable: false),
                    WatchlistMatches = table.Column<int>(type: "integer", nullable: false),
                    TruePositives = table.Column<int>(type: "integer", nullable: false),
                    FalsePositives = table.Column<int>(type: "integer", nullable: false),
                    AccuracyRate = table.Column<decimal>(type: "numeric", nullable: false),
                    AverageScreeningTime = table.Column<decimal>(type: "numeric", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScreeningMetrics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScreeningMetrics_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Alerts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: true),
                    WatchlistEntryId = table.Column<Guid>(type: "uuid", nullable: true),
                    Context = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AlertType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SimilarityScore = table.Column<double>(type: "double precision", nullable: false),
                    MatchAlgorithm = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Priority = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AssignedTo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ReviewedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ReviewedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EscalatedTo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    EscalatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OutcomeNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Outcome = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RiskLevel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ComplianceAction = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RequiresEdd = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresStr = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresSar = table.Column<bool>(type: "boolean", nullable: false),
                    StrReference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SarReference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    StrFiledAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SarFiledAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StrFiledBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SarFiledBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MatchedFields = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    MatchingDetails = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SourceList = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SourceCategory = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TransactionId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TransactionAmount = table.Column<decimal>(type: "numeric", nullable: true),
                    TransactionType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TransactionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EscalationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SlaStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SlaHours = table.Column<int>(type: "integer", nullable: true),
                    ActualHours = table.Column<int>(type: "integer", nullable: true),
                    Tags = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SubCategory = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ApprovedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ApprovedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RejectedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ApprovalComments = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    WorkflowStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EscalationLevel = table.Column<int>(type: "integer", nullable: false),
                    CurrentReviewer = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LastActionDateUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastActionType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    OpenSanctionsEntityId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    OpenSanctionsScore = table.Column<double>(type: "double precision", nullable: true),
                    OpenSanctionsDatasets = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    OpenSanctionsMatchFeatures = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    OpenSanctionsLastChecked = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OpenSanctionsEntityType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    OpenSanctionsAliases = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    OpenSanctionsSanctions = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    OpenSanctionsCountries = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    OpenSanctionsAddresses = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    OpenSanctionsFirstSeen = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OpenSanctionsLastSeen = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OpenSanctionsLastChange = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserId1 = table.Column<Guid>(type: "uuid", nullable: true),
                    WatchlistEntryId1 = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Alerts_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Alerts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Alerts_Users_UserId1",
                        column: x => x.UserId1,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Alerts_WatchlistEntries_WatchlistEntryId",
                        column: x => x.WatchlistEntryId,
                        principalTable: "WatchlistEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Alerts_WatchlistEntries_WatchlistEntryId1",
                        column: x => x.WatchlistEntryId1,
                        principalTable: "WatchlistEntries",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CustomerDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DocumentNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DocumentName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DocumentUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DocumentFormat = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IssueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IssuingAuthority = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IssuingCountry = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false),
                    VerifiedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    VerifiedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VerificationNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerDocuments_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustomerRelationships",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    RelatedCustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    RelationshipType = table.Column<string>(type: "text", nullable: false),
                    RelationshipDetails = table.Column<string>(type: "text", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerRelationships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerRelationships_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustomerRelationships_Customers_RelatedCustomerId",
                        column: x => x.RelatedCustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CustomerRiskProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentRiskScore = table.Column<double>(type: "double precision", nullable: false),
                    PreviousRiskScore = table.Column<double>(type: "double precision", nullable: false),
                    RiskTrend = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RiskLevel = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    LastAssessmentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NextAssessmentDue = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MonitoringFrequency = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AlertCount = table.Column<int>(type: "integer", nullable: false),
                    HighRiskAlertCount = table.Column<int>(type: "integer", nullable: false),
                    RiskFactorSummaryJson = table.Column<string>(type: "text", nullable: false),
                    MonitoringNotesJson = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerRiskProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerRiskProfiles_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RiskAssessments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    RiskScore = table.Column<double>(type: "double precision", nullable: false),
                    ConfidenceLevel = table.Column<double>(type: "double precision", nullable: false),
                    ModelVersion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CalculatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RiskFactorsJson = table.Column<string>(type: "text", nullable: false),
                    ComponentScoresJson = table.Column<string>(type: "text", nullable: false),
                    PredictiveInsightsJson = table.Column<string>(type: "text", nullable: false),
                    RecommendedActionsJson = table.Column<string>(type: "text", nullable: false),
                    RiskTrend = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiskAssessments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RiskAssessments_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrganizationCustomListEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomListId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PrimaryName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AlternateNames = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MiddleName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PlaceOfBirth = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Nationality = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    PostalCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    State = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PositionOrRole = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    OrganizationName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Gender = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IdType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IdNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EmailAddress = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    RiskCategory = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EntryType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CustomerType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RelationshipType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RelationshipStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RelationshipEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AccountNumbers = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TotalBalance = table.Column<decimal>(type: "numeric", nullable: true),
                    Currency = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Tags = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    AdditionalData = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    DateAddedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastVerifiedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AddedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    VerifiedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Source = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SourceReference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationCustomListEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizationCustomListEntries_OrganizationCustomLists_Custo~",
                        column: x => x.CustomListId,
                        principalTable: "OrganizationCustomLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrganizationCustomListEntries_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntityType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UserName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UserRole = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IpAddress = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SessionId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    OldValues = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    NewValues = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    AdditionalData = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Severity = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsSuccessful = table.Column<bool>(type: "boolean", nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    TimestampUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Hour = table.Column<int>(type: "integer", nullable: false),
                    OrganizationUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLogs_OrganizationUsers_OrganizationUserId",
                        column: x => x.OrganizationUserId,
                        principalTable: "OrganizationUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ComplianceReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportType = table.Column<string>(type: "text", nullable: false),
                    ReportPeriodStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReportPeriodEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReportData = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    GeneratedById = table.Column<Guid>(type: "uuid", nullable: true),
                    GeneratedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FilePath = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplianceReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComplianceReports_OrganizationUsers_GeneratedById",
                        column: x => x.GeneratedById,
                        principalTable: "OrganizationUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ComplianceReports_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SuspiciousActivityReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportNumber = table.Column<string>(type: "text", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportedById = table.Column<Guid>(type: "uuid", nullable: false),
                    ReviewedById = table.Column<Guid>(type: "uuid", nullable: true),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: true),
                    SubjectName = table.Column<string>(type: "text", nullable: false),
                    SubjectAddress = table.Column<string>(type: "text", nullable: true),
                    SubjectIdentification = table.Column<string>(type: "text", nullable: true),
                    SubjectDateOfBirth = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SuspiciousActivity = table.Column<string>(type: "text", nullable: false),
                    ActivityDescription = table.Column<string>(type: "text", nullable: false),
                    TransactionAmount = table.Column<decimal>(type: "numeric", nullable: true),
                    TransactionCurrency = table.Column<string>(type: "text", nullable: true),
                    TransactionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TransactionLocation = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    IncidentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DiscoveryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RegulatoryReferences = table.Column<string>(type: "text", nullable: true),
                    AttachedDocuments = table.Column<string>(type: "text", nullable: true),
                    InternalNotes = table.Column<string>(type: "text", nullable: true),
                    ComplianceComments = table.Column<string>(type: "text", nullable: true),
                    SubmissionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RegulatoryFilingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RegulatoryReferenceNumber = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SuspiciousActivityReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SuspiciousActivityReports_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SuspiciousActivityReports_OrganizationUsers_ReportedById",
                        column: x => x.ReportedById,
                        principalTable: "OrganizationUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SuspiciousActivityReports_OrganizationUsers_ReviewedById",
                        column: x => x.ReviewedById,
                        principalTable: "OrganizationUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SuspiciousActivityReports_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SuspiciousTransactionReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportNumber = table.Column<string>(type: "text", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportedById = table.Column<Guid>(type: "uuid", nullable: false),
                    ReviewedById = table.Column<Guid>(type: "uuid", nullable: true),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: true),
                    TransactionReference = table.Column<string>(type: "text", nullable: false),
                    TransactionAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    TransactionCurrency = table.Column<string>(type: "text", nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TransactionType = table.Column<string>(type: "text", nullable: false),
                    OriginatorName = table.Column<string>(type: "text", nullable: true),
                    OriginatorAccount = table.Column<string>(type: "text", nullable: true),
                    OriginatorBank = table.Column<string>(type: "text", nullable: true),
                    BeneficiaryName = table.Column<string>(type: "text", nullable: true),
                    BeneficiaryAccount = table.Column<string>(type: "text", nullable: true),
                    BeneficiaryBank = table.Column<string>(type: "text", nullable: true),
                    SuspicionReason = table.Column<string>(type: "text", nullable: false),
                    DetailedDescription = table.Column<string>(type: "text", nullable: false),
                    CountryOfOrigin = table.Column<string>(type: "text", nullable: true),
                    CountryOfDestination = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    RegulatoryReferences = table.Column<string>(type: "text", nullable: true),
                    AttachedDocuments = table.Column<string>(type: "text", nullable: true),
                    InternalNotes = table.Column<string>(type: "text", nullable: true),
                    ComplianceComments = table.Column<string>(type: "text", nullable: true),
                    SubmissionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RegulatoryFilingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RegulatoryReferenceNumber = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SuspiciousTransactionReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SuspiciousTransactionReports_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SuspiciousTransactionReports_OrganizationUsers_ReportedById",
                        column: x => x.ReportedById,
                        principalTable: "OrganizationUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SuspiciousTransactionReports_OrganizationUsers_ReviewedById",
                        column: x => x.ReviewedById,
                        principalTable: "OrganizationUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SuspiciousTransactionReports_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AlertActions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AlertId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActionType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PerformedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PreviousStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    NewStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PreviousAssignee = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    NewAssignee = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Comments = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    AdditionalData = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ActionDateUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlertActions_Alerts_AlertId",
                        column: x => x.AlertId,
                        principalTable: "Alerts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SarComments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SarId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Comment = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SarComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SarComments_OrganizationUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "OrganizationUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SarComments_SuspiciousActivityReports_SarId",
                        column: x => x.SarId,
                        principalTable: "SuspiciousActivityReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SarStatusHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SarId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromStatus = table.Column<int>(type: "integer", nullable: false),
                    ToStatus = table.Column<int>(type: "integer", nullable: false),
                    ChangedById = table.Column<Guid>(type: "uuid", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    ChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SarStatusHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SarStatusHistories_OrganizationUsers_ChangedById",
                        column: x => x.ChangedById,
                        principalTable: "OrganizationUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SarStatusHistories_SuspiciousActivityReports_SarId",
                        column: x => x.SarId,
                        principalTable: "SuspiciousActivityReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StrComments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StrId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Comment = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StrComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StrComments_OrganizationUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "OrganizationUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StrComments_SuspiciousTransactionReports_StrId",
                        column: x => x.StrId,
                        principalTable: "SuspiciousTransactionReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StrStatusHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StrId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromStatus = table.Column<int>(type: "integer", nullable: false),
                    ToStatus = table.Column<int>(type: "integer", nullable: false),
                    ChangedById = table.Column<Guid>(type: "uuid", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    ChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StrStatusHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StrStatusHistories_OrganizationUsers_ChangedById",
                        column: x => x.ChangedById,
                        principalTable: "OrganizationUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StrStatusHistories_SuspiciousTransactionReports_StrId",
                        column: x => x.StrId,
                        principalTable: "SuspiciousTransactionReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AIModelMetrics_Version_Date",
                table: "AIModelMetrics",
                columns: new[] { "ModelVersion", "MetricDate" });

            migrationBuilder.CreateIndex(
                name: "IX_AlertActions_AlertId",
                table: "AlertActions",
                column: "AlertId");

            migrationBuilder.CreateIndex(
                name: "IX_AlertMetrics_Org_Date",
                table: "AlertMetrics",
                columns: new[] { "OrganizationId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Alert_CreatedAtUtc",
                table: "Alerts",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Alert_OpenSanctionsEntityId",
                table: "Alerts",
                column: "OpenSanctionsEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Alert_OpenSanctionsScore",
                table: "Alerts",
                column: "OpenSanctionsScore");

            migrationBuilder.CreateIndex(
                name: "IX_Alert_Status",
                table: "Alerts",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_CustomerId",
                table: "Alerts",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_UserId",
                table: "Alerts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_UserId1",
                table: "Alerts",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_WatchlistEntryId",
                table: "Alerts",
                column: "WatchlistEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_WatchlistEntryId1",
                table: "Alerts",
                column: "WatchlistEntryId1");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_OrganizationUserId",
                table: "AuditLogs",
                column: "OrganizationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceReport_Org_Type_Period",
                table: "ComplianceReports",
                columns: new[] { "OrganizationId", "ReportType", "ReportPeriodStart" });

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceReports_GeneratedById",
                table: "ComplianceReports",
                column: "GeneratedById");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerDocuments_CustomerId",
                table: "CustomerDocuments",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerRelationships_CustomerId",
                table: "CustomerRelationships",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerRelationships_RelatedCustomerId",
                table: "CustomerRelationships",
                column: "RelatedCustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerRiskProfile_CustomerId",
                table: "CustomerRiskProfiles",
                column: "CustomerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerRiskProfile_NextAssessmentDue",
                table: "CustomerRiskProfiles",
                column: "NextAssessmentDue");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerRiskProfile_RiskLevel",
                table: "CustomerRiskProfiles",
                column: "RiskLevel");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_OrganizationId",
                table: "Customers",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_OrganizationId1",
                table: "Customers",
                column: "OrganizationId1");

            migrationBuilder.CreateIndex(
                name: "IX_DashboardMetrics_Org_Date_Type",
                table: "DashboardMetrics",
                columns: new[] { "OrganizationId", "MetricDate", "MetricType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notification_CreatedAtUtc",
                table: "Notifications",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Notification_IsRead",
                table: "Notifications",
                column: "IsRead");

            migrationBuilder.CreateIndex(
                name: "IX_Notification_Type_Priority",
                table: "Notifications",
                columns: new[] { "Type", "Priority" });

            migrationBuilder.CreateIndex(
                name: "IX_OpenSanctionsEntity_LastChange",
                table: "OpenSanctionsEntities",
                column: "LastChange");

            migrationBuilder.CreateIndex(
                name: "IX_OpenSanctionsEntity_Name",
                table: "OpenSanctionsEntities",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_OpenSanctionsEntity_Schema",
                table: "OpenSanctionsEntities",
                column: "Schema");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationConfigurations_OrganizationId",
                table: "OrganizationConfigurations",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationConfigurations_OrganizationId1",
                table: "OrganizationConfigurations",
                column: "OrganizationId1");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationCustomListEntries_CustomListId",
                table: "OrganizationCustomListEntries",
                column: "CustomListId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationCustomListEntries_OrganizationId",
                table: "OrganizationCustomListEntries",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationCustomLists_OrganizationId",
                table: "OrganizationCustomLists",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUsers_OrganizationId",
                table: "OrganizationUsers",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationWatchlists_OrganizationId",
                table: "OrganizationWatchlists",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationWatchlists_OrganizationId1",
                table: "OrganizationWatchlists",
                column: "OrganizationId1");

            migrationBuilder.CreateIndex(
                name: "IX_RiskAssessment_CalculatedAt",
                table: "RiskAssessments",
                column: "CalculatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_RiskAssessment_Customer_Date",
                table: "RiskAssessments",
                columns: new[] { "CustomerId", "CalculatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_RiskAssessment_CustomerId",
                table: "RiskAssessments",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_RiskFactorTemplate_Category",
                table: "RiskFactorTemplates",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_RiskFactorTemplate_IsActive",
                table: "RiskFactorTemplates",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_SarComments_SarId",
                table: "SarComments",
                column: "SarId");

            migrationBuilder.CreateIndex(
                name: "IX_SarComments_UserId",
                table: "SarComments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SarStatusHistories_ChangedById",
                table: "SarStatusHistories",
                column: "ChangedById");

            migrationBuilder.CreateIndex(
                name: "IX_SarStatusHistories_SarId",
                table: "SarStatusHistories",
                column: "SarId");

            migrationBuilder.CreateIndex(
                name: "IX_ScreeningJob_CreatedAtUtc",
                table: "ScreeningJobs",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_ScreeningJob_Status",
                table: "ScreeningJobs",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ScreeningMetrics_Org_Date",
                table: "ScreeningMetrics",
                columns: new[] { "OrganizationId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StrComments_StrId",
                table: "StrComments",
                column: "StrId");

            migrationBuilder.CreateIndex(
                name: "IX_StrComments_UserId",
                table: "StrComments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_StrStatusHistories_ChangedById",
                table: "StrStatusHistories",
                column: "ChangedById");

            migrationBuilder.CreateIndex(
                name: "IX_StrStatusHistories_StrId",
                table: "StrStatusHistories",
                column: "StrId");

            migrationBuilder.CreateIndex(
                name: "IX_SAR_Organization_Status",
                table: "SuspiciousActivityReports",
                columns: new[] { "OrganizationId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_SAR_ReportNumber",
                table: "SuspiciousActivityReports",
                column: "ReportNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SuspiciousActivityReports_CustomerId",
                table: "SuspiciousActivityReports",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_SuspiciousActivityReports_ReportedById",
                table: "SuspiciousActivityReports",
                column: "ReportedById");

            migrationBuilder.CreateIndex(
                name: "IX_SuspiciousActivityReports_ReviewedById",
                table: "SuspiciousActivityReports",
                column: "ReviewedById");

            migrationBuilder.CreateIndex(
                name: "IX_STR_Organization_Status",
                table: "SuspiciousTransactionReports",
                columns: new[] { "OrganizationId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_STR_ReportNumber",
                table: "SuspiciousTransactionReports",
                column: "ReportNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SuspiciousTransactionReports_CustomerId",
                table: "SuspiciousTransactionReports",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_SuspiciousTransactionReports_ReportedById",
                table: "SuspiciousTransactionReports",
                column: "ReportedById");

            migrationBuilder.CreateIndex(
                name: "IX_SuspiciousTransactionReports_ReviewedById",
                table: "SuspiciousTransactionReports",
                column: "ReviewedById");

            migrationBuilder.CreateIndex(
                name: "IX_WatchlistEntry_PrimaryName",
                table: "WatchlistEntries",
                column: "PrimaryName");

            migrationBuilder.CreateIndex(
                name: "IX_WatchlistEntry_Source",
                table: "WatchlistEntries",
                column: "Source");

            migrationBuilder.CreateIndex(
                name: "IX_WatchlistEntry_Source_PrimaryName",
                table: "WatchlistEntries",
                columns: new[] { "Source", "PrimaryName" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AIModelMetrics");

            migrationBuilder.DropTable(
                name: "AlertActions");

            migrationBuilder.DropTable(
                name: "AlertMetrics");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "ComplianceReports");

            migrationBuilder.DropTable(
                name: "CustomerDocuments");

            migrationBuilder.DropTable(
                name: "CustomerRelationships");

            migrationBuilder.DropTable(
                name: "CustomerRiskProfiles");

            migrationBuilder.DropTable(
                name: "DashboardMetrics");

            migrationBuilder.DropTable(
                name: "NotificationLogs");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "OpenSanctionsEntities");

            migrationBuilder.DropTable(
                name: "OrganizationConfigurations");

            migrationBuilder.DropTable(
                name: "OrganizationCustomListEntries");

            migrationBuilder.DropTable(
                name: "OrganizationWatchlists");

            migrationBuilder.DropTable(
                name: "RiskAssessments");

            migrationBuilder.DropTable(
                name: "RiskFactorTemplates");

            migrationBuilder.DropTable(
                name: "SarComments");

            migrationBuilder.DropTable(
                name: "SarStatusHistories");

            migrationBuilder.DropTable(
                name: "ScreeningJobs");

            migrationBuilder.DropTable(
                name: "ScreeningMetrics");

            migrationBuilder.DropTable(
                name: "StrComments");

            migrationBuilder.DropTable(
                name: "StrStatusHistories");

            migrationBuilder.DropTable(
                name: "SystemConfigurations");

            migrationBuilder.DropTable(
                name: "Alerts");

            migrationBuilder.DropTable(
                name: "OrganizationCustomLists");

            migrationBuilder.DropTable(
                name: "SuspiciousActivityReports");

            migrationBuilder.DropTable(
                name: "SuspiciousTransactionReports");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "WatchlistEntries");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "OrganizationUsers");

            migrationBuilder.DropTable(
                name: "Organizations");
        }
    }
}
