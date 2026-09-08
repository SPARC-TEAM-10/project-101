namespace Chh.Application.Abstractions;

// Blood-request domain exceptions (CHH-36/US-CHH-004-05), grouped in one file since each is a
// thin, single-message wrapper — see IndividualRegistrationExceptions.cs for why these stay
// distinct types rather than one shared class (Hellang's type-based status-code dispatch).

/// <summary>
/// Raised when a requester tries to increase the search radius of a request that isn't
/// <c>Matching</c> anymore (already Fulfilled, or Expired — CHH-36 Edge Case: "Request expires
/// while requester is viewing the dashboard"). Maps to 422 Unprocessable Entity.
/// </summary>
public class BloodRequestNotMatchingException : ChhException
{
    /// <summary>Creates the exception with a message naming the request's current state.</summary>
    /// <param name="currentStatus">The request's actual current status, for the error message.</param>
    public BloodRequestNotMatchingException(string currentStatus)
        : base($"This request is {currentStatus.ToLowerInvariant()} and can no longer be updated.")
    {
    }
}

/// <summary>
/// Raised when a requester tries to "increase" the search radius to a value that isn't actually
/// larger than the current one (AC4 — this endpoint only expands, it doesn't shrink). Maps to 422
/// Unprocessable Entity.
/// </summary>
public class RadiusMustIncreaseException : ChhException
{
    /// <summary>Creates the exception with the standard must-increase message.</summary>
    public RadiusMustIncreaseException()
        : base("The new search radius must be larger than the current one.")
    {
    }
}
