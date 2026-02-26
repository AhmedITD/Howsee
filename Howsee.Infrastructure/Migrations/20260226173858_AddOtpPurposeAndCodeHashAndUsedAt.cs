using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Howsee.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOtpPurposeAndCodeHashAndUsedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "PhoneVerificationCodes",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "CodeHash",
                table: "PhoneVerificationCodes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Purpose",
                table: "PhoneVerificationCodes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UsedAt",
                table: "PhoneVerificationCodes",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodeHash",
                table: "PhoneVerificationCodes");

            migrationBuilder.DropColumn(
                name: "Purpose",
                table: "PhoneVerificationCodes");

            migrationBuilder.DropColumn(
                name: "UsedAt",
                table: "PhoneVerificationCodes");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "PhoneVerificationCodes",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
