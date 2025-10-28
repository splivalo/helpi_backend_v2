using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Helpi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class kloInitialMigrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobInstances_ScheduleAssignments_ScheduleAssignmentId",
                table: "JobInstances");

            migrationBuilder.AlterColumn<int>(
                name: "ScheduleAssignmentId",
                table: "JobInstances",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_JobInstances_ScheduleAssignments_ScheduleAssignmentId",
                table: "JobInstances",
                column: "ScheduleAssignmentId",
                principalTable: "ScheduleAssignments",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobInstances_ScheduleAssignments_ScheduleAssignmentId",
                table: "JobInstances");

            migrationBuilder.AlterColumn<int>(
                name: "ScheduleAssignmentId",
                table: "JobInstances",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_JobInstances_ScheduleAssignments_ScheduleAssignmentId",
                table: "JobInstances",
                column: "ScheduleAssignmentId",
                principalTable: "ScheduleAssignments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
