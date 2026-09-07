using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chh.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFacilityCreationColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedByMobileNumber",
                table: "Facility",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "FacilityContact",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Facility_CreatedByMobileNumber",
                table: "Facility",
                column: "CreatedByMobileNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Facility_CreatedByMobileNumber",
                table: "Facility");

            migrationBuilder.DropColumn(
                name: "CreatedByMobileNumber",
                table: "Facility");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "FacilityContact");
        }
    }
}
