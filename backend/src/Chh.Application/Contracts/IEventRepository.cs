using Chh.Application.Dtos;
using Chh.Domain.Entities;

namespace Chh.Application.Contracts;

/// <summary>Data layer for <see cref="Event"/> (CHH-38/US-CHH-005-01).</summary>
public interface IEventRepository
{
    /// <summary>Adds a new event to the context. Does not call <c>SaveChangesAsync</c>.</summary>
    /// <param name="calendarEvent">The event to add.</param>
    /// <param name="ct">Cancellation token.</param>
    Task AddAsync(Event calendarEvent, CancellationToken ct);

    /// <summary>
    /// Returns every <see cref="Chh.Domain.Enums.EventStatus.Published"/> event whose
    /// <see cref="Event.StartAtUtc"/> is still in the future, joined with its organizing
    /// facility's name (CHH-39/US-CHH-005-02) — unfiltered by location; the caller applies the
    /// Haversine distance/radius filter in-app, matching <c>MatchingEngineService</c>'s pattern.
    /// Read-only — implementations must use <c>AsNoTracking()</c> (api-standards.md §6).
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    Task<IReadOnlyList<EventWithFacilityNameResult>> GetUpcomingPublishedWithFacilityNameAsync(CancellationToken ct);

    /// <summary>
    /// Returns the event (joined with its organizing facility's name), or <c>null</c> if none
    /// exists (CHH-40's event detail page, GET /events/{id}). Read-only — implementations must
    /// use <c>AsNoTracking()</c> (api-standards.md §6).
    /// </summary>
    /// <param name="id">The event's id.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<EventWithFacilityNameResult?> GetByIdWithFacilityNameAsync(Guid id, CancellationToken ct);

    /// <summary>
    /// Atomically reserves one spot on <paramref name="eventId"/> — increments
    /// <see cref="Event.RsvpCount"/> in a single conditional <c>UPDATE</c> statement, only if the
    /// event is still <see cref="Chh.Domain.Enums.EventStatus.Published"/> and has capacity left
    /// (CHH-40 AC2, race-safe under concurrent RSVPs — mirrors
    /// <c>BloodRequestRepository.TryAcceptUnitAsync</c>). Returns <c>false</c> if the event
    /// doesn't exist, isn't published, or is already full.
    /// </summary>
    /// <param name="eventId">The event to reserve a spot on.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<bool> TryReserveSpotAsync(Guid eventId, CancellationToken ct);

    /// <summary>
    /// Atomically releases one previously-reserved spot back to the pool (Edge Case: cancelling an
    /// RSVP). No-ops (returns without effect) if <see cref="Event.RsvpCount"/> is already zero.
    /// </summary>
    /// <param name="eventId">The event to release a spot on.</param>
    /// <param name="ct">Cancellation token.</param>
    Task ReleaseSpotAsync(Guid eventId, CancellationToken ct);

    /// <summary>
    /// Returns the event tracked by the context so mutations (CHH-41 edit/cancel) are persisted on
    /// <c>SaveChangesAsync</c> — or <c>null</c> if none exists.
    /// </summary>
    /// <param name="id">The event's id.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<Event?> GetTrackedByIdAsync(Guid id, CancellationToken ct);

    /// <summary>
    /// Returns every event (any status) organized by <paramref name="facilityId"/>, most recent
    /// start time first — the facility's own "my events" list (CHH-41's manage-event entry point).
    /// Read-only — implementations must use <c>AsNoTracking()</c> (api-standards.md §6).
    /// </summary>
    /// <param name="facilityId">The organizing facility's id.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<IReadOnlyList<Event>> GetByFacilityAsync(Guid facilityId, CancellationToken ct);
}
