namespace Chh.Infrastructure.ExternalClients;

/// <summary>Fixed Fast2SMS API contract details (not environment-specific, so not configuration — see <c>Fast2Sms:BaseUrl</c> in appsettings.json for the part that is).</summary>
public static class Fast2SmsConstants
{
    /// <summary>Relative path of the Fast2SMS bulk-send (SMS) endpoint. Currently blocked pending TRAI DLT registration — see <see cref="Fast2SmsGatewayClient"/>.</summary>
    public const string RequestUri = "dev/bulkV2";

    /// <summary>Fast2SMS route selecting the DLT-approved OTP template (bring-your-own-code mode).</summary>
    public const string OtpRoute = "otp";

    /// <summary>Relative path of the Fast2SMS WhatsApp Message API endpoint. Not subject to DLT — the current OTP channel.</summary>
    public const string WhatsAppRequestUri = "dev/whatsapp";

    /// <summary>Relative path of the Fast2SMS wallet-balance endpoint.</summary>
    public const string WalletRequestUri = "dev/wallet";

    /// <summary>Header name Fast2SMS expects the API key under.</summary>
    public const string AuthorizationHeaderName = "authorization";
}
