using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chh.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFacilitySubCategory : Migration
    {
        // Matches SeedDemoFacilityContacts' fixed demo IDs — those two rows predate this column
        // and need a real backfilled value, not the "Government" placeholder every other Hospital
        // row gets (see Up()'s defaultValue reasoning below).
        private static readonly Guid DemoNgoFacilityId = new("22222222-2222-2222-2222-222222222221");

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // "Government" is a placeholder backfill for rows created before this column existed
            // (today, only the two SeedDemoFacilityContacts demo rows) — it's a valid
            // FacilitySubCategory member so existing Hospital rows still deserialize correctly.
            // The single existing Ngo row is corrected to a valid Ngo sub-category right after.
            migrationBuilder.AddColumn<string>(
                name: "SubCategory",
                table: "Facility",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Government");

            migrationBuilder.UpdateData(
                table: "Facility",
                keyColumn: "Id",
                keyValue: DemoNgoFacilityId,
                column: "SubCategory",
                value: "RegisteredSociety");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SubCategory",
                table: "Facility");
        }
    }
}
