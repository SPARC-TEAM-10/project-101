using Chh.Domain.Enums;

namespace Chh.Application.Dtos;

/// <summary>Response item for <c>GET /api/v1/admin/facilities/pending</c> (CHH-73). Never the raw <c>Facility</c> entity (db-standards.md §2b).</summary>
public record FacilityDto
{
    /// <summary>Surrogate primary key.</summary>
    public required Guid Id { get; init; }

    /// <summary>Facility display name.</summary>
    public required string FacilityName { get; init; }

    /// <summary>Hospital/blood-bank or NGO.</summary>
    public required FacilityCategory Category { get; init; }

    /// <summary>Alphanumeric + hyphens license number.</summary>
    public required string LicenseNumber { get; init; }

    /// <summary>Facility address.</summary>
    public required string Address { get; init; }

    /// <summary>Contact persons for this facility.</summary>
    public required IReadOnlyList<FacilityContactDto> Contacts { get; init; }

    /// <summary>Verification lifecycle state.</summary>
    public required FacilityVerificationStatus VerificationStatus { get; init; }

    /// <summary>Blob storage reference for the uploaded license document; null until CHH-74's upload path populates it.</summary>
    public string? LicenseDocumentUrl { get; init; }

    /// <summary>UTC timestamp the facility record was created — also serves as "Date of Registration" (CHH-73 AC1).</summary>
    public required DateTimeOffset CreatedAtUtc { get; init; }
}
