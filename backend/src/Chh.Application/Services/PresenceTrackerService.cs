using Chh.Application.Contracts;
using Chh.Domain.Constants;

namespace Chh.Application.Services;

/// <inheritdoc cref="IPresenceTrackerService" />
public class PresenceTrackerService : IPresenceTrackerService
{
    private readonly IIndividualProfileRepository _individualProfileRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>Creates the service with its repository and unit-of-work dependencies.</summary>
    /// <param name="individualProfileRepository">Data layer for reading and persisting the profile.</param>
    /// <param name="unitOfWork">Persists the activity-timestamp update.</param>
    public PresenceTrackerService(IIndividualProfileRepository individualProfileRepository, IUnitOfWork unitOfWork)
    {
        _individualProfileRepository = individualProfileRepository;
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task TrackActivityAsync(string mobileNumber, CancellationToken ct)
    {
        var profile = await _individualProfileRepository.GetTrackedByMobileNumberAsync(mobileNumber, ct).ConfigureAwait(false);
        if (profile is null)
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;
        if (profile.LastActiveAtUtc is { } lastActive && now - lastActive < PresenceConstants.UpdateThrottle)
        {
            return;
        }

        profile.LastActiveAtUtc = now;
        await _unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);
    }
}
