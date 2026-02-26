using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Howsee.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameQiPaymentIdToWaylPaymentId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "QiPaymentId",
                table: "Invoices",
                newName: "WaylPaymentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "WaylPaymentId",
                table: "Invoices",
                newName: "QiPaymentId");
        }
    }
}
