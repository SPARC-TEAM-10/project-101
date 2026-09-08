using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chh.Infrastructure.Migrations
{
    /// <summary>Adds the DonorNotification table and IndividualProfile.LastActiveAtUtc (CHH-34).</summary>
    public partial class AddDonorNotificationAndPresenceTracking : Migration
    {
        /// <summary>Creates the DonorNotification table and adds the LastActiveAtUtc column.</summary>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastActiveAtUtc",
                table: "IndividualProfile",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DonorNotification",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    BloodRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    DonorProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    BloodGroup = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UnitsRequired = table.Column<int>(type: "integer", nullable: false),
                    Urgency = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DistanceKm = table.Column<decimal>(type: "numeric(9,3)", precision: 9, scale: 3, nullable: false),
                    AreaLabel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    SmsSent = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DonorNotification", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DonorNotification_BloodRequestId_DonorProfileId",
                table: "DonorNotification",
                columns: new[] { "BloodRequestId", "DonorProfileId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DonorNotification_DonorProfileId_CreatedAtUtc",
                table: "DonorNotification",
                columns: new[] { "DonorProfileId", "CreatedAtUtc" });
        }

        /// <summary>Drops the DonorNotification table and the LastActiveAtUtc column.</summary>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DonorNotification");

            migrationBuilder.DropColumn(
                name: "LastActiveAtUtc",
                table: "IndividualProfile");
        }
    }
}
