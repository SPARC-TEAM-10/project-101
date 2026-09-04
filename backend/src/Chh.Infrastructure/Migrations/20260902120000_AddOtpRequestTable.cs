using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chh.Infrastructure.Migrations
{
    /// <summary>Creates the <c>OtpRequest</c> table (CHH-F01, CHH-8).</summary>
    public partial class AddOtpRequestTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OtpRequest",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    MobileNumber = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false),
                    OtpCodeHash = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    OtpRequestedAtUtc = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    OtpExpiresAtUtc = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    ResendAvailableAtUtc = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OtpRequest");
        }
    }
}
