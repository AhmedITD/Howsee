using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Howsee.Infrastructure.Migrations
{
    /// <inheritdoc />
    [Migration("20260214110000_EnsurePricingPlansRoleColumn")]
    public partial class EnsurePricingPlansRoleColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DO $$
BEGIN
  IF NOT EXISTS (
    SELECT 1 FROM information_schema.columns
    WHERE table_schema = 'public' AND table_name = 'PricingPlans' AND column_name = 'Role'
  ) THEN
    ALTER TABLE ""PricingPlans"" ADD COLUMN ""Role"" integer NULL;
  END IF;
END $$;
");
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
