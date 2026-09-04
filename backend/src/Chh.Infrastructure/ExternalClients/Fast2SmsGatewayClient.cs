using System.Net.Http.Json;
using System.Text.Json;
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
/// Registered only when <c>Fast2Sms:ApiKey</c> is configured (see
/// <c>Chh.Api.Extensions.ServiceCollectionExtensions</c>) — falls back to
/// <see cref="LoggingSmsGatewayClient"/> otherwise, so local development doesn't spend paid
/// SMS credits by default.
/// </summary>
public class Fast2SmsGatewayClient : ISmsGatewayClient
{
    private const string RequestUri = "dev/bulkV2";

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
        var payload = new Fast2SmsOtpRequest(otpCode, "otp", mobileNumber);

        using var response = await _httpClient.PostAsJsonAsync(RequestUri, payload, ct).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        // Fast2SMS reports business-level failures as HTTP 200 with "return": false, so a 2xx
        // status code alone doesn't mean the OTP was actually dispatched. The response shape
        // otherwise varies (e.g. "message" is a string on some failures, an array on success),
        // so we only pick out the one field our logic depends on and log the raw body for the rest.
        var responseBody = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
        using var responseJson = JsonDocument.Parse(responseBody);
        var dispatched = responseJson.RootElement.TryGetProperty("return", out var returnProperty)
            && returnProperty.ValueKind == JsonValueKind.True;

        if (!dispatched)
        {
            _logger.LogWarning(
                "Fast2SMS reported dispatch failure for {MaskedMobileNumber}: {ResponseBody}",
                OtpConstants.MaskMobileNumber(mobileNumber), responseBody);
            throw new HttpRequestException("Fast2SMS reported a dispatch failure");
        }
    }

    private sealed record Fast2SmsOtpRequest(
        [property: JsonPropertyName("variables_values")] string VariablesValues,
        [property: JsonPropertyName("route")] string Route,
        [property: JsonPropertyName("numbers")] string Numbers);
}
