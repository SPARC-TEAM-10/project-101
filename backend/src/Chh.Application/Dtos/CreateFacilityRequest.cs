using Chh.Domain.Enums;

namespace Chh.Application.Dtos;

/// <summary>Request body for <c>POST /api/v1/facilities</c> (CHH-78).</summary>
public record CreateFacilityRequest
{
    /// <summary>Registered name of the hospital or NGO.</summary>
    public required string FacilityName { get; init; }

    /// <summary>Hospital or NGO.</summary>
    public required FacilityCategory Category { get; init; }

    /// <summary>Finer classification within <see cref="Category"/> — must be one of the values allowed for that category.</summary>
    public required FacilitySubCategory SubCategory { get; init; }

    /// <summary>Alphanumeric + hyphens license number, as printed on the licence.</summary>
    public required string LicenseNumber { get; init; }

    /// <summary>Facility address, free text.</summary>
    public required string Address { get; init; }

    /// <summary>1–3 contact persons for this facility (CHH-78 AC2).</summary>
    public required IReadOnlyList<CreateFacilityContactRequest> Contacts { get; init; }
}
