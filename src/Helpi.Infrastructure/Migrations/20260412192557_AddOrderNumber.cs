using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Helpi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrderNumber",
                table: "Orders",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Populate existing orders with per-senior sequential numbers
            migrationBuilder.Sql(@"
                UPDATE ""Orders"" SET ""OrderNumber"" = sub.rn
                FROM (
                    SELECT ""Id"", ROW_NUMBER() OVER (
                        PARTITION BY ""SeniorId"" ORDER BY ""Id""
                    ) AS rn
                    FROM ""Orders""
                ) sub
                WHERE ""Orders"".""Id"" = sub.""Id"";
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrderNumber",
                table: "Orders");
        }
    }
}
