using Chh.Domain.Entities;
using Chh.Domain.Enums;

namespace Chh.Application.Contracts;

/// <summary>Data layer for <see cref="Facility"/> (CHH-F07 Admin Command Center).</summary>
public interface IFacilityRepository
{
    /// <summary>
    /// Returns one page of facilities matching <paramref name="status"/>, ordered by
    /// <c>CreatedAtUtc</c> ascending (oldest registration first), plus the total matching count.
    /// Read-only — implementations must use <c>AsNoTracking()</c> (api-standards.md §6).
    /// </summary>
    /// <param name="status">Verification status to filter on.</param>
    /// <param name="page">1-based page number.</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<(IReadOnlyList<Facility> Items, int TotalCount)> GetByStatusAsync(
        FacilityVerificationStatus status, int page, int pageSize, CancellationToken ct);

    /// <summary>
    /// Returns the facility one of whose contacts has <paramref name="mobileNumber"/>, or
    /// <c>null</c> if no contact matches. Matches regardless of <see cref="Facility.VerificationStatus"/>
    /// (CHH-10 — role resolution is not gated on verification). Read-only — implementations must
    /// use <c>AsNoTracking()</c> (api-standards.md §6).
    /// </summary>
    /// <param name="mobileNumber">10-digit mobile number to match against <see cref="FacilityContact.Mobile"/>.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<Facility?> GetByContactMobileNumberAsync(string mobileNumber, CancellationToken ct);

    /// <summary>
    /// Returns the facility registered under <paramref name="licenseNumber"/> (read-only,
    /// untracked), or <c>null</c> if none exists (CHH-78 duplicate-registration check).
    /// </summary>
    /// <param name="licenseNumber">License number to match against <see cref="Facility.LicenseNumber"/>.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<Facility?> GetByLicenseNumberAsync(string licenseNumber, CancellationToken ct);

    /// <summary>Adds a new facility (with its contacts) to the context. Does not call <c>SaveChangesAsync</c>.</summary>
    /// <param name="facility">The facility to add.</param>
    /// <param name="ct">Cancellation token.</param>
    Task AddAsync(Facility facility, CancellationToken ct);
}
