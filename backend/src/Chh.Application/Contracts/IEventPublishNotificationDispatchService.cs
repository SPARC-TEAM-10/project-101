using Chh.Domain.Entities;

namespace Chh.Application.Contracts;

/// <summary>
/// Notifies individuals registered near a newly published event's venue (CHH-42/US-CHH-005-05
/// AC1/AC2). SMS-only — see <see cref="Services.EventPublishNotificationDispatchService"/>'s doc
/// comment for why, matching the same decision made for CHH-41's edit/cancel notifications.
/// </summary>
public interface IEventPublishNotificationDispatchService
{
    /// <summary>
    /// Notifies every active individual within the notification radius of the event's venue.
    /// Returns the number targeted (CHH-45's "notified" funnel figure), regardless of whether each
    /// individual SMS actually sent — a delivery failure doesn't remove someone from the count,
    /// since they were still a target of the attempt.
    /// </summary>
    /// <param name="calendarEvent">The newly published event, already persisted.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<int> NotifyPublishedAsync(Event calendarEvent, CancellationToken ct);
}
