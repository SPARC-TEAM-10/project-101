using Chh.Application.Dtos;

namespace Chh.Application.Contracts;

/// <summary>Logic layer for System Admin facility moderation (CHH-F07 Admin Command Center).</summary>
public interface IFacilityAdminService
{
    /// <summary>Returns one page of facilities awaiting verification (CHH-73/US-CHH-001-01 AC1).</summary>
    /// <param name="page">1-based page number.</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<PagedResponse<FacilityDto>> GetPendingFacilitiesAsync(int page, int pageSize, CancellationToken ct);
}
