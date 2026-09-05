namespace Chh.Application.Abstractions;

/// <summary>Shared base type for all CHH domain exceptions. Mapped to RFC 7807 ProblemDetails in <c>Program.cs</c>.</summary>
public abstract class ChhException : Exception
{
    /// <summary>Creates a new domain exception with the given user-facing message.</summary>
    protected ChhException(string message) : base(message)
    {
    }

    /// <summary>Creates a new domain exception with the given user-facing message and inner exception.</summary>
    protected ChhException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
