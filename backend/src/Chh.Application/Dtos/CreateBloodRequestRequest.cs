using Chh.Domain.Enums;

namespace Chh.Application.Dtos;

/// <summary>Request body for <c>POST /api/v1/blood-requests</c> (CHH-33/US-CHH-004-01).</summary>
public record CreateBloodRequestRequest
{
    /// <summary>Patient's name.</summary>
    public required string PatientName { get; init; }

    /// <summary>Required blood group.</summary>
    public required BloodGroup BloodGroup { get; init; }

    /// <summary>Number of units required — must be greater than zero.</summary>
    public required int UnitsRequired { get; init; }

    /// <summary>City/area, free text.</summary>
    public required string LocationCityArea { get; init; }

    /// <summary>
    /// Latitude from the requester's device (browser/device Geolocation) — required; its absence
    /// is the Edge Case "location coordinates cannot be resolved" (see
    /// <c>CreateBloodRequestRequestValidator</c>).
    /// </summary>
    public required decimal Latitude { get; init; }

    /// <summary>Longitude from the requester's device — see <see cref="Latitude"/>.</summary>
    public required decimal Longitude { get; init; }

    /// <summary>Search radius in kilometers — must be between 5 and 100 inclusive (AC2/AC3).</summary>
    public required int SearchRadiusKm { get; init; }

    /// <summary>Urgency of the request.</summary>
    public required UrgencyLevel Urgency { get; init; }
}
