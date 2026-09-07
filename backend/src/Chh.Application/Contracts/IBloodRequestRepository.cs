using Chh.Domain.Entities;

namespace Chh.Application.Contracts;

/// <summary>Data layer for <see cref="BloodRequest"/> (CHH-33/US-CHH-004-01).</summary>
public interface IBloodRequestRepository
{
    /// <summary>Adds a new blood request to the context. Does not call <c>SaveChangesAsync</c>.</summary>
    /// <param name="bloodRequest">The blood request to add.</param>
    /// <param name="ct">Cancellation token.</param>
    Task AddAsync(BloodRequest bloodRequest, CancellationToken ct);

    /// <summary>
    /// Returns the blood request for the given id (read-only, untracked), or <c>null</c> if none
    /// exists. Added for US-CHH-004-02/CHH-80's <c>MatchDonorsJob</c>.
    /// </summary>
    /// <param name="id">The blood request id.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<BloodRequest?> GetByIdAsync(Guid id, CancellationToken ct);

    /// <summary>
    /// Returns one page of blood requests created by <paramref name="requesterMobileNumber"/>,
    /// ordered by <c>CreatedAtUtc</c> descending (newest first — CHH-81 Individual Dashboard),
    /// plus the total matching count. Read-only — <c>AsNoTracking()</c> (api-standards.md §6).
    /// </summary>
    /// <param name="requesterMobileNumber">The requester's mobile number, from the JWT.</param>
    /// <param name="page">1-based page number.</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<(IReadOnlyList<BloodRequest> Items, int TotalCount)> GetByRequesterAsync(
        string requesterMobileNumber, int page, int pageSize, CancellationToken ct);
}
