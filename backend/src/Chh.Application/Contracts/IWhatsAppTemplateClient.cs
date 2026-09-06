namespace Chh.Application.Contracts;

/// <summary>
/// Sends an approved WhatsApp Business template via the configured provider. General-purpose —
/// OTP dispatch (<see cref="ISmsGatewayClient"/>) and any future template-based notification
/// (e.g. a donor-request broadcast) both go through this, one approved template at a time.
/// </summary>
public interface IWhatsAppTemplateClient
{
    /// <summary>Sends an approved WhatsApp template.</summary>
    /// <param name="messageId">The approved template ID.</param>
    /// <param name="mobileNumber">Recipient mobile number.</param>
    /// <param name="variableValues">
    /// Template variables, in the exact order the approved template defines them. A value
    /// containing the wire separator ('|') would silently shift every subsequent variable into
    /// the wrong slot, so implementations must reject that before sending.
    /// </param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The provider's request ID, for delivery-report correlation.</returns>
    Task<string> SendTemplateAsync(string messageId, string mobileNumber, IReadOnlyList<string> variableValues, CancellationToken ct);
}
