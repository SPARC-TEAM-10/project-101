using Chh.Domain.Entities;

namespace Chh.Application.Contracts;

/// <summary>
/// Notifies individuals registered near a newly published event's venue (CHH-42/US-CHH-005-05
/// AC1/AC2). SMS-only — see <see cref="Services.EventPublishNotificationDispatchService"/>'s doc
/// comment for why, matching the same decision made for CHH-41's edit/cancel notifications.
/// </summary>
public interface IEventPublishNotificationDispatchService
{
    /// <summary>Notifies every active individual within the notification radius of the event's venue.</summary>
    /// <param name="calendarEvent">The newly published event, already persisted.</param>
    /// <param name="ct">Cancellation token.</param>
    Task NotifyPublishedAsync(Event calendarEvent, CancellationToken ct);
}
