using Chh.Domain.Enums;

namespace Chh.Application.Dtos;

/// <summary>
/// Response body for <c>GET /api/v1/notifications/mine</c> and
/// <c>PATCH /api/v1/notifications/{id}/read</c> (CHH-34). Deliberately excludes the patient's
/// name and exact address (AC2/off-app-delivery business rule) — only the four safe facts.
/// </summary>
public record DonorNotificationDto
{
    /// <summary>Surrogate primary key.</summary>
    public required Guid Id { get; init; }

    /// <summary>The matched blood request's id — not shown to the donor, but useful for client-side dedup/linking.</summary>
    public required Guid BloodRequestId { get; init; }

    /// <summary>Blood group requested (AC2).</summary>
    public required BloodGroup BloodGroup { get; init; }

    /// <summary>Units required (AC2).</summary>
    public required int UnitsRequired { get; init; }

    /// <summary>Urgency (AC2) — "Emergency" gets high-visibility styling per the ticket's UI note.</summary>
    public required UrgencyLevel Urgency { get; init; }

    /// <summary>Approximate distance in kilometers (AC2 — "5km away").</summary>
    public required decimal DistanceKm { get; init; }

    /// <summary>The request's city/area — never the exact address.</summary>
    public required string AreaLabel { get; init; }

    /// <summary>Read status (AC3).</summary>
    public required bool IsRead { get; init; }

    /// <summary>UTC timestamp the notification was created.</summary>
    public required DateTimeOffset CreatedAtUtc { get; init; }

    /// <summary>The donor's response so far (CHH-35) — lets the client hide Accept/Decline once already responded.</summary>
    public required DonorResponseStatus ResponseStatus { get; init; }
}
