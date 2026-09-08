using Chh.Application.Dtos;

namespace Chh.Application.Contracts;

/// <summary>Logic layer for facility registration (CHH-78/US-CHH-003-01).</summary>
public interface IFacilityService
{
    /// <summary>Creates a new facility registration in "Pending" status.</summary>
    /// <param name="createdByMobileNumber">The authenticated creator's mobile number (from the JWT "sub" claim, never client-supplied).</param>
    /// <param name="request">The validated facility registration details.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<FacilityDto> CreateAsync(string createdByMobileNumber, CreateFacilityRequest request, CancellationToken ct);

    /// <summary>
    /// Returns the facility owned by <paramref name="mobileNumber"/> (CHH-28's status dashboard),
    /// or <c>null</c> if that mobile number isn't a contact on any facility.
    /// </summary>
    /// <param name="mobileNumber">The authenticated caller's mobile number (from the JWT "sub" claim).</param>
    /// <param name="ct">Cancellation token.</param>
    Task<FacilityDto?> GetMyFacilityAsync(string mobileNumber, CancellationToken ct);
}
