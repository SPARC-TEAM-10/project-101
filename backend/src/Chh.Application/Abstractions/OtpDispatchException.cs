namespace Chh.Application.Abstractions;

/// <summary>Raised when the SMS gateway fails to dispatch an OTP code. Maps to 502 Bad Gateway.</summary>
public class OtpDispatchException : ChhException
{
    private const string DispatchFailureMessage = "Could not send verification code, please try again";

    /// <summary>Creates the exception with the standard dispatch-failure message.</summary>
    public OtpDispatchException()
        : base(DispatchFailureMessage)
    {
    }

    /// <summary>Creates the exception wrapping the underlying SMS gateway failure.</summary>
    public OtpDispatchException(Exception innerException)
        : base(DispatchFailureMessage, innerException)
    {
    }
}
