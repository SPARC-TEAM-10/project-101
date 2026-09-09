using Chh.Domain.Enums;

namespace Chh.Application.Dtos;

/// <summary>Request body for <c>POST /api/v1/events</c> (CHH-38/US-CHH-005-01).</summary>
public record CreateEventRequest
{
    public required string Title { get; init; }
    public required EventType EventType { get; init; }
    public required string Description { get; init; }
    public required string VenueName { get; init; }
    public required string VenueAddress { get; init; }
    public required decimal Latitude { get; init; }
    public required decimal Longitude { get; init; }
    public required DateTimeOffset StartAtUtc { get; init; }
    public required DateTimeOffset EndAtUtc { get; init; }
    public required int Capacity { get; init; }
    public required string CoordinatorName { get; init; }
    public required string CoordinatorContact { get; init; }
    public DateTimeOffset? RsvpCutoffAtUtc { get; init; }
}
