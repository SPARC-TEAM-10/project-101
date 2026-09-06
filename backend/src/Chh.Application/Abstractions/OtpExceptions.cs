using Chh.Domain.Constants;

namespace Chh.Application.Abstractions;

// OTP-related domain exceptions (CHH-8/CHH-9), grouped in one file since each is a thin,
// single-message wrapper — kept as distinct types (not consolidated into one class) because
// Chh.Api.Extensions.ProblemDetailsServiceCollectionExtensions maps each type to a different
// HTTP status via Hellang's type-based dispatch.

/// <summary>Raised when an OTP resend is requested before the resend cooldown has elapsed. Maps to 429 Too Many Requests.</summary>
public class OtpResendCooldownException : ChhException
{
    /// <summary>Creates the exception with the standard resend-cooldown message.</summary>
    public OtpResendCooldownException()
        : base(OtpConstants.ResendCooldownMessage)
    {
    }
}

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

/// <summary>
/// Raised when a submitted OTP code is wrong, expired, or none was ever requested for the
/// mobile number. Maps to 422 Unprocessable Entity. Deliberately used for all three cases
/// (see <see cref="OtpConstants.InvalidOtpMessage"/>) so the response can't be used to enumerate
/// which mobile numbers have a pending OTP.
/// </summary>
public class InvalidOtpException : ChhException
{
    /// <summary>Creates the exception with the standard invalid-OTP message.</summary>
    public InvalidOtpException()
        : base(OtpConstants.InvalidOtpMessage)
    {
    }
}
