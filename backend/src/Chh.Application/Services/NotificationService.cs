using Chh.Application.Contracts;
using Chh.Application.Dtos;
using Chh.Domain.Constants;
using Chh.Domain.Entities;

namespace Chh.Application.Services;

/// <inheritdoc cref="INotificationService" />
public class NotificationService : INotificationService
{
    private readonly IIndividualProfileRepository _individualProfileRepository;
    private readonly IDonorNotificationRepository _donorNotificationRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>Creates the service with its dependencies.</summary>
    /// <param name="individualProfileRepository">Resolves the caller's <c>IndividualProfile.Id</c> from their mobile number.</param>
    /// <param name="donorNotificationRepository">Data layer for reading and updating notifications.</param>
    /// <param name="unitOfWork">Persists the read-status update.</param>
    public NotificationService(
        IIndividualProfileRepository individualProfileRepository,
        IDonorNotificationRepository donorNotificationRepository,
        IUnitOfWork unitOfWork)
    {
        _individualProfileRepository = individualProfileRepository;
        _donorNotificationRepository = donorNotificationRepository;
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<PagedResponse<DonorNotificationDto>> GetMyNotificationsAsync(string mobileNumber, int page, int pageSize, CancellationToken ct)
    {
        var normalizedPage = page < 1 ? 1 : page;
        var normalizedPageSize = pageSize < 1
            ? PaginationConstants.DefaultPageSize
            : Math.Min(pageSize, PaginationConstants.MaxPageSize);

        var profile = await _individualProfileRepository.GetByMobileNumberAsync(mobileNumber, ct).ConfigureAwait(false);
        if (profile is null)
        {
            return new PagedResponse<DonorNotificationDto>
            {
                Items = Array.Empty<DonorNotificationDto>(),
                TotalCount = 0,
                Page = normalizedPage,
                PageSize = normalizedPageSize
            };
        }

        var (notifications, totalCount) = await _donorNotificationRepository
            .GetByDonorAsync(profile.Id, normalizedPage, normalizedPageSize, ct)
            .ConfigureAwait(false);

        return new PagedResponse<DonorNotificationDto>
        {
            Items = notifications.Select(ToDto).ToList(),
            TotalCount = totalCount,
            Page = normalizedPage,
            PageSize = normalizedPageSize
        };
    }

    /// <inheritdoc />
    public async Task<DonorNotificationDto?> MarkReadAsync(string mobileNumber, Guid notificationId, CancellationToken ct)
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

        notification.IsRead = true;
        await _unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);

        return ToDto(notification);
    }

    private static DonorNotificationDto ToDto(DonorNotification notification) => new()
    {
        Id = notification.Id,
        BloodRequestId = notification.BloodRequestId,
        BloodGroup = notification.BloodGroup,
        UnitsRequired = notification.UnitsRequired,
        Urgency = notification.Urgency,
        DistanceKm = notification.DistanceKm,
        AreaLabel = notification.AreaLabel,
        IsRead = notification.IsRead,
        CreatedAtUtc = notification.CreatedAtUtc
    };
}
