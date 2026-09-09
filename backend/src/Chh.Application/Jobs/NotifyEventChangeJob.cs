using Chh.Application.Contracts;
using Microsoft.Extensions.Logging;

namespace Chh.Application.Jobs;

/// <summary>
/// Hangfire background job that sends the CHH-41/US-CHH-005-04 edit/cancel notification —
/// enqueued fire-and-forget by <see cref="Services.EventService"/> so the PATCH/cancel endpoint
/// returns immediately (api-standards.md §6 NFR, matching <see cref="MatchDonorsJob"/>'s pattern).
/// </summary>
public class NotifyEventChangeJob
{
    private readonly IEventRepository _eventRepository;
    private readonly IEventNotificationDispatchService _notificationDispatchService;
    private readonly ILogger<NotifyEventChangeJob> _logger;

    /// <summary>Creates the job with its dependencies.</summary>
    /// <param name="eventRepository">Reloads the event (the job runs outside the original request).</param>
    /// <param name="notificationDispatchService">Sends the SMS fan-out.</param>
    /// <param name="logger">Logs a missing-event no-op.</param>
    public NotifyEventChangeJob(
        IEventRepository eventRepository,
        IEventNotificationDispatchService notificationDispatchService,
        ILogger<NotifyEventChangeJob> logger)
    {
        _eventRepository = eventRepository;
        _notificationDispatchService = notificationDispatchService;
        _logger = logger;
    }

    /// <summary>Notifies the event's active RSVP holders that its venue and/or time changed.</summary>
    /// <param name="eventId">The updated event.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task RunUpdatedAsync(Guid eventId, CancellationToken ct)
    {
        var result = await _eventRepository.GetByIdWithFacilityNameAsync(eventId, ct).ConfigureAwait(false);
        if (result is null)
        {
            _logger.LogWarning("NotifyEventChangeJob: Event {EventId} not found — skipping update notification.", eventId);
            return;
        }

        await _notificationDispatchService.NotifyUpdatedAsync(result.Event, ct).ConfigureAwait(false);
    }

    /// <summary>Notifies the event's active RSVP holders that it was cancelled.</summary>
    /// <param name="eventId">The cancelled event.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task RunCancelledAsync(Guid eventId, CancellationToken ct)
    {
        var result = await _eventRepository.GetByIdWithFacilityNameAsync(eventId, ct).ConfigureAwait(false);
        if (result is null)
        {
            _logger.LogWarning("NotifyEventChangeJob: Event {EventId} not found — skipping cancellation notification.", eventId);
            return;
        }

        await _notificationDispatchService.NotifyCancelledAsync(result.Event, ct).ConfigureAwait(false);
    }
}
