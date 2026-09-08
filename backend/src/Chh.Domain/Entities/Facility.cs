using Chh.Domain.Enums;

namespace Chh.Domain.Entities;

/// <summary>
/// A registered hospital/blood-bank or NGO facility (CHH-F03/CHH-F07). Introduced by CHH-73 so
/// the Admin Command Center has something to query; the creation flow (CHH-78) now persists
/// through <c>FacilityFactory</c>/<c>FacilityService</c>. Properties still use plain public
/// setters rather than a factory-enforced construction pattern (contrast <see cref="BloodRequest"/>) —
/// <c>FacilityFactory</c> only ever sets these from an already-validated request, so there's no
/// invariant left unprotected.
/// </summary>
public class Facility
{
    /// <summary>Surrogate primary key.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Facility display name.</summary>
    public string FacilityName { get; set; } = default!;

    /// <summary>Hospital/blood-bank or NGO.</summary>
    public FacilityCategory Category { get; set; }

    /// <summary>Finer classification within <see cref="Category"/> (CHH-78 follow-up) — e.g. Government/Private/Trust for a hospital.</summary>
    public FacilitySubCategory SubCategory { get; set; }

    /// <summary>Alphanumeric + hyphens license number (matches frontend validation, CHH-78).</summary>
    public string LicenseNumber { get; set; } = default!;

    /// <summary>Facility address, free text.</summary>
    public string Address { get; set; } = default!;

    /// <summary>Verification lifecycle state — defaults to <see cref="FacilityVerificationStatus.Pending"/>.</summary>
    public FacilityVerificationStatus VerificationStatus { get; set; } = FacilityVerificationStatus.Pending;

    /// <summary>
    /// Blob storage reference for the uploaded license document. Null until CHH-74's upload path
    /// populates it — CHH-73's pending-list endpoint never sets this.
    /// </summary>
    public string? LicenseDocumentUrl { get; set; }

    /// <summary>UTC timestamp the facility record was created — also serves as "Date of Registration" (CHH-73 AC1).</summary>
    public DateTimeOffset CreatedAtUtc { get; set; }

    /// <summary>UTC timestamp of the last update (e.g. a CHH-75 approve/reject transition).</summary>
    public DateTimeOffset UpdatedAtUtc { get; set; }

    /// <summary>Contact persons for this facility (CHH-78 AC2, 1–3 contacts).</summary>
    public ICollection<FacilityContact> Contacts { get; set; } = new List<FacilityContact>();
}
