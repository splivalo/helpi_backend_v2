using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Helpi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class _2InitialMigrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StudentContractId",
                table: "JobInstances",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobInstances_StudentContractId",
                table: "JobInstances",
                column: "StudentContractId");

            migrationBuilder.AddForeignKey(
                name: "FK_JobInstances_StudentContracts_StudentContractId",
                table: "JobInstances",
                column: "StudentContractId",
                principalTable: "StudentContracts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobInstances_StudentContracts_StudentContractId",
                table: "JobInstances");

            migrationBuilder.DropIndex(
                name: "IX_JobInstances_StudentContractId",
                table: "JobInstances");

            migrationBuilder.DropColumn(
                name: "StudentContractId",
                table: "JobInstances");
        }
    }
}
