using Chh.Application.Dtos;

namespace Chh.Application.Contracts;

/// <summary>Logic layer for event creation (CHH-38/US-CHH-005-01).</summary>
public interface IEventService
{
    /// <summary>
    /// Creates a new published event on behalf of the caller's facility.
    /// </summary>
    /// <param name="creatorMobileNumber">The authenticated creator's mobile number (from the JWT "sub" claim, never client-supplied).</param>
    /// <param name="request">The validated event creation details.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <exception cref="Chh.Application.Abstractions.FacilityNotVerifiedException">The caller's facility isn't Verified.</exception>
    Task<EventDto> CreateAsync(string creatorMobileNumber, CreateEventRequest request, CancellationToken ct);
}
