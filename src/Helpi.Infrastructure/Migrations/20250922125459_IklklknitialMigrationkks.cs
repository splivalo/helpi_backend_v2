using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Helpi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class IklklknitialMigrationkks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "JobInstanceId",
                table: "JobRequests",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobRequests_JobInstanceId",
                table: "JobRequests",
                column: "JobInstanceId");

            migrationBuilder.AddForeignKey(
                name: "FK_JobRequests_JobInstances_JobInstanceId",
                table: "JobRequests",
                column: "JobInstanceId",
                principalTable: "JobInstances",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobRequests_JobInstances_JobInstanceId",
                table: "JobRequests");

            migrationBuilder.DropIndex(
                name: "IX_JobRequests_JobInstanceId",
                table: "JobRequests");

            migrationBuilder.DropColumn(
                name: "JobInstanceId",
                table: "JobRequests");
        }
    }
}
