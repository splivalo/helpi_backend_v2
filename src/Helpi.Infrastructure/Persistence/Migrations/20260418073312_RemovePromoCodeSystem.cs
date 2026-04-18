using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Helpi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemovePromoCodeSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_PromoCodes_PromoCodeId",
                table: "Orders");

            migrationBuilder.DropTable(
                name: "PromoCodeUsages");

            migrationBuilder.DropTable(
                name: "PromoCodes");

            migrationBuilder.RenameColumn(
                name: "PromoCodeId",
                table: "Orders",
                newName: "CouponId");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_PromoCodeId",
                table: "Orders",
                newName: "IX_Orders_CouponId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Coupons_CouponId",
                table: "Orders",
                column: "CouponId",
                principalTable: "Coupons",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Coupons_CouponId",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "CouponId",
                table: "Orders",
                newName: "PromoCodeId");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_CouponId",
                table: "Orders",
                newName: "IX_Orders_PromoCodeId");

            migrationBuilder.CreateTable(
                name: "PromoCodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CurrentUses = table.Column<int>(type: "integer", nullable: false),
                    DiscountValue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    MaxUses = table.Column<int>(type: "integer", nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    ValidFrom = table.Column<DateTime>(type: "date", nullable: true),
                    ValidUntil = table.Column<DateTime>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromoCodes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PromoCodeUsages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CustomerId = table.Column<int>(type: "integer", nullable: false),
                    OrderId = table.Column<int>(type: "integer", nullable: false),
                    PromoCodeId = table.Column<int>(type: "integer", nullable: false),
                    DiscountApplied = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromoCodeUsages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PromoCodeUsages_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PromoCodeUsages_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PromoCodeUsages_PromoCodes_PromoCodeId",
                        column: x => x.PromoCodeId,
                        principalTable: "PromoCodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PromoCodes_Code",
                table: "PromoCodes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PromoCodeUsages_CustomerId",
                table: "PromoCodeUsages",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_PromoCodeUsages_OrderId",
                table: "PromoCodeUsages",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PromoCodeUsages_PromoCodeId",
                table: "PromoCodeUsages",
                column: "PromoCodeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_PromoCodes_PromoCodeId",
                table: "Orders",
                column: "PromoCodeId",
                principalTable: "PromoCodes",
                principalColumn: "Id");
        }
    }
}
