using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Howsee.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTourShareToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ShareToken",
                table: "Tours",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            // Backfill existing rows with unique tokens
            migrationBuilder.Sql("UPDATE \"Tours\" SET \"ShareToken\" = replace(gen_random_uuid()::text, '-', '') WHERE \"ShareToken\" IS NULL");

            migrationBuilder.AlterColumn<string>(
                name: "ShareToken",
                table: "Tours",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tours_ShareToken",
                table: "Tours",
                column: "ShareToken",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tours_ShareToken",
                table: "Tours");

            migrationBuilder.DropColumn(
                name: "ShareToken",
                table: "Tours");
        }
    }
}
