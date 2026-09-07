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
}
