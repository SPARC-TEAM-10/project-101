using Chh.Application.Dtos;

namespace Chh.Application.Contracts;

/// <summary>Logic layer for facility registration (CHH-78) — distinct from <see cref="IFacilityAdminService"/>'s moderation concern.</summary>
public interface IFacilityService
{
    /// <summary>Registers a new facility (hospital/blood-bank or NGO), pending System Admin verification.</summary>
    /// <param name="request">The registration details.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <exception cref="Chh.Application.Abstractions.FacilityAlreadyRegisteredException">A facility already exists for this license number.</exception>
    Task<FacilityDto> RegisterAsync(CreateFacilityRequest request, CancellationToken ct);
}
