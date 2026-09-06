namespace Chh.Application.Dtos;

/// <summary>Response body for <c>POST /api/v1/auth/otp/verify</c>.</summary>
public record OtpVerifyResponse
{
    /// <summary>Mobile number with all but the last 2 digits masked, e.g. "********10".</summary>
    public required string MaskedMobileNumber { get; init; }

    /// <summary>UTC timestamp the OTP was successfully verified.</summary>
    public required DateTimeOffset VerifiedAtUtc { get; init; }

    /// <summary>
    /// Signed JWT session token (CHH-F01 AC3). Send as <c>Authorization: Bearer &lt;accessToken&gt;</c>
    /// on subsequent requests.
    /// </summary>
    public required string AccessToken { get; init; }

    /// <summary>UTC expiry of <see cref="AccessToken"/> (1 hour from issuance per CHH-F01 AC3).</summary>
    public required DateTimeOffset TokenExpiresAtUtc { get; init; }

    /// <summary>
    /// Role assigned for this session (PRD §4 Role &amp; Permission Matrix) — "Individual" if a
    /// completed registration exists for this mobile number (CHH-F02), otherwise "Guest".
    /// </summary>
    public required string Role { get; init; }
}
