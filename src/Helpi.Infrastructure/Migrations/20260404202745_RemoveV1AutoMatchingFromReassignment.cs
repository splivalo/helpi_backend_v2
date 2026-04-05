using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Helpi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveV1AutoMatchingFromReassignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowAutoScheduling",
                table: "ReassignmentRecords");

            migrationBuilder.DropColumn(
                name: "AttemptCount",
                table: "ReassignmentRecords");

            migrationBuilder.DropColumn(
                name: "LastAttemptAt",
                table: "ReassignmentRecords");

            migrationBuilder.DropColumn(
                name: "MaxAttempts",
                table: "ReassignmentRecords");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AllowAutoScheduling",
                table: "ReassignmentRecords",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "AttemptCount",
                table: "ReassignmentRecords",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastAttemptAt",
                table: "ReassignmentRecords",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxAttempts",
                table: "ReassignmentRecords",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
