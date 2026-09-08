using Chh.Domain.Constants;

namespace Chh.Application.Abstractions;

/// <summary>
/// Raised when facility registration is attempted with a license number that's already
/// registered (CHH-78). Maps to 409 Conflict.
/// </summary>
public class FacilityAlreadyRegisteredException : ChhException
{
    /// <summary>Creates the exception with the standard already-registered message.</summary>
    public FacilityAlreadyRegisteredException()
        : base(FacilityConstants.AlreadyRegisteredMessage)
    {
    }
}
