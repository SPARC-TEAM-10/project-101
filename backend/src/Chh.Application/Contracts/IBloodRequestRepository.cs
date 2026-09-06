using Chh.Domain.Entities;

namespace Chh.Application.Contracts;

/// <summary>Data layer for <see cref="BloodRequest"/> (CHH-33/US-CHH-004-01).</summary>
public interface IBloodRequestRepository
{
    /// <summary>Adds a new blood request to the context. Does not call <c>SaveChangesAsync</c>.</summary>
    /// <param name="bloodRequest">The blood request to add.</param>
    /// <param name="ct">Cancellation token.</param>
    Task AddAsync(BloodRequest bloodRequest, CancellationToken ct);
}
