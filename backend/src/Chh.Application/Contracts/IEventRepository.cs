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
}
