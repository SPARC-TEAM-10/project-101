namespace Chh.Application.Abstractions;

/// <summary>
/// Raised when an individual profile already exists for the given mobile number. Maps to 409 Conflict.
/// </summary>
public class IndividualAlreadyRegisteredException : ChhException
{
    /// <summary>Creates the exception with the standard already-registered message.</summary>
    public IndividualAlreadyRegisteredException()
        : base("An individual profile already exists for this mobile number")
    {
    }
}
