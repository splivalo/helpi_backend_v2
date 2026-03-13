using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Helpi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSundayHourlyRate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "SundayHourlyRate",
                table: "PricingConfigurations",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            // Set existing config to 21 EUR Sunday rate (1.5x of 14 EUR regular)
            migrationBuilder.Sql(
                "UPDATE \"PricingConfigurations\" SET \"SundayHourlyRate\" = 21 WHERE \"SundayHourlyRate\" = 0");

            migrationBuilder.AddColumn<decimal>(
                name: "NewSundayHourlyRate",
                table: "PricingChangeHistories",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "OldSundayHourlyRate",
                table: "PricingChangeHistories",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SundayHourlyRate",
                table: "PricingConfigurations");

            migrationBuilder.DropColumn(
                name: "NewSundayHourlyRate",
                table: "PricingChangeHistories");

            migrationBuilder.DropColumn(
                name: "OldSundayHourlyRate",
                table: "PricingChangeHistories");
        }
    }
}
