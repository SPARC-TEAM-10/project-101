using Chh.Application.Contracts;
using Chh.Application.Dtos;
using Chh.Application.Factories;
using Chh.Domain.Constants;
using Chh.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Chh.Application.Services;

/// <inheritdoc cref="INotificationDispatchService" />
public class NotificationDispatchService : INotificationDispatchService
{
    private readonly IDonorNotificationRepository _donorNotificationRepository;
    private readonly ISmsGatewayClient _smsGatewayClient;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<NotificationDispatchService> _logger;

    /// <summary>Creates the service with its dependencies.</summary>
    /// <param name="donorNotificationRepository">Data layer for duplicate checks and persistence.</param>
    /// <param name="smsGatewayClient">Sends the SMS fallback for donors who aren't currently active in-app.</param>
    /// <param name="unitOfWork">Persists the created notifications.</param>
    /// <param name="logger">Logs per-donor dispatch outcomes and SMS failures.</param>
    public NotificationDispatchService(
        IDonorNotificationRepository donorNotificationRepository,
        ISmsGatewayClient smsGatewayClient,
        IUnitOfWork unitOfWork,
        ILogger<NotificationDispatchService> logger)
    {
        _donorNotificationRepository = donorNotificationRepository;
        _smsGatewayClient = smsGatewayClient;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task DispatchAsync(BloodRequest bloodRequest, IReadOnlyList<MatchedDonorResult> matches, CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;
        var createdCount = 0;

        foreach (var match in matches)
        {
            // AC4: never notify the same donor twice for the same request.
            if (await _donorNotificationRepository.ExistsAsync(bloodRequest.Id, match.DonorProfileId, ct).ConfigureAwait(false))
            {
                continue;
            }

            var isCurrentlyActive = match.LastActiveAtUtc is { } lastActive && now - lastActive < PresenceConstants.ActiveWindow;
            var smsSent = false;

            if (!isCurrentlyActive)
            {
                smsSent = await TrySendSmsAsync(match, bloodRequest, ct).ConfigureAwait(false);
            }

            var notification = DonorNotificationFactory.Create(bloodRequest, match.DonorProfileId, match.DistanceKm, smsSent, now);
            await _donorNotificationRepository.AddAsync(notification, ct).ConfigureAwait(false);
            createdCount++;
        }

        if (createdCount > 0)
        {
            await _unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);
        }
    }

    // A single donor's SMS failure (e.g. gateway outage) must not stop the rest of the fan-out —
    // the in-app notification is still created for every matched donor regardless.
    private async Task<bool> TrySendSmsAsync(MatchedDonorResult match, BloodRequest bloodRequest, CancellationToken ct)
    {
        try
        {
            var message = BuildSmsMessage(bloodRequest, match.DistanceKm);
            await _smsGatewayClient.SendMessageAsync(match.MobileNumber, message, ct).ConfigureAwait(false);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "NotificationDispatchService: SMS fallback failed for BloodRequest {BloodRequestId}, donor {DonorProfileId} — in-app notification still created.",
                bloodRequest.Id, match.DonorProfileId);
            return false;
        }
    }

    // Off-app delivery business rule: blood group + units, urgency, approximate distance, area —
    // in that order, never patient name/requester identity/exact address. Reply-keyword copy per
    // the same rule; under 160 characters. Actual YES/NO reply handling is CHH-35's scope.
    private static string BuildSmsMessage(BloodRequest bloodRequest, decimal distanceKm)
    {
        var bloodGroup = BloodGroupDisplay.ToClinicalNotation(bloodRequest.BloodGroup);
        return $"{bloodGroup} blood needed, {bloodRequest.UnitsRequired} unit(s) - {bloodRequest.Urgency}. " +
               $"~{distanceKm:0.#}km away in {bloodRequest.LocationCityArea}. Reply YES to accept, NO to decline. - CHH";
    }
}
