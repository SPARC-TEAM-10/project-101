using Chh.Domain.Enums;

namespace Chh.Application.Dtos;

/// <summary>
/// Response body for <c>POST /api/v1/facilities</c> (CHH-78/US-CHH-003-01) and response item for
/// <c>GET /api/v1/admin/facilities/pending</c> (CHH-73). Never the raw <c>Facility</c> entity
/// (db-standards.md §2b).
/// </summary>
public record FacilityDto
{
    /// <summary>Surrogate primary key.</summary>
    public required Guid Id { get; init; }

    /// <summary>Registered name of the hospital or NGO.</summary>
    public required string FacilityName { get; init; }

    /// <summary>Hospital or NGO.</summary>
    public required FacilityCategory Category { get; init; }

    /// <summary>Operating license number.</summary>
    public required string LicenseNumber { get; init; }

    /// <summary>Fixed address donors are routed to.</summary>
    public required string Address { get; init; }

    /// <summary>This facility's contacts, in <c>SortOrder</c> — the first is primary.</summary>
    public required IReadOnlyList<FacilityContactDto> Contacts { get; init; }

    /// <summary>Verification lifecycle state — "Pending" immediately after creation (CHH-78 AC1).</summary>
    public required FacilityVerificationStatus VerificationStatus { get; init; }

    /// <summary>Blob storage reference for the uploaded license document; null until CHH-74's upload path populates it.</summary>
    public string? LicenseDocumentUrl { get; init; }

    /// <summary>Admin's reason for rejection; null unless <see cref="VerificationStatus"/> is "Rejected" (CHH-28).</summary>
    public string? RejectionReason { get; init; }

    /// <summary>UTC timestamp the facility record was created — also serves as "Date of Registration" (CHH-73 AC1).</summary>
    public required DateTimeOffset CreatedAtUtc { get; init; }

    /// <summary>UTC timestamp of the last update — doubles as the verification decision date on CHH-28's status dashboard.</summary>
    public required DateTimeOffset UpdatedAtUtc { get; init; }
}
