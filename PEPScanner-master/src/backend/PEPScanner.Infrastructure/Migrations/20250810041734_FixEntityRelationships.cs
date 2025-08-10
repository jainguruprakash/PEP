using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PEPScanner.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixEntityRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Alerts_Customers_CustomerId",
                table: "Alerts");

            migrationBuilder.DropForeignKey(
                name: "FK_Alerts_WatchlistEntries_WatchlistEntryId",
                table: "Alerts");

            migrationBuilder.DropForeignKey(
                name: "FK_Customers_Organizations_OrganizationId",
                table: "Customers");

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
                name: "IX_WatchlistEntries_RiskLevel",
                table: "WatchlistEntries");

            migrationBuilder.DropIndex(
                name: "IX_ScreeningJobs_JobType",
                table: "ScreeningJobs");

            migrationBuilder.DropIndex(
                name: "IX_ScreeningJobs_StartedAtUtc",
                table: "ScreeningJobs");

            migrationBuilder.DropIndex(
                name: "IX_OrganizationWatchlists_IsEnabled",
                table: "OrganizationWatchlists");

            migrationBuilder.DropIndex(
                name: "IX_OrganizationWatchlists_Priority",
                table: "OrganizationWatchlists");

            migrationBuilder.DropIndex(
                name: "IX_OrganizationWatchlists_WatchlistSource",
                table: "OrganizationWatchlists");

            migrationBuilder.DropIndex(
                name: "IX_OrganizationWatchlists_WatchlistType",
                table: "OrganizationWatchlists");

            migrationBuilder.DropIndex(
                name: "IX_OrganizationUsers_CreatedAtUtc",
                table: "OrganizationUsers");

            migrationBuilder.DropIndex(
                name: "IX_OrganizationUsers_Email",
                table: "OrganizationUsers");

            migrationBuilder.DropIndex(
                name: "IX_OrganizationUsers_IsActive",
                table: "OrganizationUsers");

            migrationBuilder.DropIndex(
                name: "IX_OrganizationUsers_Role",
                table: "OrganizationUsers");

            migrationBuilder.DropIndex(
                name: "IX_OrganizationUsers_Username",
                table: "OrganizationUsers");

            migrationBuilder.DropIndex(
                name: "IX_Organizations_Code",
                table: "Organizations");

            migrationBuilder.DropIndex(
                name: "IX_Organizations_Country",
                table: "Organizations");

            migrationBuilder.DropIndex(
                name: "IX_Organizations_CreatedAtUtc",
                table: "Organizations");

            migrationBuilder.DropIndex(
                name: "IX_Organizations_IsActive",
                table: "Organizations");

            migrationBuilder.DropIndex(
                name: "IX_Organizations_Name",
                table: "Organizations");

            migrationBuilder.DropIndex(
                name: "IX_Organizations_Type",
                table: "Organizations");

            migrationBuilder.DropIndex(
                name: "IX_OrganizationConfigurations_Category",
                table: "OrganizationConfigurations");

            migrationBuilder.DropIndex(
                name: "IX_OrganizationConfigurations_IsRequired",
                table: "OrganizationConfigurations");

            migrationBuilder.DropIndex(
                name: "IX_OrganizationConfigurations_Key",
                table: "OrganizationConfigurations");

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
                name: "IX_Customers_RiskLevel",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_CustomerRelationships_RelationshipType",
                table: "CustomerRelationships");

            migrationBuilder.DropIndex(
                name: "IX_CustomerDocuments_DocumentNumber",
                table: "CustomerDocuments");

            migrationBuilder.DropIndex(
                name: "IX_CustomerDocuments_DocumentType",
                table: "CustomerDocuments");

            migrationBuilder.DropIndex(
                name: "IX_CustomerDocuments_IsVerified",
                table: "CustomerDocuments");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_Action",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_Date",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_EntityId",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_EntityType",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_Hour",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_TimestampUtc",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_Alerts_AssignedTo",
                table: "Alerts");

            migrationBuilder.DropIndex(
                name: "IX_Alerts_Priority",
                table: "Alerts");

            migrationBuilder.DropIndex(
                name: "IX_Alerts_SimilarityScore",
                table: "Alerts");

            migrationBuilder.RenameIndex(
                name: "IX_WatchlistEntries_Source",
                table: "WatchlistEntries",
                newName: "IX_WatchlistEntry_Source");

            migrationBuilder.RenameIndex(
                name: "IX_WatchlistEntries_PrimaryName",
                table: "WatchlistEntries",
                newName: "IX_WatchlistEntry_PrimaryName");

            migrationBuilder.RenameIndex(
                name: "IX_ScreeningJobs_Status",
                table: "ScreeningJobs",
                newName: "IX_ScreeningJob_Status");

            migrationBuilder.RenameIndex(
                name: "IX_ScreeningJobs_CreatedAtUtc",
                table: "ScreeningJobs",
                newName: "IX_ScreeningJob_CreatedAtUtc");

            migrationBuilder.RenameIndex(
                name: "IX_Alerts_Status",
                table: "Alerts",
                newName: "IX_Alert_Status");

            migrationBuilder.RenameIndex(
                name: "IX_Alerts_CreatedAtUtc",
                table: "Alerts",
                newName: "IX_Alert_CreatedAtUtc");

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId1",
                table: "OrganizationWatchlists",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId1",
                table: "OrganizationUsers",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId1",
                table: "OrganizationConfigurations",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId1",
                table: "Customers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "WatchlistEntryId1",
                table: "Alerts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "NotificationLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AlertId = table.Column<Guid>(type: "TEXT", nullable: true),
                    NotificationType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Payload = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ErrorMessage = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Recipient = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Subject = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SentAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RetryCount = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WatchlistEntry_Source_PrimaryName",
                table: "WatchlistEntries",
                columns: new[] { "Source", "PrimaryName" });

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationWatchlists_OrganizationId1",
                table: "OrganizationWatchlists",
                column: "OrganizationId1");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUsers_OrganizationId1",
                table: "OrganizationUsers",
                column: "OrganizationId1");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationConfigurations_OrganizationId1",
                table: "OrganizationConfigurations",
                column: "OrganizationId1");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_OrganizationId1",
                table: "Customers",
                column: "OrganizationId1");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_WatchlistEntryId1",
                table: "Alerts",
                column: "WatchlistEntryId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Alerts_Customers_CustomerId",
                table: "Alerts",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Alerts_WatchlistEntries_WatchlistEntryId",
                table: "Alerts",
                column: "WatchlistEntryId",
                principalTable: "WatchlistEntries",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Alerts_WatchlistEntries_WatchlistEntryId1",
                table: "Alerts",
                column: "WatchlistEntryId1",
                principalTable: "WatchlistEntries",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_Organizations_OrganizationId",
                table: "Customers",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_Organizations_OrganizationId1",
                table: "Customers",
                column: "OrganizationId1",
                principalTable: "Organizations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OrganizationConfigurations_Organizations_OrganizationId1",
                table: "OrganizationConfigurations",
                column: "OrganizationId1",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrganizationUsers_Organizations_OrganizationId1",
                table: "OrganizationUsers",
                column: "OrganizationId1",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrganizationWatchlists_Organizations_OrganizationId1",
                table: "OrganizationWatchlists",
                column: "OrganizationId1",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Alerts_Customers_CustomerId",
                table: "Alerts");

            migrationBuilder.DropForeignKey(
                name: "FK_Alerts_WatchlistEntries_WatchlistEntryId",
                table: "Alerts");

            migrationBuilder.DropForeignKey(
                name: "FK_Alerts_WatchlistEntries_WatchlistEntryId1",
                table: "Alerts");

            migrationBuilder.DropForeignKey(
                name: "FK_Customers_Organizations_OrganizationId",
                table: "Customers");

            migrationBuilder.DropForeignKey(
                name: "FK_Customers_Organizations_OrganizationId1",
                table: "Customers");

            migrationBuilder.DropForeignKey(
                name: "FK_OrganizationConfigurations_Organizations_OrganizationId1",
                table: "OrganizationConfigurations");

            migrationBuilder.DropForeignKey(
                name: "FK_OrganizationUsers_Organizations_OrganizationId1",
                table: "OrganizationUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_OrganizationWatchlists_Organizations_OrganizationId1",
                table: "OrganizationWatchlists");

            migrationBuilder.DropTable(
                name: "NotificationLogs");

            migrationBuilder.DropIndex(
                name: "IX_WatchlistEntry_Source_PrimaryName",
                table: "WatchlistEntries");

            migrationBuilder.DropIndex(
                name: "IX_OrganizationWatchlists_OrganizationId1",
                table: "OrganizationWatchlists");

            migrationBuilder.DropIndex(
                name: "IX_OrganizationUsers_OrganizationId1",
                table: "OrganizationUsers");

            migrationBuilder.DropIndex(
                name: "IX_OrganizationConfigurations_OrganizationId1",
                table: "OrganizationConfigurations");

            migrationBuilder.DropIndex(
                name: "IX_Customers_OrganizationId1",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Alerts_WatchlistEntryId1",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "OrganizationId1",
                table: "OrganizationWatchlists");

            migrationBuilder.DropColumn(
                name: "OrganizationId1",
                table: "OrganizationUsers");

            migrationBuilder.DropColumn(
                name: "OrganizationId1",
                table: "OrganizationConfigurations");

            migrationBuilder.DropColumn(
                name: "OrganizationId1",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "WatchlistEntryId1",
                table: "Alerts");

            migrationBuilder.RenameIndex(
                name: "IX_WatchlistEntry_Source",
                table: "WatchlistEntries",
                newName: "IX_WatchlistEntries_Source");

            migrationBuilder.RenameIndex(
                name: "IX_WatchlistEntry_PrimaryName",
                table: "WatchlistEntries",
                newName: "IX_WatchlistEntries_PrimaryName");

            migrationBuilder.RenameIndex(
                name: "IX_ScreeningJob_Status",
                table: "ScreeningJobs",
                newName: "IX_ScreeningJobs_Status");

            migrationBuilder.RenameIndex(
                name: "IX_ScreeningJob_CreatedAtUtc",
                table: "ScreeningJobs",
                newName: "IX_ScreeningJobs_CreatedAtUtc");

            migrationBuilder.RenameIndex(
                name: "IX_Alert_Status",
                table: "Alerts",
                newName: "IX_Alerts_Status");

            migrationBuilder.RenameIndex(
                name: "IX_Alert_CreatedAtUtc",
                table: "Alerts",
                newName: "IX_Alerts_CreatedAtUtc");

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
                name: "IX_WatchlistEntries_RiskLevel",
                table: "WatchlistEntries",
                column: "RiskLevel");

            migrationBuilder.CreateIndex(
                name: "IX_ScreeningJobs_JobType",
                table: "ScreeningJobs",
                column: "JobType");

            migrationBuilder.CreateIndex(
                name: "IX_ScreeningJobs_StartedAtUtc",
                table: "ScreeningJobs",
                column: "StartedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationWatchlists_IsEnabled",
                table: "OrganizationWatchlists",
                column: "IsEnabled");

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
                name: "IX_OrganizationUsers_Role",
                table: "OrganizationUsers",
                column: "Role");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUsers_Username",
                table: "OrganizationUsers",
                column: "Username");

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
                name: "IX_Customers_RiskLevel",
                table: "Customers",
                column: "RiskLevel");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerRelationships_RelationshipType",
                table: "CustomerRelationships",
                column: "RelationshipType");

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
                name: "IX_AuditLogs_TimestampUtc",
                table: "AuditLogs",
                column: "TimestampUtc");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_AssignedTo",
                table: "Alerts",
                column: "AssignedTo");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_Priority",
                table: "Alerts",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_SimilarityScore",
                table: "Alerts",
                column: "SimilarityScore");

            migrationBuilder.AddForeignKey(
                name: "FK_Alerts_Customers_CustomerId",
                table: "Alerts",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Alerts_WatchlistEntries_WatchlistEntryId",
                table: "Alerts",
                column: "WatchlistEntryId",
                principalTable: "WatchlistEntries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_Organizations_OrganizationId",
                table: "Customers",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
