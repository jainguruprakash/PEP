using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PEPScanner.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReportsAndDashboard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AlertMetrics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TotalAlerts = table.Column<int>(type: "INTEGER", nullable: false),
                    HighPriorityAlerts = table.Column<int>(type: "INTEGER", nullable: false),
                    MediumPriorityAlerts = table.Column<int>(type: "INTEGER", nullable: false),
                    LowPriorityAlerts = table.Column<int>(type: "INTEGER", nullable: false),
                    ResolvedAlerts = table.Column<int>(type: "INTEGER", nullable: false),
                    PendingAlerts = table.Column<int>(type: "INTEGER", nullable: false),
                    EscalatedAlerts = table.Column<int>(type: "INTEGER", nullable: false),
                    AverageResolutionTime = table.Column<decimal>(type: "TEXT", nullable: false),
                    ComplianceScore = table.Column<decimal>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
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
                name: "ComplianceReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ReportType = table.Column<string>(type: "TEXT", nullable: false),
                    ReportPeriodStart = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ReportPeriodEnd = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ReportData = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    GeneratedById = table.Column<Guid>(type: "TEXT", nullable: true),
                    GeneratedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FilePath = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
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
                name: "DashboardMetrics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MetricDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    MetricType = table.Column<string>(type: "TEXT", nullable: false),
                    TotalCustomersScreened = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalTransactionsScreened = table.Column<int>(type: "INTEGER", nullable: false),
                    HighRiskMatches = table.Column<int>(type: "INTEGER", nullable: false),
                    MediumRiskMatches = table.Column<int>(type: "INTEGER", nullable: false),
                    LowRiskMatches = table.Column<int>(type: "INTEGER", nullable: false),
                    FalsePositives = table.Column<int>(type: "INTEGER", nullable: false),
                    TruePositives = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalAlertsGenerated = table.Column<int>(type: "INTEGER", nullable: false),
                    AlertsResolved = table.Column<int>(type: "INTEGER", nullable: false),
                    AlertsPending = table.Column<int>(type: "INTEGER", nullable: false),
                    AlertsEscalated = table.Column<int>(type: "INTEGER", nullable: false),
                    AverageResolutionTimeHours = table.Column<decimal>(type: "TEXT", nullable: false),
                    SarReportsCreated = table.Column<int>(type: "INTEGER", nullable: false),
                    SarReportsSubmitted = table.Column<int>(type: "INTEGER", nullable: false),
                    StrReportsCreated = table.Column<int>(type: "INTEGER", nullable: false),
                    StrReportsSubmitted = table.Column<int>(type: "INTEGER", nullable: false),
                    ComplianceScore = table.Column<decimal>(type: "TEXT", nullable: false),
                    RegulatoryDeadlinesMissed = table.Column<int>(type: "INTEGER", nullable: false),
                    RegulatoryDeadlinesMet = table.Column<int>(type: "INTEGER", nullable: false),
                    SystemUptime = table.Column<decimal>(type: "TEXT", nullable: false),
                    AverageScreeningTimeMs = table.Column<decimal>(type: "TEXT", nullable: false),
                    ApiCallsCount = table.Column<int>(type: "INTEGER", nullable: false),
                    ErrorsCount = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
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
                name: "ScreeningMetrics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CustomersScreened = table.Column<int>(type: "INTEGER", nullable: false),
                    TransactionsScreened = table.Column<int>(type: "INTEGER", nullable: false),
                    PepMatches = table.Column<int>(type: "INTEGER", nullable: false),
                    SanctionMatches = table.Column<int>(type: "INTEGER", nullable: false),
                    WatchlistMatches = table.Column<int>(type: "INTEGER", nullable: false),
                    TruePositives = table.Column<int>(type: "INTEGER", nullable: false),
                    FalsePositives = table.Column<int>(type: "INTEGER", nullable: false),
                    AccuracyRate = table.Column<decimal>(type: "TEXT", nullable: false),
                    AverageScreeningTime = table.Column<decimal>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
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
                name: "SuspiciousActivityReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ReportNumber = table.Column<string>(type: "TEXT", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ReportedById = table.Column<Guid>(type: "TEXT", nullable: false),
                    ReviewedById = table.Column<Guid>(type: "TEXT", nullable: true),
                    CustomerId = table.Column<Guid>(type: "TEXT", nullable: true),
                    SubjectName = table.Column<string>(type: "TEXT", nullable: false),
                    SubjectAddress = table.Column<string>(type: "TEXT", nullable: true),
                    SubjectIdentification = table.Column<string>(type: "TEXT", nullable: true),
                    SubjectDateOfBirth = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SuspiciousActivity = table.Column<string>(type: "TEXT", nullable: false),
                    ActivityDescription = table.Column<string>(type: "TEXT", nullable: false),
                    TransactionAmount = table.Column<decimal>(type: "TEXT", nullable: true),
                    TransactionCurrency = table.Column<string>(type: "TEXT", nullable: true),
                    TransactionDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TransactionLocation = table.Column<string>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false),
                    IncidentDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DiscoveryDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RegulatoryReferences = table.Column<string>(type: "TEXT", nullable: true),
                    AttachedDocuments = table.Column<string>(type: "TEXT", nullable: true),
                    InternalNotes = table.Column<string>(type: "TEXT", nullable: true),
                    ComplianceComments = table.Column<string>(type: "TEXT", nullable: true),
                    SubmissionDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RegulatoryFilingDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RegulatoryReferenceNumber = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
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
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ReportNumber = table.Column<string>(type: "TEXT", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ReportedById = table.Column<Guid>(type: "TEXT", nullable: false),
                    ReviewedById = table.Column<Guid>(type: "TEXT", nullable: true),
                    CustomerId = table.Column<Guid>(type: "TEXT", nullable: true),
                    TransactionReference = table.Column<string>(type: "TEXT", nullable: false),
                    TransactionAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    TransactionCurrency = table.Column<string>(type: "TEXT", nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TransactionType = table.Column<string>(type: "TEXT", nullable: false),
                    OriginatorName = table.Column<string>(type: "TEXT", nullable: true),
                    OriginatorAccount = table.Column<string>(type: "TEXT", nullable: true),
                    OriginatorBank = table.Column<string>(type: "TEXT", nullable: true),
                    BeneficiaryName = table.Column<string>(type: "TEXT", nullable: true),
                    BeneficiaryAccount = table.Column<string>(type: "TEXT", nullable: true),
                    BeneficiaryBank = table.Column<string>(type: "TEXT", nullable: true),
                    SuspicionReason = table.Column<string>(type: "TEXT", nullable: false),
                    DetailedDescription = table.Column<string>(type: "TEXT", nullable: false),
                    CountryOfOrigin = table.Column<string>(type: "TEXT", nullable: true),
                    CountryOfDestination = table.Column<string>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false),
                    RegulatoryReferences = table.Column<string>(type: "TEXT", nullable: true),
                    AttachedDocuments = table.Column<string>(type: "TEXT", nullable: true),
                    InternalNotes = table.Column<string>(type: "TEXT", nullable: true),
                    ComplianceComments = table.Column<string>(type: "TEXT", nullable: true),
                    SubmissionDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RegulatoryFilingDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RegulatoryReferenceNumber = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
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
                name: "SarComments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SarId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Comment = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
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
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SarId = table.Column<Guid>(type: "TEXT", nullable: false),
                    FromStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    ToStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    ChangedById = table.Column<Guid>(type: "TEXT", nullable: false),
                    Reason = table.Column<string>(type: "TEXT", nullable: true),
                    ChangedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
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
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    StrId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Comment = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
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
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    StrId = table.Column<Guid>(type: "TEXT", nullable: false),
                    FromStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    ToStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    ChangedById = table.Column<Guid>(type: "TEXT", nullable: false),
                    Reason = table.Column<string>(type: "TEXT", nullable: true),
                    ChangedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
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
                name: "IX_AlertMetrics_Org_Date",
                table: "AlertMetrics",
                columns: new[] { "OrganizationId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceReport_Org_Type_Period",
                table: "ComplianceReports",
                columns: new[] { "OrganizationId", "ReportType", "ReportPeriodStart" });

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceReports_GeneratedById",
                table: "ComplianceReports",
                column: "GeneratedById");

            migrationBuilder.CreateIndex(
                name: "IX_DashboardMetrics_Org_Date_Type",
                table: "DashboardMetrics",
                columns: new[] { "OrganizationId", "MetricDate", "MetricType" },
                unique: true);

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlertMetrics");

            migrationBuilder.DropTable(
                name: "ComplianceReports");

            migrationBuilder.DropTable(
                name: "DashboardMetrics");

            migrationBuilder.DropTable(
                name: "SarComments");

            migrationBuilder.DropTable(
                name: "SarStatusHistories");

            migrationBuilder.DropTable(
                name: "ScreeningMetrics");

            migrationBuilder.DropTable(
                name: "StrComments");

            migrationBuilder.DropTable(
                name: "StrStatusHistories");

            migrationBuilder.DropTable(
                name: "SuspiciousActivityReports");

            migrationBuilder.DropTable(
                name: "SuspiciousTransactionReports");
        }
    }
}
