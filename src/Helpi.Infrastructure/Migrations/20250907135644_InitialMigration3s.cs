using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Helpi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration3s : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PreferredStudentId",
                table: "ReassignmentRecords",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsRescheduled",
                table: "JobInstances",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "JobInstanceId",
                table: "JobInstances",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OriginalInstanceId",
                table: "JobInstances",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RescheduleReason",
                table: "JobInstances",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RescheduledAt",
                table: "JobInstances",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RescheduledFromId",
                table: "JobInstances",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RescheduledToId",
                table: "JobInstances",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobInstances_JobInstanceId",
                table: "JobInstances",
                column: "JobInstanceId");

            migrationBuilder.AddForeignKey(
                name: "FK_JobInstances_JobInstances_JobInstanceId",
                table: "JobInstances",
                column: "JobInstanceId",
                principalTable: "JobInstances",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobInstances_JobInstances_JobInstanceId",
                table: "JobInstances");

            migrationBuilder.DropIndex(
                name: "IX_JobInstances_JobInstanceId",
                table: "JobInstances");

            migrationBuilder.DropColumn(
                name: "PreferredStudentId",
                table: "ReassignmentRecords");

            migrationBuilder.DropColumn(
                name: "IsRescheduled",
                table: "JobInstances");

            migrationBuilder.DropColumn(
                name: "JobInstanceId",
                table: "JobInstances");

            migrationBuilder.DropColumn(
                name: "OriginalInstanceId",
                table: "JobInstances");

            migrationBuilder.DropColumn(
                name: "RescheduleReason",
                table: "JobInstances");

            migrationBuilder.DropColumn(
                name: "RescheduledAt",
                table: "JobInstances");

            migrationBuilder.DropColumn(
                name: "RescheduledFromId",
                table: "JobInstances");

            migrationBuilder.DropColumn(
                name: "RescheduledToId",
                table: "JobInstances");
        }
    }
}
