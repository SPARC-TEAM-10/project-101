namespace Chh.Application.Abstractions;

// Donor accept/decline domain exceptions (CHH-35/US-CHH-004-04), grouped in one file since each
// is a thin, single-message wrapper — see IndividualRegistrationExceptions.cs for why these stay
// distinct types rather than one shared class (Hellang's type-based status-code dispatch).

/// <summary>
/// Raised when a donor tries to accept an already-Fulfilled or already-Expired request (AC3), or
/// loses the race to the last remaining unit against another donor's simultaneous accept (Edge
/// Case). Maps to 422 Unprocessable Entity with the exact AC3 copy.
/// </summary>
public class BloodRequestNoLongerActiveException : ChhException
{
    /// <summary>Creates the exception with the standard AC3 message.</summary>
    public BloodRequestNoLongerActiveException()
        : base("This request is no longer active")
    {
    }
}

/// <summary>
/// Raised when a donor tries to accept or decline a notification they've already responded to.
/// Maps to 409 Conflict.
/// </summary>
public class DonorAlreadyRespondedException : ChhException
{
    /// <summary>Creates the exception with the standard already-responded message.</summary>
    public DonorAlreadyRespondedException()
        : base("You've already responded to this notification.")
    {
    }
}
