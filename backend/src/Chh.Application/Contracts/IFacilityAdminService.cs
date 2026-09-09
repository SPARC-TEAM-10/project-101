using Chh.Application.Dtos;
using Chh.Domain.Enums;

namespace Chh.Application.Contracts;

/// <summary>Logic layer for System Admin facility moderation (CHH-F07 Admin Command Center).</summary>
public interface IFacilityAdminService
{
    /// <summary>Returns one page of facilities awaiting verification (CHH-73/US-CHH-001-01 AC1).</summary>
    /// <param name="page">1-based page number.</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<PagedResponse<FacilityDto>> GetPendingFacilitiesAsync(int page, int pageSize, CancellationToken ct);

    /// <summary>
    /// Approves or rejects a pending facility (CHH-75/US-CHH-001-03). Returns <c>null</c> if no
    /// facility exists with the given id. Throws <see cref="Abstractions.FacilityAlreadyReviewedException"/>
    /// if the facility isn't <see cref="FacilityVerificationStatus.Pending"/> anymore.
    /// </summary>
    /// <param name="facilityId">The facility being reviewed.</param>
    /// <param name="decision">Approve or reject.</param>
    /// <param name="rejectionReason">Mandatory reason when <paramref name="decision"/> is Reject (AC2); ignored otherwise.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<FacilityDto?> ReviewFacilityAsync(Guid facilityId, FacilityVerificationDecision decision, string? rejectionReason, CancellationToken ct);
}
