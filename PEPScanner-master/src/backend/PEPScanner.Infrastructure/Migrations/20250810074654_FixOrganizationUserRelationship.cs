using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PEPScanner.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixOrganizationUserRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrganizationUsers_Organizations_OrganizationId1",
                table: "OrganizationUsers");

            migrationBuilder.DropIndex(
                name: "IX_OrganizationUsers_OrganizationId1",
                table: "OrganizationUsers");

            migrationBuilder.DropColumn(
                name: "OrganizationId1",
                table: "OrganizationUsers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId1",
                table: "OrganizationUsers",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUsers_OrganizationId1",
                table: "OrganizationUsers",
                column: "OrganizationId1");

            migrationBuilder.AddForeignKey(
                name: "FK_OrganizationUsers_Organizations_OrganizationId1",
                table: "OrganizationUsers",
                column: "OrganizationId1",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
