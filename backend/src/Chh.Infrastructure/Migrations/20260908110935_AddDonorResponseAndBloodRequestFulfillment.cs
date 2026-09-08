using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chh.Infrastructure.Migrations
{
    /// <summary>Adds donor response tracking and blood-request fulfillment tracking (CHH-35).</summary>
    public partial class AddDonorResponseAndBloodRequestFulfillment : Migration
    {
        /// <summary>Adds DonorNotification.ResponseStatus/RespondedAtUtc and BloodRequest.UnitsAccepted.</summary>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "RespondedAtUtc",
                table: "DonorNotification",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResponseStatus",
                table: "DonorNotification",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Pending");

            migrationBuilder.AddColumn<int>(
                name: "UnitsAccepted",
                table: "BloodRequest",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <summary>Drops the columns added by <see cref="Up"/>.</summary>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RespondedAtUtc",
                table: "DonorNotification");

            migrationBuilder.DropColumn(
                name: "ResponseStatus",
                table: "DonorNotification");

            migrationBuilder.DropColumn(
                name: "UnitsAccepted",
                table: "BloodRequest");
        }
    }
}
