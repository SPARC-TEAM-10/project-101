using Chh.Domain.Enums;

namespace Chh.Application.Dtos;

/// <summary>Response item for <c>GET /api/v1/events/search</c> (CHH-39/US-CHH-005-02). Never the raw <c>Event</c> entity (db-standards.md §2b).</summary>
public record EventSummaryDto
{
    public required Guid Id { get; init; }
    public required string Title { get; init; }
    public required EventType EventType { get; init; }
    public required string FacilityName { get; init; }
    public required string VenueName { get; init; }
    public required decimal Latitude { get; init; }
    public required decimal Longitude { get; init; }
    public required DateTimeOffset StartAtUtc { get; init; }
    public required DateTimeOffset EndAtUtc { get; init; }
    public required decimal DistanceKm { get; init; }
    public required int Capacity { get; init; }

    /// <summary><see cref="Capacity"/> minus the event's active (non-cancelled) RSVP count.</summary>
    public required int SpotsRemaining { get; init; }
}
