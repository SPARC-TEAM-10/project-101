using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chh.Infrastructure.Migrations
{
    /// <summary>Adds the Event table (CHH-38/US-CHH-005-01).</summary>
    public partial class AddEventTable : Migration
    {
        /// <summary>Creates the Event table.</summary>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Event",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    FacilityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EventType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    VenueName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    VenueAddress = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Latitude = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: false),
                    Longitude = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: false),
                    StartAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Capacity = table.Column<int>(type: "integer", nullable: false),
                    CoordinatorName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CoordinatorContact = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    RsvpCutoffAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Event", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Event_FacilityId",
                table: "Event",
                column: "FacilityId");

            migrationBuilder.CreateIndex(
                name: "IX_Event_Status_StartAtUtc",
                table: "Event",
                columns: new[] { "Status", "StartAtUtc" });
        }

        /// <summary>Drops the Event table.</summary>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Event");
        }
    }
}
