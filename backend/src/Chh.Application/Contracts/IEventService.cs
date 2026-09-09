using Chh.Application.Dtos;
using Chh.Domain.Enums;

namespace Chh.Application.Contracts;

/// <summary>Logic layer for event creation (CHH-38/US-CHH-005-01) and discovery (CHH-39/US-CHH-005-02).</summary>
public interface IEventService
{
    /// <summary>
    /// Creates a new published event on behalf of the caller's facility.
    /// </summary>
    /// <param name="creatorMobileNumber">The authenticated creator's mobile number (from the JWT "sub" claim, never client-supplied).</param>
    /// <param name="request">The validated event creation details.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <exception cref="Chh.Application.Abstractions.FacilityNotVerifiedException">The caller's facility isn't Verified.</exception>
    Task<EventDto> CreateAsync(string creatorMobileNumber, CreateEventRequest request, CancellationToken ct);

    /// <summary>
    /// Returns published, upcoming events within <paramref name="radiusKm"/> of the given
    /// coordinates, nearest-start-time first (AC1). <paramref name="radiusKm"/> is clamped to
    /// [5,100], not rejected (AC3's literal "caps the radius").
    /// </summary>
    /// <param name="latitude">Search origin latitude.</param>
    /// <param name="longitude">Search origin longitude.</param>
    /// <param name="radiusKm">Requested search radius in kilometers — clamped server-side.</param>
    /// <param name="eventType">Optional event type filter.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<IReadOnlyList<EventSummaryDto>> SearchAsync(
        decimal latitude, decimal longitude, int radiusKm, EventType? eventType, CancellationToken ct);
}
