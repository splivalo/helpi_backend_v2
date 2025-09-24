using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Helpi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class _2InitialMigrations4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedOn",
                table: "StudentContracts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobInstances_ContractId",
                table: "JobInstances",
                column: "ContractId");

            migrationBuilder.AddForeignKey(
                name: "FK_JobInstances_StudentContracts_ContractId",
                table: "JobInstances",
                column: "ContractId",
                principalTable: "StudentContracts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobInstances_StudentContracts_ContractId",
                table: "JobInstances");

            migrationBuilder.DropIndex(
                name: "IX_JobInstances_ContractId",
                table: "JobInstances");

            migrationBuilder.DropColumn(
                name: "DeletedOn",
                table: "StudentContracts");

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
    }
}
