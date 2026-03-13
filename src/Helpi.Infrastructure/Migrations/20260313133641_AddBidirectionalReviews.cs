using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Helpi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBidirectionalReviews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reviews_JobInstanceId",
                table: "Reviews");

            migrationBuilder.AddColumn<decimal>(
                name: "AverageRating",
                table: "Seniors",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalRatingSum",
                table: "Seniors",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "TotalReviews",
                table: "Seniors",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Reviews",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_JobInstanceId",
                table: "Reviews",
                column: "JobInstanceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reviews_JobInstanceId",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "AverageRating",
                table: "Seniors");

            migrationBuilder.DropColumn(
                name: "TotalRatingSum",
                table: "Seniors");

            migrationBuilder.DropColumn(
                name: "TotalReviews",
                table: "Seniors");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Reviews");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_JobInstanceId",
                table: "Reviews",
                column: "JobInstanceId",
                unique: true);
        }
    }
}
