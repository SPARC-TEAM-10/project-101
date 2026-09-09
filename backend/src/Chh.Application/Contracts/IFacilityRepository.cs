using Chh.Application.Dtos;
using Chh.Domain.Entities;
using Chh.Domain.Enums;

namespace Chh.Application.Contracts;

/// <summary>Data layer for <see cref="Facility"/> (CHH-78/US-CHH-003-01 creation, CHH-F07 Admin Command Center query).</summary>
public interface IFacilityRepository
{
    /// <summary>Adds a new facility (with its contacts) to the context. Does not call <c>SaveChangesAsync</c>.</summary>
    /// <param name="facility">The facility to add.</param>
    /// <param name="ct">Cancellation token.</param>
    Task AddAsync(Facility facility, CancellationToken ct);

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

    /// <summary>
    /// Returns the facility for the given id, tracked by the context so mutations made to it
    /// (CHH-79's <c>LicenseDocumentUrl</c> update) are persisted on <c>SaveChangesAsync</c> — or
    /// <c>null</c> if none exists.
    /// </summary>
    /// <param name="id">The facility id.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<Facility?> GetTrackedByIdAsync(Guid id, CancellationToken ct);

    /// <summary>
    /// Returns every <see cref="FacilityVerificationStatus.Verified"/> facility matching
    /// <paramref name="request"/>'s <c>Q</c>/<c>Category</c> filters (CHH-82/US-CHH-001-01, Epic
    /// CHH-68) — unpaged, since a distance sort must run over the full filtered set before the
    /// Service layer applies paging. Never returns a non-Verified facility. Read-only —
    /// <c>AsNoTracking()</c> (api-standards.md §6).
    /// </summary>
    /// <param name="request">The search filters (paging fields are applied by the Service layer, not here).</param>
    /// <param name="ct">Cancellation token.</param>
    Task<IReadOnlyList<Facility>> SearchAsync(SearchFacilitiesRequest request, CancellationToken ct);

    /// <summary>
    /// Returns the facility for <paramref name="id"/> only if it is
    /// <see cref="FacilityVerificationStatus.Verified"/> (CHH-82/US-CHH-001-02, Epic CHH-68) — a
    /// Guest must not be able to fetch a pending/rejected facility's detail by guessing an id.
    /// <c>null</c> if no such facility exists or it isn't Verified. Read-only — <c>AsNoTracking()</c>
    /// (api-standards.md §6).
    /// </summary>
    /// <param name="id">The facility id.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<Facility?> GetVerifiedByIdAsync(Guid id, CancellationToken ct);
}
