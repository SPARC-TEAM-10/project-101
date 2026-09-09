using Chh.Domain.Entities;

namespace Chh.Application.Contracts;

/// <summary>Data layer for <see cref="EventRsvp"/> (CHH-40/US-CHH-005-03).</summary>
public interface IEventRsvpRepository
{
    /// <summary>Adds a new RSVP row to the context. Does not call <c>SaveChangesAsync</c>.</summary>
    /// <param name="rsvp">The RSVP to add.</param>
    /// <param name="ct">Cancellation token.</param>
    Task AddAsync(EventRsvp rsvp, CancellationToken ct);

    /// <summary>
    /// Returns the caller's own RSVP row for this event, tracked by the context so mutations
    /// (Cancel, re-RSVP reactivation) are persisted on <c>SaveChangesAsync</c> — or <c>null</c>
    /// if this individual has never RSVP'd to this event.
    /// </summary>
    /// <param name="eventId">The event to look up.</param>
    /// <param name="individualProfileId">The caller's own profile id.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<EventRsvp?> GetTrackedByEventAndIndividualAsync(Guid eventId, Guid individualProfileId, CancellationToken ct);

    /// <summary>
    /// Read-only lookup of the caller's own RSVP row for this event (event detail page) — same
    /// match as <see cref="GetTrackedByEventAndIndividualAsync"/> but untracked. Implementations
    /// must use <c>AsNoTracking()</c> (api-standards.md §6).
    /// </summary>
    /// <param name="eventId">The event to look up.</param>
    /// <param name="individualProfileId">The caller's own profile id.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<EventRsvp?> GetByEventAndIndividualAsync(Guid eventId, Guid individualProfileId, CancellationToken ct);

    /// <summary>
    /// Returns the total number of RSVP rows (Going or Cancelled) ever created for this event —
    /// used to assign the next <see cref="EventRsvp.ReferenceCode"/> (best-effort sequential, see
    /// that property's doc comment on the accepted collision tradeoff).
    /// </summary>
    /// <param name="eventId">The event to count RSVPs for.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<int> CountForEventAsync(Guid eventId, CancellationToken ct);
}
