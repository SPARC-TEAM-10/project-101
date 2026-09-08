using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chh.Infrastructure.Migrations
{
    /// <summary>Adds the <c>RequesterName</c> column to <c>BloodRequest</c> (the requester's own name, distinct from the patient's).</summary>
    public partial class AddRequesterNameToBloodRequest : Migration
    {
        /// <summary>Adds the column, backfilling existing rows with an empty string.</summary>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RequesterName",
                table: "BloodRequest",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <summary>Drops the <c>RequesterName</c> column.</summary>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequesterName",
                table: "BloodRequest");
        }
    }
}
