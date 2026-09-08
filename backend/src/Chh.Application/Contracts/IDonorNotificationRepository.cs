using Chh.Application.Dtos;
using Chh.Domain.Entities;

namespace Chh.Application.Contracts;

/// <summary>Data layer for <see cref="DonorNotification"/> (CHH-34).</summary>
public interface IDonorNotificationRepository
{
    /// <summary>
    /// True if a notification already exists for this exact (request, donor) pair — the AC4
    /// duplicate-prevention check, run before creating a new one.
    /// </summary>
    /// <param name="bloodRequestId">The blood request id.</param>
    /// <param name="donorProfileId">The donor's <c>IndividualProfile</c> id.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<bool> ExistsAsync(Guid bloodRequestId, Guid donorProfileId, CancellationToken ct);

    /// <summary>Adds a new donor notification to the context. Does not call <c>SaveChangesAsync</c>.</summary>
    /// <param name="notification">The notification to add.</param>
    /// <param name="ct">Cancellation token.</param>
    Task AddAsync(DonorNotification notification, CancellationToken ct);

    /// <summary>
    /// Returns one page of notifications for the given donor, newest first (AC3), plus the total
    /// matching count. Read-only — <c>AsNoTracking()</c> (api-standards.md §6).
    /// </summary>
    /// <param name="donorProfileId">The donor's <c>IndividualProfile</c> id.</param>
    /// <param name="page">1-based page number.</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<(IReadOnlyList<DonorNotification> Items, int TotalCount)> GetByDonorAsync(
        Guid donorProfileId, int page, int pageSize, CancellationToken ct);

    /// <summary>
    /// Returns the tracked notification for the given id and donor (ownership check baked in), or
    /// <c>null</c> if it doesn't exist or belongs to a different donor.
    /// </summary>
    /// <param name="id">The notification id.</param>
    /// <param name="donorProfileId">The authenticated caller's <c>IndividualProfile</c> id.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<DonorNotification?> GetTrackedByIdForDonorAsync(Guid id, Guid donorProfileId, CancellationToken ct);

    /// <summary>
    /// Returns every notification for the given blood request, joined with each donor's identity,
    /// in stable match order (oldest first — CHH-36's "Donor 1, Donor 2, ..." anonymized labels).
    /// Read-only — <c>AsNoTracking()</c> (api-standards.md §6).
    /// </summary>
    /// <param name="bloodRequestId">The blood request to look up matches for.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<IReadOnlyList<DonorNotificationWithDonorInfo>> GetWithDonorInfoByBloodRequestIdAsync(Guid bloodRequestId, CancellationToken ct);
}
