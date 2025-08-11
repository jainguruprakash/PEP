using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PEPScanner.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrentWorkload",
                table: "OrganizationUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EscalationLevel",
                table: "OrganizationUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "ManagerId",
                table: "OrganizationUsers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxWorkload",
                table: "OrganizationUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "TeamId",
                table: "OrganizationUsers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Territory",
                table: "OrganizationUsers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Territory",
                table: "Customers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TeamId",
                table: "Alerts",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "NotificationRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    AlertType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RiskLevel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NotifyAssignee = table.Column<bool>(type: "boolean", nullable: false),
                    NotifyTeamLead = table.Column<bool>(type: "boolean", nullable: false),
                    NotifyManager = table.Column<bool>(type: "boolean", nullable: false),
                    NotifyRiskTeam = table.Column<bool>(type: "boolean", nullable: false),
                    Level1EscalationHours = table.Column<int>(type: "integer", nullable: false),
                    Level2EscalationHours = table.Column<int>(type: "integer", nullable: false),
                    Level3EscalationHours = table.Column<int>(type: "integer", nullable: false),
                    AutoAssignToTeamLead = table.Column<bool>(type: "boolean", nullable: false),
                    AutoEscalateOnSLA = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationRules_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Teams",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Department = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TeamLeadId = table.Column<Guid>(type: "uuid", nullable: true),
                    Territory = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ProductType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    MaxWorkload = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Teams_OrganizationUsers_TeamLeadId",
                        column: x => x.TeamLeadId,
                        principalTable: "OrganizationUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Teams_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUsers_ManagerId",
                table: "OrganizationUsers",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUsers_TeamId",
                table: "OrganizationUsers",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_TeamId",
                table: "Alerts",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationRules_OrganizationId",
                table: "NotificationRules",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_OrganizationId",
                table: "Teams",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_TeamLeadId",
                table: "Teams",
                column: "TeamLeadId");

            migrationBuilder.AddForeignKey(
                name: "FK_Alerts_Teams_TeamId",
                table: "Alerts",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_OrganizationUsers_OrganizationUsers_ManagerId",
                table: "OrganizationUsers",
                column: "ManagerId",
                principalTable: "OrganizationUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_OrganizationUsers_Teams_TeamId",
                table: "OrganizationUsers",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Alerts_Teams_TeamId",
                table: "Alerts");

            migrationBuilder.DropForeignKey(
                name: "FK_OrganizationUsers_OrganizationUsers_ManagerId",
                table: "OrganizationUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_OrganizationUsers_Teams_TeamId",
                table: "OrganizationUsers");

            migrationBuilder.DropTable(
                name: "NotificationRules");

            migrationBuilder.DropTable(
                name: "Teams");

            migrationBuilder.DropIndex(
                name: "IX_OrganizationUsers_ManagerId",
                table: "OrganizationUsers");

            migrationBuilder.DropIndex(
                name: "IX_OrganizationUsers_TeamId",
                table: "OrganizationUsers");

            migrationBuilder.DropIndex(
                name: "IX_Alerts_TeamId",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "CurrentWorkload",
                table: "OrganizationUsers");

            migrationBuilder.DropColumn(
                name: "EscalationLevel",
                table: "OrganizationUsers");

            migrationBuilder.DropColumn(
                name: "ManagerId",
                table: "OrganizationUsers");

            migrationBuilder.DropColumn(
                name: "MaxWorkload",
                table: "OrganizationUsers");

            migrationBuilder.DropColumn(
                name: "TeamId",
                table: "OrganizationUsers");

            migrationBuilder.DropColumn(
                name: "Territory",
                table: "OrganizationUsers");

            migrationBuilder.DropColumn(
                name: "Territory",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "TeamId",
                table: "Alerts");
        }
    }
}
