using Chh.Application.Contracts;
using Microsoft.Extensions.Options;

namespace Chh.Infrastructure.ExternalClients;

/// <summary>
/// Adapts <see cref="IWhatsAppTemplateClient"/> to <see cref="ISmsGatewayClient"/> for OTP
/// dispatch: sends the OTP code we already generated and hashed (<c>OtpService</c>) as the single
/// variable of the approved OTP template. Registered when <c>Fast2Sms:Channel</c> is
/// <c>"whatsapp"</c> — see <c>Chh.Api.Extensions.ServiceCollectionExtensions</c>.
/// </summary>
public class Fast2SmsWhatsAppGatewayClient : ISmsGatewayClient
{
    private readonly IWhatsAppTemplateClient _templateClient;
    private readonly string _otpMessageId;

    /// <summary>Creates the client with its template-client dependency and bound options.</summary>
    /// <param name="templateClient">Sends the underlying WhatsApp template.</param>
    /// <param name="options">Bound <c>Fast2Sms:WhatsApp</c> configuration, for the OTP template ID.</param>
    public Fast2SmsWhatsAppGatewayClient(IWhatsAppTemplateClient templateClient, IOptions<Fast2SmsWhatsAppOptions> options)
    {
        _templateClient = templateClient;
        _otpMessageId = options.Value.OtpMessageId;
    }

    /// <inheritdoc />
    public async Task SendOtpAsync(string mobileNumber, string otpCode, CancellationToken ct) =>
        await _templateClient.SendTemplateAsync(_otpMessageId, mobileNumber, new[] { otpCode }, ct).ConfigureAwait(false);

    /// <inheritdoc />
    /// <exception cref="NotSupportedException">
    /// WhatsApp template messaging only supports pre-approved templates (like the OTP template
    /// above) — there's no approved template for CHH-34's free-text notification copy yet. Set
    /// <c>Fast2Sms:Channel</c> to <c>"sms"</c> to use <see cref="Fast2SmsGatewayClient"/> instead.
    /// </exception>
    public Task SendMessageAsync(string mobileNumber, string message, CancellationToken ct) =>
        throw new NotSupportedException(
            "Free-text messages aren't supported over the WhatsApp template channel — no approved template exists for this message. Use Fast2Sms:Channel=\"sms\" instead.");
}
