using Chh.Application.Contracts;
using Chh.Domain.Constants;
using Microsoft.Extensions.Logging;

namespace Chh.Infrastructure.ExternalClients;

/// <summary>
/// <see cref="ISmsGatewayClient"/> implementation backed by Fast2SMS's "Quick SMS" route
/// (<c>GET /dev/bulkV2?route=q</c>) — a free-text message with no DLT template registration
/// required (see <see cref="Fast2SmsConstants.QuickSmsRoute"/>). We generate and hash the OTP
/// code ourselves (<c>OtpService</c>) and embed it directly in the message text; we deliberately
/// do NOT use Fast2SMS's own auto-generating "Smart OTP" endpoints, since those would text a
/// different code than the one already hashed and stored.
/// </summary>
/// <remarks>
/// Registered only when <c>Fast2Sms:Channel</c> is <c>"sms"</c> (see
/// <c>Chh.Api.Extensions.ServiceCollectionExtensions</c>); falls back to
/// <see cref="LoggingSmsGatewayClient"/> when no API key is configured at all.
/// </remarks>
public class Fast2SmsGatewayClient : ISmsGatewayClient
{
    private const string MessageTemplate =
        "Your Community Health Hub verification code is {0}. It expires in 5 minutes. Do not share this code with anyone.";

    private readonly HttpClient _httpClient;
    private readonly ILogger<Fast2SmsGatewayClient> _logger;

    /// <summary>Creates the client with its typed <see cref="HttpClient"/> (base address and auth header configured at registration) and logger dependencies.</summary>
    /// <param name="httpClient">Typed HTTP client pointed at the Fast2SMS API.</param>
    /// <param name="logger">Logger for dispatch-failure diagnostics.</param>
    public Fast2SmsGatewayClient(HttpClient httpClient, ILogger<Fast2SmsGatewayClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task SendOtpAsync(string mobileNumber, string otpCode, CancellationToken ct) =>
        await SendAsync(mobileNumber, string.Format(MessageTemplate, otpCode), ct).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task SendMessageAsync(string mobileNumber, string message, CancellationToken ct) =>
        await SendAsync(mobileNumber, message, ct).ConfigureAwait(false);

    private async Task SendAsync(string mobileNumber, string messageBody, CancellationToken ct)
    {
        var message = Uri.EscapeDataString(messageBody);
        var requestUri = $"{Fast2SmsConstants.RequestUri}?route={Fast2SmsConstants.QuickSmsRoute}&message={message}&numbers={mobileNumber}";

        using var response = await _httpClient.GetAsync(requestUri, ct).ConfigureAwait(false);

        // Fast2SMS reports business-level failures as HTTP 200 with "return": false, so a 2xx
        // status code alone doesn't mean the message was actually dispatched. The response shape
        // otherwise varies (e.g. "message" is a string on some failures, an array on success),
        // so we only pick out the one field our logic depends on and log the raw body for the rest.
        var responseBody = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
        var dispatched = response.IsSuccessStatusCode
            && Fast2SmsResponseParser.TryGetBooleanProperty(responseBody, "return");

        if (!dispatched)
        {
            _logger.LogWarning(
                "Fast2SMS reported dispatch failure ({StatusCode}) for {MaskedMobileNumber}: {ResponseBody}",
                (int)response.StatusCode, OtpConstants.MaskMobileNumber(mobileNumber), responseBody);
            throw new HttpRequestException(
                $"Fast2SMS reported a dispatch failure ({(int)response.StatusCode}): {responseBody}");
        }
    }
}
