using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Helpi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class kkInitialMigrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SeniorId",
                table: "HNotifications",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StudentId",
                table: "HNotifications",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_HNotifications_SeniorId",
                table: "HNotifications",
                column: "SeniorId");

            migrationBuilder.CreateIndex(
                name: "IX_HNotifications_StudentId",
                table: "HNotifications",
                column: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_HNotifications_Seniors_SeniorId",
                table: "HNotifications",
                column: "SeniorId",
                principalTable: "Seniors",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_HNotifications_Students_StudentId",
                table: "HNotifications",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HNotifications_Seniors_SeniorId",
                table: "HNotifications");

            migrationBuilder.DropForeignKey(
                name: "FK_HNotifications_Students_StudentId",
                table: "HNotifications");

            migrationBuilder.DropIndex(
                name: "IX_HNotifications_SeniorId",
                table: "HNotifications");

            migrationBuilder.DropIndex(
                name: "IX_HNotifications_StudentId",
                table: "HNotifications");

            migrationBuilder.DropColumn(
                name: "SeniorId",
                table: "HNotifications");

            migrationBuilder.DropColumn(
                name: "StudentId",
                table: "HNotifications");
        }
    }
}
