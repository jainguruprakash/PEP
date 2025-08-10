using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PEPScanner.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthenticationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "OrganizationUsers",
                type: "TEXT",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RefreshToken",
                table: "OrganizationUsers",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RefreshTokenExpiryTime",
                table: "OrganizationUsers",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "OrganizationUsers");

            migrationBuilder.DropColumn(
                name: "RefreshToken",
                table: "OrganizationUsers");

            migrationBuilder.DropColumn(
                name: "RefreshTokenExpiryTime",
                table: "OrganizationUsers");
        }
    }
}
