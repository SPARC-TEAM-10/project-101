using Chh.Domain.Enums;

namespace Chh.Application.Dtos;

/// <summary>One contact entry within a <see cref="CreateFacilityRequest"/> (CHH-78/US-CHH-003-01 AC2).</summary>
public record CreateFacilityContactRequest
{
    /// <summary>Contact's full name.</summary>
    public required string Name { get; init; }

    /// <summary>Contact's role at the facility.</summary>
    public required string Designation { get; init; }

    /// <summary>Contact's mobile number — 10 digits.</summary>
    public required string Mobile { get; init; }
}

/// <summary>Request body for <c>POST /api/v1/facilities</c> (CHH-78/US-CHH-003-01).</summary>
public record CreateFacilityRequest
{
    /// <summary>Registered name of the hospital or NGO.</summary>
    public required string FacilityName { get; init; }

    /// <summary>Hospital or NGO.</summary>
    public required FacilityCategory Category { get; init; }

    /// <summary>Operating license number — letters, numbers, and hyphens only.</summary>
    public required string LicenseNumber { get; init; }

    /// <summary>Fixed address donors are routed to.</summary>
    public required string Address { get; init; }

    /// <summary>1 to 3 contacts — the first is the primary contact (AC2).</summary>
    public required IReadOnlyList<CreateFacilityContactRequest> Contacts { get; init; }
}
