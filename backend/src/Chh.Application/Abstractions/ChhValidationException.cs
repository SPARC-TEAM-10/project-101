namespace Chh.Application.Abstractions;

/// <summary>Raised when request validation fails outside the FluentValidation pipeline. Maps to 422 Unprocessable Entity.</summary>
public class ChhValidationException : ChhException
{
    /// <summary>Validation failures, keyed by property name.</summary>
    public IDictionary<string, string[]> Failures { get; }

    /// <summary>Creates a validation exception with a single failure.</summary>
    public ChhValidationException(string message, IDictionary<string, string[]>? failures = null) : base(message)
    {
        Failures = failures ?? new Dictionary<string, string[]>();
    }
}
