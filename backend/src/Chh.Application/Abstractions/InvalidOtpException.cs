using Chh.Domain.Constants;

namespace Chh.Application.Abstractions;

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
