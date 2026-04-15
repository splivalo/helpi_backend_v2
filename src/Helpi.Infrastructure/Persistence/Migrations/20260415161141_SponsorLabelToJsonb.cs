using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Helpi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SponsorLabelToJsonb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Convert existing plain-text Label to JSON, then change column type
            migrationBuilder.Sql(
                """
                UPDATE "Sponsors"
                SET "Label" = jsonb_build_object('hr', "Label")::text
                WHERE "Label" IS NOT NULL AND "Label" NOT LIKE '{%';

                ALTER TABLE "Sponsors"
                ALTER COLUMN "Label" TYPE jsonb USING "Label"::jsonb;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Label",
                table: "Sponsors",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(Dictionary<string, string>),
                oldType: "jsonb");
        }
    }
}
