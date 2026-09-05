using Chh.Domain.Constants;

namespace Chh.Application.Abstractions;

/// <summary>Raised when the SMS gateway fails to dispatch an OTP code. Maps to 502 Bad Gateway.</summary>
public class OtpDispatchException : ChhException
{
    /// <summary>Creates the exception with the standard dispatch-failure message.</summary>
    public OtpDispatchException()
        : base(OtpConstants.DispatchFailureMessage)
    {
    }

    /// <summary>Creates the exception wrapping the underlying SMS gateway failure.</summary>
    /// <param name="innerException">The SMS gateway failure that caused the dispatch to fail.</param>
    public OtpDispatchException(Exception innerException)
        : base(OtpConstants.DispatchFailureMessage, innerException)
    {
    }
}
