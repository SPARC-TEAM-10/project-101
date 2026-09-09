using Chh.Application.Contracts;
using Chh.Domain.Constants;
using Chh.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Chh.Application.Services;

/// <summary>
/// Sends the CHH-41/US-CHH-005-04 edit/cancel SMS notifications to an event's active RSVP holders
/// — SMS-only, matching <see cref="NotificationDispatchService"/>'s established SMS-fallback
/// mechanism, which is the only notification delivery actually implemented in this codebase (no
/// FCM/push infra exists yet, despite backend/CLAUDE.md's Tech Stack row listing it as a future
/// target). A separate in-app "push" notification entity/feed for event updates was deliberately
/// scoped out — see the CHH-41 implementation plan.
/// </summary>
public class EventNotificationDispatchService : IEventNotificationDispatchService
{
    private readonly IEventRsvpRepository _eventRsvpRepository;
    private readonly ISmsGatewayClient _smsGatewayClient;
    private readonly ILogger<EventNotificationDispatchService> _logger;

    /// <summary>Creates the service with its dependencies.</summary>
    /// <param name="eventRsvpRepository">Resolves the mobile numbers of active RSVP holders to notify.</param>
    /// <param name="smsGatewayClient">Sends the notification SMS.</param>
    /// <param name="logger">Logs per-recipient dispatch failures.</param>
    public EventNotificationDispatchService(
        IEventRsvpRepository eventRsvpRepository,
        ISmsGatewayClient smsGatewayClient,
        ILogger<EventNotificationDispatchService> logger)
    {
        _eventRsvpRepository = eventRsvpRepository;
        _smsGatewayClient = smsGatewayClient;
        _logger = logger;
    }

    /// <inheritdoc />
    public Task NotifyUpdatedAsync(Event calendarEvent, CancellationToken ct) =>
        DispatchAsync(calendarEvent, BuildUpdatedMessage(calendarEvent), ct);

    /// <inheritdoc />
    public Task NotifyCancelledAsync(Event calendarEvent, CancellationToken ct) =>
        DispatchAsync(calendarEvent, BuildCancelledMessage(calendarEvent), ct);

    private async Task DispatchAsync(Event calendarEvent, string message, CancellationToken ct)
    {
        var mobileNumbers = await _eventRsvpRepository
            .GetGoingMobileNumbersAsync(calendarEvent.Id, ct)
            .ConfigureAwait(false);

        foreach (var mobileNumber in mobileNumbers)
        {
            try
            {
                await _smsGatewayClient.SendMessageAsync(mobileNumber, message, ct).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // One recipient's SMS failure (e.g. gateway outage) must not stop the rest of the
                // fan-out — matches NotificationDispatchService.TrySendSmsAsync's established rule.
                _logger.LogWarning(
                    ex,
                    "EventNotificationDispatchService: SMS notification failed for Event {EventId} — continuing with remaining recipients.",
                    calendarEvent.Id);
            }
        }
    }

    // Field order and phrasing match EventSmsPreview.dc.html's "Venue change" example, generalized
    // to also cover a start/end time change with no venue change.
    private static string BuildUpdatedMessage(Event calendarEvent)
    {
        var typeLabel = EventTypeDisplay.ToLabel(calendarEvent.EventType);
        var day = calendarEvent.StartAtUtc.ToString("ddd d MMM");
        var time = $"{calendarEvent.StartAtUtc:HH:mm}–{calendarEvent.EndAtUtc:HH:mm}";
        return $"Updated: {typeLabel} {day} is now {time} at {calendarEvent.VenueName}, {calendarEvent.VenueAddress}. — Community Health Hub";
    }

    // Matches EventSmsPreview.dc.html's "Cancelled" example — the organizer's reason is included
    // verbatim (EventEditWeb.dc.html's cancel modal: "Attendees see this word for word").
    private static string BuildCancelledMessage(Event calendarEvent)
    {
        var typeLabel = EventTypeDisplay.ToLabel(calendarEvent.EventType);
        var day = calendarEvent.StartAtUtc.ToString("ddd d MMM");
        return $"Cancelled: {typeLabel} {day}, {calendarEvent.VenueName}. {calendarEvent.CancellationReason} Do not attend. — Community Health Hub";
    }
}
