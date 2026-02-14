using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Howsee.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleToPricingPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Role",
                table: "PricingPlans",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Role",
                table: "PricingPlans");
        }
    }
}
