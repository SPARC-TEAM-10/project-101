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

    /// <summary>
    /// Verifies a submitted OTP code against the most recently issued OTP for the mobile number,
    /// and on success issues a JWT access token (CHH-F01 AC3) with a role claim resolved from
    /// whether a completed individual registration exists for this number.
    /// </summary>
    /// <param name="request">The mobile number and OTP code to verify.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <exception cref="Chh.Application.Abstractions.InvalidOtpException">The code is wrong, or none was requested for this number.</exception>
    /// <exception cref="Chh.Application.Abstractions.OtpExpiredException">The code matches a request that has since expired.</exception>
    Task<OtpVerifyResponse> VerifyOtpAsync(OtpVerifyRequest request, CancellationToken ct);
}
