using Chh.Domain.Entities;
using Chh.Domain.Enums;

namespace Chh.Application.Contracts;

/// <summary>Data layer for <see cref="Facility"/> (CHH-78/US-CHH-003-01 creation, CHH-F07 Admin Command Center query).</summary>
public interface IFacilityRepository
{
    /// <summary>Adds a new facility (with its contacts) to the context. Does not call <c>SaveChangesAsync</c>.</summary>
    /// <param name="facility">The facility to add.</param>
    /// <param name="ct">Cancellation token.</param>
    Task AddAsync(Facility facility, CancellationToken ct);

    /// <summary>
    /// Returns one page of facilities matching <paramref name="status"/>, ordered by
    /// <c>CreatedAtUtc</c> ascending (oldest registration first), plus the total matching count.
    /// Read-only — implementations must use <c>AsNoTracking()</c> (api-standards.md §6).
    /// </summary>
    /// <param name="status">Verification status to filter on.</param>
    /// <param name="page">1-based page number.</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<(IReadOnlyList<Facility> Items, int TotalCount)> GetByStatusAsync(
        FacilityVerificationStatus status, int page, int pageSize, CancellationToken ct);
}
