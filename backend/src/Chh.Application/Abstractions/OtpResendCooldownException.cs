namespace Chh.Application.Abstractions;

/// <summary>Raised when an OTP resend is requested before the resend cooldown has elapsed. Maps to 429 Too Many Requests.</summary>
public class OtpResendCooldownException : ChhException
{
    /// <summary>Creates the exception with the standard resend-cooldown message.</summary>
    public OtpResendCooldownException()
        : base("Please wait before requesting another OTP")
    {
    }
}
