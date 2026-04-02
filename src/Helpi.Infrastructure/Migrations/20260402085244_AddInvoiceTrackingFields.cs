using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Helpi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoiceTrackingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InvoiceCreationStatus",
                table: "PaymentTransactions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<byte>(
                name: "InvoiceRetryCount",
                table: "PaymentTransactions",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<int>(
                name: "MinimaxInvoiceId",
                table: "PaymentTransactions",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InvoiceCreationStatus",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "InvoiceRetryCount",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "MinimaxInvoiceId",
                table: "PaymentTransactions");
        }
    }
}
