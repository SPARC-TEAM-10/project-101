namespace Chh.Application.Dtos;

/// <summary>Response body for <c>POST /api/v1/auth/otp/verify</c>.</summary>
public record OtpVerifyResponse
{
    /// <summary>Mobile number with all but the last 2 digits masked, e.g. "********10".</summary>
    public required string MaskedMobileNumber { get; init; }

    /// <summary>UTC timestamp the OTP was successfully verified.</summary>
    public required DateTimeOffset VerifiedAtUtc { get; init; }
}
