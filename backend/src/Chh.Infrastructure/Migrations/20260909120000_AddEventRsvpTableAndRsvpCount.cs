using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chh.Infrastructure.Migrations
{
    /// <summary>Adds Event.RsvpCount and the EventRsvp table (CHH-40/US-CHH-005-03).</summary>
    public partial class AddEventRsvpTableAndRsvpCount : Migration
    {
        /// <summary>Adds the RsvpCount column and creates the EventRsvp table.</summary>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RsvpCount",
                table: "Event",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "EventRsvp",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    IndividualProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferenceCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CancelledAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventRsvp", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventRsvp_EventId_IndividualProfileId",
                table: "EventRsvp",
                columns: new[] { "EventId", "IndividualProfileId" },
                unique: true);
        }

        /// <summary>Drops the EventRsvp table and the RsvpCount column.</summary>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventRsvp");

            migrationBuilder.DropColumn(
                name: "RsvpCount",
                table: "Event");
        }
    }
}
