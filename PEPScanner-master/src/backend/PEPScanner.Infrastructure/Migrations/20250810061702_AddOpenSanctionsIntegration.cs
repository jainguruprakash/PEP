using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PEPScanner.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOpenSanctionsIntegration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OpenSanctionsAddresses",
                table: "Alerts",
                type: "TEXT",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OpenSanctionsAliases",
                table: "Alerts",
                type: "TEXT",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OpenSanctionsCountries",
                table: "Alerts",
                type: "TEXT",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OpenSanctionsDatasets",
                table: "Alerts",
                type: "TEXT",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OpenSanctionsEntityId",
                table: "Alerts",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OpenSanctionsEntityType",
                table: "Alerts",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OpenSanctionsFirstSeen",
                table: "Alerts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OpenSanctionsLastChange",
                table: "Alerts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OpenSanctionsLastChecked",
                table: "Alerts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OpenSanctionsLastSeen",
                table: "Alerts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OpenSanctionsMatchFeatures",
                table: "Alerts",
                type: "TEXT",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OpenSanctionsSanctions",
                table: "Alerts",
                type: "TEXT",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "OpenSanctionsScore",
                table: "Alerts",
                type: "REAL",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "OpenSanctionsEntities",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Schema = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Aliases = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    BirthDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Countries = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Addresses = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    Identifiers = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Sanctions = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    Datasets = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Score = table.Column<double>(type: "REAL", nullable: false),
                    FirstSeen = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastSeen = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastChange = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpenSanctionsEntities", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Alert_OpenSanctionsEntityId",
                table: "Alerts",
                column: "OpenSanctionsEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Alert_OpenSanctionsScore",
                table: "Alerts",
                column: "OpenSanctionsScore");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OpenSanctionsEntities");

            migrationBuilder.DropIndex(
                name: "IX_Alert_OpenSanctionsEntityId",
                table: "Alerts");

            migrationBuilder.DropIndex(
                name: "IX_Alert_OpenSanctionsScore",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "OpenSanctionsAddresses",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "OpenSanctionsAliases",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "OpenSanctionsCountries",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "OpenSanctionsDatasets",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "OpenSanctionsEntityId",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "OpenSanctionsEntityType",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "OpenSanctionsFirstSeen",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "OpenSanctionsLastChange",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "OpenSanctionsLastChecked",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "OpenSanctionsLastSeen",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "OpenSanctionsMatchFeatures",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "OpenSanctionsSanctions",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "OpenSanctionsScore",
                table: "Alerts");
        }
    }
}
