using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Helpi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NeedsSubstitute : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SubstitutionStatus",
                table: "JobInstances");

            migrationBuilder.AddColumn<bool>(
                name: "NeedsSubstitute",
                table: "JobInstances",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NeedsSubstitute",
                table: "JobInstances");

            migrationBuilder.AddColumn<int>(
                name: "SubstitutionStatus",
                table: "JobInstances",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
