using Chh.Application.Dtos;

namespace Chh.Application.Contracts;

/// <summary>Logic layer for System Admin user moderation (CHH-76/US-CHH-001-04, Admin Command Center).</summary>
public interface IUserAdminService
{
    /// <summary>
    /// Returns one page of individual profiles, optionally filtered by mobile number or name (UI
    /// Notes: "Search bar to find users by mobile number or name").
    /// </summary>
    /// <param name="search">Free-text search term; null/empty returns every profile.</param>
    /// <param name="page">1-based page number.</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<PagedResponse<AdminUserDto>> SearchUsersAsync(string? search, int page, int pageSize, CancellationToken ct);

    /// <summary>
    /// Suspends a user account (AC1) — sets <c>AccountStatus</c> to Suspended with the given
    /// reason. Returns <c>null</c> if no profile exists with the given id. Idempotent: suspending
    /// an already-suspended account just updates the reason.
    /// </summary>
    /// <param name="userId">The individual profile being suspended.</param>
    /// <param name="reason">Mandatory reason for the suspension.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<AdminUserDto?> SuspendUserAsync(Guid userId, string reason, CancellationToken ct);
}
