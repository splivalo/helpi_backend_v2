using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Helpi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveMaxAssignmentsAndMaxUsesPerSenior : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxAssignments",
                table: "Coupons");

            migrationBuilder.DropColumn(
                name: "MaxUsesPerSenior",
                table: "Coupons");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxAssignments",
                table: "Coupons",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxUsesPerSenior",
                table: "Coupons",
                type: "integer",
                nullable: true);
        }
    }
}
