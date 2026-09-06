using System.Text.Json;

namespace Chh.Infrastructure.ExternalClients;

/// <summary>
/// Parses Fast2SMS's JSON response bodies. The success-flag field name differs by route — the SMS
/// bulkV2 route uses <c>"return"</c>, the WhatsApp route uses <c>"status"</c> — so callers pass
/// their own field name rather than this type hardcoding one. A non-2xx response isn't guaranteed
/// to be JSON at all (e.g. an upstream proxy error page), so a parse failure returns a graceful
/// default instead of throwing.
/// </summary>
internal static class Fast2SmsResponseParser
{
    /// <summary>Returns whether the named boolean property is present and <c>true</c>. Returns <c>false</c> if the body isn't valid JSON or the property is absent/not <c>true</c>.</summary>
    /// <param name="responseBody">The raw response body.</param>
    /// <param name="propertyName">The success-flag property name for this route (e.g. "return", "status").</param>
    public static bool TryGetBooleanProperty(string responseBody, string propertyName)
    {
        try
        {
            using var responseJson = JsonDocument.Parse(responseBody);
            return responseJson.RootElement.TryGetProperty(propertyName, out var property)
                && property.ValueKind == JsonValueKind.True;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    /// <summary>Returns the named string property's value, or <c>null</c> if the body isn't valid JSON or the property is absent/not a string.</summary>
    /// <param name="responseBody">The raw response body.</param>
    /// <param name="propertyName">The property name to read.</param>
    public static string? TryGetStringProperty(string responseBody, string propertyName)
    {
        try
        {
            using var responseJson = JsonDocument.Parse(responseBody);
            return responseJson.RootElement.TryGetProperty(propertyName, out var property)
                && property.ValueKind == JsonValueKind.String
                ? property.GetString()
                : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
