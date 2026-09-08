using Chh.Domain.Entities;

namespace Chh.Application.Contracts;

/// <summary>Data layer for <see cref="Event"/> (CHH-38/US-CHH-005-01).</summary>
public interface IEventRepository
{
    /// <summary>Adds a new event to the context. Does not call <c>SaveChangesAsync</c>.</summary>
    /// <param name="calendarEvent">The event to add.</param>
    /// <param name="ct">Cancellation token.</param>
    Task AddAsync(Event calendarEvent, CancellationToken ct);
}
