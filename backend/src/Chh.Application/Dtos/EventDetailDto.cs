using Chh.Domain.Enums;

namespace Chh.Application.Dtos;

/// <summary>Response body for <c>GET /api/v1/events/{id}</c> (CHH-40/US-CHH-005-03 detail page). Never the raw <c>Event</c> entity (db-standards.md §2b).</summary>
public record EventDetailDto
{
    public required Guid Id { get; init; }
    public required string Title { get; init; }
    public required EventType EventType { get; init; }
    public required string Description { get; init; }
    public required string FacilityName { get; init; }
    public required string VenueName { get; init; }
    public required string VenueAddress { get; init; }
    public required decimal Latitude { get; init; }
    public required decimal Longitude { get; init; }
    public required DateTimeOffset StartAtUtc { get; init; }
    public required DateTimeOffset EndAtUtc { get; init; }
    public required int Capacity { get; init; }
    public required int SpotsRemaining { get; init; }
    public required string CoordinatorName { get; init; }
    public required string CoordinatorContact { get; init; }
    public DateTimeOffset? RsvpCutoffAtUtc { get; init; }
    public required EventStatus Status { get; init; }

    /// <summary>Only populated when the caller supplied their own coordinates as query params.</summary>
    public decimal? DistanceKm { get; init; }

    /// <summary>
    /// The caller's own RSVP status for this event — <c>null</c> if they've never RSVP'd (or
    /// aren't an Individual, e.g. a Hospital/Ngo/Guest viewing the page).
    /// </summary>
    public EventRsvpStatus? MyRsvpStatus { get; init; }

    /// <summary>The caller's own reference code, present only alongside a <see cref="EventRsvpStatus.Going"/> <see cref="MyRsvpStatus"/>.</summary>
    public string? MyReferenceCode { get; init; }
}
