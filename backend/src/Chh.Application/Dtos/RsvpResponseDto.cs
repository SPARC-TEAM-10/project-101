using Chh.Domain.Enums;

namespace Chh.Application.Dtos;

/// <summary>Response body for <c>POST</c>/<c>DELETE /api/v1/events/{id}/rsvp</c> (CHH-40/US-CHH-005-03).</summary>
public record RsvpResponseDto
{
    public required Guid EventId { get; init; }
    public required EventRsvpStatus Status { get; init; }

    /// <summary>Present only alongside <see cref="EventRsvpStatus.Going"/>.</summary>
    public string? ReferenceCode { get; init; }

    public required int SpotsRemaining { get; init; }
}
