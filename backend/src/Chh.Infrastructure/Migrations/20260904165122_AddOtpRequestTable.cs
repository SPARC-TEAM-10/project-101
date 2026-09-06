using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chh.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOtpRequestTable : Migration
    {
        /// <summary>Creates the <c>OtpRequest</c> table and its <c>MobileNumber</c> index.</summary>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OtpRequest",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    MobileNumber = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    OtpCodeHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    OtpRequestedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    OtpExpiresAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ResendAvailableAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OtpRequest", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OtpRequest_MobileNumber",
                table: "OtpRequest",
                column: "MobileNumber");
        }

        /// <summary>Drops the <c>OtpRequest</c> table.</summary>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OtpRequest");
        }
    }
}
