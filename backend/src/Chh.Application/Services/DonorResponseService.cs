using Chh.Application.Abstractions;
using Chh.Application.Contracts;
using Chh.Application.Dtos;
using Chh.Domain.Entities;
using Chh.Domain.Enums;

namespace Chh.Application.Services;

/// <inheritdoc cref="IDonorResponseService" />
public class DonorResponseService : IDonorResponseService
{
    private readonly IIndividualProfileRepository _individualProfileRepository;
    private readonly IDonorNotificationRepository _donorNotificationRepository;
    private readonly IBloodRequestRepository _bloodRequestRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>Creates the service with its dependencies.</summary>
    /// <param name="individualProfileRepository">Resolves the caller's <c>IndividualProfile.Id</c> from their mobile number.</param>
    /// <param name="donorNotificationRepository">Data layer for reading and updating the caller's own notification.</param>
    /// <param name="bloodRequestRepository">Data layer for the atomic accept and the confirmation-view lookup.</param>
    /// <param name="unitOfWork">Persists the response-status update.</param>
    public DonorResponseService(
        IIndividualProfileRepository individualProfileRepository,
        IDonorNotificationRepository donorNotificationRepository,
        IBloodRequestRepository bloodRequestRepository,
        IUnitOfWork unitOfWork)
    {
        _individualProfileRepository = individualProfileRepository;
        _donorNotificationRepository = donorNotificationRepository;
        _bloodRequestRepository = bloodRequestRepository;
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<DonorResponseResultDto?> AcceptAsync(string mobileNumber, Guid notificationId, CancellationToken ct)
    {
        var notification = await GetOwnedPendingNotificationAsync(mobileNumber, notificationId, ct).ConfigureAwait(false);
        if (notification is null)
        {
            return null;
        }

        var now = DateTimeOffset.UtcNow;

        // Atomic, race-safe: the "still has units remaining, still Matching, not expired" check
        // and the increment happen in one database statement (CHH-35 Edge Case). A donor who loses
        // the race to the last unit gets the same "no longer active" outcome as one who was simply
        // too late — see BloodRequestRepository.TryAcceptUnitAsync.
        var accepted = await _bloodRequestRepository.TryAcceptUnitAsync(notification.BloodRequestId, now, ct).ConfigureAwait(false);
        if (!accepted)
        {
            throw new BloodRequestNoLongerActiveException();
        }

        notification.ResponseStatus = DonorResponseStatus.Accepted;
        notification.RespondedAtUtc = now;
        await _unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);

        var bloodRequest = await _bloodRequestRepository.GetByIdAsync(notification.BloodRequestId, ct).ConfigureAwait(false);

        return new DonorResponseResultDto
        {
            NotificationId = notification.Id,
            ResponseStatus = notification.ResponseStatus,
            RequesterMobileNumber = bloodRequest?.RequesterMobileNumber,
            LocationCityArea = bloodRequest?.LocationCityArea,
            Latitude = bloodRequest?.Latitude,
            Longitude = bloodRequest?.Longitude
        };
    }

    /// <inheritdoc />
    public async Task<DonorResponseResultDto?> DeclineAsync(string mobileNumber, Guid notificationId, CancellationToken ct)
    {
        var notification = await GetOwnedPendingNotificationAsync(mobileNumber, notificationId, ct).ConfigureAwait(false);
        if (notification is null)
        {
            return null;
        }

        // AC2: no expiry/fulfillment check — declining a request that's since gone inactive is
        // harmless and still a meaningful signal ("stop reminding me"), unlike accepting one.
        notification.ResponseStatus = DonorResponseStatus.Declined;
        notification.RespondedAtUtc = DateTimeOffset.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);

        return new DonorResponseResultDto
        {
            NotificationId = notification.Id,
            ResponseStatus = notification.ResponseStatus
        };
    }

    private async Task<DonorNotification?> GetOwnedPendingNotificationAsync(string mobileNumber, Guid notificationId, CancellationToken ct)
    {
        var profile = await _individualProfileRepository.GetByMobileNumberAsync(mobileNumber, ct).ConfigureAwait(false);
        if (profile is null)
        {
            return null;
        }

        var notification = await _donorNotificationRepository
            .GetTrackedByIdForDonorAsync(notificationId, profile.Id, ct)
            .ConfigureAwait(false);
        if (notification is null)
        {
            return null;
        }

        if (notification.ResponseStatus != DonorResponseStatus.Pending)
        {
            throw new DonorAlreadyRespondedException();
        }

        return notification;
    }
}
