using Chh.Application.Contracts;
using Chh.Domain.Constants;
using Chh.Domain.Entities;
using Chh.Domain.Enums;
using Chh.Domain.Utilities;
using Microsoft.Extensions.Logging;

namespace Chh.Application.Services;

/// <summary>
/// Sends the CHH-42/US-CHH-005-05 "new event published nearby" SMS to every active individual
/// within <see cref="EventConstants.EventPublishNotificationRadiusKm"/> of the event's venue —
/// SMS-only, matching <see cref="NotificationDispatchService"/>/<see cref="EventNotificationDispatchService"/>'s
/// established SMS mechanism (the only notification delivery actually implemented in this
/// codebase; no FCM/push infra exists despite the task breakdown's "FCM alerts" wording).
/// </summary>
public class EventPublishNotificationDispatchService : IEventPublishNotificationDispatchService
{
    private readonly IIndividualProfileRepository _individualProfileRepository;
    private readonly ISmsGatewayClient _smsGatewayClient;
    private readonly ILogger<EventPublishNotificationDispatchService> _logger;

    /// <summary>Creates the service with its dependencies.</summary>
    /// <param name="individualProfileRepository">Resolves candidate recipients with known coordinates.</param>
    /// <param name="smsGatewayClient">Sends the notification SMS.</param>
    /// <param name="logger">Logs per-recipient dispatch failures — Edge Case: "notifications disabled" is logged, not thrown.</param>
    public EventPublishNotificationDispatchService(
        IIndividualProfileRepository individualProfileRepository,
        ISmsGatewayClient smsGatewayClient,
        ILogger<EventPublishNotificationDispatchService> logger)
    {
        _individualProfileRepository = individualProfileRepository;
        _smsGatewayClient = smsGatewayClient;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task NotifyPublishedAsync(Event calendarEvent, CancellationToken ct)
    {
        var candidates = await _individualProfileRepository.GetActiveWithKnownLocationAsync(ct).ConfigureAwait(false);

        foreach (var candidate in candidates)
        {
            // GetActiveWithKnownLocationAsync already filters AccountStatus/null-coordinates at the
            // DB level — re-checked here too (matches MatchingEngineService's established pattern)
            // so this service's own eligibility rule holds regardless of which
            // IIndividualProfileRepository implementation supplies the candidates.
            if (candidate.AccountStatus != AccountStatus.Active)
            {
                continue;
            }

            if (candidate.Latitude is not { } latitude || candidate.Longitude is not { } longitude)
            {
                continue;
            }

            var distanceKm = HaversineDistanceCalculator.CalculateDistanceKm(
                calendarEvent.Latitude, calendarEvent.Longitude, latitude, longitude);

            if (distanceKm > EventConstants.EventPublishNotificationRadiusKm)
            {
                continue;
            }

            try
            {
                var message = BuildSmsMessage(calendarEvent, distanceKm);
                await _smsGatewayClient.SendMessageAsync(candidate.MobileNumber, message, ct).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // One recipient's SMS failure must not stop the rest of the fan-out — also covers
                // the Edge Case ("notifications disabled ... system should log but not fail"): no
                // notification-preference flag exists in this codebase, so a delivery failure of
                // any kind is the closest analogue, and is handled the same way.
                _logger.LogWarning(
                    ex,
                    "EventPublishNotificationDispatchService: SMS notification failed for Event {EventId}, IndividualProfile {IndividualProfileId} — continuing with remaining recipients.",
                    calendarEvent.Id, candidate.Id);
            }
        }
    }

    // Field order and phrasing match EventSmsPreview.dc.html's "New event in your region" example.
    private static string BuildSmsMessage(Event calendarEvent, decimal distanceKm)
    {
        var typeLabel = EventTypeDisplay.ToLabel(calendarEvent.EventType);
        var capitalizedTypeLabel = char.ToUpperInvariant(typeLabel[0]) + typeLabel[1..];
        var day = calendarEvent.StartAtUtc.ToString("ddd d MMM HH:mm");
        var spotsRemaining = calendarEvent.Capacity - calendarEvent.RsvpCount;
        return $"{capitalizedTypeLabel} {day}, {calendarEvent.VenueName}, about {distanceKm:0}km away. " +
               $"{spotsRemaining} spots left. Reply YES to RSVP, NO to stop. — Community Health Hub";
    }
}
