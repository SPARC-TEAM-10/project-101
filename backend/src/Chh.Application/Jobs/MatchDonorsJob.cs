using Chh.Application.Contracts;
using Microsoft.Extensions.Logging;

namespace Chh.Application.Jobs;

/// <summary>
/// Hangfire background job that runs the matching engine for a newly created blood request
/// (US-CHH-004-02/CHH-80), enqueued fire-and-forget by <see cref="Services.BloodRequestService"/>
/// so the creating endpoint returns immediately (api-standards.md §6 NFR). Also dispatches a
/// per-donor notification for each match (CHH-34) — in-app always, plus SMS for donors who aren't
/// currently active in-app.
/// </summary>
public class MatchDonorsJob
{
    private readonly IBloodRequestRepository _bloodRequestRepository;
    private readonly IMatchingEngineService _matchingEngineService;
    private readonly INotificationDispatchService _notificationDispatchService;
    private readonly ILogger<MatchDonorsJob> _logger;

    /// <summary>Creates the job with its dependencies.</summary>
    /// <param name="bloodRequestRepository">Loads the blood request to match against.</param>
    /// <param name="matchingEngineService">Computes the eligible donor list.</param>
    /// <param name="notificationDispatchService">Notifies each matched donor (CHH-34).</param>
    /// <param name="logger">Logs the match outcome.</param>
    public MatchDonorsJob(
        IBloodRequestRepository bloodRequestRepository,
        IMatchingEngineService matchingEngineService,
        INotificationDispatchService notificationDispatchService,
        ILogger<MatchDonorsJob> logger)
    {
        _bloodRequestRepository = bloodRequestRepository;
        _matchingEngineService = matchingEngineService;
        _notificationDispatchService = notificationDispatchService;
        _logger = logger;
    }

    /// <summary>
    /// Runs the matching engine for <paramref name="bloodRequestId"/>. A missing or already-expired
    /// request is logged and treated as a no-op — not an error, so Hangfire does not retry it.
    /// </summary>
    /// <param name="bloodRequestId">The blood request to match donors against.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task RunAsync(Guid bloodRequestId, CancellationToken ct)
    {
        var bloodRequest = await _bloodRequestRepository.GetByIdAsync(bloodRequestId, ct).ConfigureAwait(false);
        if (bloodRequest is null)
        {
            _logger.LogWarning("MatchDonorsJob: BloodRequest {BloodRequestId} not found — skipping.", bloodRequestId);
            return;
        }

        if (bloodRequest.ExpiresAtUtc <= DateTimeOffset.UtcNow)
        {
            _logger.LogInformation("MatchDonorsJob: BloodRequest {BloodRequestId} already expired — skipping.", bloodRequestId);
            return;
        }

        var matches = await _matchingEngineService.FindEligibleDonorsAsync(bloodRequest, ct).ConfigureAwait(false);

        _logger.LogInformation(
            "MatchDonorsJob: BloodRequest {BloodRequestId} matched {DonorCount} eligible donor(s) within {SearchRadiusKm}km.",
            bloodRequestId, matches.Count, bloodRequest.SearchRadiusKm);

        if (matches.Count > 0)
        {
            await _notificationDispatchService.DispatchAsync(bloodRequest, matches, ct).ConfigureAwait(false);
        }
    }
}
