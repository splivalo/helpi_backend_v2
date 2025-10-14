using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Helpi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class llInitialMigrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvoiceEmails_Invoices_InvoiceId",
                table: "InvoiceEmails");

            migrationBuilder.DropIndex(
                name: "IX_InvoiceEmails_InvoiceId",
                table: "InvoiceEmails");

            migrationBuilder.RenameColumn(
                name: "InvoiceId",
                table: "InvoiceEmails",
                newName: "ExternalInvoiceId");

            migrationBuilder.AddColumn<int>(
                name: "EmailId",
                table: "Invoices",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_EmailId",
                table: "Invoices",
                column: "EmailId");

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_InvoiceEmails_EmailId",
                table: "Invoices",
                column: "EmailId",
                principalTable: "InvoiceEmails",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_InvoiceEmails_EmailId",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_EmailId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "EmailId",
                table: "Invoices");

            migrationBuilder.RenameColumn(
                name: "ExternalInvoiceId",
                table: "InvoiceEmails",
                newName: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceEmails_InvoiceId",
                table: "InvoiceEmails",
                column: "InvoiceId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_InvoiceEmails_Invoices_InvoiceId",
                table: "InvoiceEmails",
                column: "InvoiceId",
                principalTable: "Invoices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
