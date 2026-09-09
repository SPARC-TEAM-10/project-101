using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chh.Infrastructure.Migrations
{
    /// <summary>Adds EventRsvp.AttendedAtUtc and EventRsvp.AttendedByName (CHH-44/US-CHH-005-07).</summary>
    public partial class AddEventRsvpAttendance : Migration
    {
        /// <summary>Adds the AttendedAtUtc and AttendedByName columns.</summary>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "AttendedAtUtc",
                table: "EventRsvp",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AttendedByName",
                table: "EventRsvp",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <summary>Drops the AttendedAtUtc and AttendedByName columns.</summary>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttendedAtUtc",
                table: "EventRsvp");

            migrationBuilder.DropColumn(
                name: "AttendedByName",
                table: "EventRsvp");
        }
    }
}
