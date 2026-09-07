using Chh.Domain.Enums;

namespace Chh.Domain.Entities;

/// <summary>
/// A Hospital/NGO facility registration (CHH-78/US-CHH-003-01, part of Epic CHH-77 — CHH-F03
/// Facility Verification). A plain property bag — construction logic lives in
/// <see cref="Chh.Application.Factories.FacilityFactory"/>, matching the pattern established for
/// <see cref="BloodRequest"/>.
/// </summary>
public class Facility
{
    /// <summary>Surrogate primary key.</summary>
    public Guid Id { get; internal set; } = Guid.NewGuid();

    /// <summary>Registered name of the hospital or NGO (AC1 mandatory field).</summary>
    public string FacilityName { get; internal set; } = default!;

    /// <summary>Hospital or NGO (AC1 mandatory field) — determines what the facility can publish.</summary>
    public FacilityCategory Category { get; internal set; }

    /// <summary>Operating license number, as printed on the license (AC1 mandatory field).</summary>
    public string LicenseNumber { get; internal set; } = default!;

    /// <summary>
    /// Fixed address donors are routed to — not geocoded or tracked automatically (AC1 mandatory
    /// field), same simplification as <see cref="IndividualProfile.LocationCityArea"/>.
    /// </summary>
    public string Address { get; internal set; } = default!;

    /// <summary>
    /// Verification lifecycle state — <see cref="FacilityVerificationStatus.Pending"/> from creation
    /// (AC1). Restricts high-impact features until a System Admin approves (CHH-F03 LLD §6.1).
    /// </summary>
    public FacilityVerificationStatus VerificationStatus { get; internal set; }

    /// <summary>
    /// Mobile number of the facility admin who created this registration, taken from the JWT "sub"
    /// claim at creation time — never client-supplied (see <c>Chh.Api.Controllers.FacilitiesController</c>).
    /// </summary>
    public string CreatedByMobileNumber { get; internal set; } = default!;

    /// <summary>UTC timestamp the facility was registered.</summary>
    public DateTimeOffset CreatedAtUtc { get; internal set; }

    /// <summary>Contacts for this facility — 1 to 3, the first (lowest <see cref="FacilityContact.SortOrder"/>) is primary (AC2).</summary>
    public ICollection<FacilityContact> Contacts { get; internal set; } = new List<FacilityContact>();
}
