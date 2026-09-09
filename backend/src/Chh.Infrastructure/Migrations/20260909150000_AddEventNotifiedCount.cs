using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chh.Infrastructure.Migrations
{
    /// <summary>Adds Event.NotifiedCount (CHH-42/CHH-45, US-CHH-005-05/US-CHH-005-08).</summary>
    public partial class AddEventNotifiedCount : Migration
    {
        /// <summary>Adds the NotifiedCount column.</summary>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NotifiedCount",
                table: "Event",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <summary>Drops the NotifiedCount column.</summary>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NotifiedCount",
                table: "Event");
        }
    }
}
