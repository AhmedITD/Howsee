using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Howsee.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCurrenciesAndCurrencyId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Currencies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Symbol = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Currencies", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Currencies_Code",
                table: "Currencies",
                column: "Code",
                unique: true);

            migrationBuilder.Sql(@"
INSERT INTO ""Currencies"" (""Code"", ""Name"", ""Symbol"", ""IsActive"")
VALUES ('IQD', 'Iraqi Dinar', 'ع.د', true);
");

            migrationBuilder.AddColumn<int>(
                name: "CurrencyId",
                table: "PricingPlans",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CurrencyId",
                table: "Invoices",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql(@"
UPDATE ""PricingPlans"" SET ""CurrencyId"" = (SELECT ""Id"" FROM ""Currencies"" WHERE ""Code"" = 'IQD' LIMIT 1);
UPDATE ""Invoices"" SET ""CurrencyId"" = (SELECT ""Id"" FROM ""Currencies"" WHERE ""Code"" = 'IQD' LIMIT 1);
");

            migrationBuilder.AlterColumn<int>(
                name: "CurrencyId",
                table: "PricingPlans",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CurrencyId",
                table: "Invoices",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "PricingPlans");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "Invoices");

            migrationBuilder.CreateIndex(
                name: "IX_PricingPlans_CurrencyId",
                table: "PricingPlans",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_CurrencyId",
                table: "Invoices",
                column: "CurrencyId",
                filter: "\"IsDeleted\" = false");

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_Currencies_CurrencyId",
                table: "Invoices",
                column: "CurrencyId",
                principalTable: "Currencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PricingPlans_Currencies_CurrencyId",
                table: "PricingPlans",
                column: "CurrencyId",
                principalTable: "Currencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Currencies_CurrencyId",
                table: "Invoices");

            migrationBuilder.DropForeignKey(
                name: "FK_PricingPlans_Currencies_CurrencyId",
                table: "PricingPlans");

            migrationBuilder.DropTable(
                name: "Currencies");

            migrationBuilder.DropIndex(
                name: "IX_PricingPlans_CurrencyId",
                table: "PricingPlans");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_CurrencyId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "CurrencyId",
                table: "PricingPlans");

            migrationBuilder.DropColumn(
                name: "CurrencyId",
                table: "Invoices");

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "PricingPlans",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "Invoices",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");
        }
    }
}
