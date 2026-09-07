namespace Chh.Infrastructure.ExternalClients;

/// <summary>Fixed Fast2SMS API contract details (not environment-specific, so not configuration — see <c>Fast2Sms:BaseUrl</c> in appsettings.json for the part that is).</summary>
public static class Fast2SmsConstants
{
    /// <summary>Relative path of the Fast2SMS bulk-send (SMS) endpoint.</summary>
    public const string RequestUri = "dev/bulkV2";

    /// <summary>
    /// Fast2SMS "Quick SMS" route — sends a free-text message with no DLT template registration
    /// required (₹5/SMS via a randomly assigned numeric sender ID). Used instead of the "otp"
    /// route (which needs this account's own DLT-approved template, not yet registered) and
    /// instead of Fast2SMS's auto-generating "Smart OTP" endpoints (which would text a different
    /// code than the one <c>OtpService</c> already hashed and stored) — see
    /// <see cref="Fast2SmsGatewayClient"/>.
    /// </summary>
    public const string QuickSmsRoute = "q";

    /// <summary>Relative path of the Fast2SMS WhatsApp Message API endpoint. Not subject to DLT — the current OTP channel.</summary>
    public const string WhatsAppRequestUri = "dev/whatsapp";

    /// <summary>Relative path of the Fast2SMS wallet-balance endpoint.</summary>
    public const string WalletRequestUri = "dev/wallet";

    /// <summary>Header name Fast2SMS expects the API key under.</summary>
    public const string AuthorizationHeaderName = "authorization";
}
