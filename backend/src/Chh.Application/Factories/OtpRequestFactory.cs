using Chh.Domain.Constants;
using Chh.Domain.Entities;

namespace Chh.Application.Factories;

/// <summary>
/// Computes OTP expiry/resend-cooldown timestamps and constructs <see cref="OtpRequest"/> instances.
/// This construction logic lives here — not on the entity itself — per code-review guidance
/// (PR #1: "Do not write functions ... in entity models"); the same split applies to any future
/// entity that needs computed-at-creation fields.
/// </summary>
public static class OtpRequestFactory
{
    /// <summary>Creates a new OTP request, computing the expiry and resend-cooldown timestamps from <paramref name="requestedAtUtc"/>.</summary>
    /// <param name="mobileNumber">The 10-digit mobile number the OTP was issued for.</param>
    /// <param name="otpCodeHash">SHA-256 hex hash of the OTP code.</param>
    /// <param name="requestedAtUtc">UTC timestamp the OTP was requested.</param>
    public static OtpRequest Create(string mobileNumber, string otpCodeHash, DateTimeOffset requestedAtUtc)
    {
        return new OtpRequest(
            mobileNumber,
            otpCodeHash,
            requestedAtUtc,
            requestedAtUtc.Add(OtpConstants.OtpValidity),
            requestedAtUtc.Add(OtpConstants.ResendCooldown));
    }
}
