using Chh.Application.Dtos;

namespace Chh.Application.Contracts;

/// <summary>Logic layer for creating blood requests (CHH-33/US-CHH-004-01).</summary>
public interface IBloodRequestService
{
    /// <summary>Creates a new blood request for the given requester, transitioning it to "Matching" (AC1).</summary>
    /// <param name="requesterMobileNumber">The authenticated requester's mobile number (from the JWT "sub" claim).</param>
    /// <param name="request">The validated blood request details.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<BloodRequestDto> CreateAsync(string requesterMobileNumber, CreateBloodRequestRequest request, CancellationToken ct);

    /// <summary>Returns one page of the caller's own blood requests, newest first (CHH-81 Individual Dashboard).</summary>
    /// <param name="requesterMobileNumber">The authenticated requester's mobile number, from the JWT.</param>
    /// <param name="page">1-based page number.</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<PagedResponse<BloodRequestDto>> GetMyRequestsAsync(string requesterMobileNumber, int page, int pageSize, CancellationToken ct);

    /// <summary>
    /// Returns the caller's own request's match/response status (CHH-36 AC1/AC2/AC3) — donor
    /// identity is redacted per the guest-vs-registered visibility rule until a donor accepts.
    /// Returns <c>null</c> if the request doesn't exist or wasn't created by this caller.
    /// </summary>
    /// <param name="requesterMobileNumber">The authenticated requester's mobile number, from the JWT.</param>
    /// <param name="bloodRequestId">The blood request to look up.</param>
    /// <param name="isGuest">True if the caller's role is Guest — narrows the donor list to Accepted-only (design decision).</param>
    /// <param name="ct">Cancellation token.</param>
    Task<BloodRequestMatchStatusDto?> GetMatchStatusAsync(string requesterMobileNumber, Guid bloodRequestId, bool isGuest, CancellationToken ct);

    /// <summary>
    /// Expands the caller's own request's search radius and re-triggers the matching engine
    /// (CHH-36 AC4) — existing matches are unaffected (CHH-34's duplicate-prevention means only
    /// newly-in-range donors get a fresh notification). Returns <c>null</c> if the request doesn't
    /// exist or wasn't created by this caller.
    /// </summary>
    /// <param name="requesterMobileNumber">The authenticated requester's mobile number, from the JWT.</param>
    /// <param name="bloodRequestId">The blood request to update.</param>
    /// <param name="newRadiusKm">The new search radius — must be larger than the current one.</param>
    /// <param name="isGuest">True if the caller's role is Guest — see <see cref="GetMatchStatusAsync"/>.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <exception cref="Abstractions.BloodRequestNotMatchingException">The request is no longer Matching (fulfilled or expired).</exception>
    /// <exception cref="Abstractions.RadiusMustIncreaseException">The new radius isn't larger than the current one.</exception>
    Task<BloodRequestMatchStatusDto?> UpdateRadiusAsync(string requesterMobileNumber, Guid bloodRequestId, int newRadiusKm, bool isGuest, CancellationToken ct);
}
