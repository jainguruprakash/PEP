using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PEPScanner.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizationMultiTenancy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AddedBy",
                table: "WatchlistEntries",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "WatchlistEntries",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AircraftOperator",
                table: "WatchlistEntries",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AircraftRegistration",
                table: "WatchlistEntries",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AircraftType",
                table: "WatchlistEntries",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Citizenship",
                table: "WatchlistEntries",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "WatchlistEntries",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Comments",
                table: "WatchlistEntries",
                type: "TEXT",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CountryOfResidence",
                table: "WatchlistEntries",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataQuality",
                table: "WatchlistEntries",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateLastUpdatedUtc",
                table: "WatchlistEntries",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirthFrom",
                table: "WatchlistEntries",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirthTo",
                table: "WatchlistEntries",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EntityType",
                table: "WatchlistEntries",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalId",
                table: "WatchlistEntries",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalReference",
                table: "WatchlistEntries",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "WatchlistEntries",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsWhitelisted",
                table: "WatchlistEntries",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LicenseNumber",
                table: "WatchlistEntries",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ListType",
                table: "WatchlistEntries",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MediaCategory",
                table: "WatchlistEntries",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "MediaDate",
                table: "WatchlistEntries",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MediaSource",
                table: "WatchlistEntries",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MediaSummary",
                table: "WatchlistEntries",
                type: "TEXT",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MediaUrl",
                table: "WatchlistEntries",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NationalIdNumber",
                table: "WatchlistEntries",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Nationality",
                table: "WatchlistEntries",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PassportNumber",
                table: "WatchlistEntries",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PepCategory",
                table: "WatchlistEntries",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PepCountry",
                table: "WatchlistEntries",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PepDescription",
                table: "WatchlistEntries",
                type: "TEXT",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PepEndDate",
                table: "WatchlistEntries",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PepPosition",
                table: "WatchlistEntries",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PepStartDate",
                table: "WatchlistEntries",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlaceOfBirth",
                table: "WatchlistEntries",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                table: "WatchlistEntries",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegistrationNumber",
                table: "WatchlistEntries",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RemovedBy",
                table: "WatchlistEntries",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RiskLevel",
                table: "WatchlistEntries",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SanctionAuthority",
                table: "WatchlistEntries",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SanctionEndDate",
                table: "WatchlistEntries",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SanctionReason",
                table: "WatchlistEntries",
                type: "TEXT",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SanctionReference",
                table: "WatchlistEntries",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SanctionStartDate",
                table: "WatchlistEntries",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SanctionType",
                table: "WatchlistEntries",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "State",
                table: "WatchlistEntries",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaxIdNumber",
                table: "WatchlistEntries",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "WatchlistEntries",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VesselCallSign",
                table: "WatchlistEntries",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VesselFlag",
                table: "WatchlistEntries",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VesselImoNumber",
                table: "WatchlistEntries",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VesselName",
                table: "WatchlistEntries",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VesselType",
                table: "WatchlistEntries",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccountNumber",
                table: "Customers",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccountType",
                table: "Customers",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AverageTransactionAmount",
                table: "Customers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Customers",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "Customers",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Customers",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAtUtc",
                table: "Customers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EddNotes",
                table: "Customers",
                type: "TEXT",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmailAddress",
                table: "Customers",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Employer",
                table: "Customers",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FaceBiometricData",
                table: "Customers",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FingerprintData",
                table: "Customers",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdentificationType",
                table: "Customers",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Customers",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Customers",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastScreeningDate",
                table: "Customers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlyTransactionVolume",
                table: "Customers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextScreeningDate",
                table: "Customers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "Customers",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "PepCountry",
                table: "Customers",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PepEndDate",
                table: "Customers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PepPosition",
                table: "Customers",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PepStartDate",
                table: "Customers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Customers",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhotoUrl",
                table: "Customers",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                table: "Customers",
                type: "TEXT",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresEdd",
                table: "Customers",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RiskLevel",
                table: "Customers",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ScreeningFrequency",
                table: "Customers",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "State",
                table: "Customers",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Customers",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "CustomerRelationships",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "CustomerRelationships",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RelationshipDetails",
                table: "CustomerRelationships",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "CustomerRelationships",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "CustomerRelationships",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ActualHours",
                table: "Alerts",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AlertType",
                table: "Alerts",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Alerts",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ComplianceAction",
                table: "Alerts",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Alerts",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "Alerts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EscalatedAtUtc",
                table: "Alerts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EscalatedTo",
                table: "Alerts",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EscalationDate",
                table: "Alerts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MatchedFields",
                table: "Alerts",
                type: "TEXT",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MatchingDetails",
                table: "Alerts",
                type: "TEXT",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Outcome",
                table: "Alerts",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Priority",
                table: "Alerts",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "RequiresEdd",
                table: "Alerts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresSar",
                table: "Alerts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresStr",
                table: "Alerts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedAtUtc",
                table: "Alerts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewedBy",
                table: "Alerts",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RiskLevel",
                table: "Alerts",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SarFiledAtUtc",
                table: "Alerts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SarFiledBy",
                table: "Alerts",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SarReference",
                table: "Alerts",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SlaHours",
                table: "Alerts",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SlaStatus",
                table: "Alerts",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceCategory",
                table: "Alerts",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceList",
                table: "Alerts",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StrFiledAtUtc",
                table: "Alerts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StrFiledBy",
                table: "Alerts",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StrReference",
                table: "Alerts",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubCategory",
                table: "Alerts",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tags",
                table: "Alerts",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TransactionAmount",
                table: "Alerts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TransactionDate",
                table: "Alerts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TransactionId",
                table: "Alerts",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TransactionType",
                table: "Alerts",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Alerts",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CustomerDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CustomerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DocumentType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    DocumentNumber = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    DocumentName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    DocumentUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    DocumentFormat = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    IssueDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IssuingAuthority = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    IssuingCountry = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    IsVerified = table.Column<bool>(type: "INTEGER", nullable: false),
                    VerifiedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    VerifiedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    VerificationNotes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true)
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
                name: "Organizations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Type = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Industry = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Country = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    State = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    City = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Address = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    PostalCode = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    ContactPerson = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ContactEmail = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ContactPhone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Website = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    LicenseNumber = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    RegulatoryBody = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    SubscriptionStartDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SubscriptionEndDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SubscriptionPlan = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    MaxUsers = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxCustomers = table.Column<int>(type: "INTEGER", nullable: false),
                    IsTrial = table.Column<bool>(type: "INTEGER", nullable: false),
                    Configuration = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    TimeZone = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Currency = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Language = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScreeningJobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    JobName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    JobType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CompletedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TotalRecords = table.Column<int>(type: "INTEGER", nullable: false),
                    ProcessedRecords = table.Column<int>(type: "INTEGER", nullable: false),
                    MatchesFound = table.Column<int>(type: "INTEGER", nullable: false),
                    AlertsGenerated = table.Column<int>(type: "INTEGER", nullable: false),
                    ErrorMessage = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    InputFile = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    OutputFile = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    TriggeredBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    AssignedTo = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false),
                    Configuration = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScreeningJobs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrganizationConfigurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Key = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    DataType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    IsRequired = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsEncrypted = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
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
                });

            migrationBuilder.CreateTable(
                name: "OrganizationUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Username = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    FullName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Role = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Department = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Position = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsEmailVerified = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastLoginAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PasswordChangedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Permissions = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    TimeZone = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Language = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false)
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
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    WatchlistSource = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    WatchlistType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsRequired = table.Column<bool>(type: "INTEGER", nullable: false),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false),
                    MatchThreshold = table.Column<double>(type: "REAL", nullable: false),
                    RiskLevel = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    AutoAlert = table.Column<bool>(type: "INTEGER", nullable: false),
                    RequireReview = table.Column<bool>(type: "INTEGER", nullable: false),
                    ReviewRole = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    LastUpdateAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    NextUpdateAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateFrequency = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Configuration = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false)
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
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Action = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    EntityType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    EntityId = table.Column<Guid>(type: "TEXT", nullable: true),
                    UserId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    UserName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    UserRole = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    IpAddress = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    UserAgent = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    SessionId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    OldValues = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    NewValues = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    AdditionalData = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Severity = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    IsSuccessful = table.Column<bool>(type: "INTEGER", nullable: false),
                    ErrorMessage = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    TimestampUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Hour = table.Column<int>(type: "INTEGER", nullable: false),
                    OrganizationUserId = table.Column<Guid>(type: "TEXT", nullable: true)
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

            migrationBuilder.CreateIndex(
                name: "IX_WatchlistEntries_Country",
                table: "WatchlistEntries",
                column: "Country");

            migrationBuilder.CreateIndex(
                name: "IX_WatchlistEntries_DateAddedUtc",
                table: "WatchlistEntries",
                column: "DateAddedUtc");

            migrationBuilder.CreateIndex(
                name: "IX_WatchlistEntries_IsActive",
                table: "WatchlistEntries",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_WatchlistEntries_ListType",
                table: "WatchlistEntries",
                column: "ListType");

            migrationBuilder.CreateIndex(
                name: "IX_WatchlistEntries_PrimaryName",
                table: "WatchlistEntries",
                column: "PrimaryName");

            migrationBuilder.CreateIndex(
                name: "IX_WatchlistEntries_RiskLevel",
                table: "WatchlistEntries",
                column: "RiskLevel");

            migrationBuilder.CreateIndex(
                name: "IX_WatchlistEntries_Source",
                table: "WatchlistEntries",
                column: "Source");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_CreatedAtUtc",
                table: "Customers",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_FullName",
                table: "Customers",
                column: "FullName");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_IdentificationNumber",
                table: "Customers",
                column: "IdentificationNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_IsActive",
                table: "Customers",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_IsPep",
                table: "Customers",
                column: "IsPep");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_OrganizationId",
                table: "Customers",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_RiskLevel",
                table: "Customers",
                column: "RiskLevel");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerRelationships_RelationshipType",
                table: "CustomerRelationships",
                column: "RelationshipType");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_AssignedTo",
                table: "Alerts",
                column: "AssignedTo");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_CreatedAtUtc",
                table: "Alerts",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_Priority",
                table: "Alerts",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_SimilarityScore",
                table: "Alerts",
                column: "SimilarityScore");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_Status",
                table: "Alerts",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Action",
                table: "AuditLogs",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Date",
                table: "AuditLogs",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityId",
                table: "AuditLogs",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityType",
                table: "AuditLogs",
                column: "EntityType");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Hour",
                table: "AuditLogs",
                column: "Hour");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_OrganizationUserId",
                table: "AuditLogs",
                column: "OrganizationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_TimestampUtc",
                table: "AuditLogs",
                column: "TimestampUtc");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerDocuments_CustomerId",
                table: "CustomerDocuments",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerDocuments_DocumentNumber",
                table: "CustomerDocuments",
                column: "DocumentNumber");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerDocuments_DocumentType",
                table: "CustomerDocuments",
                column: "DocumentType");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerDocuments_IsVerified",
                table: "CustomerDocuments",
                column: "IsVerified");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationConfigurations_Category",
                table: "OrganizationConfigurations",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationConfigurations_IsRequired",
                table: "OrganizationConfigurations",
                column: "IsRequired");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationConfigurations_Key",
                table: "OrganizationConfigurations",
                column: "Key");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationConfigurations_OrganizationId",
                table: "OrganizationConfigurations",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_Code",
                table: "Organizations",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_Country",
                table: "Organizations",
                column: "Country");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_CreatedAtUtc",
                table: "Organizations",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_IsActive",
                table: "Organizations",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_Name",
                table: "Organizations",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_Type",
                table: "Organizations",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUsers_CreatedAtUtc",
                table: "OrganizationUsers",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUsers_Email",
                table: "OrganizationUsers",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUsers_IsActive",
                table: "OrganizationUsers",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUsers_OrganizationId",
                table: "OrganizationUsers",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUsers_Role",
                table: "OrganizationUsers",
                column: "Role");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUsers_Username",
                table: "OrganizationUsers",
                column: "Username");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationWatchlists_IsEnabled",
                table: "OrganizationWatchlists",
                column: "IsEnabled");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationWatchlists_OrganizationId",
                table: "OrganizationWatchlists",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationWatchlists_Priority",
                table: "OrganizationWatchlists",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationWatchlists_WatchlistSource",
                table: "OrganizationWatchlists",
                column: "WatchlistSource");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationWatchlists_WatchlistType",
                table: "OrganizationWatchlists",
                column: "WatchlistType");

            migrationBuilder.CreateIndex(
                name: "IX_ScreeningJobs_CreatedAtUtc",
                table: "ScreeningJobs",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_ScreeningJobs_JobType",
                table: "ScreeningJobs",
                column: "JobType");

            migrationBuilder.CreateIndex(
                name: "IX_ScreeningJobs_StartedAtUtc",
                table: "ScreeningJobs",
                column: "StartedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_ScreeningJobs_Status",
                table: "ScreeningJobs",
                column: "Status");

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_Organizations_OrganizationId",
                table: "Customers",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customers_Organizations_OrganizationId",
                table: "Customers");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "CustomerDocuments");

            migrationBuilder.DropTable(
                name: "OrganizationConfigurations");

            migrationBuilder.DropTable(
                name: "OrganizationWatchlists");

            migrationBuilder.DropTable(
                name: "ScreeningJobs");

            migrationBuilder.DropTable(
                name: "OrganizationUsers");

            migrationBuilder.DropTable(
                name: "Organizations");

            migrationBuilder.DropIndex(
                name: "IX_WatchlistEntries_Country",
                table: "WatchlistEntries");

            migrationBuilder.DropIndex(
                name: "IX_WatchlistEntries_DateAddedUtc",
                table: "WatchlistEntries");

            migrationBuilder.DropIndex(
                name: "IX_WatchlistEntries_IsActive",
                table: "WatchlistEntries");

            migrationBuilder.DropIndex(
                name: "IX_WatchlistEntries_ListType",
                table: "WatchlistEntries");

            migrationBuilder.DropIndex(
                name: "IX_WatchlistEntries_PrimaryName",
                table: "WatchlistEntries");

            migrationBuilder.DropIndex(
                name: "IX_WatchlistEntries_RiskLevel",
                table: "WatchlistEntries");

            migrationBuilder.DropIndex(
                name: "IX_WatchlistEntries_Source",
                table: "WatchlistEntries");

            migrationBuilder.DropIndex(
                name: "IX_Customers_CreatedAtUtc",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_FullName",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_IdentificationNumber",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_IsActive",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_IsPep",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_OrganizationId",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_RiskLevel",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_CustomerRelationships_RelationshipType",
                table: "CustomerRelationships");

            migrationBuilder.DropIndex(
                name: "IX_Alerts_AssignedTo",
                table: "Alerts");

            migrationBuilder.DropIndex(
                name: "IX_Alerts_CreatedAtUtc",
                table: "Alerts");

            migrationBuilder.DropIndex(
                name: "IX_Alerts_Priority",
                table: "Alerts");

            migrationBuilder.DropIndex(
                name: "IX_Alerts_SimilarityScore",
                table: "Alerts");

            migrationBuilder.DropIndex(
                name: "IX_Alerts_Status",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "AddedBy",
                table: "WatchlistEntries");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "WatchlistEntries");

            migrationBuilder.DropColumn(
                name: "AircraftOperator",
                table: "WatchlistEntries");

            migrationBuilder.DropColumn(
                name: "AircraftRegistration",
                table: "WatchlistEntries");

            migrationBuilder.DropColumn(
                name: "AircraftType",
                table: "WatchlistEntries");

            migrationBuilder.DropColumn(
                name: "Citizenship",
                table: "WatchlistEntries");

            migrationBuilder.DropColumn(
                name: "City",
                table: "WatchlistEntries");

            migrationBuilder.DropColumn(
                name: "Comments",
                table: "WatchlistEntries");

            migrationBuilder.DropColumn(
                name: "CountryOfResidence",
                table: "WatchlistEntries");

            migrationBuilder.DropColumn(
                name: "DataQuality",
                table: "WatchlistEntries");

            migrationBuilder.DropColumn(
                name: "DateLastUpdatedUtc",
                table: "WatchlistEntries");

            migrationBuilder.DropColumn(
                name: "DateOfBirthFrom",
                table: "WatchlistEntries");

            migrationBuilder.DropColumn(
                name: "DateOfBirthTo",
                table: "WatchlistEntries");

            migrationBuilder.DropColumn(
                name: "EntityType",
                table: "WatchlistEntries");

            migrationBuilder.DropColumn(
                name: "ExternalId",
                table: "WatchlistEntries");

            migrationBuilder.DropColumn(
                name: "ExternalReference",
                table: "WatchlistEntries");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "WatchlistEntries");

            migrationBuilder.DropColumn(
                name: "IsWhitelisted",
                table: "WatchlistEntries");

            migrationBuilder.DropColumn(
                name: "LicenseNumber",
                table: "WatchlistEntries");

            migrationBuilder.DropColumn(
                name: "ListType",
                table: "WatchlistEntries");

            migrationBuilder.DropColumn(
                name: "MediaCategory",
                table: "WatchlistEntries");

            migrationBuilder.DropColumn(
                name: "MediaDate",
                table: "WatchlistEntries");

            migrationBuilder.DropColumn(
                name: "MediaSource",
                table: "WatchlistEntries");

            migrationBuilder.DropColumn(
                name: "MediaSummary",
                table: "WatchlistEntries");

            migrationBuilder.DropColumn(
                name: "MediaUrl",
                table: "WatchlistEntries");

            migrationBuilder.DropColumn(
                name: "NationalIdNumber",
                table: "WatchlistEntries");

            migrationBuilder.DropColumn(
                name: "Nationality",
                table: "WatchlistEntries");

            migrationBuilder.DropColumn(
                name: "PassportNumber",
                table: "WatchlistEntries");

            migrationBuilder.DropColumn(
                name: "PepCategory",
                table: "WatchlistEntries");

            migrationBuilder.DropColumn(
                name: "PepCountry",
                table: "WatchlistEntries");

            migrationBuilder.DropColumn(
                name: "PepDescription",
                table: "WatchlistEntries");

            migrationBuilder.DropColumn(
                name: "PepEndDate",
                table: "WatchlistEntries");

            migrationBuilder.DropColumn(
                name: "PepPosition",
                table: "WatchlistEntries");

            migrationBuilder.DropColumn(
                name: "PepStartDate",
                table: "WatchlistEntries");

            migrationBuilder.DropColumn(
                name: "PlaceOfBirth",
                table: "WatchlistEntries");

            migrationBuilder.DropColumn(
                name: "PostalCode",
                table: "WatchlistEntries");

            migrationBuilder.DropColumn(
                name: "RegistrationNumber",
                table: "WatchlistEntries");

            migrationBuilder.DropColumn(
                name: "RemovedBy",
                table: "WatchlistEntries");

            migrationBuilder.DropColumn(
                name: "RiskLevel",
                table: "WatchlistEntries");

            migrationBuilder.DropColumn(
                name: "SanctionAuthority",
                table: "WatchlistEntries");

            migrationBuilder.DropColumn(
                name: "SanctionEndDate",
                table: "WatchlistEntries");

            migrationBuilder.DropColumn(
                name: "SanctionReason",
                table: "WatchlistEntries");

            migrationBuilder.DropColumn(
                name: "SanctionReference",
                table: "WatchlistEntries");

            migrationBuilder.DropColumn(
                name: "SanctionStartDate",
                table: "WatchlistEntries");

            migrationBuilder.DropColumn(
                name: "SanctionType",
                table: "WatchlistEntries");

            migrationBuilder.DropColumn(
                name: "State",
                table: "WatchlistEntries");

            migrationBuilder.DropColumn(
                name: "TaxIdNumber",
                table: "WatchlistEntries");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "WatchlistEntries");

            migrationBuilder.DropColumn(
                name: "VesselCallSign",
                table: "WatchlistEntries");

            migrationBuilder.DropColumn(
                name: "VesselFlag",
                table: "WatchlistEntries");

            migrationBuilder.DropColumn(
                name: "VesselImoNumber",
                table: "WatchlistEntries");

            migrationBuilder.DropColumn(
                name: "VesselName",
                table: "WatchlistEntries");

            migrationBuilder.DropColumn(
                name: "VesselType",
                table: "WatchlistEntries");

            migrationBuilder.DropColumn(
                name: "AccountNumber",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "AccountType",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "AverageTransactionAmount",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "DeletedAtUtc",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "EddNotes",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "EmailAddress",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "Employer",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "FaceBiometricData",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "FingerprintData",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "IdentificationType",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "LastScreeningDate",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "MonthlyTransactionVolume",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "NextScreeningDate",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "PepCountry",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "PepEndDate",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "PepPosition",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "PepStartDate",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "PhotoUrl",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "PostalCode",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "RequiresEdd",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "RiskLevel",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "ScreeningFrequency",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "State",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "CustomerRelationships");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "CustomerRelationships");

            migrationBuilder.DropColumn(
                name: "RelationshipDetails",
                table: "CustomerRelationships");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "CustomerRelationships");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "CustomerRelationships");

            migrationBuilder.DropColumn(
                name: "ActualHours",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "AlertType",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "ComplianceAction",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "EscalatedAtUtc",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "EscalatedTo",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "EscalationDate",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "MatchedFields",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "MatchingDetails",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "Outcome",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "RequiresEdd",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "RequiresSar",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "RequiresStr",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "ReviewedAtUtc",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "ReviewedBy",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "RiskLevel",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "SarFiledAtUtc",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "SarFiledBy",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "SarReference",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "SlaHours",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "SlaStatus",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "SourceCategory",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "SourceList",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "StrFiledAtUtc",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "StrFiledBy",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "StrReference",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "SubCategory",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "Tags",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "TransactionAmount",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "TransactionDate",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "TransactionId",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "TransactionType",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Alerts");
        }
    }
}
