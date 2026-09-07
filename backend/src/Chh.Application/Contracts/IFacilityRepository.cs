using Chh.Domain.Entities;

namespace Chh.Application.Contracts;

/// <summary>Data layer for <see cref="Facility"/> (CHH-78/US-CHH-003-01).</summary>
public interface IFacilityRepository
{
    /// <summary>Adds a new facility (with its contacts) to the context. Does not call <c>SaveChangesAsync</c>.</summary>
    /// <param name="facility">The facility to add.</param>
    /// <param name="ct">Cancellation token.</param>
    Task AddAsync(Facility facility, CancellationToken ct);
}
