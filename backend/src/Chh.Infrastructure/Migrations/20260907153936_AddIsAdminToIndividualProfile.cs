using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chh.Infrastructure.Migrations
{
    /// <summary>Adds the <c>IsAdmin</c> column to <c>IndividualProfile</c> (CHH-F07).</summary>
    public partial class AddIsAdminToIndividualProfile : Migration
    {
        /// <summary>Adds the <c>IsAdmin</c> column, defaulting existing rows to <c>false</c>.</summary>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAdmin",
                table: "IndividualProfile",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <summary>Drops the <c>IsAdmin</c> column.</summary>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAdmin",
                table: "IndividualProfile");
        }
    }
}
