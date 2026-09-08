namespace Chh.Application.Abstractions;

// Event creation domain exceptions (CHH-38/US-CHH-005-01).

/// <summary>
/// Raised when the authenticated caller's mobile number doesn't resolve to a
/// <see cref="Chh.Domain.Enums.FacilityVerificationStatus.Verified"/> facility (spec's Business
/// Rule: "Events must be created by verified facilities only"). Maps to 403 Forbidden.
/// </summary>
public class FacilityNotVerifiedException : ChhException
{
    /// <summary>Creates the exception with the standard not-verified message.</summary>
    public FacilityNotVerifiedException()
        : base(Chh.Domain.Constants.EventConstants.FacilityNotVerifiedMessage)
    {
    }
}
