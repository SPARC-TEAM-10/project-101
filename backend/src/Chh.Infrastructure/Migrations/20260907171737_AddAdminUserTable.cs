using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chh.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminUserTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdminUser",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    MobileNumber = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    IsAdmin = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminUser", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdminUser_MobileNumber",
                table: "AdminUser",
                column: "MobileNumber",
                unique: true);

            // Preserves the CHH-F07 admin identity previously hardcoded as
            // RoleConstants.AdminMobileNumber (removed per PR #18 review feedback) — without this
            // seed row, the number that used to resolve to SystemAdmin would silently stop working.
            migrationBuilder.InsertData(
                table: "AdminUser",
                columns: new[] { "Id", "MobileNumber", "IsAdmin", "CreatedAtUtc" },
                values: new object[] { Guid.NewGuid(), "7907468509", true, DateTimeOffset.UtcNow });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdminUser");
        }
    }
}
