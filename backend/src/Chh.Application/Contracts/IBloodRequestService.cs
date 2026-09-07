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
}
