using Chh.Domain.Constants;

namespace Chh.Application.Abstractions;

// Individual-registration domain exceptions (CHH-F02), grouped in one file since each is a thin,
// single-message wrapper — kept as distinct types (not consolidated into one class) because
// Chh.Api.Extensions.ProblemDetailsServiceCollectionExtensions maps each type to a different
// HTTP status via Hellang's type-based dispatch.

/// <summary>
/// Raised when registration is attempted for a mobile number that has no verified OTP (CHH-9).
/// Maps to 422 Unprocessable Entity.
/// </summary>
public class MobileNumberNotVerifiedException : ChhException
{
    /// <summary>Creates the exception with the standard not-verified message.</summary>
    public MobileNumberNotVerifiedException()
        : base(IndividualProfileConstants.MobileNumberNotVerifiedMessage)
    {
    }
}

/// <summary>
/// Raised when an individual profile already exists for the given mobile number. Maps to 409 Conflict.
/// </summary>
public class IndividualAlreadyRegisteredException : ChhException
{
    /// <summary>Creates the exception with the standard already-registered message.</summary>
    public IndividualAlreadyRegisteredException()
        : base(IndividualProfileConstants.AlreadyRegisteredMessage)
    {
    }
}
