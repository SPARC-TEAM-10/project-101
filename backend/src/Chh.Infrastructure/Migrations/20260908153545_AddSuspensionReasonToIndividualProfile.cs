using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chh.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSuspensionReasonToIndividualProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SuspensionReason",
                table: "IndividualProfile",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SuspensionReason",
                table: "IndividualProfile");
        }
    }
}
