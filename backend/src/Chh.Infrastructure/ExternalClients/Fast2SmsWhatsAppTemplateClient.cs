using Chh.Application.Contracts;
using Chh.Domain.Constants;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Chh.Infrastructure.ExternalClients;

/// <summary>
/// <see cref="IWhatsAppTemplateClient"/> implementation backed by Fast2SMS's WhatsApp Message API
/// (<c>GET /dev/whatsapp</c>). Not subject to TRAI DLT registration, unlike the SMS bulkV2 route —
/// see <see cref="Fast2SmsGatewayClient"/>'s doc comment for why that route is currently blocked.
/// </summary>
/// <remarks>
/// This is deliberately NOT Fast2SMS's "Smart OTP" flow (<c>POST /dev/otp/send</c> +
/// <c>/dev/otp/verify</c>) — that has Fast2SMS generate the code itself, which would make
/// <c>OtpService</c>'s generation/hashing/storage/expiry dead code and move verification off our
/// servers entirely. This client only ever delivers a code <em>we</em> already generated, exactly
/// like the SMS route did.
/// </remarks>
public class Fast2SmsWhatsAppTemplateClient : IWhatsAppTemplateClient
{
    private readonly HttpClient _httpClient;
    private readonly string _phoneNumberId;
    private readonly ILogger<Fast2SmsWhatsAppTemplateClient> _logger;

    /// <summary>Creates the client with its typed <see cref="HttpClient"/> (base address and auth header configured at registration), bound options, and logger dependencies.</summary>
    /// <param name="httpClient">Typed HTTP client pointed at the Fast2SMS API.</param>
    /// <param name="options">Bound <c>Fast2Sms:WhatsApp</c> configuration.</param>
    /// <param name="logger">Logger for dispatch-failure diagnostics.</param>
    public Fast2SmsWhatsAppTemplateClient(
        HttpClient httpClient,
        IOptions<Fast2SmsWhatsAppOptions> options,
        ILogger<Fast2SmsWhatsAppTemplateClient> logger)
    {
        _httpClient = httpClient;
        _phoneNumberId = options.Value.PhoneNumberId;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<string> SendTemplateAsync(
        string messageId, string mobileNumber, IReadOnlyList<string> variableValues, CancellationToken ct)
    {
        foreach (var value in variableValues)
        {
            if (value.Contains('|'))
            {
                // A stray separator silently shifts every subsequent variable into the wrong
                // template slot — a *delivered* message with values in the wrong places, which
                // will not fail loudly. Reject before the request goes anywhere near the wire.
                throw new ArgumentException(
                    "WhatsApp template variable values must not contain '|' (the wire separator).",
                    nameof(variableValues));
            }
        }

        var query = new Dictionary<string, string?>
        {
            ["message_id"] = "1252891671245102",
            ["phone_number_id"] = "1252891671245102",
            ["numbers"] = Fast2SmsMobileNumberNormalizer.Normalize(mobileNumber),
            ["variables_values"] = string.Join('|', variableValues)
        };
        // QueryHelpers percent-encodes each value (the '|' separator becomes %7C) — string
        // concatenation would either throw or silently mangle it depending on the HttpClient version.
        var requestUri = QueryHelpers.AddQueryString(Fast2SmsConstants.WhatsAppRequestUri, query);

        using var response = await _httpClient.GetAsync(requestUri, ct).ConfigureAwait(false);

        // Same shape caveat as the SMS route (see Fast2SmsGatewayClient): a non-2xx status isn't
        // guaranteed to be JSON, and a 2xx status doesn't guarantee the send actually succeeded —
        // this route's success flag is "status", not "return" (Fast2SmsResponseParser takes the
        // field name per call precisely because the two routes disagree on it).
        var responseBody = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
        var dispatched = response.IsSuccessStatusCode
            && Fast2SmsResponseParser.TryGetBooleanProperty(responseBody, "status");

        if (!dispatched)
        {
            _logger.LogWarning(
                "Fast2SMS WhatsApp reported dispatch failure ({StatusCode}) for {MaskedMobileNumber}: {ResponseBody}",
                (int)response.StatusCode, OtpConstants.MaskMobileNumber(mobileNumber), responseBody);
            throw new HttpRequestException(
                $"Fast2SMS WhatsApp reported a dispatch failure ({(int)response.StatusCode}): {responseBody}");
        }

        var requestId = Fast2SmsResponseParser.TryGetStringProperty(responseBody, "request_id") ?? string.Empty;
        _logger.LogInformation(
            "Fast2SMS WhatsApp dispatched to {MaskedMobileNumber}, request_id={RequestId}",
            OtpConstants.MaskMobileNumber(mobileNumber), requestId);
        return requestId;
    }
}
