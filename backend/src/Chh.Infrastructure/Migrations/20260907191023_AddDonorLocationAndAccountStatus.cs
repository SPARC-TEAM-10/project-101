using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chh.Infrastructure.Migrations
{
    /// <summary>
    /// Adds registered <c>Latitude</c>/<c>Longitude</c> and <c>AccountStatus</c> columns to
    /// <c>IndividualProfile</c> (US-CHH-004-02/CHH-80) — required for proximity donor matching.
    /// </summary>
    public partial class AddDonorLocationAndAccountStatus : Migration
    {
        /// <summary>Adds the columns, defaulting <c>AccountStatus</c> to <c>Active</c> for existing rows.</summary>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccountStatus",
                table: "IndividualProfile",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Active");

            migrationBuilder.AddColumn<decimal>(
                name: "Latitude",
                table: "IndividualProfile",
                type: "numeric(9,6)",
                precision: 9,
                scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Longitude",
                table: "IndividualProfile",
                type: "numeric(9,6)",
                precision: 9,
                scale: 6,
                nullable: true);
        }

        /// <summary>Drops the three columns.</summary>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountStatus",
                table: "IndividualProfile");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "IndividualProfile");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "IndividualProfile");
        }
    }
}
