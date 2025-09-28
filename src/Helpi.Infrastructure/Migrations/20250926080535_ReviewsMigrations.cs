using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Helpi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReviewsMigrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "Rating",
                table: "Reviews",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "smallint");

            migrationBuilder.AddColumn<bool>(
                name: "IsPending",
                table: "Reviews",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MaxRetry",
                table: "Reviews",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextRetryAt",
                table: "Reviews",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "RetryCount",
                table: "Reviews",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPending",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "MaxRetry",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "NextRetryAt",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "RetryCount",
                table: "Reviews");

            migrationBuilder.AlterColumn<byte>(
                name: "Rating",
                table: "Reviews",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision");
        }
    }
}
