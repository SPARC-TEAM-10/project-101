namespace Chh.Application.Abstractions;

// Facility-review domain exceptions (CHH-75/US-CHH-001-03), grouped in one file since each is a
// thin, single-message wrapper — see BloodRequestExceptions.cs for why these stay distinct types
// rather than one shared class (Hellang's type-based status-code dispatch).

/// <summary>
/// Raised when a System Admin tries to approve/reject a facility that isn't
/// <see cref="Chh.Domain.Enums.FacilityVerificationStatus.Pending"/> anymore — already reviewed by
/// another admin, or in a race with a concurrent review. Maps to 409 Conflict.
/// </summary>
public class FacilityAlreadyReviewedException : ChhException
{
    /// <summary>Creates the exception with a message naming the facility's current status.</summary>
    /// <param name="currentStatus">The facility's actual current verification status.</param>
    public FacilityAlreadyReviewedException(string currentStatus)
        : base($"This facility has already been reviewed (currently {currentStatus}).")
    {
    }
}
