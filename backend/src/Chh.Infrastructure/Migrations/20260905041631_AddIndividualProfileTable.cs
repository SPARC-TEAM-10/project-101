using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chh.Infrastructure.Migrations
{
    /// <summary>Creates the <c>IndividualProfile</c> table (CHH-F02).</summary>
    public partial class AddIndividualProfileTable : Migration
    {
        /// <summary>Creates the <c>IndividualProfile</c> table and its unique <c>MobileNumber</c> index.</summary>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IndividualProfile",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    MobileNumber = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    FullName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    BloodGroup = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DateOfBirth = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Gender = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LocationCityArea = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IsChronicIllness = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    HasRecentSurgery = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    IsInfectiousDisease = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    IsUnderweight = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    IsOtherIllness = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    OtherIllnessDetails = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    IsReceiverOnly = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndividualProfile", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IndividualProfile_MobileNumber",
                table: "IndividualProfile",
                column: "MobileNumber",
                unique: true);
        }

        /// <summary>Drops the <c>IndividualProfile</c> table.</summary>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IndividualProfile");
        }
    }
}
