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
}
