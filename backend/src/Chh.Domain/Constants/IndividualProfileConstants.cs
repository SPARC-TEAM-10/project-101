namespace Chh.Domain.Constants;

/// <summary>User-facing messages for CHH-F02 individual registration, centralized per the same convention as <see cref="OtpConstants"/>.</summary>
public static class IndividualProfileConstants
{
    /// <summary>User-facing message when registration is attempted for a mobile number with no verified OTP.</summary>
    public const string MobileNumberNotVerifiedMessage = "Please verify your mobile number with an OTP before registering";

    /// <summary>User-facing message when an individual profile already exists for the mobile number.</summary>
    public const string AlreadyRegisteredMessage = "An individual profile already exists for this mobile number";
}
