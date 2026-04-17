using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Helpi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCouponSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Coupons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Value = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    IsCombainable = table.Column<bool>(type: "boolean", nullable: false),
                    CityId = table.Column<int>(type: "integer", nullable: true),
                    ValidFrom = table.Column<DateTime>(type: "date", nullable: false),
                    ValidUntil = table.Column<DateTime>(type: "date", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    MaxAssignments = table.Column<int>(type: "integer", nullable: true),
                    MaxUsesPerSenior = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Coupons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Coupons_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "CouponAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CouponId = table.Column<int>(type: "integer", nullable: false),
                    SeniorId = table.Column<int>(type: "integer", nullable: false),
                    AssignedByAdminId = table.Column<int>(type: "integer", nullable: true),
                    RemainingValue = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CouponAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CouponAssignments_Admins_AssignedByAdminId",
                        column: x => x.AssignedByAdminId,
                        principalTable: "Admins",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CouponAssignments_Coupons_CouponId",
                        column: x => x.CouponId,
                        principalTable: "Coupons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CouponAssignments_Seniors_SeniorId",
                        column: x => x.SeniorId,
                        principalTable: "Seniors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CouponUsages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CouponAssignmentId = table.Column<int>(type: "integer", nullable: false),
                    JobInstanceId = table.Column<int>(type: "integer", nullable: false),
                    CoveredAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CoveredHours = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    UsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CouponUsages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CouponUsages_CouponAssignments_CouponAssignmentId",
                        column: x => x.CouponAssignmentId,
                        principalTable: "CouponAssignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CouponUsages_JobInstances_JobInstanceId",
                        column: x => x.JobInstanceId,
                        principalTable: "JobInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CouponAssignments_AssignedByAdminId",
                table: "CouponAssignments",
                column: "AssignedByAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_CouponAssignments_CouponId",
                table: "CouponAssignments",
                column: "CouponId");

            migrationBuilder.CreateIndex(
                name: "IX_CouponAssignments_SeniorId",
                table: "CouponAssignments",
                column: "SeniorId");

            migrationBuilder.CreateIndex(
                name: "IX_Coupons_CityId",
                table: "Coupons",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Coupons_Code",
                table: "Coupons",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CouponUsages_CouponAssignmentId",
                table: "CouponUsages",
                column: "CouponAssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_CouponUsages_JobInstanceId",
                table: "CouponUsages",
                column: "JobInstanceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CouponUsages");

            migrationBuilder.DropTable(
                name: "CouponAssignments");

            migrationBuilder.DropTable(
                name: "Coupons");
        }
    }
}
