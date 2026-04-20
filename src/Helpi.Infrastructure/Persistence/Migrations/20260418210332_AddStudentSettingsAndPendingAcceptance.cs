using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Helpi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentSettingsAndPendingAcceptance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AvailabilityChangeCutoffHours",
                table: "PricingConfigurations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "AvailabilityChangeEnabled",
                table: "PricingConfigurations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "StudentCancelEnabled",
                table: "PricingConfigurations",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvailabilityChangeCutoffHours",
                table: "PricingConfigurations");

            migrationBuilder.DropColumn(
                name: "AvailabilityChangeEnabled",
                table: "PricingConfigurations");

            migrationBuilder.DropColumn(
                name: "StudentCancelEnabled",
                table: "PricingConfigurations");
        }
    }
}
