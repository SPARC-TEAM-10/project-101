using Chh.Domain.Enums;

namespace Chh.Application.Dtos;

/// <summary>
/// Public read shape for the Emergency Services Hub (CHH-82/US-CHH-001-01/02, Epic CHH-68) —
/// deliberately not <see cref="FacilityDto"/>: excludes <c>LicenseNumber</c>,
/// <c>LicenseDocumentUrl</c>, and <c>RejectionReason</c>, which `.claude/rules/db-standards.md`
/// §3 gates to System Admin. Only ever returned for a <see cref="FacilityVerificationStatus.Verified"/>
/// facility.
/// </summary>
public record PublicFacilityDto
{
    /// <summary>Surrogate primary key.</summary>
    public required Guid Id { get; init; }

    /// <summary>Registered name of the facility.</summary>
    public required string FacilityName { get; init; }

    /// <summary>Hospital, Ambulance, or NGO.</summary>
    public required FacilityCategory Category { get; init; }

    /// <summary>Finer classification within <see cref="Category"/>.</summary>
    public required FacilitySubCategory SubCategory { get; init; }

    /// <summary>Full address for the facility detail view (CHH-70 AC1).</summary>
    public required string Address { get; init; }

    /// <summary>Registered latitude; null if this facility has no stored coordinates yet.</summary>
    public decimal? Latitude { get; init; }

    /// <summary>Registered longitude — see <see cref="Latitude"/>.</summary>
    public decimal? Longitude { get; init; }

    /// <summary>This facility's contacts, for click-to-call (CHH-70 AC2).</summary>
    public required IReadOnlyList<FacilityContactDto> Contacts { get; init; }

    /// <summary>
    /// Great-circle distance from the caller's supplied coordinates, in kilometers. Null on
    /// <c>GET /facilities/{id}</c>, and on search results where the caller didn't supply
    /// <c>latitude</c>/<c>longitude</c>.
    /// </summary>
    public decimal? DistanceKm { get; init; }
}
