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

    /// <summary>
    /// Returns the event detail view, or <c>null</c> if no event exists with that id. Includes the
    /// caller's own RSVP status (null if they've never RSVP'd, e.g. a non-Individual role or an
    /// Individual who hasn't responded). <paramref name="latitude"/>/<paramref name="longitude"/>
    /// are optional — when both are supplied, <see cref="EventDetailDto.DistanceKm"/> is computed.
    /// </summary>
    /// <param name="eventId">The event to fetch.</param>
    /// <param name="callerMobileNumber">The authenticated caller's mobile number (from the JWT "sub" claim).</param>
    /// <param name="latitude">Optional caller latitude, for distance display.</param>
    /// <param name="longitude">Optional caller longitude, for distance display.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<EventDetailDto?> GetByIdAsync(
        Guid eventId, string callerMobileNumber, decimal? latitude, decimal? longitude, CancellationToken ct);

    /// <summary>
    /// RSVPs the caller (must be a registered Individual) to the event, or <c>null</c> if no event
    /// exists with that id.
    /// </summary>
    /// <param name="mobileNumber">The authenticated caller's mobile number (from the JWT "sub" claim).</param>
    /// <param name="eventId">The event to RSVP to.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <exception cref="Chh.Application.Abstractions.EventFullException">The event has no remaining capacity (AC2).</exception>
    /// <exception cref="Chh.Application.Abstractions.AlreadyRsvpdException">The caller already has an active RSVP (AC3).</exception>
    Task<RsvpResponseDto?> RsvpAsync(string mobileNumber, Guid eventId, CancellationToken ct);

    /// <summary>
    /// Cancels the caller's own active RSVP and releases the spot, or <c>null</c> if the caller has
    /// no active RSVP for that event (or no event exists with that id).
    /// </summary>
    /// <param name="mobileNumber">The authenticated caller's mobile number (from the JWT "sub" claim).</param>
    /// <param name="eventId">The event to cancel the RSVP for.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<RsvpResponseDto?> CancelRsvpAsync(string mobileNumber, Guid eventId, CancellationToken ct);

    /// <summary>
    /// Returns every event (any status) belonging to the caller's own facility, most recent start
    /// time first — the "my events" list backing CHH-41's manage-event entry point.
    /// </summary>
    /// <param name="callerMobileNumber">The authenticated caller's mobile number (from the JWT "sub" claim).</param>
    /// <param name="ct">Cancellation token.</param>
    Task<IReadOnlyList<EventDto>> GetMineAsync(string callerMobileNumber, CancellationToken ct);

    /// <summary>
    /// Partially updates the event, or <c>null</c> if no event exists with that id
    /// (CHH-41/US-CHH-005-04 AC3). A venue or start/end time change enqueues an attendee
    /// notification (AC3) — a description/title/coordinator/capacity-only change does not.
    /// </summary>
    /// <param name="callerMobileNumber">The authenticated caller's mobile number (from the JWT "sub" claim).</param>
    /// <param name="eventId">The event to update.</param>
    /// <param name="request">The partial update — unset properties leave the current value unchanged.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <exception cref="Chh.Application.Abstractions.EventNotOwnedByCallerException">The caller's facility doesn't own this event.</exception>
    /// <exception cref="Chh.Application.Abstractions.EventAlreadyCancelledException">The event is already cancelled.</exception>
    /// <exception cref="Chh.Application.Abstractions.EventAlreadyStartedException">The event's start time has already passed.</exception>
    /// <exception cref="Chh.Application.Abstractions.CapacityBelowRsvpCountException">The requested capacity is below the current RSVP count.</exception>
    Task<EventDto?> UpdateAsync(string callerMobileNumber, Guid eventId, UpdateEventRequest request, CancellationToken ct);

    /// <summary>
    /// Cancels the event and enqueues an attendee notification (AC1/AC2), or <c>null</c> if no
    /// event exists with that id.
    /// </summary>
    /// <param name="callerMobileNumber">The authenticated caller's mobile number (from the JWT "sub" claim).</param>
    /// <param name="eventId">The event to cancel.</param>
    /// <param name="request">The mandatory cancellation reason, shown verbatim to attendees.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <exception cref="Chh.Application.Abstractions.EventNotOwnedByCallerException">The caller's facility doesn't own this event.</exception>
    /// <exception cref="Chh.Application.Abstractions.EventAlreadyCancelledException">The event is already cancelled.</exception>
    /// <exception cref="Chh.Application.Abstractions.EventAlreadyStartedException">The event's start time has already passed.</exception>
    Task<EventDto?> CancelAsync(string callerMobileNumber, Guid eventId, CancelEventRequest request, CancellationToken ct);
}
