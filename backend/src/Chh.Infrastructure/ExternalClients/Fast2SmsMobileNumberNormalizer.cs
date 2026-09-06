namespace Chh.Infrastructure.ExternalClients;

/// <summary>
/// Normalizes a mobile number to bare 10 digits for Fast2SMS's WhatsApp API. Unlike the SMS
/// bulkV2 route (where <c>OtpRequestRequestValidator</c> already guarantees exactly 10 digits),
/// callers of the WhatsApp route may pass a "+91"/"91"-prefixed or formatted number.
/// </summary>
internal static class Fast2SmsMobileNumberNormalizer
{
    private const int IndianMobileDigitCount = 10;
    private const string CountryCode = "91";

    /// <summary>Strips non-digit characters and a leading "+91"/"91" country code, if present.</summary>
    /// <param name="mobileNumber">The mobile number to normalize, in any common formatting.</param>
    public static string Normalize(string mobileNumber)
    {
        var digitsOnly = new string(mobileNumber.Where(char.IsDigit).ToArray());

        if (digitsOnly.Length == IndianMobileDigitCount + CountryCode.Length
            && digitsOnly.StartsWith(CountryCode, StringComparison.Ordinal))
        {
            digitsOnly = digitsOnly[CountryCode.Length..];
        }

        return digitsOnly;
    }
}
