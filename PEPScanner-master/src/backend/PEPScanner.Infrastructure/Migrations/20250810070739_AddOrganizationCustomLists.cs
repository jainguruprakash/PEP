using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PEPScanner.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizationCustomLists : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrganizationCustomLists",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ListName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ListType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsPrivate = table.Column<bool>(type: "INTEGER", nullable: false),
                    RiskLevel = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    MatchThreshold = table.Column<double>(type: "REAL", nullable: false),
                    AutoAlert = table.Column<bool>(type: "INTEGER", nullable: false),
                    RequireReview = table.Column<bool>(type: "INTEGER", nullable: false),
                    ReviewRole = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    UpdateFrequency = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    LastUpdateAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    NextUpdateAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TotalEntries = table.Column<int>(type: "INTEGER", nullable: false),
                    ActiveEntries = table.Column<int>(type: "INTEGER", nullable: false),
                    SourceFormat = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    SourceLocation = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Configuration = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
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
                name: "OrganizationCustomListEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CustomListId = table.Column<Guid>(type: "TEXT", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ExternalId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    PrimaryName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    AlternateNames = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    MiddleName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PlaceOfBirth = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Nationality = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Country = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Address = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    PostalCode = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    City = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    State = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    PositionOrRole = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    OrganizationName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Gender = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    IdType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    IdNumber = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    EmailAddress = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    RiskCategory = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    EntryType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CustomerType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    RelationshipType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    RelationshipStartDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RelationshipEndDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AccountNumbers = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    TotalBalance = table.Column<decimal>(type: "TEXT", nullable: true),
                    Currency = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsVerified = table.Column<bool>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Tags = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    AdditionalData = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    DateAddedUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastUpdatedUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastVerifiedUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AddedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    VerifiedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Source = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    SourceReference = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationCustomListEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizationCustomListEntries_OrganizationCustomLists_CustomListId",
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrganizationCustomListEntries");

            migrationBuilder.DropTable(
                name: "OrganizationCustomLists");
        }
    }
}
