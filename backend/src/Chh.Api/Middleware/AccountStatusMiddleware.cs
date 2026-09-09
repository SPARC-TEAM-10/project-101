using System.Security.Claims;
using Chh.Application.Abstractions;
using Chh.Application.Contracts;
using Chh.Domain.Constants;
using Chh.Domain.Enums;

namespace Chh.Api.Middleware;

/// <summary>
/// Blocks a suspended account's already-issued JWT on its next authenticated request
/// (CHH-76/US-CHH-001-04 AC1 — "current session should be invalidated"). JWTs are stateless
/// (api-standards.md §1) with no revocation store, so this per-request DB check is what actually
/// invalidates a live session rather than waiting out the 24h token expiry — same tradeoff
/// <c>ActivityTrackingMiddleware</c> already makes for CHH-34 presence tracking. Scoped to
/// Individual/SystemAdmin — the only two roles backed by an <c>IndividualProfile</c> with an
/// <c>AccountStatus</c>; Hospital/Ngo/Guest carry no such flag.
/// </summary>
public class AccountStatusMiddleware
{
    private readonly RequestDelegate _next;

    /// <summary>Creates the middleware with the next delegate in the pipeline.</summary>
    /// <param name="next">The next middleware to invoke.</param>
    public AccountStatusMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>Rejects the request with 403 if the authenticated caller's account is suspended, otherwise invokes the rest of the pipeline.</summary>
    /// <param name="context">The current HTTP context.</param>
    /// <param name="individualProfileRepository">Scoped repository used to read the caller's current account status.</param>
    public async Task InvokeAsync(HttpContext context, IIndividualProfileRepository individualProfileRepository)
    {
        var role = context.User.FindFirstValue(ClaimTypes.Role);
        if (role is RoleConstants.Individual or RoleConstants.SystemAdmin)
        {
            var mobileNumber = context.User.FindFirstValue(ClaimTypes.MobilePhone);
            if (!string.IsNullOrEmpty(mobileNumber))
            {
                var profile = await individualProfileRepository
                    .GetByMobileNumberAsync(mobileNumber, context.RequestAborted)
                    .ConfigureAwait(false);
                if (profile is { AccountStatus: AccountStatus.Suspended })
                {
                    throw new AccountSuspendedException();
                }
            }
        }

        await _next(context).ConfigureAwait(false);
    }
}
