using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Helpi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSystemSettingsToPricingConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PaymentTimingMinutes",
                table: "PricingConfigurations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SeniorCancelCutoffHours",
                table: "PricingConfigurations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StudentCancelCutoffHours",
                table: "PricingConfigurations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TravelBufferMinutes",
                table: "PricingConfigurations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "VatPercentage",
                table: "PricingConfigurations",
                type: "numeric(5,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentTimingMinutes",
                table: "PricingConfigurations");

            migrationBuilder.DropColumn(
                name: "SeniorCancelCutoffHours",
                table: "PricingConfigurations");

            migrationBuilder.DropColumn(
                name: "StudentCancelCutoffHours",
                table: "PricingConfigurations");

            migrationBuilder.DropColumn(
                name: "TravelBufferMinutes",
                table: "PricingConfigurations");

            migrationBuilder.DropColumn(
                name: "VatPercentage",
                table: "PricingConfigurations");
        }
    }
}
