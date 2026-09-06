namespace Chh.Application.Dtos;

/// <summary>Request body for <c>POST /api/v1/auth/otp/verify</c>.</summary>
public record OtpVerifyRequest
{
    /// <summary>Mobile number the OTP was issued to. Exactly 10 digits, numeric only.</summary>
    public required string MobileNumber { get; init; }

    /// <summary>The 6-digit code to verify.</summary>
    public required string OtpCode { get; init; }
}
