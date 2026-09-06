using Chh.Application.Dtos;

namespace Chh.Application.Contracts;

/// <summary>Logic layer for individual registration (CHH-F02).</summary>
public interface IIndividualProfileService
{
    /// <summary>Registers a new individual profile for an OTP-verified mobile number.</summary>
    /// <param name="request">The registration details.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <exception cref="Chh.Application.Abstractions.MobileNumberNotVerifiedException">The mobile number has no verified OTP.</exception>
    /// <exception cref="Chh.Application.Abstractions.IndividualAlreadyRegisteredException">A profile already exists for this mobile number.</exception>
    Task<IndividualProfileDto> RegisterAsync(CreateIndividualProfileRequest request, CancellationToken ct);
}
