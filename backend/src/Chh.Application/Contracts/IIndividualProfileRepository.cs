using Chh.Domain.Entities;

namespace Chh.Application.Contracts;

/// <summary>Data layer for <see cref="IndividualProfile"/>.</summary>
public interface IIndividualProfileRepository
{
    /// <summary>Returns the individual profile for the given mobile number (read-only, untracked), or <c>null</c> if none exists.</summary>
    /// <param name="mobileNumber">The mobile number to look up.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<IndividualProfile?> GetByMobileNumberAsync(string mobileNumber, CancellationToken ct);

    /// <summary>Adds a new individual profile to the context. Does not call <c>SaveChangesAsync</c>.</summary>
    /// <param name="individualProfile">The individual profile to add.</param>
    /// <param name="ct">Cancellation token.</param>
    Task AddAsync(IndividualProfile individualProfile, CancellationToken ct);
}
