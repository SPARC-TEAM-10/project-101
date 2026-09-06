namespace Chh.Application.Dtos;

/// <summary>Request body for <c>POST /api/v1/auth/otp/request</c>.</summary>
public record OtpRequestRequest
{
    /// <summary>Mobile number to send the OTP to. Exactly 10 digits, numeric only.</summary>
    public required string MobileNumber { get; init; }
}
