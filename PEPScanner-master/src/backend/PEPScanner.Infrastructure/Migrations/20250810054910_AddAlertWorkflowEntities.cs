using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PEPScanner.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAlertWorkflowEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApprovalComments",
                table: "Alerts",
                type: "TEXT",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAtUtc",
                table: "Alerts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovedBy",
                table: "Alerts",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CurrentReviewer",
                table: "Alerts",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EscalationLevel",
                table: "Alerts",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastActionDateUtc",
                table: "Alerts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastActionType",
                table: "Alerts",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RejectedAtUtc",
                table: "Alerts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectedBy",
                table: "Alerts",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "Alerts",
                type: "TEXT",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Alerts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId1",
                table: "Alerts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkflowStatus",
                table: "Alerts",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "AlertActions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AlertId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ActionType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    PerformedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    PreviousStatus = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    NewStatus = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    PreviousAssignee = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    NewAssignee = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Comments = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Reason = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    AdditionalData = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ActionDateUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IpAddress = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    UserAgent = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
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
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Role = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Department = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ManagerEmail = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    JobTitle = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    PasswordHash = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    LastLoginUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CanCreateAlerts = table.Column<bool>(type: "INTEGER", nullable: false),
                    CanReviewAlerts = table.Column<bool>(type: "INTEGER", nullable: false),
                    CanApproveAlerts = table.Column<bool>(type: "INTEGER", nullable: false),
                    CanEscalateAlerts = table.Column<bool>(type: "INTEGER", nullable: false),
                    CanCloseAlerts = table.Column<bool>(type: "INTEGER", nullable: false),
                    CanViewAllAlerts = table.Column<bool>(type: "INTEGER", nullable: false),
                    CanAssignAlerts = table.Column<bool>(type: "INTEGER", nullable: false),
                    CanGenerateReports = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_UserId",
                table: "Alerts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_UserId1",
                table: "Alerts",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_AlertActions_AlertId",
                table: "AlertActions",
                column: "AlertId");

            migrationBuilder.AddForeignKey(
                name: "FK_Alerts_Users_UserId",
                table: "Alerts",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Alerts_Users_UserId1",
                table: "Alerts",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Alerts_Users_UserId",
                table: "Alerts");

            migrationBuilder.DropForeignKey(
                name: "FK_Alerts_Users_UserId1",
                table: "Alerts");

            migrationBuilder.DropTable(
                name: "AlertActions");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Alerts_UserId",
                table: "Alerts");

            migrationBuilder.DropIndex(
                name: "IX_Alerts_UserId1",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "ApprovalComments",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "ApprovedAtUtc",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "ApprovedBy",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "CurrentReviewer",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "EscalationLevel",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "LastActionDateUtc",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "LastActionType",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "RejectedAtUtc",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "RejectedBy",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "WorkflowStatus",
                table: "Alerts");
        }
    }
}
