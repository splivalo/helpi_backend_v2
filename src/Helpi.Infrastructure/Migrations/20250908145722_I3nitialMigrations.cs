using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Helpi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class I3nitialMigrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "allowAutoScheduling",
                table: "OrderSchedules",
                newName: "AllowAutoScheduling");

            migrationBuilder.AddColumn<bool>(
                name: "AllowAutoScheduling",
                table: "ReassignmentRecords",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowAutoScheduling",
                table: "ReassignmentRecords");

            migrationBuilder.RenameColumn(
                name: "AllowAutoScheduling",
                table: "OrderSchedules",
                newName: "allowAutoScheduling");
        }
    }
}
