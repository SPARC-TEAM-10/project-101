namespace Chh.Application.Dtos;

/// <summary>Response body for <c>POST /api/v1/auth/otp/request</c>.</summary>
public record OtpRequestResponse
{
    /// <summary>Mobile number with all but the last 2 digits masked, e.g. "********10".</summary>
    public required string MaskedMobileNumber { get; init; }

    /// <summary>UTC timestamp the OTP code expires.</summary>
    public required DateTimeOffset OtpExpiresAtUtc { get; init; }

    /// <summary>UTC timestamp a resend becomes available.</summary>
    public required DateTimeOffset ResendAvailableAtUtc { get; init; }
}
