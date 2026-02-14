using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Howsee.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addpricetoprop : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Properties",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Price",
                table: "Properties");
        }
    }
}
