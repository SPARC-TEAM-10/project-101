using Chh.Application.Contracts;
using Chh.Application.Dtos;
using Chh.Domain.Constants;
using Chh.Domain.Entities;
using Chh.Domain.Enums;
using Chh.Domain.Utilities;

namespace Chh.Application.Services;

/// <summary>Proximity donor matching engine (US-CHH-004-02/CHH-80).</summary>
public class MatchingEngineService : IMatchingEngineService
{
    private readonly IIndividualProfileRepository _individualProfileRepository;

    /// <summary>Creates the service with its repository dependency.</summary>
    /// <param name="individualProfileRepository">Data layer for querying candidate donors.</param>
    public MatchingEngineService(IIndividualProfileRepository individualProfileRepository)
    {
        _individualProfileRepository = individualProfileRepository;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<MatchedDonorResult>> FindEligibleDonorsAsync(BloodRequest request, CancellationToken ct)
    {
        var compatibleGroups = BloodCompatibility.GetCompatibleDonorGroups(request.BloodGroup);
        var candidates = await _individualProfileRepository
            .GetActiveDonorsByBloodGroupsAsync(compatibleGroups, ct)
            .ConfigureAwait(false);

        var matches = new List<MatchedDonorResult>();
        foreach (var candidate in candidates)
        {
            // The repository query already filters AccountStatus/IsReceiverOnly/null-coordinates
            // at the DB level for efficiency — re-checked here too (AC3, Edge Case) so this
            // service's own eligibility rules hold regardless of which IIndividualProfileRepository
            // implementation supplies the candidates (e.g. a test double that doesn't pre-filter).
            if (candidate.AccountStatus != AccountStatus.Active || candidate.IsReceiverOnly)
            {
                continue;
            }

            if (candidate.Latitude is not { } donorLatitude || candidate.Longitude is not { } donorLongitude)
            {
                continue;
            }

            var distanceKm = HaversineDistanceCalculator.CalculateDistanceKm(
                request.Latitude, request.Longitude, donorLatitude, donorLongitude);

            if (distanceKm > request.SearchRadiusKm)
            {
                continue;
            }

            matches.Add(new MatchedDonorResult
            {
                DonorProfileId = candidate.Id,
                MobileNumber = candidate.MobileNumber,
                DistanceKm = distanceKm
            });
        }

        return matches.OrderBy(m => m.DistanceKm).ToList();
    }
}
