using Chh.Domain.Enums;

namespace Chh.Domain.Entities;

/// <summary>
/// A registered hospital/blood-bank or NGO facility (CHH-F03/CHH-F07). Introduced by CHH-73 for
/// the Admin Command Center's pending-list query; CHH-78 adds the creation path. Construction
/// logic lives in <see cref="Chh.Application.Factories.FacilityFactory"/> — upgraded from CHH-73's
/// plain-setter design to the factory + internal-set pattern established by
/// <see cref="BloodRequest"/>, per that entity's own doc comment inviting this once a creation
/// flow existed.
/// </summary>
public class Facility
{
    /// <summary>Surrogate primary key.</summary>
    public Guid Id { get; internal set; } = Guid.NewGuid();

    /// <summary>Registered name of the hospital or NGO (CHH-78 AC1 mandatory field).</summary>
    public string FacilityName { get; internal set; } = default!;

    /// <summary>Hospital or NGO (CHH-78 AC1 mandatory field) — determines what the facility can publish.</summary>
    public FacilityCategory Category { get; internal set; }

    /// <summary>Operating license number, as printed on the license (CHH-78 AC1 mandatory field).</summary>
    public string LicenseNumber { get; internal set; } = default!;

    /// <summary>
    /// Fixed address donors are routed to — not geocoded or tracked automatically (CHH-78 AC1
    /// mandatory field), same simplification as <see cref="IndividualProfile.LocationCityArea"/>.
    /// </summary>
    public string Address { get; internal set; } = default!;

    /// <summary>
    /// Verification lifecycle state — <see cref="FacilityVerificationStatus.Pending"/> from creation
    /// (CHH-78 AC1). Restricts high-impact features until a System Admin approves (CHH-F03 LLD §6.1).
    /// </summary>
    public FacilityVerificationStatus VerificationStatus { get; internal set; } = FacilityVerificationStatus.Pending;

    /// <summary>
    /// Blob storage reference for the uploaded license document. Null until CHH-74's upload path
    /// populates it — CHH-78's creation endpoint never sets this, CHH-73's pending-list endpoint
    /// never sets this either.
    /// </summary>
    public string? LicenseDocumentUrl { get; internal set; }

    /// <summary>
    /// Mobile number of the facility admin who created this registration, taken from the JWT "sub"
    /// claim at creation time — never client-supplied (see <c>Chh.Api.Controllers.FacilitiesController</c>).
    /// </summary>
    public string CreatedByMobileNumber { get; internal set; } = default!;

    /// <summary>UTC timestamp the facility record was created — also serves as "Date of Registration" (CHH-73 AC1).</summary>
    public DateTimeOffset CreatedAtUtc { get; internal set; }

    /// <summary>UTC timestamp of the last update (e.g. a CHH-75 approve/reject transition). Set to <see cref="CreatedAtUtc"/> on creation.</summary>
    public DateTimeOffset UpdatedAtUtc { get; internal set; }

    /// <summary>
    /// Admin's reason for rejecting the facility (CHH-28's status dashboard shows this). Null unless
    /// <see cref="VerificationStatus"/> is <see cref="FacilityVerificationStatus.Rejected"/> — no
    /// reject-with-reason endpoint exists yet (CHH-75), so this is currently only ever set by a
    /// manual DB operation, same as <see cref="IndividualProfile.IsAdmin"/> until its own ticket lands.
    /// </summary>
    public string? RejectionReason { get; internal set; }

    /// <summary>Contacts for this facility — 1 to 3, the first (lowest <see cref="FacilityContact.SortOrder"/>) is primary (CHH-78 AC2).</summary>
    public ICollection<FacilityContact> Contacts { get; internal set; } = new List<FacilityContact>();
}
