using Chh.Domain.Constants;

namespace Chh.Application.Abstractions;

/// <summary>
/// Raised when an uploaded license document fails server-side validation — wrong content type or
/// too large (CHH-79/US-CHH-003-02 AC2/AC3). Maps to 422 Unprocessable Entity. Message is one of
/// <see cref="FacilityDocumentConstants"/>'s three copy constants, set by the caller.
/// </summary>
public class InvalidFacilityDocumentException : ChhException
{
    /// <summary>Creates the exception with the given validation-failure message.</summary>
    /// <param name="message">One of <see cref="FacilityDocumentConstants"/>'s message constants.</param>
    public InvalidFacilityDocumentException(string message)
        : base(message)
    {
    }
}
