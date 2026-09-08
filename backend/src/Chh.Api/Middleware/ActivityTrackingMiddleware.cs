using System.Security.Claims;
using Chh.Application.Contracts;

namespace Chh.Api.Middleware;

/// <summary>
/// Records "last seen active" for an authenticated individual on every request (CHH-34 presence
/// tracking), feeding <c>NotificationDispatchService</c>'s in-app-vs-SMS channel decision.
/// Registered after <c>UseAuthentication</c>/<c>UseAuthorization</c> in <c>Program.cs</c> so
/// <see cref="HttpContext.User"/> is populated; runs fire-and-forget-cheap since
/// <see cref="IPresenceTrackerService"/> itself throttles the actual database write.
/// </summary>
public class ActivityTrackingMiddleware
{
    private readonly RequestDelegate _next;

    /// <summary>Creates the middleware with the next delegate in the pipeline.</summary>
    /// <param name="next">The next middleware to invoke.</param>
    public ActivityTrackingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>Tracks activity for the authenticated caller, then invokes the rest of the pipeline.</summary>
    /// <param name="context">The current HTTP context.</param>
    /// <param name="presenceTrackerService">Scoped service that performs the (throttled) update.</param>
    public async Task InvokeAsync(HttpContext context, IPresenceTrackerService presenceTrackerService)
    {
        var mobileNumber = context.User.FindFirstValue(ClaimTypes.MobilePhone);
        if (!string.IsNullOrEmpty(mobileNumber))
        {
            await presenceTrackerService.TrackActivityAsync(mobileNumber, context.RequestAborted).ConfigureAwait(false);
        }

        await _next(context).ConfigureAwait(false);
    }
}
