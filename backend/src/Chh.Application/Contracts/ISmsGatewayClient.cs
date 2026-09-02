namespace Chh.Application.Contracts;

/// <summary>Provider-agnostic SMS gateway abstraction. The real provider (Twilio vs Firebase) is an open PRD question.</summary>
public interface ISmsGatewayClient
{
    /// <summary>Sends the given OTP code to the given mobile number.</summary>
    Task SendOtpAsync(string mobileNumber, string otpCode, CancellationToken ct);
}
