using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chh.Infrastructure.Migrations
{
    /// <summary>
    /// Adds the <c>FacilityContact.Mobile</c> index backing CHH-10's OTP-verify role lookup, and
    /// seeds one demo Hospital and one demo NGO facility (with one contact each) so the new
    /// Hospital/Ngo redirect can be exercised end-to-end — CHH-73/74/78 have not yet wired a real
    /// facility self-registration endpoint to create one. DEMO DATA ONLY: fixed GUIDs and mobile
    /// numbers (9000000001 / 9000000002), documented here and in the CHH-10 PR description, same
    /// pattern as the temporary <c>OtpConstants.MasterOtpCode</c> bypass. Remove once CHH-F03/CHH-78
    /// provide real facility registration.
    /// </summary>
    public partial class SeedDemoFacilityContacts : Migration
    {
        private static readonly Guid DemoHospitalFacilityId = new("11111111-1111-1111-1111-111111111111");
        private static readonly Guid DemoHospitalContactId = new("11111111-1111-1111-1111-111111111112");
        private static readonly Guid DemoNgoFacilityId = new("22222222-2222-2222-2222-222222222221");
        private static readonly Guid DemoNgoContactId = new("22222222-2222-2222-2222-222222222222");

        private static readonly DateTimeOffset SeedTimestamp = new(2026, 9, 7, 0, 0, 0, TimeSpan.Zero);

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_FacilityContact_Mobile",
                table: "FacilityContact",
                column: "Mobile");

            migrationBuilder.InsertData(
                table: "Facility",
                columns: new[]
                {
                    "Id", "FacilityName", "Category", "LicenseNumber", "Address",
                    "VerificationStatus", "LicenseDocumentUrl", "CreatedAtUtc", "UpdatedAtUtc"
                },
                values: new object[,]
                {
                    {
                        DemoHospitalFacilityId, "Demo City Hospital (seed data)", "Hospital", "DEMO-LIC-HOSP-001",
                        "123 Demo Street, Demo City", "Verified", null, SeedTimestamp, SeedTimestamp
                    },
                    {
                        DemoNgoFacilityId, "Demo Community NGO (seed data)", "Ngo", "DEMO-LIC-NGO-001",
                        "456 Demo Avenue, Demo City", "Verified", null, SeedTimestamp, SeedTimestamp
                    }
                });

            migrationBuilder.InsertData(
                table: "FacilityContact",
                columns: new[] { "Id", "FacilityId", "Name", "Designation", "Mobile" },
                values: new object[,]
                {
                    { DemoHospitalContactId, DemoHospitalFacilityId, "Demo Hospital Contact", "Administrator", "9000000001" },
                    { DemoNgoContactId, DemoNgoFacilityId, "Demo NGO Contact", "Coordinator", "9000000002" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(table: "FacilityContact", keyColumn: "Id", keyValue: DemoHospitalContactId);
            migrationBuilder.DeleteData(table: "FacilityContact", keyColumn: "Id", keyValue: DemoNgoContactId);
            migrationBuilder.DeleteData(table: "Facility", keyColumn: "Id", keyValue: DemoHospitalFacilityId);
            migrationBuilder.DeleteData(table: "Facility", keyColumn: "Id", keyValue: DemoNgoFacilityId);

            migrationBuilder.DropIndex(
                name: "IX_FacilityContact_Mobile",
                table: "FacilityContact");
        }
    }
}
