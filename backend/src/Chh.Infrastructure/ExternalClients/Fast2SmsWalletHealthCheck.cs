using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Chh.Infrastructure.ExternalClients;

/// <summary>
/// Background health check against Fast2SMS's wallet endpoint (<c>GET /dev/wallet</c>) — a
/// drained wallet fails OTP/WhatsApp sends silently from the user's point of view otherwise.
/// </summary>
/// <remarks>
/// Checks connectivity and logs the raw response rather than parsing a specific balance field:
/// the spec this was built from didn't include a sample response body, so the exact field name
/// and a sensible low-balance threshold need verifying against the live API (same category of
/// gap as the "open questions" already called out for the WhatsApp route) before this can safely
/// report Unhealthy on a real low-balance condition instead of just a connectivity failure.
/// </remarks>
public class Fast2SmsWalletHealthCheck : IHealthCheck
{
    private readonly HttpClient _httpClient;

    /// <summary>Creates the health check with its typed <see cref="HttpClient"/> (base address and auth header configured at registration).</summary>
    /// <param name="httpClient">Typed HTTP client pointed at the Fast2SMS API.</param>
    public Fast2SmsWalletHealthCheck(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken ct = default)
    {
        using var response = await _httpClient.GetAsync(Fast2SmsConstants.WalletRequestUri, ct).ConfigureAwait(false);
        var body = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

        return response.IsSuccessStatusCode
            ? HealthCheckResult.Healthy(body)
            : HealthCheckResult.Unhealthy($"Fast2SMS wallet check returned {(int)response.StatusCode}: {body}");
    }
}
