using Chh.Application.Contracts;
using Chh.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Chh.Application.Jobs;

/// <summary>
/// Hangfire background job that sends the CHH-42/US-CHH-005-05 "new event published nearby"
/// notification — enqueued fire-and-forget by <see cref="Services.EventService"/> after event
/// creation so the POST /events response isn't delayed (api-standards.md §6 NFR, matching
/// <see cref="MatchDonorsJob"/>'s pattern).
/// </summary>
public class NotifyEventPublishedJob
{
    private readonly IEventRepository _eventRepository;
    private readonly IEventPublishNotificationDispatchService _notificationDispatchService;
    private readonly ILogger<NotifyEventPublishedJob> _logger;

    /// <summary>Creates the job with its dependencies.</summary>
    /// <param name="eventRepository">Reloads the event (the job runs outside the original request).</param>
    /// <param name="notificationDispatchService">Sends the SMS fan-out.</param>
    /// <param name="logger">Logs a missing-event or already-past no-op.</param>
    public NotifyEventPublishedJob(
        IEventRepository eventRepository,
        IEventPublishNotificationDispatchService notificationDispatchService,
        ILogger<NotifyEventPublishedJob> logger)
    {
        _eventRepository = eventRepository;
        _notificationDispatchService = notificationDispatchService;
        _logger = logger;
    }

    /// <summary>
    /// Notifies individuals near the event's venue. A missing, cancelled, or already-started event
    /// is logged and treated as a no-op — not an error, so Hangfire does not retry it (Business
    /// Rule: "Notifications should not be sent for events in the past").
    /// </summary>
    /// <param name="eventId">The newly published event.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task RunAsync(Guid eventId, CancellationToken ct)
    {
        var result = await _eventRepository.GetByIdWithFacilityNameAsync(eventId, ct).ConfigureAwait(false);
        if (result is null)
        {
            _logger.LogWarning("NotifyEventPublishedJob: Event {EventId} not found — skipping.", eventId);
            return;
        }

        var calendarEvent = result.Event;
        if (calendarEvent.Status != EventStatus.Published || calendarEvent.StartAtUtc <= DateTimeOffset.UtcNow)
        {
            _logger.LogInformation(
                "NotifyEventPublishedJob: Event {EventId} is not an upcoming published event — skipping.", eventId);
            return;
        }

        var notifiedCount = await _notificationDispatchService.NotifyPublishedAsync(calendarEvent, ct).ConfigureAwait(false);
        await _eventRepository.SetNotifiedCountAsync(eventId, notifiedCount, ct).ConfigureAwait(false);
    }
}
