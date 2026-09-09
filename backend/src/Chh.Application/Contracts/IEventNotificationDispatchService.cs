using Chh.Domain.Entities;

namespace Chh.Application.Contracts;

/// <summary>
/// Notifies an event's active RSVP holders of a change (CHH-41/US-CHH-005-04 AC2/AC3). SMS-only —
/// see <see cref="Services.EventNotificationDispatchService"/>'s doc comment for why no separate
/// in-app notification entity exists.
/// </summary>
public interface IEventNotificationDispatchService
{
    /// <summary>Notifies every <c>Going</c> RSVP holder that the venue and/or start/end time changed.</summary>
    /// <param name="calendarEvent">The updated event, already persisted.</param>
    /// <param name="ct">Cancellation token.</param>
    Task NotifyUpdatedAsync(Event calendarEvent, CancellationToken ct);

    /// <summary>Notifies every <c>Going</c> RSVP holder that the event was cancelled.</summary>
    /// <param name="calendarEvent">The cancelled event, already persisted, with <see cref="Event.CancellationReason"/> set.</param>
    /// <param name="ct">Cancellation token.</param>
    Task NotifyCancelledAsync(Event calendarEvent, CancellationToken ct);
}
