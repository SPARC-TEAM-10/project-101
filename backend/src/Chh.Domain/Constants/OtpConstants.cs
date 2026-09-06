namespace Chh.Domain.Constants;

/// <summary>
/// OTP-related constants (code shape, masking, validity/cooldown windows, user-facing messages)
/// shared across Domain, Application, and Infrastructure. Centralized here — rather than
/// duplicated as private consts in each class that needs them — per code-review guidance
/// (PR #1: "keep constants in a separate file").
/// </summary>
public static class OtpConstants
{
    /// <summary>Number of digits in a generated OTP code.</summary>
    public const int CodeLength = 6;

    /// <summary>Number of trailing mobile-number digits left unmasked in logs and responses (api-standards.md §8).</summary>
    public const int MaskedVisibleDigits = 2;

    /// <summary>Character used to mask hidden mobile-number digits.</summary>
    public const char MaskChar = '*';

    /// <summary>Regex an OTP-eligible mobile number must match: exactly 10 digits.</summary>
    public const string MobileNumberPattern = "^[0-9]{10}$";

    /// <summary>Regex an OTP code must match: exactly 6 digits.</summary>
    public const string OtpCodePattern = "^[0-9]{6}$";

    /// <summary>User-facing message for an invalid mobile number.</summary>
    public const string InvalidMobileNumberMessage = "Please enter a valid 10-digit mobile number";

    /// <summary>User-facing message for a malformed OTP code (wrong length/non-numeric).</summary>
    public const string InvalidOtpCodeShapeMessage = "Please enter the 6-digit code";

    /// <summary>
    /// User-facing message when a submitted OTP code is wrong, or none was ever requested for
    /// the mobile number. Deliberately the same message for both cases — distinguishing "wrong
    /// code" from "never requested" would let a caller enumerate valid mobile numbers. An expired
    /// OTP is reported separately via <see cref="OtpExpiredMessage"/> since, once a matching OTP
    /// record is confirmed to exist, expiry is unambiguous and safe to surface distinctly.
    /// </summary>
    public const string InvalidOtpMessage = "Invalid OTP. Please try again.";

    /// <summary>User-facing message when a submitted OTP code matches a request that has since expired.</summary>
    public const string OtpExpiredMessage = "This OTP has expired. Please request a new one.";

    /// <summary>User-facing message when the SMS gateway fails to dispatch an OTP.</summary>
    public const string DispatchFailureMessage = "Could not send verification code, please try again";

    /// <summary>User-facing message when a resend is requested before the cooldown elapses.</summary>
    public const string ResendCooldownMessage = "Please wait before requesting another OTP";

    /// <summary>How long an issued OTP code remains valid.</summary>
    public static readonly TimeSpan OtpValidity = TimeSpan.FromMinutes(5);

    /// <summary>Minimum wait between an OTP request and the next allowed resend.</summary>
    public static readonly TimeSpan ResendCooldown = TimeSpan.FromSeconds(120);

    /// <summary>Masks a mobile number, leaving only the last <see cref="MaskedVisibleDigits"/> digits visible (e.g. "********10"). Shared by every place that logs or returns a mobile number (api-standards.md §8).</summary>
    /// <param name="mobileNumber">The mobile number to mask.</param>
    public static string MaskMobileNumber(string mobileNumber)
    {
        if (mobileNumber.Length <= MaskedVisibleDigits)
        {
            return new string(MaskChar, mobileNumber.Length);
        }

        var visible = mobileNumber[^MaskedVisibleDigits..];
        var maskedLength = mobileNumber.Length - MaskedVisibleDigits;
        return new string(MaskChar, maskedLength) + visible;
    }
}
