using Chh.Application.Dtos;

namespace Chh.Application.Contracts;

/// <summary>Logic layer for issuing OTP codes.</summary>
public interface IOtpService
{
    /// <summary>Generates, persists, and dispatches a new OTP for the given mobile number.</summary>
    /// <param name="request">The mobile number to send the OTP to.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <exception cref="Chh.Application.Abstractions.OtpResendCooldownException">A resend was requested before the cooldown elapsed.</exception>
    /// <exception cref="Chh.Application.Abstractions.OtpDispatchException">The SMS gateway failed to dispatch the code.</exception>
    Task<OtpRequestResponse> RequestOtpAsync(OtpRequestRequest request, CancellationToken ct);
}
