using Chh.Application.Dtos;
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

    /// <summary>
    /// Returns the mobile numbers of every individual with an active (<c>Going</c>) RSVP for the
    /// event — CHH-41's SMS notification fan-out on a notify-worthy edit or cancellation.
    /// Read-only — implementations must use <c>AsNoTracking()</c> (api-standards.md §6).
    /// </summary>
    /// <param name="eventId">The event whose attendees should be notified.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<IReadOnlyList<string>> GetGoingMobileNumbersAsync(Guid eventId, CancellationToken ct);

    /// <summary>
    /// Returns the RSVP row for <paramref name="rsvpId"/>, tracked by the context so the
    /// attendance-marking mutation (CHH-44) is persisted on <c>SaveChangesAsync</c> — or
    /// <c>null</c> if none exists.
    /// </summary>
    /// <param name="rsvpId">The RSVP row's id.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<EventRsvp?> GetTrackedByIdAsync(Guid rsvpId, CancellationToken ct);

    /// <summary>
    /// Returns every <c>Going</c> or <c>Attended</c> RSVP for the event whose individual's full
    /// name contains <paramref name="search"/> (case-insensitive) or whose mobile number equals it
    /// exactly, joined with that individual's name/mobile (CHH-44's participant search, AC1/AC2).
    /// Read-only — implementations must use <c>AsNoTracking()</c> (api-standards.md §6).
    /// </summary>
    /// <param name="eventId">The event to search participants for.</param>
    /// <param name="search">Already-validated search term (name fragment or full mobile number).</param>
    /// <param name="ct">Cancellation token.</param>
    Task<IReadOnlyList<EventRsvpWithProfileResult>> SearchParticipantsAsync(Guid eventId, string search, CancellationToken ct);

    /// <summary>
    /// Returns every RSVP row for the event (any status), joined with that individual's
    /// name/mobile — CHH-45's attendance analytics (summary counts, the filterable participant
    /// list, and CSV export). Read-only — implementations must use <c>AsNoTracking()</c>
    /// (api-standards.md §6).
    /// </summary>
    /// <param name="eventId">The event to load every RSVP for.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<IReadOnlyList<EventRsvpWithProfileResult>> GetAllWithProfileAsync(Guid eventId, CancellationToken ct);
}
