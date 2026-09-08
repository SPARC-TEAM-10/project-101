using Chh.Application.Dtos;
using Chh.Domain.Entities;

namespace Chh.Application.Contracts;

/// <summary>Proximity donor matching (US-CHH-004-02/CHH-80).</summary>
public interface IMatchingEngineService
{
    /// <summary>
    /// Returns eligible donors for the given blood request: compatible blood group (AC1), within
    /// <see cref="BloodRequest.SearchRadiusKm"/> of the request's registered coordinates (AC2/AC4),
    /// excluding suspended or receiver-only donors (AC3) and donors with no registered coordinates.
    /// Sorted by ascending distance. Never throws for zero matches — returns an empty list.
    /// </summary>
    /// <param name="request">The blood request to match donors against.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<IReadOnlyList<MatchedDonorResult>> FindEligibleDonorsAsync(BloodRequest request, CancellationToken ct);
}
