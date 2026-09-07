using Chh.Domain.Enums;

namespace Chh.Application.Dtos;

/// <summary>Response body for <c>POST /api/v1/blood-requests</c> (CHH-33/US-CHH-004-01). Never the raw <c>BloodRequest</c> entity (db-standards.md §2b).</summary>
public record BloodRequestDto
{
    /// <summary>Surrogate primary key.</summary>
    public required Guid Id { get; init; }

    /// <summary>Patient's name.</summary>
    public required string PatientName { get; init; }

    /// <summary>Required blood group.</summary>
    public required BloodGroup BloodGroup { get; init; }

    /// <summary>Number of units required.</summary>
    public required int UnitsRequired { get; init; }

    /// <summary>City/area, free text.</summary>
    public required string LocationCityArea { get; init; }

    /// <summary>Search radius in kilometers.</summary>
    public required int SearchRadiusKm { get; init; }

    /// <summary>Urgency of the request.</summary>
    public required UrgencyLevel Urgency { get; init; }

    /// <summary>Lifecycle state — "Matching" immediately after creation (AC1).</summary>
    public required BloodRequestStatus Status { get; init; }

    /// <summary>UTC timestamp the request was created.</summary>
    public required DateTimeOffset CreatedAtUtc { get; init; }

    /// <summary>UTC timestamp the request auto-expires (6 hours after creation).</summary>
    public required DateTimeOffset ExpiresAtUtc { get; init; }
}
