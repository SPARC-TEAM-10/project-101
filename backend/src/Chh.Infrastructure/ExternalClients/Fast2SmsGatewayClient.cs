using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Chh.Application.Contracts;
using Chh.Domain.Constants;
using Microsoft.Extensions.Logging;

namespace Chh.Infrastructure.ExternalClients;

/// <summary>
/// <see cref="ISmsGatewayClient"/> implementation backed by the Fast2SMS "OTP" route
/// (<c>POST /dev/bulkV2</c>, <c>route=otp</c>). We generate and hash the OTP code ourselves
/// (<c>OtpService</c>) and pass it via <c>variables_values</c> for Fast2SMS to substitute into
/// its DLT-approved OTP template — we deliberately do NOT use Fast2SMS's own auto-generating
/// "Smart OTP" flow, since that would text a different code than the one hashed and stored.
/// </summary>
/// <remarks>
/// Currently unused in practice — this account has no TRAI DLT registration, so every call fails
/// with Fast2SMS status_code 996. Kept (not deleted) for when DLT registration completes, and as
/// a fallback if <see cref="Fast2SmsWhatsAppGatewayClient"/> (the current OTP channel — see its
/// doc comment) proves unreliable. Registered only when <c>Fast2Sms:Channel</c> is <c>"sms"</c>
/// (see <c>Chh.Api.Extensions.ServiceCollectionExtensions</c>); falls back to
/// <see cref="LoggingSmsGatewayClient"/> when no API key is configured at all.
/// </remarks>
public class Fast2SmsGatewayClient : ISmsGatewayClient
{
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
    public async Task SendOtpAsync(string mobileNumber, string otpCode, CancellationToken ct)
    {
        var payload = new Fast2SmsOtpRequest(otpCode, Fast2SmsConstants.OtpRoute, mobileNumber);

        using var response = await _httpClient.PostAsJsonAsync(Fast2SmsConstants.RequestUri, payload, ct).ConfigureAwait(false);

        // Fast2SMS reports business-level failures as HTTP 200 with "return": false, so a 2xx
        // status code alone doesn't mean the OTP was actually dispatched. The response shape
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

    private sealed record Fast2SmsOtpRequest(
        [property: JsonPropertyName("variables_values")] string VariablesValues,
        [property: JsonPropertyName("route")] string Route,
        [property: JsonPropertyName("numbers")] string Numbers);
}
