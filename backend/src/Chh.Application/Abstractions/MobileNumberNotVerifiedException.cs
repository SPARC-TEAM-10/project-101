namespace Chh.Application.Abstractions;

/// <summary>
/// Raised when registration is attempted for a mobile number that has no verified OTP (CHH-9).
/// Maps to 422 Unprocessable Entity.
/// </summary>
public class MobileNumberNotVerifiedException : ChhException
{
    /// <summary>Creates the exception with the standard not-verified message.</summary>
    public MobileNumberNotVerifiedException()
        : base("Please verify your mobile number with an OTP before registering")
    {
    }
}
