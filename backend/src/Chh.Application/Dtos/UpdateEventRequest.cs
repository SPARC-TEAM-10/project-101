using Chh.Domain.Enums;

namespace Chh.Application.Dtos;

/// <summary>
/// Request body for <c>PATCH /api/v1/events/{id}</c> (CHH-41/US-CHH-005-04 AC3) — a partial update;
/// every property is optional, and an unset property leaves the current value unchanged.
/// </summary>
public record UpdateEventRequest
{
    public string? Title { get; init; }
    public EventType? EventType { get; init; }
    public string? Description { get; init; }
    public string? VenueName { get; init; }
    public string? VenueAddress { get; init; }
    public decimal? Latitude { get; init; }
    public decimal? Longitude { get; init; }
    public DateTimeOffset? StartAtUtc { get; init; }
    public DateTimeOffset? EndAtUtc { get; init; }
    public int? Capacity { get; init; }
    public string? CoordinatorName { get; init; }
    public string? CoordinatorContact { get; init; }
    public DateTimeOffset? RsvpCutoffAtUtc { get; init; }
}
