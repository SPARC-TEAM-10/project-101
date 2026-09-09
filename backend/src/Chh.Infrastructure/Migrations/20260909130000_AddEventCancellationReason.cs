using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chh.Infrastructure.Migrations
{
    /// <summary>Adds Event.CancellationReason (CHH-41/US-CHH-005-04).</summary>
    public partial class AddEventCancellationReason : Migration
    {
        /// <summary>Adds the CancellationReason column.</summary>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CancellationReason",
                table: "Event",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);
        }

        /// <summary>Drops the CancellationReason column.</summary>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancellationReason",
                table: "Event");
        }
    }
}
