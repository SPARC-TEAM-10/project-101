using Chh.Application.Dtos;
using Chh.Domain.Entities;

namespace Chh.Application.Contracts;

/// <summary>
/// Dispatches a per-donor notification for each donor matched by <see cref="IMatchingEngineService"/>
/// (CHH-34), called from <c>Chh.Application.Jobs.MatchDonorsJob</c> right after matching completes.
/// </summary>
public interface INotificationDispatchService
{
    /// <summary>
    /// Notifies each matched donor: creates the in-app notification (AC1/AC2/AC3), skipping any
    /// donor already notified for this exact request (AC4), and additionally sends an SMS fallback
    /// (via <see cref="ISmsGatewayClient"/>) to donors who aren't currently active in-app.
    /// </summary>
    /// <param name="bloodRequest">The matched blood request.</param>
    /// <param name="matches">The eligible donors found by the matching engine, with their distances.</param>
    /// <param name="ct">Cancellation token.</param>
    Task DispatchAsync(BloodRequest bloodRequest, IReadOnlyList<MatchedDonorResult> matches, CancellationToken ct);
}
