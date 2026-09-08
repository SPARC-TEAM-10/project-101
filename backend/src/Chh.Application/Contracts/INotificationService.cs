using Chh.Application.Dtos;

namespace Chh.Application.Contracts;

/// <summary>
/// API-facing logic for a donor's own notifications (CHH-34) — distinct from
/// <see cref="INotificationDispatchService"/>, which creates notifications from the matching job.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Returns one page of the caller's own notifications, newest first (AC3), or an empty page if
    /// the caller hasn't registered an individual profile yet.
    /// </summary>
    /// <param name="mobileNumber">The authenticated caller's mobile number, from the JWT.</param>
    /// <param name="page">1-based page number.</param>
    /// <param name="pageSize">Items per page.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<PagedResponse<DonorNotificationDto>> GetMyNotificationsAsync(string mobileNumber, int page, int pageSize, CancellationToken ct);

    /// <summary>
    /// Marks one of the caller's own notifications read. Returns <c>null</c> if it doesn't exist,
    /// belongs to a different donor, or the caller hasn't registered an individual profile yet.
    /// </summary>
    /// <param name="mobileNumber">The authenticated caller's mobile number, from the JWT.</param>
    /// <param name="notificationId">The notification to mark read.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<DonorNotificationDto?> MarkReadAsync(string mobileNumber, Guid notificationId, CancellationToken ct);
}
