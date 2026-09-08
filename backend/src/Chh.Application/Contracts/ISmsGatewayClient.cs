namespace Chh.Application.Contracts;

/// <summary>Provider-agnostic SMS gateway abstraction. The real provider (Twilio vs Firebase) is an open PRD question.</summary>
public interface ISmsGatewayClient
{
    /// <summary>Sends the given OTP code to the given mobile number.</summary>
    /// <param name="mobileNumber">The mobile number to send the OTP to.</param>
    /// <param name="otpCode">The plaintext OTP code to send. Never logged.</param>
    /// <param name="ct">Cancellation token.</param>
    Task SendOtpAsync(string mobileNumber, string otpCode, CancellationToken ct);

    /// <summary>
    /// Sends a free-text message to the given mobile number (CHH-34 SMS notification fallback) —
    /// distinct from <see cref="SendOtpAsync"/> since the message isn't a fixed OTP template.
    /// </summary>
    /// <param name="mobileNumber">The mobile number to send the message to.</param>
    /// <param name="message">The plaintext message body. Caller is responsible for length/content rules (e.g. under 160 characters).</param>
    /// <param name="ct">Cancellation token.</param>
    Task SendMessageAsync(string mobileNumber, string message, CancellationToken ct);
}
