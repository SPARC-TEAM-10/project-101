using Chh.Domain.Constants;

namespace Chh.Application.Abstractions;

/// <summary>
/// Raised when a suspended account (CHH-76/US-CHH-001-04) attempts OTP login (AC2) or makes an
/// authenticated request with an already-issued token (AC1 "current session should be
/// invalidated" — see <c>Chh.Api.Middleware.AccountStatusMiddleware</c>). Maps to 403 Forbidden.
/// </summary>
public class AccountSuspendedException : ChhException
{
    /// <summary>Creates the exception with the standard AC2 message.</summary>
    public AccountSuspendedException()
        : base(AccountConstants.SuspendedMessage)
    {
    }
}
