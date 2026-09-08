using Chh.Domain.Entities;
using Chh.Domain.Enums;

namespace Chh.Application.Contracts;

/// <summary>Data layer for <see cref="IndividualProfile"/>.</summary>
public interface IIndividualProfileRepository
{
    /// <summary>Returns the individual profile for the given mobile number (read-only, untracked), or <c>null</c> if none exists.</summary>
    /// <param name="mobileNumber">The mobile number to look up.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<IndividualProfile?> GetByMobileNumberAsync(string mobileNumber, CancellationToken ct);

    /// <summary>
    /// Returns the individual profile for the given mobile number, tracked by the context so
    /// mutations made to it are persisted on <c>SaveChangesAsync</c> — or <c>null</c> if none exists.
    /// </summary>
    /// <param name="mobileNumber">The mobile number to look up.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<IndividualProfile?> GetTrackedByMobileNumberAsync(string mobileNumber, CancellationToken ct);

    /// <summary>Adds a new individual profile to the context. Does not call <c>SaveChangesAsync</c>.</summary>
    /// <param name="individualProfile">The individual profile to add.</param>
    /// <param name="ct">Cancellation token.</param>
    Task AddAsync(IndividualProfile individualProfile, CancellationToken ct);

    /// <summary>
    /// Returns candidate donors (read-only, untracked) for proximity matching
    /// (US-CHH-004-02/CHH-80): active, not receiver-only, with a registered blood group in
    /// <paramref name="bloodGroups"/> and non-null registered coordinates. Distance filtering
    /// against a specific request's radius happens in <see cref="IMatchingEngineService"/>, not here.
    /// </summary>
    /// <param name="bloodGroups">The compatible donor blood groups to match against.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<IReadOnlyList<IndividualProfile>> GetActiveDonorsByBloodGroupsAsync(IReadOnlySet<BloodGroup> bloodGroups, CancellationToken ct);
}
